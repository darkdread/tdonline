using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public enum EnemyType {
    Melee,
    Ranged
}

public class Enemy : MonoBehaviour {
    public EnemyData enemyData;
    
    public EnemyType enemyType;
    public int health;

    public Transform targetPosition;
    public PhotonView photonView;

    private Rigidbody2D rb;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        health = enemyData.health;
        enemyType = enemyData.enemyType;
    }

    [PunRPC]
    private void SetTarget(int targetId){
        Transform target = PhotonNetwork.GetPhotonView(targetId).transform;

        targetPosition = target;
        rb.isKinematic = true;
        rb.velocity = TdGameManager.GetDirectionOfTransform2D(transform);
    }

    [PunRPC]
    private void SetHealth(int viewId, int health, int playerViewId){
        Enemy enemy = PhotonNetwork.GetPhotonView(viewId).GetComponent<Enemy>();
        enemy.health = health;

        if (PhotonNetwork.IsMasterClient && enemy.health <= 0){
            TdGameManager.instance.photonView.RPC("DestroySceneObject", RpcTarget.MasterClient, viewId);
        }

        if (playerViewId != 0 && enemy.health <= 0){
            TdPlayerController hittingPlayer = PhotonNetwork.GetPhotonView(playerViewId).GetComponent<TdPlayerController>();
            hittingPlayer.playerEndGameData.UpdateKillCount(enemyData.name);
        }
    }

    public void SetHealth(int health, int playerViewId = 0){
        photonView.RPC("SetHealth", RpcTarget.All, photonView.ViewID, health, playerViewId);
    }

    private bool IsNearObjective(float distance){
        return Vector3.Distance(transform.position, targetPosition.position) < distance;
    }

    private void Update(){
        if (!PhotonNetwork.IsMasterClient){
            return;
        }

        if (TdGameManager.isPaused){
            return;
        }

        if (IsNearObjective(0.1f)){
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;

            TdGameManager.castle.SetHealth(TdGameManager.castle.health - 1);
            TdGameManager.instance.photonView.RPC("DestroySceneObject", RpcTarget.MasterClient, photonView.ViewID);
        }
    }
}
