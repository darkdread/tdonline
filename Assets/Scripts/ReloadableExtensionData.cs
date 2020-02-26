using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ReloadableExtensionData : TurretExtensionData {
    public List<GameObject> ammunition = new List<GameObject>();
    public List<ItemSlot> itemSlots = new List<ItemSlot>();
    public int maxAmmunition = 0;

    [PunRPC]
    public void AddAmmunition(int viewId){
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);
        
        // Moves the carried object somewhere else so that when the
        // Projectile is fired, the distance is > 1, and therefore
        // teleports in front of the catapult.
        // view.gameObject.transform.position = Vector3.zero;

        view.gameObject.SetActive(false);
        ammunition.Add(view.gameObject);
    }

    [PunRPC]
    public void RemoveAmmunition(int viewId){
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);
        
        // view.gameObject.SetActive(true);
        ammunition.Remove(view.gameObject);
    }

    public GameObject GetLastAmmunitionLoaded(){
        GameObject lastAmmunitionLoaded = ammunition[ammunition.Count - 1];
        return lastAmmunitionLoaded;
    }

    public GameObject RemoveLastAmmunitionLoaded(){
        GameObject lastAmmunitionLoaded = GetLastAmmunitionLoaded();
        photonView.RPC("RemoveAmmunition", RpcTarget.All, lastAmmunitionLoaded.GetComponent<PhotonView>().ViewID);
        return lastAmmunitionLoaded;
    }

    override public void OnPhotonInstantiate(PhotonMessageInfo info){
        ReloadableExtension reloadableExtension = (ReloadableExtension) turretExtension;
        this.transform.SetParent(TdGameManager.instance.gameUiCanvas);

        maxAmmunition = reloadableExtension.maxAmmunition;
        
        for(int i = 0; i < this.maxAmmunition; i++){
            ItemSlot itemSlot = Instantiate<ItemSlot>(reloadableExtension.itemSlotPrefab, this.transform);
            itemSlots.Add(itemSlot);
        }

        // Bug? Why is only the first instantiated scene object small?
        // And it's small only for clients other than master.

        // After investigation, it's because of the gameCanvas parent scale.
        // Although it still doesn't make sense of why the master client has a normal scale.
        transform.localScale = Vector3.one;

        base.OnPhotonInstantiate(info);
    }
}
