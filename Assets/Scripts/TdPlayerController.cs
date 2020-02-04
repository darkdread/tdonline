using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public enum PlayerState {
    Climbing = 1,
    UsingTurret = 2,
    CarryingItem = 4,
}

public class TdPlayerController : MonoBehaviour
{
    [HideInInspector]
    public PhotonView photonView;

    [HideInInspector]
    public Collider2D playerCollider;

    [HideInInspector]
    public Rigidbody2D playerRigidbody;

    public Transform playerCarryTransform;
    public Collider2D playerFeetCollider;


    [SerializeField]
    public ContactFilter2D ladderFilter2D;
    public GameObject playerClimbingLadder;

    [HideInInspector]
    public TdPlayerUi playerUi;

    public PlayerState playerState;

    public float moveSpeed = 1f;

    private void Start(){
        photonView = GetComponent<PhotonView>();
        playerCollider = GetComponent<Collider2D>();
        playerRigidbody = GetComponent<Rigidbody2D>();

        playerUi = Instantiate<TdPlayerUi>(TdGameManager.gameSettings.playerUiPrefab,
                            TdGameManager.instance.gameCanvas.transform);
        playerUi.SetTarget(this);
    }

    [PunRPC]
    private void OnStartClimb(){
        playerState = playerState | PlayerState.Climbing;

        playerCollider.isTrigger = true;
        playerRigidbody.gravityScale = 0f;
    }

    [PunRPC]
    private void OnStopClimb(){
        playerState = playerState & ~PlayerState.Climbing;

        playerCollider.isTrigger = false;
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

    private void Update(){
        if (!photonView.IsMine){
            return;
        }

        print(playerState);

        if ((playerState & PlayerState.UsingTurret) != 0){
            playerRigidbody.velocity = Vector2.zero;
            return;
        }

        // Movement logic.

        float horizontalAxis = Input.GetAxisRaw("Horizontal");
        float verticalAxis = Input.GetAxisRaw("Vertical");

        Vector2 velocityDelta = new Vector3(horizontalAxis, verticalAxis);

        if ((playerState & PlayerState.Climbing) != PlayerState.Climbing){
            
            GameObject ladder = GetCollidingLadder();

            if (ladder){
                Bounds ladderBounds = ladder.GetComponent<Collider2D>().bounds;
                Bounds entityBounds = playerFeetCollider.bounds;

                // Player is pressing vertical input, is colliding with ladder, and
                // player presses up input when he is below half the ladder, vice-versa.

                if (Mathf.Abs(verticalAxis) > 0.01 && MyUtilityScript.IsInBounds(ladderBounds, entityBounds)
                && ((verticalAxis > 0 && entityBounds.min.y < ladderBounds.center.y)
                || (verticalAxis < 0 && entityBounds.min.y > ladderBounds.center.y))){
                    playerClimbingLadder = ladder;
                    photonView.RPC("OnStartClimb", RpcTarget.All);
                    // OnStartClimb(ladder);
                }
            }

            velocityDelta.y = 0f;

        } else {
            velocityDelta.x = 0f;

            Bounds ladderBounds = playerClimbingLadder.GetComponent<Collider2D>().bounds;
            Bounds entityBounds = playerFeetCollider.bounds;

            // Player is not colliding with ladder, or player is climbing down and
            // player's collider's lowest y point is < ladder's collider's lowest y point
            // with offset.

            // To prevent player from clipping through bottom of ladder.
            if (!IsCollidingLadder() 
            || verticalAxis < 0 && entityBounds.min.y < ladderBounds.min.y + 0.3f){
                playerClimbingLadder = null;
                photonView.RPC("OnStopClimb", RpcTarget.All);
                // OnStopClimb();
            }
        }

        playerRigidbody.velocity = velocityDelta * moveSpeed;
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
