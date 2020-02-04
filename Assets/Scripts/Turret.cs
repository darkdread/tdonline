using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public enum TurretState {
    InUse = 1
}

public class Turret : Interactable {
    [HideInInspector]
    public PhotonView photonView;

    protected Collider2D turretCollider;
    public TdPlayerController controllingPlayer;

    public TurretState turretState;

    [PunRPC]
    protected void SetTurretState(TurretState state) {
        turretState = state;
    }

    [PunRPC]
    protected void SetTurretUser(int viewId) {
        if (viewId == -1) {
            controllingPlayer = null;
            return;
        }

        controllingPlayer = PhotonNetwork.GetPhotonView(viewId).GetComponent<TdPlayerController>();
    }

    protected void OnActivateTurret(TdPlayerController playerController) {
        if ((turretState & TurretState.InUse) == TurretState.InUse) {
            print($"Already in use by Player {controllingPlayer.photonView.Owner.UserId}");
            return;
        }

        controllingPlayer = playerController;
        controllingPlayer.photonView.RPC("OnUsingTurret", RpcTarget.All);

        TurretState newTurretState = turretState | TurretState.InUse;
        photonView.RPC("SetTurretState", RpcTarget.All, newTurretState);
        photonView.RPC("SetTurretUser", RpcTarget.All, controllingPlayer.photonView.ViewID);
    }

    protected void OnDeactivateTurret() {
        if ((turretState & TurretState.InUse) == TurretState.InUse) {
            controllingPlayer.photonView.RPC("OnStopUsingTurret", RpcTarget.All);

            TurretState newTurretState = turretState & ~TurretState.InUse;
            photonView.RPC("SetTurretState", RpcTarget.All, newTurretState);
            photonView.RPC("SetTurretUser", RpcTarget.All, -1);
        }
    }

    override protected void OnEnterInteractRadius(TdPlayerController playerController) {
        // Show interact button if not in use.
        if ((turretState & TurretState.InUse) != TurretState.InUse) {
            base.OnEnterInteractRadius(playerController);
        }
    }

    override protected void OnExitInteractRadius(TdPlayerController playerController) {
        base.OnExitInteractRadius(playerController);
    }

    private void Awake() {
        photonView = GetComponent<PhotonView>();
        turretCollider = GetComponent<Collider2D>();
    }

    protected override void OnInteract(TdPlayerController playerController) {
        if (controllingPlayer != null && controllingPlayer != playerController) {
            return;
        }

        if (!IsInRadius(playerController.transform.position)) {
            return;
        }

        if ((turretState & TurretState.InUse) == TurretState.InUse) {
            OnDeactivateTurret();
        } else {
            OnActivateTurret(playerController);
        }

        base.OnInteract(playerController);
    }

    protected override void OnInteractRadiusStay(TdPlayerController playerController) {
        base.OnInteractRadiusStay(playerController);

        // Update interactivity button when state is in use.
        if ((turretState & TurretState.InUse) == TurretState.InUse) {
            OnExitInteractRadius(playerController);
        }
    }
}
