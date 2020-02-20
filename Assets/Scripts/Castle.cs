using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Castle : MonoBehaviourPunCallbacks {
    public CastleUi castleUi;
    public int maxHealth;
    public int health;

    private void Awake(){
        SetHealth(maxHealth);
    }

    [PunRPC]
    private void SetHealthRpc(int value){
        health = value;
        castleUi.healthSlider.value = (float) health / maxHealth;
    }

    public void SetHealth(int value){
        photonView.RPC("SetHealthRpc", RpcTarget.All, value);
    }
}
