using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "ReloadableExtension", menuName = "TurretExtension/ReloadableExtension")]
public class ReloadableExtension : TurretExtension {

    public List<GameObject> compatibleObjects = new List<GameObject>();
    public ReloadableExtensionData uiPrefab;
    public ItemSlot itemSlotPrefab;

    override public void OnLoadExtension(Turret turret){
        Debug.Log(uiPrefab);
        Debug.Log(TdGameManager.instance.gameCanvas);

        // Create UI and attach to game canvas.
        // ReloadableExtensionData data = Instantiate<ReloadableExtensionData>(uiPrefab, TdGameManager.instance.gameCanvas.transform);

        if (PhotonNetwork.IsMasterClient){
            CreatePhotonData(turret, uiPrefab.name);
            Debug.Log("CreatePhotonDataMaster");
        }
    }
    
    public void LoadObject(Turret turret, ReloadableExtensionData data, PhotonView view){
        data.photonView.RPC("AddAmmunition", RpcTarget.All, view.ViewID);
    }

    private void UpdateUi(Turret turret){
        ReloadableExtensionData data = turret.GetTurretExtensionData(this) as ReloadableExtensionData;
        data.transform.position = Camera.main.WorldToScreenPoint(turret.transform.position);

        // Logic for item to show up in ui here.
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
            }
        }

        if (compatibleObject == null){
            Debug.Log("Not a compatible item!");
            return;
        }

        playerController.DropObject();

        ReloadableExtensionData data = turret.GetTurretExtensionData(this) as ReloadableExtensionData;
        LoadObject(turret, data, compatibleObject.GetComponent<PhotonView>());
    }

    override public void UpdateTurretExtension(Turret turret){
        UpdateUi(turret);
    }
}
