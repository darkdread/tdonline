using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ActivateTrapExtensionData : TurretExtensionData {

    [Header("Runtime Variables")]
    public Animator animator;
    public TdPlayerController playerController;
    public ActivateTrapExtensionInit activateTrapExtensionInit;

    override public void OnLoadAfter(){
        turretExtension = turret.GetTurretExtension<ActivateTrapExtension>();
        activateTrapExtensionInit = turret.GetComponent<ActivateTrapExtensionInit>();

        animator = turret.GetComponentInChildren<Animator>();
        if (animator){
            // shootAnimationCompleteTime = MyUtilityScript.GetAnimationDuration(animator, "Shoot");
        }
    }
}
