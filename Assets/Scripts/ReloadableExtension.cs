using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "ReloadableExtension", menuName = "TurretExtension/ReloadableExtension")]
public class ReloadableExtension : TurretExtension {

    public GameObject requiredObject;
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
    
    public void LoadObject(Turret turret, ReloadableExtensionData data){
        Debug.Log(data.photonView);
        data.photonView.RPC("SetAmmunition", RpcTarget.All, data.ammunition + 1);
    }

    private void UpdateUi(Turret turret){
        ReloadableExtensionData data = turret.GetTurretExtensionData(this) as ReloadableExtensionData;
        data.transform.position = Camera.main.WorldToScreenPoint(turret.transform.position);
    }

    override public void OnInteract(Turret turret, TdPlayerController playerController){
        if (!playerController.IsCarryingObject()){
            return;
        }

        // Check if item is the supposed type.
        if (playerController.playerCarriedObject.GetComponent<SpriteRenderer>().sprite != requiredObject.GetComponent<SpriteRenderer>().sprite){
            Debug.Log("Not same item!");
        }

        playerController.DropObject(true);

        ReloadableExtensionData data = turret.GetTurretExtensionData(this) as ReloadableExtensionData;
        LoadObject(turret, data);

        Debug.Log(data.ammunition);
    }

    override public void UpdateTurretExtension(Turret turret){
        UpdateUi(turret);
    }
}
