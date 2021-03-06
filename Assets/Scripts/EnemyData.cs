using Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy")]
public class EnemyData: ScriptableObject {
    public EnemyType enemyType;
    public int health = 1;
    public AudioClipObject audioClipObject;
    public float attackTime = 1f;
    public float movespeed = 1f;
    public float stunnedTime = 1f;
    public float attackRange = 0.9f;

    [Header("If ranged, damage derives from projectile.")]
    public int damage = 1;
    public Projectile projectile;
}