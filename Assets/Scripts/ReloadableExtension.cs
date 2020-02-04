using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class ReloadableExtensionData : TurretExtensionData {
    public int ammunition = 0;
}

[CreateAssetMenu(fileName = "ReloadableExtension", menuName = "TurretExtension/ReloadableExtension")]
public class ReloadableExtension : TurretExtension {

    public GameObject requiredObject;

    override public void OnLoadExtension(Turret turret){
        ReloadableExtensionData data = new ReloadableExtensionData(){
            ammunition = 5
        };

        turret.turretExtensionDatas.Add(data);
    }

    [PunRPC]
    public void LoadObject(){

    }

    private void UpdateUi(Turret turret){

    }

    override public void OnInteract(Turret turret){
        ReloadableExtensionData data = turret.GetTurretExtensionData(this) as ReloadableExtensionData;
        data.ammunition -= 1;

        Debug.Log(data.ammunition);
    }

    override public void UpdateTurretExtension(Turret turret){
        UpdateUi(turret);
    }
}
