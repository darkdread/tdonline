using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ReloadableExtensionData : TurretExtensionData {
    public Stack<GameObject> ammunition = new Stack<GameObject>();
    public int maxAmmunition = 2;

    [PunRPC]
    public void AddAmmunition(int viewId){
        PhotonView view = PhotonNetwork.GetPhotonView(viewId);

        // Moves the carried object somewhere else.
        view.gameObject.transform.position = Vector3.zero;
        ammunition.Push(view.gameObject);
    }

    override public void OnPhotonInstantiate(PhotonMessageInfo info){
        ReloadableExtension reloadableExtension = (ReloadableExtension) turretExtension;
        this.transform.SetParent(TdGameManager.instance.gameCanvas.transform);

        for(int i = 0; i < this.maxAmmunition; i++){
            ItemSlot itemSlot = Instantiate<ItemSlot>(reloadableExtension.itemSlotPrefab, this.transform);
            // itemSlot.itemImage.sprite = requiredObject.GetComponent<SpriteRenderer>().sprite;
        }

        // Bug? Why is only the first instantiated scene object small?
        // And it's small only for clients other than master.

        // After investigation, it's because of the gameCanvas parent scale.
        // Although it still doesn't make sense of why the master client has a normal scale.
        transform.localScale = Vector3.one;

        base.OnPhotonInstantiate(info);
    }
}