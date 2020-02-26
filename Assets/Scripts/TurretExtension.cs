using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Photon.Pun;

public abstract class TurretExtension : ScriptableObject {
    
    /// <summary>
    /// // Called on Awake.
    /// </summary>
    /// <param name="turret"></param>
    public abstract void OnLoadExtension(Turret turret);

    /// <summary>
    /// Called when interacted upon before turret state has been set.
    /// </summary>
    /// <param name="turret"></param>
    /// <param name="playerController"></param>
    public virtual void OnInteract(Turret turret, TdPlayerController playerController){}

    /// <summary>
    /// Called when interacted upon after turret state has been set.
    /// </summary>
    /// <param name="turret"></param>
    /// <param name="playerController"></param>
    public virtual void OnInteractAfter(Turret turret, TdPlayerController playerController){}

    /// <summary>
    /// Called on Update.
    /// </summary>
    /// <param name="turret"></param>
    public abstract void UpdateTurretExtension(Turret turret);

    public void CreatePhotonData(Turret turret, string resourceName){
        // Apparently view ids start from 0, for each unique player.
        // In order for us to instantiate with our own view id, we have to create our own params.
        // Pun.InstantiateParameters netParams = new InstantiateParameters(prefabName, position, rotation, group, data, currentLevelPrefix, null, LocalPlayer, ServerTimestamp);
        // However, since the method to use the params is private, we can't really access it.

        // Workaround: Modify all view ids of instances of objects in scene to a high number.
        Debug.Log($"CreatePhotonData {this} {turret}");

        string pathToData = Path.Combine(TdGameManager.gameSettings.turretExtensionResourceDirectory, resourceName);
        GameObject obj = PhotonNetwork.InstantiateSceneObject(pathToData, Vector3.zero, Quaternion.identity);
        PhotonView photonView = obj.GetComponent<PhotonView>();
        photonView.RPC("OnLoad", RpcTarget.AllBuffered, turret.photonView.ViewID);
    }
}
