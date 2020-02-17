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

    public EnemyTypeObjective[] enemyTypeObjectives;
    public Wave[] waves;
    public int spawnerId = 0;
    public int currentWaveId = 0;
    public int currentSpawnId = 0;
    public int currentDelayMs = 0;
    
    private Coroutine spawnWaveRoutine;

    public void SpawnEnemy(string resourceName){
        GameObject go = PhotonNetwork.InstantiateSceneObject(
            Path.Combine(TdGameManager.gameSettings.enemyResourceDirectory, resourceName),
            transform.position, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();

        foreach(EnemyTypeObjective eto in enemyTypeObjectives){
            if (enemy.enemyType == eto.enemyType){
                Vector3 direction = (eto.objective.transform.position - transform.position).normalized;
                go.transform.localScale = new Vector3(direction.x * go.transform.localScale.x,
                    go.transform.localScale.y, go.transform.localScale.z);

                enemy.GetComponent<PhotonView>().RPC("SetTarget", RpcTarget.AllBuffered, 
                    eto.objective.GetComponent<PhotonView>().ViewID);
                break;
            }
        }
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

    public IEnumerator SpawnWave(int waveId){
        
        // Go through current wave, spawn all enemies.
        while(currentSpawnId < waves[waveId].waveStructs.Length){

            // Get wave struct, which contains enemy and delay.
            WaveStruct waveStruct = waves[waveId].waveStructs[currentSpawnId];
            currentDelayMs = waveStruct.waveDelayMs;
            SaveWaveProgress();

            // Delay > spawn. If wave contains 2 enemy, order goes as such:
            // D1 > S1 > D2 > S2.
            yield return new WaitForSeconds(currentDelayMs/1000f);

            SpawnEnemy(waveStruct.waveEnemy.name);
            currentSpawnId += 1;
        }

        currentWaveId += 1;
        currentSpawnId = 0;
        currentDelayMs = 0;

        SaveWaveProgress();
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

        if (spawnWaveRoutine != null){
            StopCoroutine(spawnWaveRoutine);
        }
        spawnWaveRoutine = StartCoroutine(SpawnWave(currentWaveId));
    }
}
