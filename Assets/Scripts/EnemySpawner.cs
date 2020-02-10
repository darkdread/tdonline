using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
public class EnemySpawner : MonoBehaviour
{
    public Transform objective;
    public Wave[] waves;
    public int currentWaveId = -1;

    public void SpawnEnemy(string resourceName){
        Vector3 direction = (objective.transform.position - transform.position).normalized;

        GameObject go = PhotonNetwork.InstantiateSceneObject(resourceName, transform.position, Quaternion.Euler(direction));
        go.GetComponent<Rigidbody2D>().isKinematic = true;
        go.GetComponent<Rigidbody2D>().velocity = direction;
    }

    public IEnumerator SpawnWave(int waveId){
        WaveStruct waveStruct = waves[waveId].waveStruct;
        
        for(int i = 0; i < waveStruct.waveDelays.Length; i++){
            SpawnEnemy(waveStruct.waveEnemies[i].name);
            yield return new WaitForSeconds(waveStruct.waveDelays[i]);
        }
    }

    public void SpawnNextWave(){
        currentWaveId += 1;

        if (currentWaveId >= waves.Length){
            return;
        }

        StartCoroutine(SpawnWave(currentWaveId));
    }
}
