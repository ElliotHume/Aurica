using UnityEngine;
using System.Collections;
using Photon.Pun;

public class PlayerMovementManager : MonoBehaviourPun
{
    public float AimSensitivity = 1.5f;

    private Animator animator;

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }
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
        if (Input.GetButton("Fire2")) {
            float x = Input.GetAxis("Mouse X");
            transform.RotateAround (transform.position, -Vector3.up, -x * AimSensitivity);
        }
    }
}