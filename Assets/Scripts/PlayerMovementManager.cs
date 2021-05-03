using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerMovementManager : MonoBehaviourPun, IPunObservable {
    public float AimSensitivity = 1.5f;
    public float PlayerSpeed = 1f, JumpHeight = 1f, JumpSpeed = 3f, Mass = 3f;
    public AudioSource footStepSource;
    public AudioClip[] footsteps;
    public AudioSource jumpSource;
    public AudioClip[] jumpingSounds;

    private Animator animator;
    private CharacterController characterController;
    private Dictionary<int, string> castAnimationTypes;
    private bool isRooted, isStunned, isBlocking, isBeingDisplaced, jumping, casting;
    private Vector3 playerVelocity, impact, velocity;

    private float movementSpeed;

    Vector3 networkPosition;
    Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            // CRITICAL DATA
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        } else {
            // Network player, receive data
            // CRITICAL DATA
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (velocity * lag);
        }
    }

    // Use this for initialization
    void Start() {
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }
        animator.speed *= GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        movementSpeed = PlayerSpeed;

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
        if (!photonView.IsMine && PhotonNetwork.IsConnected) {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * movementSpeed * 2f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 1000f);
            return;
        }
        if (!animator) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && !isRooted && !isBlocking && !isBeingDisplaced && !jumping) {
            animator.SetTrigger("Jump");
            jumping = true;
        }

        animator.SetBool("Moving", (h != 0f || v != 0f) && !isRooted && !isBlocking && !isBeingDisplaced);
        animator.SetBool("Running", !isRooted && !isBlocking && !isBeingDisplaced);
        animator.SetFloat("Forwards-Backwards", h);
        animator.SetFloat("Right-Left", v);

        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

        // Apply motion after turning
        Vector3 oldPosition = transform.position;
        if (!jumping && !casting && !isBlocking && !isRooted && !isStunned && !isBeingDisplaced) characterController.Move((transform.forward * v + transform.right * h).normalized * movementSpeed * Time.deltaTime);

        // Apply Gravity
        // if (!characterController.isGrounded) characterController.Move(Physics.gravity * Time.deltaTime);

        // Apply impact force:
        if (impact.magnitude > 0.2) characterController.Move(impact * Time.deltaTime);

        // Consume the impact energy each cycle:
        impact = characterController.isGrounded ? Vector3.Lerp(impact, Vector3.zero, 5 * Time.deltaTime) : Vector3.Lerp(impact, Vector3.zero, 2.5f * Time.deltaTime);

        // Calculate velocity for lag compensation
        velocity = transform.position - oldPosition;
    }

    public void PlayCastingAnimation(int animationType) {
        if (!isStunned && !isBlocking) {
            // Local method
            //animator.Play(castAnimationTypes[animationType]);

            // animator method
            animator.SetTrigger("Cast");
            animator.SetInteger("CastType", animationType);
            casting = true;
        }
    }

    public void EndCast() {
        casting = false;
    }

    public void Footstep() {
        footStepSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        footStepSource.Play ();
    }

    public void JumpLift() {
        if (!photonView.IsMine) return;
        Debug.Log("JUMP");
        jumpSource.clip = jumpingSounds[0];
        jumpSource.Play();
        //JumpImpulse();
        StartCoroutine(JumpRoutine());
    }

    IEnumerator JumpRoutine() {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float timer = 0.5f;

        // Do it once so we are off the ground
        Vector3 movement = transform.forward * v + transform.right * h;
        movement.y += JumpHeight;
        characterController.Move(movement * Time.deltaTime * JumpSpeed);

        while (timer > 0f && !characterController.isGrounded) {
            movement = transform.forward * v + transform.right * h;
            movement.y += JumpHeight;
            characterController.Move(movement * Time.deltaTime * JumpSpeed);

            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (!characterController.isGrounded) {
            movement = transform.forward * v + transform.right * h;
            characterController.Move(movement * Time.deltaTime * JumpSpeed);
            yield return new WaitForEndOfFrame();
        }
        jumpSource.clip = jumpingSounds[1];
        jumpSource.Play();
    }

    public void EndJump() {
        jumping = false;
    }

    void JumpImpulse() {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 movement = transform.forward * v + transform.right * h + transform.up;
        AddImpact(movement * JumpHeight, JumpSpeed, true);
    }

    public void Displace(Vector3 direction, float distance, float speed, bool isWorldSpaceDirection) {
        // TODO: Animation system
        if (isRooted || isBeingDisplaced) return;
        AddImpact(direction, distance * speed, isWorldSpaceDirection);
        // StartCoroutine(DisplacementRoutine(direction, distance, speed, isWorldSpaceDirection));
    }

    // DEPRECATED
    IEnumerator DisplacementRoutine(Vector3 direction, float distance, float speed, bool isWorldSpaceDirection) {
        isBeingDisplaced = true;
        float timer = distance / speed;
        while (timer > 0f) {
            Vector3 movement = !isWorldSpaceDirection
                ? transform.forward * direction.z + transform.right * direction.x + Vector3.up * direction.y
                : direction;
            characterController.Move(movement * Time.deltaTime * speed);

            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isBeingDisplaced = false;
    }

    // call this function to add an impact force:
    public void AddImpact(Vector3 direction, float forceValue, bool isWorldSpaceDirection = false) {
        // direction.Normalize();
        if (direction.y < 0) direction.y = -direction.y; // reflect down force on the ground
        Vector3 movement = !isWorldSpaceDirection
                ? transform.forward * direction.z + transform.right * direction.x + Vector3.up * direction.y
                : direction;
        impact += movement * forceValue / Mass;
    }

    public void ChangeMovementSpeed(float multiplier) {
        movementSpeed *= multiplier;
    }

    public void ResetMovementSpeed() {
        movementSpeed = PlayerSpeed;
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