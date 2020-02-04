using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class TurretExtensionData : MonoBehaviour {

}


public abstract class TurretExtension : ScriptableObject {
    // Called on Awake.
    public abstract void OnLoadExtension(Turret turret);

    // Called when interacted upon.
    public abstract void OnInteract(Turret turret);

    // Called per Update.
    public abstract void UpdateTurretExtension(Turret turret);
}
