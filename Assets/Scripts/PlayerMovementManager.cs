using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerMovementManager : MonoBehaviourPun
{
    public float AimSensitivity = 1.5f;

    private Animator animator;
    private Dictionary<int, string> castAnimationTypes;

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }
        animator.speed *= GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;

        // Init dictionary with cast types
        castAnimationTypes = new Dictionary<int, string>();
        castAnimationTypes.Add(0, "1H Up Cast");
        castAnimationTypes.Add(1, "1H Throw");
        castAnimationTypes.Add(2, "1H Lob");
        castAnimationTypes.Add(3, "2H Up Cast");
        castAnimationTypes.Add(4, "2H Down Cast");
        castAnimationTypes.Add(5, "2h Explosion");
        castAnimationTypes.Add(6, "2H Forwards Cast 1");
        castAnimationTypes.Add(7, "2H Forwards Cast 2");
        castAnimationTypes.Add(8, "2H Finger Channel");
        castAnimationTypes.Add(9, "2H Hand Channel");
        castAnimationTypes.Add(10, "2H Clap Cast");
    }

    // Update is called once per frame
    void Update()
    {
        if (!animator || (photonView.IsMine == false && PhotonNetwork.IsConnected == true)) {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool running = Input.GetKey(KeyCode.LeftShift);

        animator.SetBool("Moving", (h != 0f || v != 0f));
        animator.SetBool("Running", running);
        animator.SetFloat("Forwards-Backwards", h);
        animator.SetFloat("Right-Left", v);

        // while mouse right-click is being held, dragging the mouse will turn the character
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
    }

    public void PlayCastingAnimation(int animationType) {
        animator.Play(castAnimationTypes[animationType]);
    }
}