using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

using Newtonsoft.Json;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using PhotonPeer = ExitGames.Client.Photon.PhotonPeer;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TdGame {
    public static string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
    public static string WAVE_INFO = "CurrentWaveInfo_";
}

public struct ResourceData {
    public Sprite sprite;
    public int amount;
}

public enum EndGameEnum {
    Collected,
    Shot,
    Killed,
}

[System.Serializable]
public struct EndGameData {

    public Dictionary<EndGameEnum, Dictionary<string, ResourceData>> resourceDict;

    public Dictionary<string, ResourceData> GetDictionaryEnum(EndGameEnum endGameEnum){
        return resourceDict[endGameEnum];
    }

    public void UpdateCount(EndGameEnum endGameEnum, Sprite data, int amount = 1){

        System.Diagnostics.Debug.WriteLine(data.name);

        if (resourceDict.ContainsKey(endGameEnum)){
            ResourceData r = resourceDict[endGameEnum][data.name];
            r.amount += amount;
            resourceDict[endGameEnum][data.name] = r;
            return;
        }

        Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();

        ResourceData resourceData = new ResourceData(){
            amount = amount,
            sprite = data,
        };

        resources.Add(data.name, resourceData);
        resourceDict.Add(endGameEnum, resources);
    }

    public static implicit operator EndGameData(string defaults) {
        return new EndGameData() {
            resourceDict = new Dictionary<EndGameEnum, Dictionary<string, ResourceData>>()
        };
    }
}

public class TdGameManager : MonoBehaviourPunCallbacks
{
    public static TdGameManager instance;
    public static int playersLoaded;
    public static bool isPaused;
    public static volatile TdGameSettings gameSettings;
    public static Castle castle;
    public static List<TdPlayerController> players = new List<TdPlayerController>();

    private static int globalSpawnTime;

    [Header("Game Interface")]
    public GameObject gameCanvas;
    public LoseUi loseUi;

    private void Awake(){
        instance = this;
        gameSettings = GetComponent<TdGameSettings>();
        castle = GetComponentInChildren<Castle>();
        Enemy.enemyList = new List<Enemy>();

        PhotonView[] photonViews = Resources.FindObjectsOfTypeAll(typeof(PhotonView)) as PhotonView[];

        int nextInstanceId = 0;
        foreach(PhotonView photonView in photonViews){
            if (photonView.IsSceneView && photonView.ViewID != 0){
                nextInstanceId += 1;
                print(photonView);
            }
        }

        // Note that we modified the property of lastUsedViewSubIdStatic from private to public.
        PhotonNetwork.lastUsedViewSubIdStatic = nextInstanceId;

        int spawnerStaticId = 0;
        foreach(EnemySpawner spawner in gameSettings.enemySpawners){
            spawnerStaticId += 1;
            spawner.spawnerId = spawnerStaticId;
        }

        isPaused = false;
        playersLoaded = 0;
    }

    private void Start(){
        // InfoText.text = "Waiting for other players...";

        Hashtable props = new Hashtable {
            {TdGame.PLAYER_LOADED_LEVEL, true}
        };

        // if (PhotonPeer.RegisterType(typeof(CollectablePun), (byte)'Z', CollectablePun.Serialize, CollectablePun.Deserialize)){
        //     print("Registered type CollectablePun.");
        // } else {
        //     print("Failed to register type CollectablePun.");
        // }

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    [PunRPC]
    public void ShowGameObject(int viewId, bool show){
        PhotonView obj = PhotonNetwork.GetPhotonView(viewId);
        obj.gameObject.SetActive(show);
    }

    [PunRPC]
    public void AddProjectileComponent(int viewId){
        PhotonView obj = PhotonNetwork.GetPhotonView(viewId);
        Projectile projectile = obj.gameObject.AddComponent<Projectile>();

        projectile.projectileData = projectile.GetComponent<Collectable>().projectileData;
        projectile.gameObject.layer = 12;
        projectile.transform.position = Vector3.up;
    }

    [PunRPC]
    private void DestroySceneObjectRpc(int viewId){
        PhotonView obj = PhotonNetwork.GetPhotonView(viewId);
        PhotonNetwork.Destroy(obj);
    }

    public void DestroySceneObject(int viewId){
        photonView.RPC("DestroySceneObjectRpc", RpcTarget.MasterClient, viewId);
    }

    [PunRPC]
    public void SpawnCollectableObject(byte[] customType){
        CollectablePun data = (CollectablePun) CollectablePun.Deserialize(customType);

        string resourceName = data.resourceName;

        if (PhotonNetwork.IsMasterClient){
            GameObject spawnedObject = PhotonNetwork.InstantiateSceneObject(resourceName,
                    Vector3.zero, Quaternion.identity);
            PhotonView objectPhotonView = spawnedObject.GetComponent<PhotonView>();

            if (data.playerViewId != 0){
                TdPlayerController playerController = PhotonNetwork.GetPhotonView(data.playerViewId).GetComponent<TdPlayerController>();
                playerController.photonView.RPC("OnCarryGameObject", RpcTarget.All, objectPhotonView.ViewID);
            }
        }

        if (data.playerViewId != 0){
            TdPlayerController playerController = PhotonNetwork.GetPhotonView(data.playerViewId).GetComponent<TdPlayerController>();
            GameObject projectile = GetPrefabFromResource(resourceName);
            playerController.playerEndGameData.UpdateCount(EndGameEnum.Collected, projectile.GetComponent<SpriteRenderer>().sprite);
        }
    }

    public void Lose(){
        PauseGame(true);

        // Load lose ui.
        loseUi.gameObject.SetActive(true);
        loseUi.LoadEndGameData();
    }

    public static void PlayerLeaveGame(){
        if (PhotonNetwork.LocalPlayer.IsLocal){
            PhotonNetwork.Disconnect();
            // PhotonNetwork.LoadLevel("LobbyScene");
        }
    }

    override public void OnDisconnected(DisconnectCause cause){
        if (PhotonNetwork.LocalPlayer.IsLocal){
            print(cause);
            StartCoroutine(LoadLobby());
        }
    }

    private IEnumerator LoadLobby(){
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene("LobbyScene");
    }

    public static string StripCloneFromString(string input){
        return input.Replace("(Clone)", "");
    }

    public static GameObject GetPrefabFromResource(string resourcePath){
        return Resources.Load<GameObject>(resourcePath);
    }

    private void SpawnEnemies(){
        int longestWaveDuration = 0;
        int waveFade = 5000;
        
        foreach(EnemySpawner spawner in gameSettings.enemySpawners){
            if (spawner.currentWaveId + 1 >= spawner.waves.Length){
                print("Reset wave");
                spawner.currentWaveId = 0;
            }
            
            spawner.SpawnWave();

            int waveDuration = spawner.GetWaveSpawnDuration(spawner.currentWaveId);
            if (waveDuration > longestWaveDuration){
                longestWaveDuration = waveDuration;
            }
        }

        longestWaveDuration += waveFade;
        globalSpawnTime = longestWaveDuration;
    }

    public override void OnMasterClientSwitched(Player newMasterClient){
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber){

            // Sync the wave id.
            foreach(EnemySpawner spawner in gameSettings.enemySpawners){
                WaveSpawnInfo waveInfo = WaveSpawnInfo.Deserialize((byte[]) PhotonNetwork.CurrentRoom.CustomProperties[TdGame.WAVE_INFO + spawner.spawnerId]);
                
                spawner.LoadWaveProgress(waveInfo);
            }
            StartSpawnTimer();
        }
    }

    [PunRPC]
    private void AddPlayerToList(int viewId){
        TdPlayerController playerController = PhotonNetwork.GetPhotonView(viewId).GetComponent<TdPlayerController>();
        players.Add(playerController);
    }

    private void StartGame(){
        int localPlayerId = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        Vector3 position = gameSettings.playerStartPositions[localPlayerId].position;
        Quaternion rotation = Quaternion.identity;

        // Instantiate the player, player name, and set custom property of player controller for local player.
        GameObject playerObject = PhotonNetwork.Instantiate("Player", position, rotation, 0);
        TdPlayerController playerController = playerObject.GetComponent<TdPlayerController>();
        photonView.RPC("AddPlayerToList", RpcTarget.All, playerController.photonView.ViewID);

        playerObject.name = playerObject.name + localPlayerId;

        if (PhotonNetwork.IsMasterClient)
        {
            StartSpawnTimer();
        }
    }

    private void StartSpawnTimer(){
        foreach(EnemySpawner spawner in gameSettings.enemySpawners){
            print(spawner.name);
        }

        globalSpawnTime = 0;
    }

    public static Enemy[] GetEnemiesOverlapSphere(Vector2 position, float radius){
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius, 1 << 10);

        List<Enemy> enemies = new List<Enemy>();
        foreach(Collider2D c in colliders){
            enemies.Add(c.GetComponent<Enemy>());
        }

        return enemies.ToArray();
    }

    public static TdPlayerController[] GetTdPlayerControllersNearPosition(Vector2 position, float radius){
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius, 1 << 8);

        List<TdPlayerController> controllers = new List<TdPlayerController>();
        foreach(Collider2D c in colliders){
            controllers.Add(c.GetComponent<TdPlayerController>());
        }

        return controllers.ToArray();
    }

    public static Vector3 GetDirectionOfTransform2D(Transform transform){
        return transform.localScale.x >= 0 ? Vector3.right : Vector3.left;
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps){
        object playerLoadedLevel;

        int playerId = targetPlayer.GetPlayerNumber();

        if (changedProps.TryGetValue(TdGame.PLAYER_LOADED_LEVEL, out playerLoadedLevel)){
            playersLoaded += 1;

            if (playersLoaded == PhotonNetwork.CurrentRoom.PlayerCount){
                StartGame();
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged){

        // foreach(object key in PhotonNetwork.CurrentRoom.CustomProperties.Keys){
        //     print(key);
        // }
    }

    [PunRPC]
    private void PauseGameRpc(bool pause){
        isPaused = pause;

        foreach(Enemy enemy in Enemy.enemyList){
            enemy.StopMovement(pause);
        }

        // Time.timeScale = isPaused ? 0 : 1;
    }

    public void PauseGame(bool pause){
        photonView.RPC("PauseGameRpc", RpcTarget.All, pause);
    }

    private void Update(){

        if (PhotonNetwork.IsMasterClient && !isPaused){
            globalSpawnTime -= (int) (Time.deltaTime * 1000);

            if (globalSpawnTime <= 0){
                SpawnEnemies();
            }
        }

        if (Input.GetKeyDown(KeyCode.X)){

            // Sync the wave id.
            foreach(EnemySpawner spawner in gameSettings.enemySpawners){
                WaveSpawnInfo waveInfo = WaveSpawnInfo.Deserialize((byte[]) PhotonNetwork.CurrentRoom.CustomProperties[TdGame.WAVE_INFO + spawner.spawnerId]);
                
                spawner.LoadWaveProgress(waveInfo);
            }

            StartSpawnTimer();
        } else if (Input.GetKeyDown(KeyCode.Z)) {
            foreach(EnemySpawner spawner in gameSettings.enemySpawners){
                spawner.SaveWaveProgress();
            }
        } else if (Input.GetKeyDown(KeyCode.Q)){
            foreach(TdPlayerController playerController in players){
                print(JsonConvert.SerializeObject(playerController.playerEndGameData));
            }
        } else if (Input.GetKeyDown(KeyCode.Escape)){
            // PauseGame(!isPaused);
            Lose();
        }
    }
}
