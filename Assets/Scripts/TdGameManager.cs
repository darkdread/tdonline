﻿using System.Collections;
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
}

public class TdGameManager : MonoBehaviourPunCallbacks
{
    public static TdGameManager instance = null;
    public static int playersLoaded = 0;
    public static TdGameSettings gameSettings;

    [Header("Game Interface")]
    public GameObject gameCanvas;

    private void Awake(){
        instance = this;
        gameSettings = GetComponent<TdGameSettings>();
    }

    private void Start(){
        // InfoText.text = "Waiting for other players...";

        Hashtable props = new Hashtable {
            {TdGame.PLAYER_LOADED_LEVEL, true}
        };

        if (PhotonPeer.RegisterType(typeof(CollectablePun), (byte)'Z', CollectablePun.Serialize, CollectablePun.Deserialize)){
            print("he");
        } else {
            print("hez");
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
        print(resourceName);

        GameObject spawnedObject = PhotonNetwork.InstantiateSceneObject(resourceName,
                Vector3.zero, Quaternion.identity);
        PhotonView objectPhotonView = spawnedObject.GetComponent<PhotonView>();

        if (data.playerViewId != 0){
            TdPlayerController playerController = PhotonNetwork.GetPhotonView(data.playerViewId).GetComponent<TdPlayerController>();
            playerController.photonView.RPC("OnCarryGameObject", RpcTarget.All, objectPhotonView.ViewID);
        }
    }

    private IEnumerator SpawnEnemies(){
        for(int i = 0; i < 100; i++){
            if (!PhotonNetwork.IsMasterClient){
                yield break;
            }

            float longestWaveDuration = 0f;
            float waveFade = 5f;

            foreach(EnemySpawner spawner in gameSettings.enemySpawners){
                if (spawner.currentWaveId + 1 >= spawner.waves.Length){
                    spawner.currentWaveId = -1;
                }
                spawner.SpawnNextWave();

                float waveDuration = spawner.GetWaveSpawnDuration(spawner.currentWaveId);
                if (waveDuration > longestWaveDuration){
                    longestWaveDuration = waveDuration;
                }
            }

            longestWaveDuration += waveFade;

            yield return new WaitForSeconds(longestWaveDuration);
        }
    }

    private void StartGame(){
        int localPlayerId = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        print(localPlayerId);

        Vector3 position = gameSettings.playerStartPositions[localPlayerId].position;
        Quaternion rotation = Quaternion.identity;

        // Instantiate the player, player name, and set custom property of player controller for local player.
        GameObject playerObject = PhotonNetwork.Instantiate("Player", position, rotation, 0);
        TdPlayerController playerController = playerObject.GetComponent<TdPlayerController>();

        playerObject.name = playerObject.name + localPlayerId;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnEnemies());
        }
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
}
