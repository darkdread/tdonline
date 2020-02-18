using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Interactable : MonoBehaviour {

    protected bool canInteract = true;
    protected HashSet<TdPlayerController> interactedPrevious = new HashSet<TdPlayerController>();

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

    /// <summary>
    /// Triggers when local player presses the Interact button.
    /// </summary>
    /// <param name="playerController">Local player</param>
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

        // Only show interact button if player is not doing something.
        bool isDoingSomething = playerController.IsDoingSomething();
        playerController.playerUi.ShowUseButton(!isDoingSomething);

        if (Input.GetButtonDown("Use") && !playerController.IsInteracting()) {
            OnInteract(playerController);
        }
    }

    /// <summary>
    /// Force all stored player controllers to be removed from the hashset. Also calls OnExit for each of them.
    /// </summary>
    public void RemoveAllInteracted(){
        print("RemoveAllInteracted");

        foreach(TdPlayerController playerController in interactedPrevious){
            OnExitInteractRadius(playerController);
        }

        interactedPrevious = new HashSet<TdPlayerController>();
    }

    /// <summary>
    /// Handles all Interact methods. Uses a hashset internally to store all player controllers that enter
    /// its radius.
    /// </summary>
    public void UpdateInteractivity(){
        List<Collider2D> colliders = new List<Collider2D>();
        interactableTrigger.GetContacts(colliders);

        HashSet<TdPlayerController> tempHashset = new HashSet<TdPlayerController>(interactedPrevious);
        HashSet<TdPlayerController> current = new HashSet<TdPlayerController>();

        foreach (Collider2D c in colliders) {
            TdPlayerController playerController = c.GetComponent<TdPlayerController>();

            if (!playerController){
                continue;
            }

            current.Add(playerController);
            OnInteractRadiusStay(playerController);
        }

        // Previous - current.
        tempHashset.ExceptWith(current);
        foreach(TdPlayerController playerController in tempHashset){
            OnExitInteractRadius(playerController);
        }

        // Store temp as current.
        tempHashset = new HashSet<TdPlayerController>(current);

        // Current - previous.
        tempHashset.ExceptWith(interactedPrevious);
        foreach(TdPlayerController playerController in tempHashset){
            OnEnterInteractRadius(playerController);
        }

        // Set previous to current.
        interactedPrevious = current;
    }

    protected virtual void Update() {
        if (!canInteract) {
            return;
        }

        // TdPlayerController[] playerControllers = TdGameManager.GetTdPlayerControllersNearPosition(transform.position, 2f);
        UpdateInteractivity();
    }
}
