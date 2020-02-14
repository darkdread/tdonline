using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Projectile : MonoBehaviour
{
    public Rigidbody2D rb;
    public ProjectileData projectileData;
    public PhotonView photonView;

    private void Awake(){
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void ShootProjectile(int turretId, Vector3 angleVec, float speed){
        GameObject turret = PhotonNetwork.GetPhotonView(turretId).gameObject;

        transform.position = turret.transform.position + angleVec;
        rb.velocity = angleVec * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision){

        if (!PhotonNetwork.IsMasterClient){
            return;
        }

        // Ground layermask
        if (collision.gameObject.layer == 11){
            Enemy[] enemies = TdGameManager.GetEnemiesOverlapSphere(collision.GetContact(0).point, projectileData.areaOfEffect);

            foreach(Enemy enemy in enemies){
                enemy.photonView.RPC("SetHealth", RpcTarget.All, enemy.photonView.ViewID, enemy.health - projectileData.damage);
            }

            TdGameManager.instance.photonView.RPC("DestroySceneObject", RpcTarget.MasterClient, photonView.ViewID);
        }
        
        // // Enemy layermask
        // if (collision.gameObject.layer == 10){
        //     Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        //     enemy.photonView.RPC("SetHealth", RpcTarget.All, enemy.photonView.ViewID, enemy.health - projectileData.damage);
        // }
    }
}
