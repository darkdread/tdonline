﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Projectile : MonoBehaviour, IAudioClipObject
{
    public EndData endData;
    public TdPlayerController owningPlayer;
    public Rigidbody2D rb;
    public ProjectileData projectileData;
    public PhotonView photonView;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    // For turrets
    private void ShootProjectile(int turretId, Vector3 angleVec, float speed){
        Turret turret = PhotonNetwork.GetPhotonView(turretId).GetComponent<Turret>();
        gameObject.SetActive(true);

        owningPlayer = turret.GetTurretExtensionData<FiringExtensionData>().playerController;
        owningPlayer.playerEndGameData.UpdateCount(EndGameEnum.Shot, endData);
        transform.position = turret.transform.position + angleVec;
        rb.velocity = angleVec * speed;

        TdGameManager.instance.PlaySound(turret.photonView.ViewID,
            "Shoot");
    }

    private void Update(){
        // Face forward direction.
        transform.right = rb.velocity.normalized;
    }

    private void UpdateView(Collision2D collision){

        print("hell2o");
        
        // If projectile is player-owned.
        if (owningPlayer != null){
            print(owningPlayer);
            print(collision.gameObject);

            // Ground layermask
            if (collision.gameObject.layer == 11){
                GameObject explosion = projectileData.GetExplosionPrefab();
                print("hello");

                if (explosion){
                    GameObject spawnedExplosion = Instantiate(explosion, collision.contacts[0].point, Quaternion.identity);
                    spawnedExplosion.transform.localScale = Vector3.one * projectileData.areaOfEffect / 2;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){

        // Function doesn't run for big boulder, only client who invoked projectile can.
        UpdateView(collision);

        if (!photonView.IsMine){
            return;
        }

        // If projectile is player-owned.
        if (owningPlayer != null){

            // Ground layermask
            if (collision.gameObject.layer == 11){
                Enemy[] enemies = TdGameManager.GetEnemiesOverlapSphere(collision.GetContact(0).point, projectileData.areaOfEffect);

                foreach(Enemy enemy in enemies){
                    enemy.SetHealth(enemy.health - projectileData.damage, owningPlayer.photonView.ViewID);
                }

                TdGameManager.instance.PlaySound(photonView.ViewID, "Hit");
                TdGameManager.instance.DestroyPhotonNetworkedObject(photonView);
            }
        } else {
            // If projectile is enemy-owned.

            // Gate layermask
            if (collision.gameObject.layer == 13){
                TdGameManager.castle.SetHealth(TdGameManager.castle.health - projectileData.damage);

                TdGameManager.instance.PlaySound(photonView.ViewID, "Hit");
                TdGameManager.instance.DestroyPhotonNetworkedObject(photonView);
            }
        }
        
        // // Enemy layermask
        // if (collision.gameObject.layer == 10){
        //     Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        //     enemy.photonView.RPC("SetHealth", RpcTarget.All, enemy.photonView.ViewID, enemy.health - projectileData.damage);
        // }
    }

    public AudioClipObject GetAudioClipObject()
    {
        return projectileData.audioClipObject;
    }
}
