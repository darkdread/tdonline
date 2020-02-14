using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using PhotonPeer = ExitGames.Client.Photon.PhotonPeer;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TdGame {
    public static string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
    public static string WAVE_INFO = "CurrentWaveInfo_";
}

public class TdGameManager : MonoBehaviourPunCallbacks
{
    public static TdGameManager instance = null;
    public static int playersLoaded = 0;
    public static TdGameSettings gameSettings;

    private Coroutine spawnEnemiesRoutine;

    [Header("Game Interface")]
    public GameObject gameCanvas;

    private void Awake(){
        instance = this;
        gameSettings = GetComponent<TdGameSettings>();

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
    }

    private void Start(){
        // InfoText.text = "Waiting for other players...";

        Hashtable props = new Hashtable {
            {TdGame.PLAYER_LOADED_LEVEL, true}
        };

        if (PhotonPeer.RegisterType(typeof(CollectablePun), (byte)'Z', CollectablePun.Serialize, CollectablePun.Deserialize)){
            print("Registered type CollectablePun.");
        } else {
            print("Failed to register type CollectablePun.");
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    [PunRPC]
    public void DestroySceneObject(int viewId){
        PhotonView obj = PhotonNetwork.GetPhotonView(viewId);
        PhotonNetwork.Destroy(obj);
    }

    [PunRPC]
    public void SpawnCollectableObject(byte[] customType){
        print("SpawnCollectableObject");

        CollectablePun data = (CollectablePun) CollectablePun.Deserialize(customType);

        string resourceName = data.resourceName;
        // print(resourceName);

        GameObject spawnedObject = PhotonNetwork.InstantiateSceneObject(resourceName,
                Vector3.zero, Quaternion.identity);
        PhotonView objectPhotonView = spawnedObject.GetComponent<PhotonView>();

        if (data.playerViewId != 0){
            TdPlayerController playerController = PhotonNetwork.GetPhotonView(data.playerViewId).GetComponent<TdPlayerController>();
            playerController.photonView.RPC("OnCarryGameObject", RpcTarget.All, objectPhotonView.ViewID);
        }
    }

    private IEnumerator SpawnEnemies(){
        while(true){
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

            yield return new WaitForSeconds(longestWaveDuration/1000f);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient){
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber){

            // Sync the wave id.
            foreach(EnemySpawner spawner in gameSettings.enemySpawners){
                WaveSpawnInfo waveInfo = WaveSpawnInfo.Deserialize((byte[]) PhotonNetwork.CurrentRoom.CustomProperties[TdGame.WAVE_INFO + spawner.spawnerId]);
                
                spawner.LoadWaveProgress(waveInfo);
            }
            StartCoroutine(SpawnEnemies());
        }
    }

    private void StartGame(){
        int localPlayerId = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        Vector3 position = gameSettings.playerStartPositions[localPlayerId].position;
        Quaternion rotation = Quaternion.identity;

        // Instantiate the player, player name, and set custom property of player controller for local player.
        GameObject playerObject = PhotonNetwork.Instantiate("Player", position, rotation, 0);
        TdPlayerController playerController = playerObject.GetComponent<TdPlayerController>();

        playerObject.name = playerObject.name + localPlayerId;

        if (PhotonNetwork.IsMasterClient)
        {
            spawnEnemiesRoutine = StartCoroutine(SpawnEnemies());
        }
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

    private void Update(){

        if (Input.GetKeyDown(KeyCode.X)){

            // Sync the wave id.
            foreach(EnemySpawner spawner in gameSettings.enemySpawners){
                WaveSpawnInfo waveInfo = WaveSpawnInfo.Deserialize((byte[]) PhotonNetwork.CurrentRoom.CustomProperties[TdGame.WAVE_INFO + spawner.spawnerId]);
                
                spawner.LoadWaveProgress(waveInfo);
            }

            if (spawnEnemiesRoutine != null){
                StopCoroutine(spawnEnemiesRoutine);
            }
            spawnEnemiesRoutine = StartCoroutine(SpawnEnemies());
        } else if (Input.GetKeyDown(KeyCode.Z)) {
            foreach(EnemySpawner spawner in gameSettings.enemySpawners){
                spawner.SaveWaveProgress();
            }
        }
    }
}
