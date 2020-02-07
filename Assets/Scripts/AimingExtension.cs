using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "AimingExtension", menuName = "TurretExtension/AimingExtension")]
public class AimingExtension : TurretExtension {

    public AimingExtensionData prefab;
    public Vector2 minMaxRotation = new Vector2(20f, 60f);

    override public void OnLoadExtension(Turret turret){

        if (PhotonNetwork.IsMasterClient){
            CreatePhotonData(turret, prefab.name);
        }
    }

    override public void OnInteract(Turret turret, TdPlayerController playerController){
        
    }

    override public void UpdateTurretExtension(Turret turret){
        if (turret.IsInUse() && turret.controllingPlayer.photonView.ControllerActorNr == PhotonNetwork.LocalPlayer.ActorNumber){
            int yAxis = (int) Input.GetAxisRaw("Vertical");
            
            AimingExtensionData data = (AimingExtensionData) turret.GetTurretExtensionData(this);

            data.gameObject.SetActive(true);

            // Set rotation of aim.
            data.aimRotation += yAxis;
            data.aimRotation = Mathf.Clamp(data.aimRotation, (int) minMaxRotation.x, (int) minMaxRotation.y);

            // Calculate direction and distance.
            Vector3 direction = turret.transform.localScale.x >= 0 ? Vector3.right : Vector3.left;
            float distance = direction.magnitude * 10f;

            float currentSpeed = ProjectileMath.LaunchSpeed(distance, 0f, Physics.gravity.magnitude, data.aimRotation * Mathf.Deg2Rad);
            data.arc.UpdateArc(turret.transform.position, currentSpeed, distance, Physics.gravity.magnitude, data.aimRotation * Mathf.Deg2Rad, direction, true);
        } else {
            AimingExtensionData data = (AimingExtensionData) turret.GetTurretExtensionData(this);

            data.gameObject.SetActive(false);
        }
    }
}
