using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public EndData data;
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

    public void UpdateCount(EndGameEnum endGameEnum, EndData data, int amount = 1){

        Sprite sprite = data.sprite;

        if (resourceDict.ContainsKey(endGameEnum)){

            // Add new resource to resource data.
            if (!resourceDict[endGameEnum].ContainsKey(data.name)){
                ResourceData newResourceData = new ResourceData(){
                    amount = amount,
                    data = data
                };

                resourceDict[endGameEnum].Add(data.name, newResourceData);
                return;
            }

            ResourceData r = resourceDict[endGameEnum][data.name];
            r.amount += amount;
            resourceDict[endGameEnum][data.name] = r;
            return;
        }

        // First entry of EndGameEnum.
        Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();
        ResourceData resourceData = new ResourceData(){
            amount = amount,
            data = data
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
    public static List<TdPlayerController> players;

    private static int globalSpawnTime;
    private bool allowSpawn;

    [Header("Game Interface")]
    public Transform gameCanvas;
    public Transform gameUiCanvas;
    public Transform emoteCanvas;
    public EndGameUi endGameUi;

    [Header("Misc")]
    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;

    private void Awake(){
        instance = this;
        gameSettings = GetComponent<TdGameSettings>();
        castle = GetComponentInChildren<Castle>();
        players = new List<TdPlayerController>();
        Enemy.enemyList = new List<Enemy>();
        EnemySpawner.wavesFinished = false;

        PhotonView[] photonViews = Resources.FindObjectsOfTypeAll(typeof(PhotonView)) as PhotonView[];

        int nextInstanceId = 0;
        foreach(PhotonView photonView in photonViews){
            if (photonView.IsSceneView && photonView.ViewID != 0){
                nextInstanceId += 1;
                print(photonView);
            }
        }

        // nextInstanceId = 100;

        // Note that we modified the property of lastUsedViewSubIdStatic from private to public.
        PhotonNetwork.lastUsedViewSubIdStatic = nextInstanceId;

        int spawnerStaticId = 0;
        foreach(EnemySpawner spawner in gameSettings.enemySpawners){
            spawnerStaticId += 1;
            spawner.spawnerId = spawnerStaticId;
        }

        isPaused = false;
        playersLoaded = 0;

        if (gameSettings.progressInstant){
            gameSettings.progressCollectTime = 0f;
            gameSettings.progressReloadTime = 0f;
        }
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
    private void SetOwningPlayerRpc(int playerId, int projectileId){
        PhotonNetwork.GetPhotonView(projectileId).GetComponent<Projectile>().owningPlayer = PhotonNetwork.GetPhotonView(playerId).GetComponent<TdPlayerController>();
    }

    public void SetOwningPlayer(int playerId, int projectileId){
        photonView.RPC("SetOwningPlayerRpc", RpcTarget.All, playerId, projectileId);
    }

    [PunRPC]
    private void UpdateCountRpc(int playerId, int projectileId, EndGameEnum endGameEnum){
        PhotonNetwork.GetPhotonView(playerId).GetComponent<TdPlayerController>().playerEndGameData.UpdateCount(
            endGameEnum, PhotonNetwork.GetPhotonView(projectileId).GetComponent<Projectile>().endData
        );
    }

    public void UpdateCount(int playerId, int projectileId, EndGameEnum endGameEnum){
        photonView.RPC("UpdateCountRpc", RpcTarget.All, playerId, projectileId, endGameEnum);
    }

    [PunRPC]
    private void ShowGameObjectRpc(int viewId, bool show){
        PhotonView obj = PhotonNetwork.GetPhotonView(viewId);
        obj.gameObject.SetActive(show);
    }

    public void ShowGameObject(int viewId, bool show){
        photonView.RPC("ShowGameObjectRpc", RpcTarget.All, viewId, show);
    }

    [PunRPC]
    public void AddProjectileComponentRpc(int viewId){
        PhotonView obj = PhotonNetwork.GetPhotonView(viewId);
        Projectile projectile = obj.gameObject.AddComponent<Projectile>();

        projectile.endData = projectile.GetComponent<Collectable>().endData;
        projectile.projectileData = projectile.GetComponent<Collectable>().projectileData;
        projectile.gameObject.layer = 12;
    }

    public void AddProjectileComponent(int viewId){
        photonView.RPC("AddProjectileComponentRpc", RpcTarget.All, viewId);
    }

    [PunRPC]
    private void DestroyPhotonNetworkedObjectRpc(int viewId){
        PhotonView obj = PhotonNetwork.GetPhotonView(viewId);

        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy){
            Enemy.enemyList.Remove(enemy);
            if (EnemySpawner.wavesFinished && Enemy.enemyList.Count <= 0){
                TdGameManager.instance.Win();
            }
        }

        Projectile projectile = obj.GetComponent<Projectile>();
        if (projectile){
            Projectile.projectileList.Remove(projectile);

            projectile.SpawnExplosion();
        }

        if (obj.IsMine){
            PhotonNetwork.Destroy(obj.gameObject);
        }
    }

    public void DestroyPhotonNetworkedObject(PhotonView objectPhotonView){
        photonView.RPC("DestroyPhotonNetworkedObjectRpc", RpcTarget.All, objectPhotonView.ViewID);
    }

    [PunRPC]
    private void PlaySoundRpc(int viewId, string clipName, float volume){
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);
        AudioSource source = view.GetComponent<AudioSource>();
        MonoBehaviour[] scripts = view.GetComponents<MonoBehaviour>();

        IAudioClipObject audioClipObjectInterface = null;

        foreach(MonoBehaviour script in scripts){
            audioClipObjectInterface = script as IAudioClipObject;
            if (audioClipObjectInterface != null){
                break;
            }
        }

        if (audioClipObjectInterface == null){
            return;
        }

        AudioClipObject audioClipObject = audioClipObjectInterface.GetAudioClipObject();

        if (source){
            source.PlayOneShot(audioClipObject.GetAudioClipFromString(clipName), volume);
            return;
        }

        sfxAudioSource.PlayOneShot(audioClipObject.GetAudioClipFromString(clipName), volume);
    }

    public void PlaySound(int viewId, string clipName, float volume = 1f){
        photonView.RPC("PlaySoundRpc", RpcTarget.All, viewId, clipName, volume);
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
            GameObject spawnedObject = GetPrefabFromResource(resourceName);
            playerController.playerEndGameData.UpdateCount(EndGameEnum.Collected, spawnedObject.GetComponent<Collectable>().endData);
        }
    }

    public void Win(){
        PauseGame(true);

        endGameUi.gameObject.SetActive(true);
        endGameUi.LoadEndGameData();
        endGameUi.SetResult(gameSettings.resultWinText);
    }

    public void Lose(){
        PauseGame(true);

        endGameUi.gameObject.SetActive(true);
        endGameUi.LoadEndGameData();
        endGameUi.SetResult(gameSettings.resultLoseText);
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
            if (spawner.currentWaveId >= spawner.waves.Length){
                // print("Reset wave");
                continue;
                spawner.currentWaveId = 0;
            }
            
            spawner.SpawnWave();

            int waveDuration = spawner.GetWaveSpawnDuration(spawner.currentWaveId);
            if (waveDuration > longestWaveDuration){
                longestWaveDuration = waveDuration;
            }
        }

        // All waves have ended.
        if (longestWaveDuration == 0){
            photonView.RPC("SetWaveFinished", RpcTarget.All, true);
            return;
        }

        longestWaveDuration += waveFade;
        globalSpawnTime = longestWaveDuration;
    }

    [PunRPC]
    private void SetWaveFinished(bool finished){
        EnemySpawner.wavesFinished = finished;
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
        GameObject playerObject = PhotonNetwork.Instantiate(TdGameManager.gameSettings.playerPrefab, position, rotation, 0);
        TdPlayerController playerController = playerObject.GetComponent<TdPlayerController>();
        photonView.RPC("AddPlayerToList", RpcTarget.All, playerController.photonView.ViewID);

        playerObject.name = playerObject.name + localPlayerId;

        if (PhotonNetwork.IsMasterClient && gameSettings.spawnEnemies)
        {
            StartSpawnTimer();
        }
    }

    private void StartSpawnTimer(){
        allowSpawn = true;
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

        foreach(Projectile projectile in Projectile.projectileList){
            projectile.StopMovement(pause);
        }

        // Time.timeScale = isPaused ? 0 : 1;
    }

    public void PauseGame(bool pause){
        photonView.RPC("PauseGameRpc", RpcTarget.All, pause);
    }

    private void Update(){

        if (PhotonNetwork.IsMasterClient && !isPaused && allowSpawn && gameSettings.spawnEnemies && !EnemySpawner.wavesFinished){
            globalSpawnTime -= (int) (Time.deltaTime * 1000);

            if (globalSpawnTime <= 0){
                SpawnEnemies();
            }
        }

        // if (Input.GetKeyDown(KeyCode.X)){

        //     // Sync the wave id.
        //     foreach(EnemySpawner spawner in gameSettings.enemySpawners){
        //         WaveSpawnInfo waveInfo = WaveSpawnInfo.Deserialize((byte[]) PhotonNetwork.CurrentRoom.CustomProperties[TdGame.WAVE_INFO + spawner.spawnerId]);
                
        //         spawner.LoadWaveProgress(waveInfo);
        //     }

        //     StartSpawnTimer();
        // } else if (Input.GetKeyDown(KeyCode.Z)) {
        //     foreach(EnemySpawner spawner in gameSettings.enemySpawners){
        //         spawner.SaveWaveProgress();
        //     }
        // } else if (Input.GetKeyDown(KeyCode.Q)){
        //     foreach(TdPlayerController playerController in players){
        //         print(JsonConvert.SerializeObject(playerController.playerEndGameData));
        //     }
        // } else if (Input.GetKeyDown(KeyCode.Escape)){
        //     // PauseGame(!isPaused);
        //     Lose();
        // }
    }
}
