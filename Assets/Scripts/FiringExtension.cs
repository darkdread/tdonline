using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[CreateAssetMenu(fileName = "FiringExtension", menuName = "TurretExtension/FiringExtension")]
public class FiringExtension : TurretExtension {

    public FiringExtensionData prefab;
    public int arcIterations = 20;
    public float launchSpeed = 10f;
    public Vector2 minMaxRotation = new Vector2(20f, 60f);

    override public void OnLoadExtension(Turret turret){

        if (PhotonNetwork.IsMasterClient){
            CreatePhotonData(turret, prefab.name);
        }
    }

    override public void OnInteractAfter(Turret turret, TdPlayerController playerController){
        FiringExtensionData data = turret.GetTurretExtensionData<FiringExtensionData>();

        if (turret.IsInUse() && turret.controllingPlayer.photonView.ControllerActorNr == PhotonNetwork.LocalPlayer.ActorNumber){
            data.gameObject.SetActive(true);
        } else {
            data.gameObject.SetActive(false);
        }
    }

    private void ShootProjectile(FiringExtensionData data, Turret turret, GameObject projectile, float speed){
        Vector3 direction = TdGameManager.GetDirectionOfTransform2D(turret.transform);
        Vector3 angleVec = Quaternion.AngleAxis(direction.x * data.aimRotation, Vector3.forward) * direction;

        PhotonView projectileView = projectile.GetComponent<PhotonView>();

        // Add Projectile component.
        TdGameManager.instance.AddProjectileComponent(projectileView.ViewID);

        System.Action shootProjectile = delegate{
            // Shoot projectile.
            projectile.GetComponent<PhotonView>().RPC("ShootProjectile", RpcTarget.All, turret.photonView.ViewID, angleVec, speed);
        };

        // Animation for turret to shoot.
        // If false, means turret has no animation.
        if (data.ShootProjectileAnimation(shootProjectile)){
            // int playerTurretShootTime = (int) (data.shootAnimationCompleteTime * 1000) / Application.targetFrameRate;

            // Debug.Log(playerTurretShootTime);
            // // Set player interacted.
            // turret.controllingPlayer.SetInteractingDelayFrameInstant(true, false, playerTurretShootTime);
            
        } else {
            Debug.Log("Pass away is good.");
            // Immediately shoot projectile if animation is not found.
            shootProjectile();
        }
    }

    override public void UpdateTurretExtension(Turret turret){
        FiringExtensionData data = turret.GetTurretExtensionData<FiringExtensionData>();

        if (!data){
            return;
        }

        // Handles animation callback.
        if (data.shootCallback != null){
            data.shootAnimationTime -= Time.deltaTime;
            if (data.shootAnimationTime <= 0){
                data.shootCallback.Invoke();
                data.shootCallback = null;
            }
        }

        if (data.gameObject.activeSelf){

            int yAxis = (int) Input.GetAxisRaw("Vertical");

            // Set rotation of aim.
            data.aimRotation += yAxis;
            data.aimRotation = Mathf.Clamp(data.aimRotation, (int) minMaxRotation.x, (int) minMaxRotation.y);

            // Calculate direction and distance.
            Vector3 direction = TdGameManager.GetDirectionOfTransform2D(turret.transform);
            Vector3 angleVec = Quaternion.AngleAxis(direction.x * data.aimRotation, Vector3.forward) * direction;
            
            float distance = direction.magnitude * 10f;
            float launchSpeed = ProjectileMath.LaunchSpeed(distance, 0f, Physics2D.gravity.magnitude, data.aimRotation * Mathf.Deg2Rad);
            launchSpeed = this.launchSpeed;
            distance = launchSpeed * (2 * (launchSpeed)/Physics2D.gravity.magnitude);

            data.SetProjectileIterations(arcIterations);
            data.arc.UpdateArc(turret.transform.position + angleVec, launchSpeed, distance,
                Physics2D.gravity.magnitude, data.aimRotation * Mathf.Deg2Rad, direction, true);

            ReloadableExtensionData reloadableExtensionData = turret.GetTurretExtensionData<ReloadableExtensionData>();
            if (reloadableExtensionData && data.shootCallback == null){

                // The following code below requires at least an ammunition.
                if (reloadableExtensionData.ammunition.Count <= 0){
                    return;
                }

                Collectable collectable = reloadableExtensionData.GetLastAmmunitionLoaded().GetComponent<Collectable>();

                // Update arc to show aoe radius.
                data.arc.projectileData = collectable.projectileData;

                if (Input.GetButtonDown("Shoot")){
                    GameObject projectile = reloadableExtensionData.RemoveLastAmmunitionLoaded();

                    // Hide aoe radius.
                    data.arc.projectileData = null;

                    ShootProjectile(data, turret, projectile, launchSpeed);
                }
            }
        }
    }

}
