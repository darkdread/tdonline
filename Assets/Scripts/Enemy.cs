using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public enum EnemyType {
    Melee,
    Ranged
}

public class Enemy : MonoBehaviour {
    public EnemyType enemyType;
    public Transform targetPosition;
    public int health;
    public PhotonView photonView;

    private Rigidbody2D rb;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void SetTarget(int targetId){
        Transform target = PhotonNetwork.GetPhotonView(targetId).transform;

        targetPosition = target;
        rb.isKinematic = true;
        rb.velocity = TdGameManager.GetDirectionOfTransform2D(transform);
    }

    [PunRPC]
    public void SetHealth(int viewId, int health){
        Enemy enemy = PhotonNetwork.GetPhotonView(viewId).GetComponent<Enemy>();
        enemy.health = health;

        if (PhotonNetwork.IsMasterClient && enemy.health <= 0){
            TdGameManager.instance.photonView.RPC("DestroySceneObject", RpcTarget.MasterClient, viewId);
        }
    }

    private bool IsNearObjective(float distance){
        return Vector3.Distance(transform.position, targetPosition.position) < distance;
    }

    private void Update(){
        if (!PhotonNetwork.IsMasterClient){
            return;
        }

        if (IsNearObjective(0.1f)){
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
        }
    }
}
