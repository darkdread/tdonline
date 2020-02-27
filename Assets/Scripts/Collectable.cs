using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Photon.Pun;

public class Collectable : Interactable {

    public PhotonView photonView;

    [Header("Infinite Stack")]
    public bool infinite = true;
    public bool isBeingLooted = false;
    public GameObject collectablePrefab;

    [Header("One Time Loot")]
    public EndData endData;
    public ProjectileData projectileData;

    private void Awake() {
        PhotonView photonView = GetComponent<PhotonView>();
        print(photonView);
        print(this);

        if (photonView == null) {
            print($"Collectable {this.gameObject} does not have photon view!");
        }
    }

    [PunRPC]
    private void SetLootedRpc(bool lootedState){
        isBeingLooted = lootedState;
    }

    public void SetLooted(bool lootedState){
        photonView.RPC("SetLootedRpc", RpcTarget.All, lootedState);
    }

    private void CollectingFromStack(TdPlayerController playerController) {
        if (playerController.IsDoingSomething()){
            print("Already doing something!");
            return;
        }

        if (isBeingLooted){
            print("Already being looted!");
            return;
        }

        SetLooted(true);

        System.Action completeCallback = delegate{
            StartCoroutine(SpawnAndCarry(playerController));
            SetLooted(false);
        };

        System.Action failedCallback = delegate{
            SetLooted(false);
        };

        playerController.progressBarUi.StartProgressBar(TdGameManager.gameSettings.progressCollectTime, completeCallback, failedCallback);
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
        } else {
            // Carry collectable object.
            StartCoroutine(Carry(playerController));
        }
        
        base.OnInteract(playerController);
    }
}
