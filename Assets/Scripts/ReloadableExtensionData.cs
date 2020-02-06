using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ReloadableExtensionData : TurretExtensionData {
    // public PhotonView photonView;
    public int ammunition = 0;
    public int maxAmmunition = 2;

    [PunRPC]
    public void SetAmmunition(int amount){
        ammunition = amount;
        Debug.Log(ammunition);
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

        turret.turretExtensionDatas.Add(this);

        base.OnPhotonInstantiate(info);
    }
}