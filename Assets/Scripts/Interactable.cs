using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Interactable : MonoBehaviour {
    [SerializeField]
    protected Collider2D interactableTrigger;

    protected virtual void OnEnterInteractRadius(TdPlayerController playerController) {
        print("EnterInteractRadius");
        print(playerController);
        playerController.playerUi.ShowUseButton(true);
    }

    protected virtual void OnExitInteractRadius(TdPlayerController playerController) {
        playerController.playerUi.ShowUseButton(false);
    }

    protected bool IsInRadius(Vector2 position) {
        return interactableTrigger.OverlapPoint(position);
    }

    protected virtual void OnInteract(TdPlayerController playerController) {
        print("OnInteract");
        print(playerController);
    }

    protected virtual void OnInteractRadiusStay(TdPlayerController playerController) {
        if (!playerController.photonView.IsMine) {
            return;
        }

        // Show it's usable.
        if (IsInRadius(playerController.transform.position)) {
            OnEnterInteractRadius(playerController);

            if (Input.GetButtonDown("Use")) {
                OnInteract(playerController);
            }
        } else {
            OnExitInteractRadius(playerController);
        }
    }

    protected void Update() {
        TdPlayerController[] playerControllers = TdGameManager.GetTdPlayerControllersNearPosition(transform.position, 2f);

        foreach (TdPlayerController playerController in playerControllers) {
            OnInteractRadiusStay(playerController);
        }
    }
}
