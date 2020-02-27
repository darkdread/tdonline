using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using System.IO;

public class Trap : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public PhotonView photonView;
    public string resourceName = "Boulder";

    private void Awake(){
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ShowTrap(bool show){
        TdGameManager.instance.ShowGameObject(photonView.ViewID, show);
    }

    public void ShowOutline(Color color){
        spriteRenderer.color = color;
    }

    public void Trigger(TdPlayerController playerController){
        GameObject go = PhotonNetwork.Instantiate(Path.Combine(TdGameManager.gameSettings.trapResourceDirectory, resourceName, resourceName)
            , transform.position, transform.rotation);
        // TdGameManager.instance.AddProjectileComponent(go.GetComponent<PhotonView>().ViewID);
        TdGameManager.instance.SetOwningPlayer(playerController.GetComponent<PhotonView>().ViewID, go.GetComponent<PhotonView>().ViewID);
    }
}
