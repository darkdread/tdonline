using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class Castle : MonoBehaviourPunCallbacks {
    public CastleUi castleUi;
    public int health;

    private void Awake(){
        if (!photonView.IsMine){
            return;
        }

        SetHealth(TdGameManager.gameSettings.castleMaxHealth);
    }

    [PunRPC]
    private void SetHealthRpc(int value){
        health = value;
        castleUi.healthSlider.value = (float) health / TdGameManager.gameSettings.castleMaxHealth;

        if (health <= 0){
            TdGameManager.instance.Lose();
        }
    }

    public void SetHealth(int value){
        photonView.RPC("SetHealthRpc", RpcTarget.All, value);
    }
}
