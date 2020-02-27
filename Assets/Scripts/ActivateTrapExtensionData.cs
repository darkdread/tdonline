using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ActivateTrapExtensionData : TurretExtensionData, ITdProgressBarUi {

    public TdProgressBarUi progressBarUi {get; set;}

    [Header("Runtime Variables")]
    public Animator animator;
    public TdPlayerController playerController;
    public ActivateTrapExtensionInit activateTrapExtensionInit;

    // [PunRPC]
    // public void SetTargetRpc(int referenceId){
    //     this.SetTarget(referenceId);
    // }

    [PunRPC]
    public void StartProgressBarRpc(float duration){
        this.StartProgressBar(duration);
    }

    [PunRPC]
    public void StopProgressBarRpc(){
        this.StopProgressBar();
    }

    override public void OnLoadAfter(){
        turretExtension = turret.GetTurretExtension<ActivateTrapExtension>();
        activateTrapExtensionInit = turret.GetComponent<ActivateTrapExtensionInit>();

        progressBarUi = TdProgressBarUi.Spawn(photonView);
        // progressBarUi.SetTarget(photonView);

        animator = turret.GetComponentInChildren<Animator>();
        if (animator){
            // shootAnimationCompleteTime = MyUtilityScript.GetAnimationDuration(animator, "Shoot");
        }
    }
}
