﻿using System.Collections;
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
    private Rigidbody2D rb;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
    }

    [PunRPC]
    public void SetTarget(int targetId){
        Transform target = PhotonNetwork.GetPhotonView(targetId).transform;

        targetPosition = target;
        rb.isKinematic = true;
        rb.velocity = TdGameManager.GetDirectionOfTransform2D(transform);
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
