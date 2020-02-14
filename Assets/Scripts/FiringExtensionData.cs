using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class FiringExtensionData : TurretExtensionData {

    public ProjectileArc arc;
    public int aimRotation = 0;

    private void Awake(){
        arc = GetComponent<ProjectileArc>();
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void SetAsProjectile(int viewId){
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);
        
        view.gameObject.AddComponent(typeof(Projectile));
    }

    public void SetProjectileIterations(int iterations){
        arc.iterations = iterations;
    }

    override public void OnPhotonInstantiate(PhotonMessageInfo info){
        FiringExtension aimingExtension = (FiringExtension) turretExtension;
        
        base.OnPhotonInstantiate(info);
    }
}
