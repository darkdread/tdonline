using System.Collections;
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
        GameObject turret = PhotonNetwork.GetPhotonView(turretId).gameObject;

        owningPlayer = turret.GetComponent<Turret>().controllingPlayer;
        owningPlayer.playerEndGameData.UpdateCount(EndGameEnum.Shot, endData);
        transform.position = turret.transform.position + angleVec;
        rb.velocity = angleVec * speed;

        TdGameManager.instance.PlaySound(turret.GetComponent<Turret>().photonView.ViewID,
            "Shoot");
    }

    private void Update(){
        // Face forward direction.
        transform.right = rb.velocity.normalized;
    }

    private void OnCollisionEnter2D(Collision2D collision){

        if (!PhotonNetwork.IsMasterClient){
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
                TdGameManager.instance.DestroySceneObject(photonView);
            }
        } else {
            // If projectile is enemy-owned.

            // Gate layermask
            if (collision.gameObject.layer == 13){
                TdGameManager.castle.SetHealth(TdGameManager.castle.health - projectileData.damage);

                TdGameManager.instance.PlaySound(photonView.ViewID, "Hit");
                TdGameManager.instance.DestroySceneObject(photonView);
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
