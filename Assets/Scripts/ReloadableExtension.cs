using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "ReloadableExtension", menuName = "TurretExtension/ReloadableExtension")]
public class ReloadableExtension : TurretExtension {

    public List<GameObject> compatibleObjects = new List<GameObject>();
    public ReloadableExtensionData uiPrefab;
    public ItemSlot itemSlotPrefab;
    public int maxAmmunition;

    override public void OnLoadExtension(Turret turret){
        // Create UI and attach to game canvas.
        // ReloadableExtensionData data = Instantiate<ReloadableExtensionData>(uiPrefab, TdGameManager.instance.gameCanvas.transform);

        if (PhotonNetwork.IsMasterClient){
            CreatePhotonData(turret, uiPrefab.name);
        }
    }
    
    public void LoadObject(Turret turret, ReloadableExtensionData data, PhotonView view){
        data.photonView.RPC("AddAmmunition", RpcTarget.All, view.ViewID);
    }

    private void UpdateUi(Turret turret){
        ReloadableExtensionData data = turret.GetTurretExtensionData<ReloadableExtensionData>();

        if (!data){
            return;
        }

        data.transform.position = Camera.main.WorldToScreenPoint(turret.transform.position);

        // Logic for item to show up in ui here.
        for(int i = 0; i < data.itemSlots.Count; i++){
            ItemSlot itemSlot = data.itemSlots[i];

            if (i >= data.ammunition.Count){
                itemSlot.SetEmpty();
                continue;
            }

            itemSlot.SetSprite(data.ammunition[i].GetComponent<SpriteRenderer>().sprite);
        }
    }

    override public void OnInteract(Turret turret, TdPlayerController playerController){
        if (!playerController.IsCarryingObject()){
            return;
        }

        // Check if item is the supposed type.
        GameObject compatibleObject = null;
        foreach(GameObject go in compatibleObjects){
            if (playerController.playerCarriedObject.GetComponent<SpriteRenderer>().sprite == go.GetComponent<SpriteRenderer>().sprite){
                compatibleObject = playerController.playerCarriedObject;
                break;
            }
        }

        if (compatibleObject == null){
            Debug.Log("Not a compatible item!");
            playerController.SetInteractingDelayFrameInstant(true, false, 1);
            turret.BlockTurretExtensionUntilSeconds(typeof(FiringExtension));
            return;
        }

        ReloadableExtensionData data = turret.GetTurretExtensionData<ReloadableExtensionData>();
        if (data.ammunition.Count >= data.maxAmmunition){
            Debug.Log("Max ammunition reached!");
            playerController.SetInteractingDelayFrameInstant(true, false, 1);
            turret.BlockTurretExtensionUntilSeconds(typeof(FiringExtension));
            return;
        }

        if (playerController.progressBarUi.IsRunning()){
            Debug.Log("Player is doing something!");
            playerController.SetInteractingDelayFrameInstant(true, false, 1);
            turret.BlockTurretExtensionUntilSeconds(typeof(FiringExtension));
            return;
        }

        playerController.SetInteractingDelayFrameInstant(true, false, 1);

        System.Action completeCallback = delegate{
            playerController.DropObject();
            LoadObject(turret, data, compatibleObject.GetComponent<PhotonView>());
        };

        playerController.progressBarUi.StartProgressBar(TdGameManager.gameSettings.progressReloadTime, completeCallback, null);
    }

    override public void UpdateTurretExtension(Turret turret){
        UpdateUi(turret);
    }
}
