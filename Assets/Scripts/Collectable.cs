using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Photon.Pun;

public class Collectable : Interactable {

    [Header("Infinite Stack")]
    public bool infinite = true;
    public GameObject collectablePrefab;

    [Header("One Time Loot")]
    public PhotonView photonView;
    public ProjectileData projectileData;

    private void Awake() {
        PhotonView photonView = GetComponent<PhotonView>();

        if (!infinite && photonView == null) {
            print($"Collectable {this.gameObject} does not have photon view!");
        }
    }

    private void CollectingFromStack(TdPlayerController playerController) {
        if (playerController.IsDoingSomething()){
            print("Already doing something!");
            return;
        }

        playerController.StartProgressBar(2f, delegate{
            StartCoroutine(SpawnAndCarry(playerController));
        });
    }

    private IEnumerator SpawnAndCarry(TdPlayerController playerController) {
        CollectablePun data = new CollectablePun(){
            resourceName = Path.Combine(TdGameManager.gameSettings.collectableResourceDirectory, collectablePrefab.name),
            playerViewId = playerController.photonView.ViewID
        };

        TdGameManager.instance.photonView.RPC("SpawnCollectableObject", RpcTarget.All, CollectablePun.Serialize(data));
        yield return null;
    }

    
    private IEnumerator Carry(TdPlayerController playerController) {
        playerController.photonView.RPC("OnCarryGameObject", RpcTarget.All, photonView.ViewID);
        yield return null;
    }

    protected override void OnInteractRadiusStay(TdPlayerController playerController){
        // Only allow a player who can carry object to interact.
        if (!playerController.CanCarryObject()){
            return;
        }
        
        base.OnInteractRadiusStay(playerController);
    }

    protected override void OnInteract(TdPlayerController playerController) {

        // If it's a stack, spawn the object and force player to carry it.
        if (infinite) {
            CollectingFromStack(playerController);
            // StartCoroutine(SpawnAndCarry(playerController));
        } else {
            // Carry collectable object.
            StartCoroutine(Carry(playerController));
        }
        
        base.OnInteract(playerController);
    }
}
