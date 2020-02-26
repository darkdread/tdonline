using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class FiringExtensionData : TurretExtensionData {

    [Header("Runtime Variables")]
    public ProjectileArc arc;
    public int aimRotation = 0;
    public float shootAnimationCompleteTime = 0f;
    public float shootAnimationTime = 0f;
    public Animator animator;
    public TdPlayerController playerController;
    public System.Action shootCallback;

    private void Awake(){
        arc = GetComponent<ProjectileArc>();
        gameObject.SetActive(false);
    }

    override public void OnLoadAfter(){
        turretExtension = turret.GetTurretExtension<FiringExtension>();

        animator = turret.GetComponentInChildren<Animator>();
        if (animator){
            shootAnimationCompleteTime = MyUtilityScript.GetAnimationDuration(animator, "Shoot");
        }
    }

    [PunRPC]
    private void ShootProjectileAnimationRpc(int playerView){
        playerController = PhotonNetwork.GetPhotonView(playerView).GetComponent<TdPlayerController>();
        animator.SetTrigger("Shoot");
    }

    public bool ShootProjectileAnimation(System.Action callback){
        if (animator){
            shootAnimationTime = shootAnimationCompleteTime;
            shootCallback = callback;
            photonView.RPC("ShootProjectileAnimationRpc", RpcTarget.All,
                turret.controllingPlayer.photonView.ViewID);
            return true;
        }

        return false;
    }

    public void SetProjectileIterations(int iterations){
        arc.iterations = iterations;
    }
}
