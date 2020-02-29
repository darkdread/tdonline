using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct KeyCodeCallback {
    public KeyCode code;
    public UnityEngine.Events.UnityEvent callback;
}

public class TdDebugManager : MonoBehaviour
{
    public static TdDebugManager instance;
    // public string sceneToStart = "TileMapTest";

    public bool enableDebug = false;
    public KeyCodeCallback[] debugCalls;

    private void Awake(){
        if (instance == null && enableDebug){
            instance = this;
            DontDestroyOnLoad(gameObject);

            return;
        }

        Destroy(gameObject);
    }

    public void KillAllEnemies(){
        TdPlayerController player = TdGameManager.players[0];

        foreach(Enemy enemy in Enemy.enemyList){
            enemy.SetHealth(0, player.photonView.ViewID);
        }
    }
}
