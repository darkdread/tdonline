using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ActivateTrapExtensionData : TurretExtensionData {

    [Header("Runtime Variables")]
    public TdProgressBarUi progressBarUi;
    public Animator animator;
    public TdPlayerController playerController;
    public ActivateTrapExtensionInit activateTrapExtensionInit;

    [PunRPC]
    public void SetTargetRpc(int referenceId){
        progressBarUi._referencedPhotonView = PhotonNetwork.GetPhotonView(referenceId);
    }

    [PunRPC]
    private void StartProgressBarRpc(float duration){
        progressBarUi.progressCurrent = 0f;
        progressBarUi.progressMax = duration;
        progressBarUi.SetProgressBar(0f);
        progressBarUi.ShowProgressBar(true);
    }

    [PunRPC]
    private void StopProgressBarRpc(){
        progressBarUi.progressMax = -1f;
        progressBarUi.ShowProgressBar(false);
    }

    override public void OnLoadAfter(){
        turretExtension = turret.GetTurretExtension<ActivateTrapExtension>();
        activateTrapExtensionInit = turret.GetComponent<ActivateTrapExtensionInit>();

        progressBarUi = TdProgressBarUi.Spawn(photonView);
        progressBarUi.SetTarget(photonView);

        animator = turret.GetComponentInChildren<Animator>();
        if (animator){
            // shootAnimationCompleteTime = MyUtilityScript.GetAnimationDuration(animator, "Shoot");
        }
    }
}
