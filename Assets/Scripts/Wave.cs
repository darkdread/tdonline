using Unity;
using UnityEngine;

[System.Serializable]
public struct WaveStruct {
    public Enemy[] waveEnemies;
    public float[] waveDelays;
}

[CreateAssetMenu(fileName = "Wave", menuName = "Wave")]
public class Wave: ScriptableObject {
    public WaveStruct waveStruct;

    
}