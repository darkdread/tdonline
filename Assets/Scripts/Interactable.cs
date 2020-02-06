using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Interactable : MonoBehaviour {

    protected bool canInteract = true;
    protected bool wasInRadius = false;

    [SerializeField]
    protected Collider2D interactableTrigger;

    public bool IsInteractable() {
        return canInteract;
    }

    public void IsInteractable(bool interactable) {
        canInteract = interactable;
    }

    protected virtual void OnEnterInteractRadius(TdPlayerController playerController) {
        print("EnterInteractRadius");
        print(this);
        wasInRadius = true;
    }

    protected virtual void OnExitInteractRadius(TdPlayerController playerController) {
        print("ExitInteractRadius");
        print(this);
        wasInRadius = false;
        playerController.playerUi.ShowUseButton(false);
    }

    protected bool IsInRadius(Vector2 position) {
        return interactableTrigger.OverlapPoint(position);
    }

    protected virtual void OnInteract(TdPlayerController playerController) {
        playerController.SetInteractingInstant(true);
        playerController.SetInteractingDelayFrame(false, 1);
        print("OnInteract");
        print(this);
    }

    protected virtual void OnInteractRadiusStay(TdPlayerController playerController) {
        if (!playerController.photonView.IsMine) {
            return;
        }

        // Show it's usable.
        if (IsInRadius(playerController.transform.position)) {

            if (!wasInRadius){
                OnEnterInteractRadius(playerController);
            }

            playerController.playerUi.ShowUseButton(true);
            if (Input.GetButtonDown("Use") && !playerController.IsInteracting()) {
                OnInteract(playerController);
            }
        } else if (wasInRadius) {
            OnExitInteractRadius(playerController);
        }
    }

    protected virtual void Update() {
        if (!canInteract) {
            return;
        }

        // TdPlayerController[] playerControllers = TdGameManager.GetTdPlayerControllersNearPosition(transform.position, 2f);

        List<Collider2D> colliders = new List<Collider2D>();
        interactableTrigger.GetContacts(colliders);

        foreach (Collider2D c in colliders) {
            TdPlayerController playerController = c.GetComponent<TdPlayerController>();

            if (!playerController){
                continue;
            }

            OnInteractRadiusStay(playerController);
        }
    }
}
