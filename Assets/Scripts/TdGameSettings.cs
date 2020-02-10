using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TdGameSettings : MonoBehaviour
{
    [Header("Initialization")]
    public Transform[] playerStartPositions;
    public Color[] playerColors = new Color[]{
        Color.red,
        Color.blue,
        Color.cyan,
        Color.green
    };
    public TdPlayerUi playerUiPrefab;

    public EnemySpawner[] enemySpawners;
}
