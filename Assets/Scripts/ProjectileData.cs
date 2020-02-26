using Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Projectile")]
public class ProjectileData: ScriptableObject {
    public int damage;
    public float areaOfEffect;
    public AudioClipObject audioClipObject;
    public GameObject[] explosionPrefabs;

    [Header("For enemy")]
    public int arcAngle = 45;
    public float gravity = 9.81f;

    public GameObject GetExplosionPrefab(int index = -1){
        if (index == -1){
            return explosionPrefabs[Random.Range(0, explosionPrefabs.Length)];
        }

        return explosionPrefabs[index];
    }
}
