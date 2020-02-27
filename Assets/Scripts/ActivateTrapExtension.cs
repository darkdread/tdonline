using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "ActivateTrapExtension", menuName = "TurretExtension/ActivateTrapExtension")]
public class ActivateTrapExtension : TurretExtension {

    public ActivateTrapExtensionData prefab;
    public float trapReloadTime = 20f;
    public Color outlineColor = Color.blue;

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

    public void ReloadTrap(Turret turret){
        ActivateTrapExtensionData data = turret.GetTurretExtensionData<ActivateTrapExtensionData>();

        for(int i = 0; i < data.activateTrapExtensionInit.traps.Length; i++){
            Trap trap = data.activateTrapExtensionInit.traps[i];

            trap.ShowTrap(true);
        }

        Debug.Log("Reloaded!");
    }

    override public void OnInteract(Turret turret, TdPlayerController playerController){
        ActivateTrapExtensionData data = turret.GetTurretExtensionData<ActivateTrapExtensionData>();

        // Block player from using the turret.
        playerController.SetInteractingDelayFrameInstant(true, false, 1);

        if (data.progressBarUi.IsRunning()){
            Debug.Log("Trap still reloading.");
            return;
        }

        for(int i = 0; i < data.activateTrapExtensionInit.traps.Length; i++){
            Trap trap = data.activateTrapExtensionInit.traps[i];

            trap.Trigger(playerController);
            trap.ShowTrap(false);
        }

        data.progressBarUi.StartProgressBar(trapReloadTime, delegate {
            ReloadTrap(turret);
        });
    }

    override public void OnEnterInteractRadius(Turret turret, TdPlayerController playerController){
        ActivateTrapExtensionData data = turret.GetTurretExtensionData<ActivateTrapExtensionData>();

        if (!data){
            return;
        }

        if (!playerController.photonView.IsMine){
            return;
        }
        
        // Show which traps the turret controls.
        if (!data.progressBarUi.IsRunning()){
            foreach(Trap trap in data.activateTrapExtensionInit.traps){
                trap.ShowOutline(outlineColor);
            }
        }
    }

    override public void OnExitInteractRadius(Turret turret, TdPlayerController playerController){
        ActivateTrapExtensionData data = turret.GetTurretExtensionData<ActivateTrapExtensionData>();

        if (!data){
            return;
        }

        if (!playerController.photonView.IsMine){
            return;
        }

        // Reset trap color.
        foreach(Trap trap in data.activateTrapExtensionInit.traps){
            trap.ShowOutline(Color.white);
        }
    }

    override public void UpdateTurretExtension(Turret turret){
        ActivateTrapExtensionData data = turret.GetTurretExtensionData<ActivateTrapExtensionData>();

        if (!data){
            return;
        }

        // Data consist of progress bar position, therefore we have to always center it.
        data.transform.position = turret.transform.position;
    }

}
