using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class TdGame {
    public static string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
}

public class TdGameManager : MonoBehaviourPunCallbacks
{
    public static TdGameManager instance = null;
    public static int playersLoaded = 0;

    [Header("Setup")]
    public Transform[] playerStartPositions;


    [Header("Game Interface")]
    public GameObject gameCanvas;


    private Dictionary<int, TdPlayerController> playerControllers = new Dictionary<int, TdPlayerController>();
    private Dictionary<int, TextMeshProUGUI> playerNameTexts = new Dictionary<int, TextMeshProUGUI>();

    private void Awake(){
        instance = this;
    }

    private void Start(){
        // InfoText.text = "Waiting for other players...";

        Hashtable props = new Hashtable {
            {TdGame.PLAYER_LOADED_LEVEL, true}
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public TdPlayerController GetPlayerController(int playerId){
        return playerControllers[playerId];
    }

    private void Update(){
        foreach(KeyValuePair<int, TextMeshProUGUI> kvp in playerNameTexts){
            TdPlayerController currentPlayer = GetPlayerController(kvp.Key);

            if (currentPlayer == null){
                print($"Player {kvp.Key} is empty!");
                continue;
            }

            kvp.Value.transform.position = Camera.main.WorldToScreenPoint(currentPlayer.transform.position);
        }
    }

    private void StartGame(){
        int localPlayerId = PhotonNetwork.LocalPlayer.GetPlayerNumber();

        print(localPlayerId);

        Vector3 position = instance.playerStartPositions[localPlayerId].position;
        Quaternion rotation = Quaternion.identity;

        // Instantiate the player, player name, and set custom property of player controller for local player.
        GameObject playerObject = PhotonNetwork.Instantiate("Player", position, rotation, 0);
        TdPlayerController playerController = playerObject.GetComponent<TdPlayerController>();

        if (PhotonNetwork.IsMasterClient)
        {
            // StartCoroutine(SpawnAsteroid());
        }
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
