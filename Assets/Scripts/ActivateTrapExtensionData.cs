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
    public float animationCompleteTime = 0f;
    public float animationTime = 0f;
    public System.Action animationCompleteCallback;

    [PunRPC]
    public void StartProgressBarRpc(float duration){
        this.StartProgressBar(duration);

        animator.SetBool("reloaded", false);
    }

    [PunRPC]
    public void StopProgressBarRpc(){
        this.StopProgressBar();

        animator.SetBool("reloaded", true);
    }

    public bool TriggerAnimation(System.Action callback){
        if (animator){
            animationTime = animationCompleteTime;
            animationCompleteCallback = callback;
            return true;
        }

        return false;
    }

    override public void OnLoadAfter(){
        turretExtension = turret.GetTurretExtension<ActivateTrapExtension>();
        activateTrapExtensionInit = turret.GetComponent<ActivateTrapExtensionInit>();

        progressBarUi = TdProgressBarUi.Spawn(photonView);

        animator = turret.GetComponentInChildren<Animator>();
        if (animator){
            animationCompleteTime = MyUtilityScript.GetAnimationDuration(animator, "Trigger");
        }
    }
}
