using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Interactable : MonoBehaviour {

    protected bool canInteract = true;
    protected HashSet<TdPlayerController> interactedCached = new HashSet<TdPlayerController>();

    [SerializeField]
    protected Collider2D interactableTrigger;

    public bool IsInteractable() {
        return canInteract;
    }

    public void SetInteractable(bool interactable) {
        canInteract = interactable;

        if (!canInteract){
            RemoveAllInteracted();
        }
        // UpdateInteractivity();
    }

    protected virtual void OnEnterInteractRadius(TdPlayerController playerController) {
        print("EnterInteractRadius");
        print(this);
    }

    protected virtual void OnExitInteractRadius(TdPlayerController playerController) {
        print("ExitInteractRadius");
        print(this);
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
        // if (IsInRadius(playerController.transform.position)) {

            playerController.playerUi.ShowUseButton(true);

            if (Input.GetButtonDown("Use") && !playerController.IsInteracting()) {
                OnInteract(playerController);
            }
        // }
    }

    public void RemoveAllInteracted(){
        print("RemoveAllInteracted");

        foreach(TdPlayerController playerController in interactedCached){
            OnExitInteractRadius(playerController);
        }

        interactedCached = new HashSet<TdPlayerController>();
    }

    public void UpdateInteractivity(){
        print("UpdateInteractivity");

        List<Collider2D> colliders = new List<Collider2D>();
        interactableTrigger.GetContacts(colliders);

        HashSet<TdPlayerController> cached = new HashSet<TdPlayerController>(interactedCached);
        HashSet<TdPlayerController> current = new HashSet<TdPlayerController>();

        foreach (Collider2D c in colliders) {
            TdPlayerController playerController = c.GetComponent<TdPlayerController>();

            if (!playerController){
                continue;
            }

            current.Add(playerController);
            OnInteractRadiusStay(playerController);
        }

        cached.ExceptWith(current);

        foreach(TdPlayerController playerController in cached){
            OnExitInteractRadius(playerController);
        }

        interactedCached = current;
    }

    protected virtual void Update() {
        if (!canInteract) {
            return;
        }

        // TdPlayerController[] playerControllers = TdGameManager.GetTdPlayerControllersNearPosition(transform.position, 2f);
        UpdateInteractivity();
        
    }
}
