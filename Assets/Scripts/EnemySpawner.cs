using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

[System.Serializable]
public struct EnemyTypeObjective {
    public EnemyType enemyType;
    public Transform objective;
}

public class EnemySpawner : MonoBehaviour
{
    public EnemyTypeObjective[] enemyTypeObjectives;
    public Wave[] waves;
    public int currentWaveId = -1;

    public void SpawnEnemy(string resourceName){
        GameObject go = PhotonNetwork.InstantiateSceneObject(resourceName, transform.position, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();

        foreach(EnemyTypeObjective eto in enemyTypeObjectives){
            if (enemy.enemyType == eto.enemyType){
                Vector3 direction = (eto.objective.transform.position - transform.position).normalized;
                go.transform.localScale = new Vector3(direction.x * go.transform.localScale.x,
                    go.transform.localScale.y, go.transform.localScale.z);

                enemy.SetTarget(eto.objective.transform);
                break;
            }
        }
    }

    public IEnumerator SpawnWave(int waveId){
        foreach(WaveStruct waveStruct in waves[waveId].waveStructs){
        
            SpawnEnemy(waveStruct.waveEnemy.name);
            yield return new WaitForSeconds(waveStruct.waveDelay);
        }
    }

    public float GetWaveSpawnDuration(int waveId){
        float duration = 0f;
        
        foreach(WaveStruct waveStruct in waves[waveId].waveStructs){
            duration += waveStruct.waveDelay;
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
