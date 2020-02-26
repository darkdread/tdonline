using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public enum TurretState {
    InUse = 1,
}

public class Turret : Interactable {
    [HideInInspector]
    public PhotonView photonView;
    protected Collider2D turretCollider;

    public List<TurretExtension> turretExtensions = new List<TurretExtension>();

    // For TurretExtensions' usage.
    [HideInInspector]
    private List<TurretExtensionData> turretExtensionDatas = new List<TurretExtensionData>();
    [HideInInspector]
    private HashSet<System.Type> blockTurretExtensions = new HashSet<System.Type>();


    [Header("Runtime Variables")]
    public TdPlayerController controllingPlayer;
    public TurretState turretState;

    protected virtual void Awake(){
        photonView = GetComponent<PhotonView>();
        turretCollider = GetComponent<Collider2D>();

        // Creates data for each attached extensions.
        // OnLoadExtension adds an instance of TurretExtensionData to the list turretExtensionDatas.
        foreach(TurretExtension turretExtension in turretExtensions){
            turretExtension.OnLoadExtension(this);
        }
    }

    public void AddTurretExtensionData(TurretExtensionData turretExtensionData){
        turretExtensionDatas.Add(turretExtensionData);
    }

    public TurretExtensionData GetTurretExtensionData(System.Type turretExtensionType){
        foreach(TurretExtensionData data in turretExtensionDatas){
            if (data.GetType() == turretExtensionType){
                return data;
            }
        }

        return null;
    }

    public TurretExtensionData GetTurretExtensionData(TurretExtension turretExtension){
        // Turret extension datas have not been initialized yet.
        // We have to wait for OnLoadExtension -> CreatePhotonData.
        if (turretExtensionDatas.Count == 0){
            return null;
        }

        return turretExtensionDatas[turretExtensions.IndexOf(turretExtension)];
    }

    private IEnumerator BlockTurretExtension(System.Type turretExtensionType, float seconds){
        blockTurretExtensions.Add(turretExtensionType);
        yield return new WaitForSeconds(seconds);
        UnblockTurretExtension(turretExtensionType);
    }

    public bool IsExtensionBlocked(System.Type turretExtensionType){
        return blockTurretExtensions.Contains(turretExtensionType);
    }

    public void UnblockTurretExtension(System.Type turretExtensionType){
        blockTurretExtensions.Remove(turretExtensionType);
    }

    public void BlockTurretExtensionUntilSeconds(System.Type turretExtensionType, float seconds = 1f){
        StartCoroutine(BlockTurretExtension(turretExtensionType, seconds));
    }

    public bool IsInUse(){
        return (turretState & TurretState.InUse) == TurretState.InUse;
    }

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
        if (IsInUse()) {
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
        if (IsInUse()) {
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

    protected override void OnInteract(TdPlayerController playerController){

        foreach(TurretExtension turretExtension in turretExtensions){
            if (IsExtensionBlocked(turretExtension.GetType())){
                continue;
            }

            turretExtension.OnInteract(this, playerController);
        }

        if (controllingPlayer != null && controllingPlayer != playerController) {
            return;
        }

        if (playerController.IsInteracting()){
            print("Player is interacting, probably one of the extension set player interaction to true.");
            return;
        }

        if (IsInUse()) {
            OnDeactivateTurret();
        } else {
            OnActivateTurret(playerController);
        }

        foreach(TurretExtension turretExtension in turretExtensions){
            if (IsExtensionBlocked(turretExtension.GetType())){
                continue;
            }

            turretExtension.OnInteractAfter(this, playerController);
        }

        base.OnInteract(playerController);
    }

    protected override void OnInteractRadiusStay(TdPlayerController playerController) {
        base.OnInteractRadiusStay(playerController);

        // Update interactivity button when state is in use.
        if (IsInUse() && !playerController.IsCarryingObject()) {
            playerController.playerUi.ShowUseButton(false);
        }
    }

    protected override void Update(){
        base.Update();

        foreach(TurretExtension turretExtension in turretExtensions){
            if (IsExtensionBlocked(turretExtension.GetType())){
                continue;
            }
            
            turretExtension.UpdateTurretExtension(this);
        }
    }
}
