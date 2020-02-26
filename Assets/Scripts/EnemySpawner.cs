using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public struct EnemyTypeObjective {
    public EnemyType enemyType;
    public Transform objective;
    public Transform gate;
}

public struct WaveSpawnInfo {
    public int waveId;
    public int waveLastSpawnId;
    public int waveDelayToSpawnNext;

    public static WaveSpawnInfo Deserialize(byte[] data){
        WaveSpawnInfo result = new WaveSpawnInfo();

        using (MemoryStream m = new MemoryStream(data)) {
            using (BinaryReader reader = new BinaryReader(m)) {
                result.waveId = reader.ReadInt32();
                result.waveLastSpawnId = reader.ReadInt32();
                result.waveDelayToSpawnNext = reader.ReadInt32();
            }
        }
      return result;
    }

  public static byte[] Serialize(object customType){
        WaveSpawnInfo info = (WaveSpawnInfo)customType;

        using (MemoryStream m = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(m)) {
                writer.Write(info.waveId);
                writer.Write(info.waveLastSpawnId);
                writer.Write(info.waveDelayToSpawnNext);
            }

            return m.ToArray();
        }
    }
}

public class EnemySpawner : MonoBehaviour {

    public static int spawnCount = 0;
    public Transform gateTransform;

    public Wave[] waves;

    [Header("Runtime Variables")]
    public int spawnerId = 0;
    public int currentWaveId = 0;
    public int currentSpawnId = 0;
    public int currentDelayMs = 0;
    public int currentWaveMaxSpawn = 0;

    public int nextSpawn = 0;
    
    private Coroutine spawnWaveRoutine;

    public void SpawnEnemy(string resourceName){
        GameObject go = PhotonNetwork.InstantiateSceneObject(
            Path.Combine(TdGameManager.gameSettings.enemyResourceDirectory, resourceName, resourceName),
            transform.position, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();

        enemy.SetTarget(gateTransform);
    }

    public void LoadWaveProgress(WaveSpawnInfo info){
        print($"Setting wave of {spawnerId} to {info.waveId}");
        print($"Setting waveLastSpawn of {spawnerId} to {info.waveLastSpawnId}");
        print($"Setting waveDelayMs of {spawnerId} to {info.waveDelayToSpawnNext}");

        currentWaveId = info.waveId;
        currentSpawnId = info.waveLastSpawnId;
        currentDelayMs = info.waveDelayToSpawnNext;
    }

    public void SaveWaveProgress(){
        // Set the current wave progress in this struct.
        WaveSpawnInfo info = new WaveSpawnInfo(){
            waveId = currentWaveId,
            waveLastSpawnId = currentSpawnId,
            waveDelayToSpawnNext = currentDelayMs
        };

        // Create a key with unique id of the spawner's id, with
        // value of the current wave progress.
        Hashtable props = new Hashtable {
            {TdGame.WAVE_INFO + spawnerId, WaveSpawnInfo.Serialize(info)}
        };

        // Send current wave progress to the room property.
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public void WaveSpawnEnemy(int waveId, int spawnId){
        // Get wave struct, which contains enemy and delay.
        WaveStruct waveStruct = waves[waveId].waveStructs[currentSpawnId];
        SpawnEnemy(waveStruct.waveEnemy.name);
    }

    public int GetWaveSpawnDuration(int waveId){
        int duration = 0;
        
        foreach(WaveStruct waveStruct in waves[waveId].waveStructs){
            duration += waveStruct.waveDelayMs;
        }

        return duration;
    }

    public void SpawnWave(){
        if (currentWaveId >= waves.Length){
            return;
        }

        currentSpawnId = 0;
        nextSpawn = waves[currentWaveId].waveStructs[0].waveDelayMs;
        currentWaveMaxSpawn = waves[currentWaveId].waveStructs.Length;
    }

    private void Update(){
        if (TdGameManager.isPaused || currentSpawnId >= currentWaveMaxSpawn){
            return;
        }

        // Delay > spawn. If wave contains 2 enemy, order goes as such:
        // D1 > S1 > D2 > S2.
        nextSpawn -= (int) (Time.deltaTime * 1000);
        if (nextSpawn <= 0){
            WaveSpawnEnemy(currentWaveId, currentSpawnId);

            currentSpawnId += 1;
            if (currentSpawnId < currentWaveMaxSpawn){
                WaveStruct waveStruct = waves[currentWaveId].waveStructs[currentSpawnId];
                currentDelayMs = waveStruct.waveDelayMs;
                nextSpawn = waveStruct.waveDelayMs;

                SaveWaveProgress();
            } else {
                currentWaveId += 1;
                currentDelayMs = 0;
                currentSpawnId = 0;
                SaveWaveProgress();
                currentSpawnId = currentWaveMaxSpawn;
            }
        }

    }
}
