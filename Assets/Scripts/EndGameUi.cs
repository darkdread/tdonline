﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class EndGameUi : MonoBehaviour
{
    public TextMeshProUGUI resultText;
    public Button backToMain;

    public ImageWithText imageWithTextPrefab;
    public Transform collectedResources;
    public Transform killedResources;
    public Transform shotResources;

    private void Awake(){
        backToMain.onClick.AddListener(TdGameManager.PlayerLeaveGame);
    }

    public void SetResult(string text){
        resultText.text = text;
    }

    public void LoadEndGameData(){

        EndGameData overallData = "Defaults";

        // Add up all the player data.
        for(int i = 0; i < TdGameManager.players.Count; i++){
            TdPlayerController player = TdGameManager.players[i];

            foreach(KeyValuePair<EndGameEnum, Dictionary<string, ResourceData>> kvp in player.playerEndGameData.resourceDict){

                foreach(KeyValuePair<string, ResourceData> kvp2 in kvp.Value){
                    overallData.UpdateCount(kvp.Key, kvp2.Value.data, kvp2.Value.amount);
                }
            }
        }

        // Display overall data.
        foreach(KeyValuePair<EndGameEnum, Dictionary<string, ResourceData>> kvp in overallData.resourceDict){

            Transform parentTransform = collectedResources;

            if (kvp.Key == EndGameEnum.Killed){
                parentTransform = killedResources;
            } else if (kvp.Key == EndGameEnum.Shot){
                parentTransform = shotResources;
            }

            // Destroy all UI object, just in case this function is called more than once.
            foreach(Transform t in parentTransform){
                Destroy(t.gameObject);
            }

            foreach(KeyValuePair<string, ResourceData> kvp2 in kvp.Value){
                ImageWithText imageWithText = Instantiate<ImageWithText>(imageWithTextPrefab, parentTransform);
                Sprite sprite = kvp2.Value.data.sprite;

                imageWithText.SetImageSprite(sprite);
                imageWithText.SetResourceText($"x{kvp2.Value.amount}");
            }
        }
    }
}
