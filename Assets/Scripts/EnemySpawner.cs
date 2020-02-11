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
    public int currentWaveId = -1;
    public int currentSpawnId = 0;
    public int currentDelayMs = 0;

    private void Awake(){
        
    }

    public void SpawnEnemy(string resourceName){
        GameObject go = PhotonNetwork.InstantiateSceneObject(resourceName, transform.position, Quaternion.identity);
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

    public IEnumerator SpawnWave(int waveId){
        if (currentDelayMs > 0){
            yield return new WaitForSeconds(currentDelayMs/1000f);
        }
        
        while(currentSpawnId < waves[waveId].waveStructs.Length){

            WaveStruct waveStruct = waves[waveId].waveStructs[currentSpawnId];
            currentDelayMs = waveStruct.waveDelayMs;

            WaveSpawnInfo info = new WaveSpawnInfo(){
                waveId = waveId,
                waveLastSpawnId = currentSpawnId,
                waveDelayToSpawnNext = currentDelayMs
            };

            Hashtable props = new Hashtable {
                {TdGame.WAVE_INFO + spawnerId, WaveSpawnInfo.Serialize(info)}
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            SpawnEnemy(waveStruct.waveEnemy.name);
            
            yield return new WaitForSeconds(waveStruct.waveDelayMs/1000f);
            currentSpawnId += 1;
        }

        currentSpawnId = 0;
        currentDelayMs = 0;
    }

    public int GetWaveSpawnDuration(int waveId){
        int duration = 0;
        
        foreach(WaveStruct waveStruct in waves[waveId].waveStructs){
            duration += waveStruct.waveDelayMs;
        }

        return duration;
    }

    public void SpawnNextWave(){
        currentWaveId += 1;

        if (currentWaveId >= waves.Length){
            return;
        }

        StartCoroutine(SpawnWave(currentWaveId));
    }
}
