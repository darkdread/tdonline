using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Collectable : Interactable {

    [Header("Infinite Stack")]
    public bool infinite = true;
    public GameObject collectablePrefab;

    [Header("One Time Loot")]
    public PhotonView photonView;

    private void Awake() {
        PhotonView photonView = GetComponent<PhotonView>();

        if (!infinite && photonView == null) {
            print($"Collectable {this.gameObject} does not have photon view!");
        }
    }

    private IEnumerator SpawnAndCarry(TdPlayerController playerController) {
        CollectablePun data = new CollectablePun(){
            resourceName = collectablePrefab.name,
            playerViewId = playerController.photonView.ViewID
        };

        TdGameManager.instance.photonView.RPC("SpawnObject", RpcTarget.MasterClient, CollectablePun.Serialize(data));
        yield return null;
    }

    
    private IEnumerator Carry(TdPlayerController playerController) {
        playerController.photonView.RPC("OnCarryGameObject", RpcTarget.All, photonView.ViewID);
        yield return null;
    }

    protected override void OnInteractRadiusStay(TdPlayerController playerController){
        // Don't allow a player who's carrying an object to interact.
        if (playerController.IsCarryingObject()){
            return;
        }
        
        base.OnInteractRadiusStay(playerController);
    }

    protected override void OnInteract(TdPlayerController playerController) {

        // If it's a stack, spawn the object and force player to carry it.
        if (infinite) {
            StartCoroutine(SpawnAndCarry(playerController));
        } else {
            // Carry collectable object.
            StartCoroutine(Carry(playerController));
        }
        
        base.OnInteract(playerController);
    }
}
