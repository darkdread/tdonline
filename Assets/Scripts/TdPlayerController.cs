using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TdPlayerController : MonoBehaviour
{
    [HideInInspector]
    public PhotonView photonView;
    public TdPlayerUi playerUiPrefab;

    public float moveSpeed = 1f;

    private void Awake(){
        photonView = GetComponent<PhotonView>();
        TdPlayerUi playerUi = Instantiate<TdPlayerUi>(playerUiPrefab,
                            TdGameManager.instance.gameCanvas.transform);

        playerUi.SetTarget(this);
    }

    private void Update(){
        if (!photonView.IsMine){
            return;
        }

        float horizontalAxis = Input.GetAxis("Horizontal");
        float verticalAxis = Input.GetAxis("Vertical");

        transform.position += new Vector3(horizontalAxis, verticalAxis, 0f) * Time.deltaTime * moveSpeed;
    }
}
