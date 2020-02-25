﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Photon.Pun;
using Photon;

public enum EnemyType {
    Melee,
    Ranged
}

public enum EnemyState {
    normal,
    stunned
}

public class Enemy : MonoBehaviour, IAudioClipObject {
    public EndData endData;
    public EnemyData enemyData;
    public static List<Enemy> enemyList;
    private AudioSource enemyAudioSource;
    
    [Header("Runtime Variables")]
    public EnemyType enemyType;
    public EnemyState enemyState;
    public int health;
    public float stunnedTime;

    public Transform hitPositionTransform;
    public Transform targetGateTransform;
    public PhotonView photonView;

    private Animator animator;
    private Rigidbody2D rb;

    private float attackAnimationCompleteTime;
    private bool isDying;

    public float attackAnimationFinishHitTime;
    public float attackCooldown;
    public float deathCooldown;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponentInChildren<Animator>();
        enemyAudioSource = GetComponent<AudioSource>();
        health = enemyData.health;
        enemyType = enemyData.enemyType;

        deathCooldown = Mathf.Infinity;
        enemyList.Add(this);

        attackAnimationCompleteTime = GetAnimationDuration("Attack");
    }

    private void Die(){
        isDying = true;
        enemyList.Remove(this);

        StopMovement(true);
        // 1f for death sound to finish playing.
        deathCooldown = GetAnimationDuration("Death") + 1f;
        animator.SetTrigger("Death");
        animator.speed = 1f;

        PlaySoundLocal("Death");
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
    private void SetTargetRpc(int hitTransformId, int gateTransformId){
        Transform target = PhotonNetwork.GetPhotonView(hitTransformId).transform;
        hitPositionTransform = target;

        Transform gate = PhotonNetwork.GetPhotonView(gateTransformId).transform;
        targetGateTransform = gate;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.localScale = new Vector3(direction.x * transform.localScale.x,
            transform.localScale.y, transform.localScale.z);

        rb.isKinematic = true;
        rb.velocity = TdGameManager.GetDirectionOfTransform2D(transform) * enemyData.movespeed;
    }

    public void SetTarget(Transform hitTransform, Transform gateTransform){
        photonView.RPC("SetTargetRpc", RpcTarget.All, hitTransform.GetComponent<PhotonView>().ViewID,
            gateTransform.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void SetHealthRpc(int viewId, int health, int playerViewId){
        Enemy enemy = PhotonNetwork.GetPhotonView(viewId).GetComponent<Enemy>();
        enemy.health = health;

        if (enemy.health <= 0){
            Die();
        } else {
            StunEnemy(true);
        }

        if (playerViewId != 0 && enemy.health <= 0){
            TdPlayerController hittingPlayer = PhotonNetwork.GetPhotonView(playerViewId).GetComponent<TdPlayerController>();
            hittingPlayer.playerEndGameData.UpdateCount(EndGameEnum.Killed, endData);
        }
    }

    public void SetHealth(int health, int playerViewId = 0){
        photonView.RPC("SetHealthRpc", RpcTarget.All, photonView.ViewID, health, playerViewId);
    }

    public void PlaySound(string clipName){
        TdGameManager.instance.PlaySound(photonView.ViewID, clipName);
    }

    public void PlaySoundLocal(string clipName){
        AudioClip audioClip = enemyData.audioClipObject.GetAudioClipFromString(clipName);
        if (audioClip == null){
            return;
        }
        enemyAudioSource.PlayOneShot(audioClip);
    }

    public void StunEnemy(bool stun){
        if (stun){
            enemyState = enemyState | EnemyState.stunned;
            stunnedTime = enemyData.stunnedTime;
        } else {
            enemyState = enemyState & ~EnemyState.stunned;
            stunnedTime = 0f;
        }
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

    public void StopMovement(bool stop){
        rb.isKinematic = !stop;

        if (stop){
            rb.velocity = Vector3.zero;
        } else {
            rb.velocity = TdGameManager.GetDirectionOfTransform2D(transform) * enemyData.movespeed;
        }
    }

    public bool IsEnemyStunned(){
        return (enemyState & EnemyState.stunned) == EnemyState.stunned;
    }

    private void UpdateEnemy(){
        // If enemy's attack time is faster than animation time.
        // We have to speed up the animation.
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")){
            if (enemyData.attackTime < attackAnimationCompleteTime){
                animator.speed = attackAnimationCompleteTime / enemyData.attackTime;
            }
        }

        if (IsEnemyStunned()){
            stunnedTime -= Time.deltaTime;

            if (stunnedTime <= 0){
                StunEnemy(false);
            }
        }
    }

    private void Update(){
        if (TdGameManager.isPaused){
            return;
        }

        if (!isDying){
            UpdateEnemy();
        }

        if (!PhotonNetwork.IsMasterClient){
            return;
        }

        if (isDying){
            deathCooldown -= Time.deltaTime;
            if (deathCooldown <= 0f){
                TdGameManager.instance.DestroySceneObject(photonView);
            }

            return;
        }

        if (IsNearObjective(0.1f)){

            // Just reached objective.
            if (rb.isKinematic){
                StopMovement(true);
            }
            
            if (IsEnemyStunned()){
                return;
            }

            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f){
                attackCooldown = enemyData.attackTime;
                animator.SetTrigger("Attack");
                attackAnimationFinishHitTime = Mathf.Clamp(attackAnimationCompleteTime, 0, attackCooldown);
            }

            attackAnimationFinishHitTime -= Time.deltaTime;
            if (attackAnimationFinishHitTime <= 0f){
                attackAnimationFinishHitTime = Mathf.Infinity;
                PlaySound("Attack");

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

    public AudioClipObject GetAudioClipObject()
    {
        return enemyData.audioClipObject;
    }
}
