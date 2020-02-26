using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "ActivateTrapExtension", menuName = "TurretExtension/ActivateTrapExtension")]
public class ActivateTrapExtension : TurretExtension {

    public ActivateTrapExtensionData prefab;

    override public void OnLoadExtension(Turret turret){

        if (PhotonNetwork.IsMasterClient){
            // Check for dependencies.
            ActivateTrapExtensionInit activateTrapExtensionInit = turret.GetComponent<ActivateTrapExtensionInit>();

            if (!activateTrapExtensionInit){
                Debug.Log("ActivateTrapExtension requires ActivateTrapExtensionInit on turret.");
                return;
            }

            CreatePhotonData(turret, prefab.name);
        }
    }

    override public void OnInteract(Turret turret, TdPlayerController playerController){
        ActivateTrapExtensionData data = turret.GetTurretExtensionData<ActivateTrapExtensionData>();
    }

    override public void UpdateTurretExtension(Turret turret){
        ActivateTrapExtensionData data = turret.GetTurretExtensionData<ActivateTrapExtensionData>();

        if (!data){
            return;
        }

        Debug.Log(data.activateTrapExtensionInit);
    }

}
