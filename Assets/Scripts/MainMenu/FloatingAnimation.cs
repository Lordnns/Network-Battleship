using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    private Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0.02f;
    }
    
}
