using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerMovementManager : MonoBehaviourPun, IPunObservable {
    public float PlayerSpeed = 1f, JumpHeight = 1f, JumpSpeed = 3f, JumpLiftTime = 0.3f, Mass = 3f, SlowFallAccelerantScaling = 0.1f;
    public Vector3 GroundBoostVector, JumpBoostVector;
    public float BoostDistance=200f, BoostSpeed=1f, AccelerationTime=1f;
    public AudioSource footStepSource;
    public AudioClip[] footsteps;
    public AudioSource jumpSource;
    public AudioClip[] jumpingSounds;
    public AudioSource boostSound;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    private Animator animator;
    private CharacterController characterController;
    private Dictionary<int, string> castAnimationTypes;
    private bool isRooted, isStunned, isChannelling, isBeingDisplaced, jumping, running = true, casting, slowFall, groundedStatusEffect;
    private Vector3 playerVelocity, impact, velocity;
    private float movementSpeed, slowFallPercent, gravity = 9.81f, appliedGravityForce = 0f;
    private float forwardsAcceleration = 0, sidewaysAcceleration = 0;
    private PlayerManager playerManager;
    private PlayerParticleManager particleManager;
    private CharacterMaterialManager materialManager;
    private GameUIPanelManager gameUIManager;

    Vector3 networkPosition;
    Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            // CRITICAL DATA
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(running);
        } else {
            // Network player, receive data
            // CRITICAL DATA
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            running = (bool)stream.ReceiveNext();

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

        playerManager = GetComponent<PlayerManager>();
        characterController = GetComponent<CharacterController>();
        particleManager = GetComponent<PlayerParticleManager>();
        materialManager = GetComponentInChildren<CharacterMaterialManager>();

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
        castAnimationTypes.Add(11, "2h Explosion Cast FAST");
    }

    // Update is called once per frame
    void Update() {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * movementSpeed * 3f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 1000f);

            if (!running) {
                playerManager.Sneak();
            } else {
                playerManager.EndSneak();
            }
            return;
        }
        if (!animator) return;
        if (gameUIManager == null) gameUIManager = GameUIPanelManager.Instance;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (InputManager.Instance.GetKeyDown(KeybindingActions.Jump) && !isRooted && !isStunned && !isChannelling && !isBeingDisplaced && !jumping && Grounded && !groundedStatusEffect) {
            animator.SetTrigger("Jump");
            jumping = true;
        }

        if (InputManager.Instance.GetKey(KeybindingActions.Sneak)) {
            if (running) {
                movementSpeed /= 3f;
                playerManager.Sneak();
            }
            running = false;
        } else {
            if (!running) {
                movementSpeed *= 3f;
                playerManager.EndSneak();
            }
            running = true;
        }

        animator.SetBool("Moving", (h != 0f || v != 0f) && !isRooted && !isChannelling && !isBeingDisplaced && !gameUIManager.IsEditingInputField());
        animator.SetBool("Running", running);
        animator.SetFloat("Forwards-Backwards", h);
        animator.SetFloat("Right-Left", v);
        GroundedCheck();

        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);

        // Apply motion after turning
        Vector3 oldPosition = transform.position;
        if (!isChannelling && !isRooted && !isStunned && !isBeingDisplaced && !gameUIManager.IsEditingInputField()) {
            if ((!casting || (slowFall && !Grounded))) {
                // Create momentum, speeding up if you move in the same direction
                if (Mathf.Approximately(v, 0f)){ 
                    forwardsAcceleration = Mathf.Max(forwardsAcceleration - Time.deltaTime * 2f, 0f);
                } else {
                    if (v*forwardsAcceleration <= 0.0f) forwardsAcceleration = 0f;
                    forwardsAcceleration = Mathf.Clamp(forwardsAcceleration + (v * Time.deltaTime * AccelerationTime), -0.5f, 0.5f);
                }
                if (Mathf.Approximately(h, 0f)){ 
                    sidewaysAcceleration = Mathf.Max(sidewaysAcceleration - Time.deltaTime * 2f, 0f);
                } else {
                    if (h*sidewaysAcceleration <= 0.0f) sidewaysAcceleration = 0f;
                    sidewaysAcceleration = Mathf.Clamp(sidewaysAcceleration + (h * Time.deltaTime * AccelerationTime), -0.5f, 0.5f);
                }            
                // Debug.Log("Forwards acc: "+forwardsAcceleration+"  sidways: "+sidewaysAcceleration+"   vec: "+Vector3.ClampMagnitude((transform.forward * v * forwardsAcceleration + transform.right * h * sidewaysAcceleration), 0.33f));
                characterController.Move((Vector3.ClampMagnitude((transform.forward * v + transform.right * h), 0.8f) + Vector3.ClampMagnitude((transform.forward * v * Mathf.Abs(forwardsAcceleration) + transform.right * h * Mathf.Abs(sidewaysAcceleration)), 0.5f)) * movementSpeed * Time.deltaTime * GameManager.GLOBAL_PLAYER_MOVEMENT_SPEED_MULTIPLIER);
            } else {
                forwardsAcceleration = 0f;
                sidewaysAcceleration = 0f;
                characterController.Move(Vector3.ClampMagnitude((transform.forward * v + transform.right * h), 0.7f) * movementSpeed * Time.deltaTime * GameManager.GLOBAL_PLAYER_MOVEMENT_SPEED_MULTIPLIER);
            }
        }

        // Apply impact force:
        if (impact.magnitude > 0.2) characterController.Move(impact * Time.deltaTime);

        // Consume the impact energy each cycle:
        float impactConsumptionMultiplier = (Grounded ? 5f : 2.5f) * (groundedStatusEffect ? 2f : 1f);
        impact = Vector3.Lerp(impact, Vector3.zero, impactConsumptionMultiplier * Time.deltaTime);

        // Apply gravity
        if (Grounded){
            appliedGravityForce = 0f;
        } else {
            float addedForce = gravity * Time.deltaTime;
            if (slowFall) addedForce *= Mathf.Clamp(1f/Mathf.Abs(appliedGravityForce), 0f, 1f) * Mathf.Max(1f - slowFallPercent, 0f);
            if (groundedStatusEffect) addedForce *= 3f;
            appliedGravityForce -= addedForce;
            characterController.Move(transform.up * appliedGravityForce * Time.deltaTime);
        }

        // Calculate velocity for lag compensation
        velocity = transform.position - oldPosition;
    }

    public void PlayCastingAnimation(int animationType) {
        // animator method
        casting = true;
        animator.SetBool("Cast", casting);
        animator.SetInteger("CastType", animationType);
        if (animationType != 8 && animationType != 12) animator.SetLayerWeight(1, 1f);;
    }

    public bool CanCast() {
        return !isStunned && !casting && (Grounded || slowFall) && !isBeingDisplaced && (!jumping || slowFall);
    }

    public void EndCast() {
        jumping = false;
        animator.SetBool("Cast", false);
        StartCoroutine(ResetCasting());
    }

    private IEnumerator ResetCasting() {
        float weight = 1f;
        while (weight > 0f) {
            weight -= Time.deltaTime * 5f;
            if (weight <= 0.05f) weight = 0f;
            animator.SetLayerWeight(1, weight);
            yield return new WaitForFixedUpdate();
        }
        casting = false;
    }

    public void Footstep() {
        // no footsteps if walking
        if (!running) return;

        footStepSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        footStepSource.Play ();
    }

    public void JumpLift() {
        if (!photonView.IsMine) return;
        jumpSource.clip = jumpingSounds[0];
        jumpSource.Play();
        // StartCoroutine(JumpRoutine());

        // Jump Displacement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        AddImpact(((transform.forward * v + transform.right * h).normalized / 3f) + (Vector3.up * JumpHeight), JumpSpeed, true);
    }

    public void JumpLand() {
        jumpSource.clip = jumpingSounds[1];
        jumpSource.Play();
        jumping = false;

        forwardsAcceleration = 0f;
        sidewaysAcceleration = 0f;
    }

    private void GroundedCheck() {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        animator.SetBool("FreeFall", !Grounded);
    }

    public void Boost() {
        Vector3 boostDirection = jumping ? JumpBoostVector : GroundBoostVector;
        Displace(boostDirection, BoostDistance, BoostSpeed, false);
        if (boostSound != null) boostSound.Play();
    }

    public void Displace(Vector3 direction, float distance, float speed, bool isWorldSpaceDirection) {
        // TODO: Animation system
        if (isBeingDisplaced || groundedStatusEffect) return;
        AddImpact(direction, distance * speed, isWorldSpaceDirection);
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
        movementSpeed = (running ? PlayerSpeed : PlayerSpeed / 3f) * multiplier;
    }

    public void ResetMovementSpeed() {
        movementSpeed = running ? PlayerSpeed : PlayerSpeed / 3f;
    }

    public void StartChannelling(int animationType, bool block = false) {
        isChannelling = true;
        animator.SetBool("Channelling", isChannelling);
        animator.SetBool("Blocking", block);

        if (!block) {
            PlayCastingAnimation(animationType);
        }
    }

    public void StopChannelling() {
        isChannelling = false;
        animator.SetBool("Channelling", isChannelling);
        animator.SetBool("Blocking", isChannelling);
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

    public void SlowFall(bool sf, float percentage = 0f) {
        slowFall = sf;
        if (percentage != 0f) slowFallPercent = percentage;
    }

    public void Ground(bool sf) {
        groundedStatusEffect = sf;
    }


    private void OnDrawGizmosSelected() {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;
        
        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}