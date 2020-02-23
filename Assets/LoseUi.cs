using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class LoseUi : MonoBehaviour
{
    public Button backToMain;

    public ImageWithText imageWithTextPrefab;
    public Transform collectedResources;
    public Transform killedResources;
    public Transform shotResources;

    private void Awake(){
        backToMain.onClick.AddListener(TdGameManager.PlayerLeaveGame);
    }

    public void LoadEndGameData(){

        EndGameData overallData = "Defaults";

        for(int i = 0; i < TdGameManager.players.Count; i++){
            TdPlayerController player = TdGameManager.players[i];

            foreach(KeyValuePair<EndGameEnum, Dictionary<string, ResourceData>> kvp in player.playerEndGameData.resourceDict){

                foreach(KeyValuePair<string, ResourceData> kvp2 in kvp.Value){
                    print(kvp.Key);
                    overallData.UpdateCount(kvp.Key, kvp2.Value.sprite, kvp2.Value.amount);
                }
            }
        }

        foreach(KeyValuePair<EndGameEnum, Dictionary<string, ResourceData>> kvp in overallData.resourceDict){

            Transform parentTransform = collectedResources;

            if (kvp.Key == EndGameEnum.Killed){
                parentTransform = killedResources;
            } else if (kvp.Key == EndGameEnum.Shot){
                parentTransform = shotResources;
            }

            ImageWithText imageWithText = Instantiate<ImageWithText>(imageWithTextPrefab, parentTransform);

            foreach(KeyValuePair<string, ResourceData> kvp2 in kvp.Value){
                Sprite sprite = kvp2.Value.sprite;

                imageWithText.SetImageSprite(sprite);
                imageWithText.SetResourceText($"x{kvp2.Value.amount}");
            }
        }
    }
}
