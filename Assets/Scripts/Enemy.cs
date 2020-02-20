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

    private Animator animator;
    private Rigidbody2D rb;

    public float attackActualHit;
    public float attackCooldown;
    public float deathCooldown;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponentInChildren<Animator>();
        health = enemyData.health;
        enemyType = enemyData.enemyType;

        deathCooldown = Mathf.Infinity;
    }

    private void Die(){
        StopMovement();
        deathCooldown = GetAnimationDuration("Death");
        animator.SetTrigger("Death");
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

        if (enemy.health <= 0){
            Die();
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

    private float GetAnimationDuration(string clipName){
        AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;

        foreach(AnimationClip animationClip in animationClips){
            if (animationClip.name == clipName){
                return animationClip.length;
            }
        }

        return 0;
    }

    private void StopMovement(){
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
    }

    private void Update(){
        if (!PhotonNetwork.IsMasterClient){
            return;
        }

        if (TdGameManager.isPaused){
            return;
        }

        deathCooldown -= Time.deltaTime;
        if (deathCooldown <= 0f){
            TdGameManager.instance.DestroySceneObject(photonView.ViewID);
        }

        if (IsNearObjective(0.1f)){

            // Just reached gate.
            if (rb.isKinematic){
                StopMovement();
            }

            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f){
                attackCooldown = enemyData.enemyAttackTime;
                animator.SetTrigger("Attack");
                attackActualHit = GetAnimationDuration("Attack");
            }

            attackActualHit -= Time.deltaTime;
            if (attackActualHit <= 0f){
                attackActualHit = Mathf.Infinity;

                if (enemyType == EnemyType.Melee){
                    TdGameManager.castle.SetHealth(TdGameManager.castle.health - enemyData.damage);
                } else {
                    // Spawn projectile
                }
            }

            // TdGameManager.castle.SetHealth(TdGameManager.castle.health - 1);
            // TdGameManager.instance.photonView.RPC("DestroySceneObject", RpcTarget.MasterClient, photonView.ViewID);
        }
    }
}
