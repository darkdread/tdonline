using Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Projectile")]
public class ProjectileData: ScriptableObject {
    public int damage;
    public float areaOfEffect;

}