using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public enum PlayerState {
    Climbing = 1,
    UsingTurret = 2,
    CarryingObject = 4,
    Interacting = 8,
    IsDoingSomething = 16,

    CarryingOrUsingTurret = UsingTurret | CarryingObject
}

[System.Serializable]
public struct PlayerEmoteSprite {
    public Sprite sprite;

    [Header("This string needs to be in the Input Manager.")]
    public string buttonString;
    public string emoteButtonDisplayString;
    public string soundEffectString;
}

public class TdPlayerController : MonoBehaviour, ITdProgressBarUi
{
    [HideInInspector]
    public PhotonView photonView;

    [HideInInspector]
    public Collider2D playerCollider;

    [HideInInspector]
    public Rigidbody2D playerRigidbody;

    [Header("Initialization")]
    public AudioClipObject audioClipObject;
    public AudioSource audioSource;
    public Transform playerCarryTransform;
    public Collider2D playerFeetCollider;

    [Header("Ladder")]
    [SerializeField]
    public ContactFilter2D ladderFilter2D;
    public GameObject playerClimbingLadder;

    [Header("Carry")]
    public GameObject playerCarriedObject;

    [HideInInspector]
    public TdPlayerUi playerUi;

    [HideInInspector]
    public TdProgressBarUi progressBarUi {get; set;}

    [Header("Runtime Variables")]
    public PlayerState playerState;

    public EndGameData playerEndGameData;
    private Animator animator;

    public float moveSpeed = 1f;

    private void Awake(){
        photonView = GetComponent<PhotonView>();
        playerCollider = GetComponent<Collider2D>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        playerUi = Instantiate<TdPlayerUi>(TdGameManager.gameSettings.playerUiPrefab,
            TdGameManager.instance.gameUiCanvas);
        playerUi.SetTarget(this);

        progressBarUi = TdProgressBarUi.Spawn(photonView);

        playerEndGameData = "Defaults";
    }

    [PunRPC]
    public void StartProgressBarRpc(float duration){
        this.StartProgressBar(duration);
    }

    [PunRPC]
    public void StopProgressBarRpc(){
        this.StopProgressBar();
    }

    [PunRPC]
    private void ShowEmoteRpc(string buttonString, float duration){
        PlayerEmoteSprite playerEmoteSprite = TdGameManager.gameSettings.GetPlayerEmoteSpriteFromString(buttonString);

        AudioClip emoteClip = audioClipObject.GetAudioClipFromString(playerEmoteSprite.soundEffectString);
        if (emoteClip != null){
            audioSource.clip = emoteClip;
            audioSource.Play();
        }
        playerUi.SetEmote(playerEmoteSprite.sprite, duration);
    }

    public void ShowEmote(string buttonString, float duration){
        photonView.RPC("ShowEmoteRpc", RpcTarget.All, buttonString, duration);
    }

    [PunRPC]
    private void OnStartClimb(){
        playerState = playerState | PlayerState.Climbing;

        playerCollider.isTrigger = true;
        playerFeetCollider.isTrigger = true;
        playerRigidbody.gravityScale = 0f;
    }

    [PunRPC]
    private void OnStopClimb(){
        playerState = playerState & ~PlayerState.Climbing;

        playerCollider.isTrigger = false;
        playerFeetCollider.isTrigger = false;
        playerRigidbody.gravityScale = 1f;
    }

    [PunRPC]
    private void OnUsingTurret(){
        playerState = playerState | PlayerState.UsingTurret;
    }

    [PunRPC]
    private void OnStopUsingTurret(){
        playerState = playerState & ~PlayerState.UsingTurret;
    }

    [PunRPC]
    public void OnCarryGameObject(int viewId) {
        // print($"Player {photonView.ControllerActorNr} Carrying {viewId}");

        // Current carried item of player.
        if (playerCarriedObject != null){
            playerCarriedObject.GetComponent<Interactable>().SetInteractable(true);
            playerCarriedObject.GetComponent<Rigidbody2D>().isKinematic = false;
        }

        // Stop carrying item.
        if (viewId == -1) {
            playerState = playerState & ~PlayerState.CarryingObject;
            playerCarriedObject = null;
            return;
        }

        // Carry item.
        playerState = playerState | PlayerState.CarryingObject;
        playerCarriedObject = PhotonNetwork.GetPhotonView(viewId).gameObject;
        playerCarriedObject.GetComponent<Rigidbody2D>().isKinematic = true;

        // Update the view here, so that the collider doesn't collide with other interactables.
        // Doesn't work, not sure why. Instead, we force remove all interacted objects with
        // RemoveAllInteracted().
        // UpdateView();

        playerCarriedObject.GetComponent<Interactable>().SetInteractable(false);
        // playerUi.ShowUseButton(false);
    }

    private IEnumerator SetInteractDelayFrame(bool interacting, int frameCount){
        for(int i = 0; i < frameCount; i++){
            yield return null;
        }

        SetInteractingInstant(interacting);
    }

    public void SetInteractingDelayFrame(bool interacting, int frameCount){
        StartCoroutine(SetInteractDelayFrame(interacting, frameCount));
    }

    public void SetInteractingDelayFrameInstant(bool instantInteracting, bool interacting, int frameCount){
        SetInteractingInstant(instantInteracting);
        StartCoroutine(SetInteractDelayFrame(interacting, frameCount));
    }

    public void SetInteractingInstant(bool interacting){
        if (interacting){
            playerState = playerState | PlayerState.Interacting;
        } else {
            playerState = playerState & ~PlayerState.Interacting;
        }
    }

    public bool IsInteracting(){
        return (playerState & PlayerState.Interacting) != 0;
    }

    public bool IsDoingSomething(){
        return progressBarUi.IsRunning();
    }

    public bool IsClimbing(){
        return (playerState & PlayerState.Climbing) != 0;
    }

    public bool IsCarryingObject(){
        return (playerState & PlayerState.CarryingObject) != 0;
    }

    public bool CanCarryObject(){
        return (playerState & PlayerState.CarryingOrUsingTurret) == 0;
    }

    public void DropObject(bool remove = false){
        PhotonView objectPhotonView = playerCarriedObject.GetComponent<PhotonView>();

        if (remove){
            // It is a scene object, run rpc to destroy.
            if (objectPhotonView.IsSceneView){
                TdGameManager.instance.DestroyPhotonNetworkedObject(objectPhotonView);
            } else {
                PhotonNetwork.Destroy(objectPhotonView);
            }
        }

        SetInteractingDelayFrameInstant(true, false, 1);
        
        photonView.RPC("OnCarryGameObject", RpcTarget.All, -1);
    }

    private void UpdateView() {
        if (IsCarryingObject()) {
            playerCarriedObject.transform.position = playerCarryTransform.transform.position;
        }

        animator.SetFloat("speed", Mathf.Abs(playerRigidbody.velocity.x));
        animator.SetBool("isClimbing", IsClimbing());
    }

    private void Update(){
        if (TdGameManager.isPaused){
            return;
        }

        UpdateView();

        if (!photonView.IsMine){
            return;
        }

        // print(playerState);

        // Stop movement if using turret.
        if ((playerState & PlayerState.UsingTurret) != 0){
            playerRigidbody.velocity = Vector2.zero;
            return;
        }

        // Drop object if carrying.
        if (IsCarryingObject()) {
            if (Input.GetButtonDown("Use") && !IsInteracting()) {
                DropObject();
            }
        }

        // Movement logic.
        float horizontalAxis = Input.GetAxisRaw("Horizontal");
        float verticalAxis = Input.GetAxisRaw("Vertical");

        Vector2 velocityDelta = new Vector3(horizontalAxis, verticalAxis) * moveSpeed;

        if ((playerState & PlayerState.Climbing) != PlayerState.Climbing){
            
            GameObject ladder = GetCollidingLadder();

            if (ladder){
                Bounds ladderBounds = ladder.GetComponent<Collider2D>().bounds;
                Bounds entityBounds = playerFeetCollider.bounds;

                // Player is pressing vertical input, is colliding with ladder, and
                // player presses up input when he is below half the ladder, vice-versa.

                // print(MyUtilityScript.IsInBounds(ladderBounds, entityBounds));
                if (Mathf.Abs(verticalAxis) > 0.01 && MyUtilityScript.IsInBounds(ladderBounds, entityBounds)
                && ((verticalAxis > 0 && entityBounds.min.y < ladderBounds.center.y)
                || (verticalAxis < 0 && entityBounds.min.y > ladderBounds.center.y))){
                    playerClimbingLadder = ladder;
                    photonView.RPC("OnStartClimb", RpcTarget.All);
                }
            }

            velocityDelta.y = playerRigidbody.velocity.y;
        } else {
            velocityDelta.x = 0f;

            Bounds ladderBounds = playerClimbingLadder.GetComponent<Collider2D>().bounds;
            Bounds entityBounds = playerFeetCollider.bounds;

            // Player is not colliding with ladder, or player is climbing down and
            // player's collider's lowest y point is < ladder's collider's lowest y point
            // with offset.

            // To prevent player from clipping through bottom of ladder.
            if (!IsCollidingLadder() || verticalAxis < 0 && entityBounds.min.y < ladderBounds.min.y + 0.2f){
                playerClimbingLadder = null;
                velocityDelta.y = 0f;
                photonView.RPC("OnStopClimb", RpcTarget.All);
            }
        }

        playerRigidbody.velocity = velocityDelta;
        // print(playerRigidbody.velocity);

        int playerFacingDir = System.Math.Sign(velocityDelta.x);
        if (playerFacingDir != 0){
            transform.localScale = new Vector3(playerFacingDir, transform.localScale.y, transform.localScale.z);
        }

        // Stop progress bar logic.
        if (IsDoingSomething()){

            // If player is moving while doing something, stop progress.
            if (playerFacingDir != 0){
                progressBarUi.StopProgressBar();
            }
        }

        if (Input.GetButtonDown("ToggleEmoteList")){
            playerUi.ShowEmoteList(!TdGameManager.instance.emoteCanvas.gameObject.activeSelf);
        }

        if (TdGameManager.instance.emoteCanvas.gameObject.activeSelf){
            foreach(PlayerEmoteSprite playerEmoteSprite in TdGameManager.gameSettings.playerEmoteSprites){
                if (Input.GetButtonDown(playerEmoteSprite.buttonString)){
                    ShowEmote(playerEmoteSprite.buttonString, TdGameManager.gameSettings.playerEmoteDuration);
                }
            }
        }

        // transform.position += new Vector3(velocityDelta.x, velocityDelta.y, 0f) * Time.deltaTime * moveSpeed;
    }

    public bool IsCollidingGround(){
        List<Collider2D> colliders = new List<Collider2D>();
        ContactFilter2D contactFilter2D = new ContactFilter2D();
        Physics2D.OverlapCollider(playerCollider, contactFilter2D, colliders);

        foreach(Collider2D c in colliders){
            if (c.tag == "Ground"){
                return true;
            }
        }

        return false;
    }

    public bool IsCollidingLadder(){
        if (GetCollidingLadder() != null){
            return true;
        }

        return false;
    }

    public GameObject GetCollidingLadder(){
        List<Collider2D> colliders = new List<Collider2D>();
        Physics2D.OverlapCollider(playerFeetCollider, ladderFilter2D, colliders);

        foreach(Collider2D c in colliders){
            if (c.tag == "Ladder"){
                return c.gameObject;
            }
        }

        return null;
    }
}
