using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public enum TurretState {
    InUse = 1
}

public class Turret : MonoBehaviour
{
    [HideInInspector]
    public PhotonView photonView;

    protected Collider2D turretCollider;
    [SerializeField]
    protected Collider2D turretActivationTrigger;
    public TdPlayerController controllingPlayer;

    public TurretState turretState;

    protected void OnActivateTurret(TdPlayerController playerController){
        if ((turretState & TurretState.InUse) == TurretState.InUse){
            print($"Already in use by Player {controllingPlayer.photonView.Owner.UserId}");
            return;
        }

        controllingPlayer = playerController;
        controllingPlayer.photonView.RPC("OnUsingTurret", RpcTarget.All);
        turretState = turretState | TurretState.InUse;
    }

    protected void OnDeactivateTurret(){
        if ((turretState & TurretState.InUse) == TurretState.InUse){
            controllingPlayer.photonView.RPC("OnStopUsingTurret", RpcTarget.All);
            turretState = turretState & ~TurretState.InUse;
        }
    }

    private void Awake(){
        // photonView = GetComponent<PhotonView>();
        turretCollider = GetComponent<Collider2D>();
    }

    private void Update(){
        TdPlayerController[] playerControllers = TdGameManager.GetTdPlayerControllersNearPosition(transform.position, 2f);
        Bounds triggerBounds = turretActivationTrigger.bounds;

        if ((turretState & TurretState.InUse) != TurretState.InUse){
            // Show it's usable.

            foreach(TdPlayerController playerController in playerControllers){
                if (!playerController.photonView.IsMine){
                    continue;
                }

                print(playerController);

                if (MyUtilityScript.IsInBounds(playerController.playerCollider.bounds, triggerBounds)){
                    playerController.playerUi.ShowUseButton(true);
                } else {
                    playerController.playerUi.ShowUseButton(false);
                }
            }
        }

        if (Input.GetButtonDown("Use")){
            foreach(TdPlayerController playerController in playerControllers){
                if (!playerController.photonView.IsMine){
                    continue;
                }

                if (!MyUtilityScript.IsInBounds(playerController.playerCollider.bounds, triggerBounds)){
                    break;
                }

                if ((turretState & TurretState.InUse) == TurretState.InUse){
                    OnDeactivateTurret();
                } else {
                    OnActivateTurret(playerController);
                }
            }
        }
    }
}
