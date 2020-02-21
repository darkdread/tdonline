using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Photon.Pun;
using Photon;

public enum EnemyType {
    Melee,
    Ranged
}

public class Enemy : MonoBehaviour {
    public EnemyData enemyData;
    
    [Header("Runtime Variables")]
    public EnemyType enemyType;
    public int health;

    public Transform hitPositionTransform;
    public Transform targetGateTransform;
    public PhotonView photonView;

    private Animator animator;
    private Rigidbody2D rb;

    public float attackAnimationTime;
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
    private void ShootProjectileRpc(string resourceName){
        string resourcePath = Path.Combine(TdGameManager.gameSettings.enemyResourceDirectory, TdGameManager.StripCloneFromString(name), resourceName);
        Projectile p = PhotonNetwork.InstantiateSceneObject(resourcePath, transform.position, Quaternion.identity).GetComponent<Projectile>();

        // Velocity logic.
        float distance = Vector2.Distance(p.transform.position, targetGateTransform.position);
        float angle = 45;
        float speed = ProjectileMath.LaunchSpeed(distance, 0, Physics.gravity.magnitude, angle * Mathf.Deg2Rad);
        // speed = Mathf.Sqrt(Physics2D.gravity.magnitude * distance / (2 * Mathf.Cos(angle) * Mathf.Sin(angle)));

        Vector3 direction = TdGameManager.GetDirectionOfTransform2D(transform);
        Vector3 angleVec = Quaternion.AngleAxis(direction.x * angle, Vector3.forward) * direction;
        p.transform.rotation = Quaternion.AngleAxis(direction.x * angle, Vector3.forward);

        p.rb.velocity = angleVec * speed;
    }

    public void ShootProjectile(Projectile projectile, Transform target){
        photonView.RPC("ShootProjectileRpc", RpcTarget.MasterClient, projectile.name);
    }

    [PunRPC]
    private void SetTarget(int hitTransformId, int gateTransformId){
        Transform target = PhotonNetwork.GetPhotonView(hitTransformId).transform;
        hitPositionTransform = target;

        Transform gate = PhotonNetwork.GetPhotonView(gateTransformId).transform;
        targetGateTransform = gate;

        rb.isKinematic = true;
        rb.velocity = TdGameManager.GetDirectionOfTransform2D(transform) * enemyData.movespeed;
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
        return Vector3.Distance(transform.position, hitPositionTransform.position) < distance;
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
                attackCooldown = enemyData.attackTime;
                animator.SetTrigger("Attack");
                attackAnimationTime = GetAnimationDuration("Attack");

                // If attack animation time is faster than next attack.
                // We have to speed up the animation.
                if (attackAnimationTime > attackCooldown){
                    animator.speed = attackAnimationTime / attackCooldown;
                    attackAnimationTime = attackCooldown;
                }
            }

            attackAnimationTime -= Time.deltaTime;
            if (attackAnimationTime <= 0f){
                animator.speed = 1f;
                attackAnimationTime = Mathf.Infinity;

                if (enemyType == EnemyType.Melee){
                    TdGameManager.castle.SetHealth(TdGameManager.castle.health - enemyData.damage);
                } else {
                    // Spawn projectile
                    ShootProjectile(enemyData.projectile, hitPositionTransform);
                }
            }

            // TdGameManager.castle.SetHealth(TdGameManager.castle.health - 1);
            // TdGameManager.instance.photonView.RPC("DestroySceneObject", RpcTarget.MasterClient, photonView.ViewID);
        }
    }
}
