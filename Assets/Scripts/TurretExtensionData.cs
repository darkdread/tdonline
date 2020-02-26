using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Photon.Pun;
using System.Reflection;

public class TurretExtensionData : MonoBehaviourPunCallbacks {
    [Header("Runtime Variables")]
    [SerializeField]
    public TurretExtension turretExtension;
    public Turret turret;

    [PunRPC]
    public void OnLoad(int viewId){
        PhotonView turretPhoton = PhotonNetwork.GetPhotonView(viewId);
        turret = turretPhoton.GetComponent<Turret>();
        turret.AddTurretExtensionData(this);

        OnLoadAfter();
    }

    /// <summary>
    /// Runs for every player after instantiation of Photon.
    /// </summary>
    public virtual void OnLoadAfter(){
        turretExtension = turret.GetTurretExtension<TurretExtension>();
    }
}
