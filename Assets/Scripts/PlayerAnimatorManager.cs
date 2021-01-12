using UnityEngine;
using System.Collections;

public class PlayerAnimatorManager : MonoBehaviour
{
    private Animator animator;
    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
        if (!animator)
        {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!animator) {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        animator.SetBool("Moving", (h != 0f || v != 0f));
        animator.SetFloat("Forwards-Backwards", h);
        animator.SetFloat("Right-Left", v);
    }
}