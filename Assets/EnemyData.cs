using Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy")]
public class EnemyData: ScriptableObject {
    public EnemyType enemyType;
    public int health;
}