using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "FiringExtension", menuName = "TurretExtension/FiringExtension")]
public class FiringExtension : TurretExtension {

    public FiringExtensionData prefab;
    public Vector2 minMaxRotation = new Vector2(20f, 60f);

    override public void OnLoadExtension(Turret turret){

        if (PhotonNetwork.IsMasterClient){
            CreatePhotonData(turret, prefab.name);
        }
    }

    override public void OnInteractAfter(Turret turret, TdPlayerController playerController){
        FiringExtensionData data = (FiringExtensionData) turret.GetTurretExtensionData(this);

        if (turret.IsInUse() && turret.controllingPlayer.photonView.ControllerActorNr == PhotonNetwork.LocalPlayer.ActorNumber){
            data.gameObject.SetActive(true);
        } else {
            data.gameObject.SetActive(false);
        }
    }

    private void ShootProjectile(FiringExtensionData data, GameObject turret, GameObject projectile, float speed){
        GameObject go = projectile;
        Vector3 direction = TdGameManager.GetDirectionOfTransform2D(turret.transform);
        Vector3 angleVec = Quaternion.AngleAxis(direction.x * data.aimRotation, Vector3.forward) * direction;

        go.transform.position = turret.transform.position + angleVec;
        go.GetComponent<Rigidbody2D>().velocity = angleVec * speed;
    }

    override public void UpdateTurretExtension(Turret turret){
        FiringExtensionData data = (FiringExtensionData) turret.GetTurretExtensionData(this);

        if (data.gameObject.activeSelf){
            int yAxis = (int) Input.GetAxisRaw("Vertical");

            // Set rotation of aim.
            data.aimRotation += yAxis;
            data.aimRotation = Mathf.Clamp(data.aimRotation, (int) minMaxRotation.x, (int) minMaxRotation.y);

            // Calculate direction and distance.
            Vector3 direction = TdGameManager.GetDirectionOfTransform2D(turret.transform);
            Vector3 angleVec = Quaternion.AngleAxis(direction.x * data.aimRotation, Vector3.forward) * direction;
            float distance = direction.magnitude * 10f;

            float launchSpeed = ProjectileMath.LaunchSpeed(distance, 0f, Physics.gravity.magnitude, data.aimRotation * Mathf.Deg2Rad);
            data.arc.UpdateArc(turret.transform.position + angleVec, launchSpeed, distance, Physics.gravity.magnitude, data.aimRotation * Mathf.Deg2Rad, direction, true);

            if (Input.GetButtonDown("Shoot")){
                ReloadableExtensionData reloadableExtensionData = (ReloadableExtensionData) turret.GetTurretExtensionData(typeof(ReloadableExtensionData));

                if (!reloadableExtensionData){
                    Debug.Log("AimingExtension requires ReloadableExtension!");
                    return;
                }

                if (reloadableExtensionData && reloadableExtensionData.ammunition.Count <= 0){
                    Debug.Log("No ammunition!");
                    return;
                }

                GameObject projectile = reloadableExtensionData.RemoveLastAmmunitionLoaded();

                ShootProjectile(data, turret.gameObject, projectile, launchSpeed);
            }
        }
    }
}
