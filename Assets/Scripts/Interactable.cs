using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    protected Collider2D interactableTrigger;
    
    protected virtual void OnEnterInteractRadius(TdPlayerController playerController){
        print("EnterInteractRadius");
        print(playerController);
        playerController.playerUi.ShowUseButton(true);
    }

    protected virtual void OnExitInteractRadius(TdPlayerController playerController){
        playerController.playerUi.ShowUseButton(false);
    }

    protected bool IsInRadius(Vector2 position){
        return interactableTrigger.OverlapPoint(position);
    }

    protected virtual void Update(){
        TdPlayerController[] playerControllers = TdGameManager.GetTdPlayerControllersNearPosition(transform.position, 2f);

        foreach(TdPlayerController playerController in playerControllers){
            if (!playerController.photonView.IsMine){
                continue;
            }

            // Show it's usable.
            if (IsInRadius(playerController.transform.position)){
                OnEnterInteractRadius(playerController);
            } else {
                OnExitInteractRadius(playerController);
            }
        }
    }
}
