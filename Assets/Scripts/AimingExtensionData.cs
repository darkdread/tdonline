using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class AimingExtensionData : TurretExtensionData {

    public ProjectileArc arc;
    public int aimRotation = 0;

    private void Awake(){
        arc = GetComponent<ProjectileArc>();
    }

    override public void OnPhotonInstantiate(PhotonMessageInfo info){
        AimingExtension aimingExtension = (AimingExtension) turretExtension;
        
        base.OnPhotonInstantiate(info);
    }
}
