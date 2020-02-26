using Unity;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class VisualEffectHandler: MonoBehaviour {
    
    private Animator animator;
    public float lifetime;

    private void Awake(){
        animator = GetComponent<Animator>();

        lifetime = MyUtilityScript.GetAnimationDuration(animator, "Effect");
    }

    private void Update(){
        if (TdGameManager.isPaused){
            return;
        }

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f){
            Destroy(gameObject);
        }
    }
}
