using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerMovementManager : MonoBehaviourPun {
    public float AimSensitivity = 1.5f;
    public float JumpHeight = 1f, JumpSpeed = 3f;

    private Animator animator;
    private CharacterController characterController;
    private Dictionary<int, string> castAnimationTypes;
    private bool isRooted, isStunned, isBlocking;
    private float gravityValue = -9.81f;
    private Vector3 playerVelocity;

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }
        animator.speed *= GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;

        characterController = GetComponent<CharacterController>();

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
    void Update() {
        if (!animator || (photonView.IsMine == false && PhotonNetwork.IsConnected == true)) {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool running = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetButtonDown("Jump")) animator.SetTrigger("Jump");

        animator.SetBool("Moving", (h != 0f || v != 0f) && !isRooted && !isBlocking);
        animator.SetBool("Running", running && !isRooted && !isBlocking);
        animator.SetFloat("Forwards-Backwards", h);
        animator.SetFloat("Right-Left", v);

        // while mouse right-click is being held, dragging the mouse will turn the character
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
    }

    public void PlayCastingAnimation(int animationType) {
        if (!isStunned && !isBlocking) {
            // Local method
            //animator.Play(castAnimationTypes[animationType]);

            // animator method
            animator.SetTrigger("Cast");
            animator.SetInteger("CastType", animationType);
        }
    }

    public void JumpLift() {
        if (!photonView.IsMine) return;
        Debug.Log("JUMP");
        StartCoroutine(JumpRoutine());
    }

    IEnumerator JumpRoutine() {
        float timer = 0.75f;
        while (timer > 0f) {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 movement = transform.forward * v + transform.right * h;
            movement.y += JumpHeight;
            characterController.Move(movement * Time.deltaTime * JumpSpeed);

            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void Displace(Vector3 direction, float distance, float speed) {
        // TODO: Better animation system
        if (isRooted) return;
        animator.SetTrigger("Cast");
        animator.SetInteger("CastType", 11);

        StartCoroutine(DisplacementRoutine(direction, distance, speed));
    }

    IEnumerator DisplacementRoutine(Vector3 direction, float distance, float speed) {
        float timer = distance / speed;
        while (timer > 0f) {
            Vector3 movement = transform.forward * direction.x + transform.right * direction.z + Vector3.up * direction.y;
            characterController.Move(movement * Time.deltaTime * speed);

            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void StartBlock() {
        isBlocking = true;
        animator.SetBool("Blocking", isBlocking);
    }

    public void StopBlock() {
        isBlocking = false;
        animator.SetBool("Blocking", isBlocking);
    }

    public void Root(bool rooted) {
        isRooted = rooted;
    }

    public void Stun(bool stunned) {
        if (stunned) {
            isStunned = true;
            animator.speed = 0;
        } else {
            isStunned = false;
            animator.speed = 1f * GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        }
    }
}