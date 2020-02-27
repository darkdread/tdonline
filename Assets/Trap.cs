using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using System.IO;

public class Trap : MonoBehaviour
{

    public PhotonView photonView;
    public string resourceName = "Boulder";

    public void ShowTrap(bool show){
        TdGameManager.instance.ShowGameObject(photonView.ViewID, show);
    }

    public void Trigger(TdPlayerController playerController){
        GameObject go = PhotonNetwork.Instantiate(Path.Combine(TdGameManager.gameSettings.trapResourceDirectory, resourceName, resourceName)
            , transform.position, transform.rotation);
        go.GetComponent<Rigidbody2D>().isKinematic = false;
        TdGameManager.instance.AddProjectileComponent(go.GetComponent<PhotonView>().ViewID);
        TdGameManager.instance.SetOwningPlayer(playerController.GetComponent<PhotonView>().ViewID, go.GetComponent<PhotonView>().ViewID);
    }
}
