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

    private IEnumerator SpawnAndTake(TdPlayerController playerController) {
        yield return null;

        GameObject spawnedObject = PhotonNetwork.InstantiateSceneObject(collectablePrefab.name,
                playerController.playerCarryTransform.position, Quaternion.identity);
        PhotonView objectPhotonView = spawnedObject.GetComponent<PhotonView>();

        playerController.photonView.RPC("OnCarryGameObject", RpcTarget.All, objectPhotonView.ViewID);
    }

    protected override void OnInteract(TdPlayerController playerController) {
        // If it's a stack, don't allow a player who's carrying object to take from stack.
        if (infinite) {
             if (!playerController.IsCarryingObject()){
                 StartCoroutine(SpawnAndTake(playerController));
             }
        } else {
            // Carry collectable object.
            playerController.photonView.RPC("OnCarryGameObject", RpcTarget.All, photonView.ViewID);
        }
        
        base.OnInteract(playerController);
    }
}
