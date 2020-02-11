using Unity;
using UnityEngine;

[System.Serializable]
public struct WaveStruct {
    public Enemy waveEnemy;
    public int waveDelayMs;
}

[CreateAssetMenu(fileName = "Wave", menuName = "Wave")]
public class Wave: ScriptableObject {
    public WaveStruct[] waveStructs;

    
}