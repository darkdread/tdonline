using Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "Projectile")]
public class ProjectileData: ScriptableObject {
    public int damage;
    public float areaOfEffect;
    public AudioClipObject audioClipObject;

    [Header("For enemy")]
    public int arcAngle = 45;
    public float gravity = 9.81f;

}