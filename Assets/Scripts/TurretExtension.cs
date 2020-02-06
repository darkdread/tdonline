using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TurretExtensionData : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback {
    [SerializeField]
    public TurretExtension turretExtension;
    public Turret turret;

    [PunRPC]
    public void OnLoad(int viewId){
        PhotonView turretPhoton = PhotonNetwork.GetPhotonView(viewId);
        turret = turretPhoton.GetComponent<Turret>();
        turret.turretExtensionDatas.Add(this);
    }

    public virtual void OnPhotonInstantiate(PhotonMessageInfo info){
        
    }
}


public abstract class TurretExtension : ScriptableObject {
    // Called on Awake.
    public abstract void OnLoadExtension(Turret turret);

    // Called when interacted upon.
    public abstract void OnInteract(Turret turret, TdPlayerController playerController);

    // Called per Update.
    public abstract void UpdateTurretExtension(Turret turret);

    public void CreatePhotonData(Turret turret, string resourceName){
        // Apparently view ids start from 0, for each unique player.
        // In order for us to instantiate with our own view id, we have to create our own params.
        // Pun.InstantiateParameters netParams = new InstantiateParameters(prefabName, position, rotation, group, data, currentLevelPrefix, null, LocalPlayer, ServerTimestamp);
        // However, since the method to use the params is private, we can't really access it.

        // Workaround: Modify all view ids of instances of objects in scene to a high number.
        GameObject obj = PhotonNetwork.InstantiateSceneObject(resourceName, Vector3.zero, Quaternion.identity);
        PhotonView photonView = obj.GetComponent<PhotonView>();
        photonView.RPC("OnLoad", RpcTarget.AllBufferedViaServer, turret.photonView.ViewID);
    }
}
