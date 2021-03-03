// // based on Unity's FirstPersonController & ThirdPersonController scripts
// using UnityEngine;
// using Random = UnityEngine.Random;

// // MoveState as byte for minimal bandwidth (otherwise it's int by default)
// // note: distinction between WALKING and RUNNING in case we need to know the
// //       difference somewhere (e.g. for endurance recovery)
// public enum MoveState : byte {IDLE, WALKING, RUNNING, CROUCHING, CRAWLING, AIRBORNE, CLIMBING, SWIMMING, MOUNTED, MOUNTED_AIRBORNE, MOUNTED_SLIDING, MOUNTED_SWIMMING, DEAD}

// [RequireComponent(typeof(CharacterController2k))]
// [RequireComponent(typeof(AudioSource))]
// public class PlayerMovement : MonoBehaviour
// {
//     // components to be assigned in inspector
//     [Header("Components")]
//     public Animator animator;
//     public AudioSource feetAudio;
//     public Combat combat;
//     public PlayerLook look;
//     // the collider for the character controller. NOT the hips collider. this
//     // one is NOT affected by animations and generally a better choice for state
//     // machine logic.
//     public CapsuleCollider controllerCollider;
// #pragma warning disable CS0109 // member does not hide accessible member
//     new Camera camera;
// #pragma warning restore CS0109 // member does not hide accessible member

//     [Header("State")]
//     public MoveState state = MoveState.IDLE;
//     [HideInInspector] public Vector3 moveDir;

//     // it's useful to have both strafe movement (WASD) and rotations (QE)
//     // => like in WoW, it more fun to play this way.
//     [Header("Rotation")]
//     public float rotationSpeed = 150;

//     [Header("Walking")]
//     public float walkSpeed = 5;
//     public float walkAcceleration = 15; // set to maxint for instant speed
//     public float walkDeceleration = 20; // feels best if higher than acceleration

//     [Header("Running")]
//     public float runSpeed = 8;
//     [Range(0f, 1f)] public float runStepLength = 0.7f;
//     public float runStepInterval = 3;
//     public float runCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
//     public KeyCode runKey = KeyCode.LeftShift;
//     float stepCycle;
//     float nextStep;

//     [Header("Crouching")]
//     public float crouchSpeed = 1.5f;
//     public KeyCode crouchKey = KeyCode.C;
//     bool crouchKeyPressed;

//     [Header("Crawling")]
//     public float crawlSpeed = 1;
//     public KeyCode crawlKey = KeyCode.V;
//     bool crawlKeyPressed;

//     [Header("Swimming")]
//     public float swimSpeed = 4;
//     public float swimSurfaceOffset = 0.25f;
//     Collider waterCollider;
//     bool inWater => waterCollider != null; // standing in water / touching it?
//     bool underWater; // deep enough in water so we need to swim?
//     public LayerMask canStandInWaterCheckLayers = Physics.DefaultRaycastLayers; // set this to everything except water layer
//     [Header("Swimming [CAREFUL]")]
//     [Tooltip("Percentage of body that needs to be underwater to start swimming. Change this carefully and make sure to test it both on foot and mounted. Higher values might work well on foot, but mounted it would stay underwater for way too long.")]
//     [Range(0, 1)] public float underwaterThreshold = 0.7f; // percent of body that need to be underwater to start swimming

//     [Header("Jumping")]
//     public float jumpSpeed = 7;
//     [HideInInspector] public float jumpLeg;
//     bool jumpKeyPressed;

//     [Header("Falling")]
//     public float fallMinimumMagnitude = 9; // walking down steps shouldn't count as falling and play no falling sound.
//     public float fallDamageMinimumMagnitude = 13;
//     public float fallDamageMultiplier = 2;
//     [HideInInspector] public Vector3 lastFall;
//     bool sprintingBeforeAirborne; // don't allow sprint key to accelerate while jumping. decision has to be made before that.

//     [Header("Climbing")]
//     public float climbSpeed = 3;
//     Collider ladderCollider;

//     [Header("Mounted")]
//     public float mountedRotationSpeed = 100;

//     [Header("Physics")]
//     public float gravityMultiplier = 2;

//     // helper property to check grounded with some tolerance. technically we
//     // aren't grounded when walking down steps, but this way we factor in a
//     // minimum fall magnitude. useful for more tolerant jumping etc.
//     // (= while grounded or while velocity not smaller than min fall yet)
//     public bool isGroundedWithinTolerance =>
//         controller.isGrounded || controller.velocity.y > -fallMinimumMagnitude;

//     [Header("Sounds")]
//     public AudioClip[] footstepSounds;    // an array of footstep sounds that will be randomly selected from.
//     public AudioClip jumpSound;           // the sound played when character leaves the ground.
//     public AudioClip landSound;           // the sound played when character touches back on ground.

//     void Awake()
//     {
//         camera = Camera.main;
//     }

//     // input directions ////////////////////////////////////////////////////////
//     Vector2 GetInputDirection()
//     {
//         // get input direction while alive and while not typing in chat
//         // (otherwise 0 so we keep falling even if we die while jumping etc.)
//         float horizontal = 0;
//         float vertical = 0;
//         if (!UIUtils.AnyInputActive())
//         {
//             horizontal = Input.GetAxis("Horizontal");
//             vertical = Input.GetAxis("Vertical");
//         }

//         // normalize ONLY IF needs to be normalized (if length>1).
//         // we use GetAxis instead of GetAxisRaw, so we may get vectors like
//         // (0.1, 0). if we normalized this in all cases, then it would always be
//         // (1, 0) even if we slowly push the controller forward.
//         Vector2 input = new Vector2(horizontal, vertical);
//         if (input.magnitude > 1)
//         {
//             input = input.normalized;
//         }
//         return input;
//     }

//     Vector3 GetDesiredDirection(Vector2 inputDir)
//     {
//         // always move along the camera forward as it is the direction that is being aimed at
//         return transform.forward * inputDir.y + transform.right * inputDir.x;
//     }

//     // movement state machine //////////////////////////////////////////////////
//     bool EventDied()
//     {
//         return health.current == 0;
//     }

//     bool EventJumpRequested()
//     {
//         // only while grounded, so jump key while jumping doesn't start a new
//         // jump immediately after landing
//         // => and not while sliding, otherwise we could climb slides by jumping
//         // => not even while SlidingState.Starting, so we aren't able to avoid
//         //    sliding by bunny hopping.
//         // => grounded check uses min fall tolerance so we can actually still
//         //    jump when walking down steps.
//         return isGroundedWithinTolerance &&
//                controller.slidingState == SlidingState.NONE &&
//                jumpKeyPressed;
//     }

//     bool EventCrouchToggle()
//     {
//         return crouchKeyPressed;
//     }

//     bool EventCrawlToggle()
//     {
//         return crawlKeyPressed;
//     }

//     bool EventFalling()
//     {
//         // use minimum fall magnitude so walking down steps isn't detected as
//         // falling! otherwise walking down steps would show the fall animation
//         // and play the landing sound.
//         return !isGroundedWithinTolerance;
//     }

//     bool EventLanded()
//     {
//         return controller.isGrounded;
//     }

//     bool EventMounted()
//     {
//         return mountUsage.IsMounted();
//     }

//     bool EventDismounted()
//     {
//         return !mountUsage.IsMounted();
//     }

//     bool EventUnderWater()
//     {
//         // we can't really make it player position dependent, because he might
//         // swim to the surface at which point it might be detected as standing
//         // in water but not being under water, etc.
//         if (inWater) // in water and valid water collider?
//         {
//             // raycasting from water to the bottom at the position of the player
//             // seems like a very precise solution
//             Vector3 origin = new Vector3(transform.position.x,
//                                          waterCollider.bounds.max.y,
//                                          transform.position.z);
//             float distance = controllerCollider.height * underwaterThreshold;
//             Debug.DrawLine(origin, origin + Vector3.down * distance, Color.cyan);

//             // we are underwater if the raycast doesn't hit anything
//             return !Utils.RaycastWithout(origin, Vector3.down, out RaycastHit hit, distance, gameObject, canStandInWaterCheckLayers);
//         }
//         return false;
//     }

//     bool EventLadderEnter()
//     {
//         return ladderCollider != null;
//     }

//     bool EventLadderExit()
//     {
//         // OnTriggerExit isn't good enough to detect ladder exits because we
//         // shouldn't exit as soon as our head sticks out of the ladder collider.
//         // only if we fully left it. so check this manually here:
//         return ladderCollider != null &&
//                !ladderCollider.bounds.Intersects(controllerCollider.bounds);
//     }

//     // helper function to apply gravity based on previous Y direction
//     float ApplyGravity(float moveDirY)
//     {
//         // apply full gravity while falling
//         if (!controller.isGrounded)
//             // gravity needs to be * Time.fixedDeltaTime even though we multiply
//             // the final controller.Move * Time.fixedDeltaTime too, because the
//             // unit is 9.81m/s²
//             return moveDirY + Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
//         // if grounded then apply no force. the new OpenCharacterController
//         // doesn't need a ground stick force. it would only make the character
//         // slide on all uneven surfaces.
//         return 0;
//     }

//     // helper function to get move or walk speed depending on key press & endurance
//     float GetWalkOrRunSpeed()
//     {
//         bool runRequested = !UIUtils.AnyInputActive() && Input.GetKey(runKey);
//         return runRequested && endurance.current > 0 ? runSpeed : walkSpeed;
//     }

//     void ApplyFallDamage()
//     {
//         // measure only the Y direction. we don't want to take fall damage
//         // if we jump forward into a wall because xz is high.
//         float fallMagnitude = Mathf.Abs(lastFall.y);
//         if(fallMagnitude >= fallDamageMinimumMagnitude)
//         {
//             int damage = Mathf.RoundToInt(fallMagnitude * fallDamageMultiplier);
//             health.current -= damage;
//             combat.ShowDamageEffect(damage, transform.position, -lastFall);
//         }
//     }

//     // acceleration can be different when accelerating/decelerating
//     float AccelerateSpeed(Vector2 inputDir, float currentSpeed, float targetSpeed, float acceleration)
//     {
//         // desired speed is between 'speed' and '0'
//         float desiredSpeed = inputDir.magnitude * targetSpeed;

//         // accelerate speed
//         return Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
//     }

//     // rotate with QE keys
//     void RotateWithKeys()
//     {
//         if (!UIUtils.AnyInputActive())
//         {
//             float horizontal2 = Input.GetAxis("Horizontal2");
//             transform.Rotate(Vector3.up * horizontal2 * rotationSpeed * Time.fixedDeltaTime);
//         }
//     }

//     void EnterLadder()
//     {
//         // make player look directly at ladder forward. but we also initialize
//         // freelook manually already to overwrite the initial rotation, so
//         // that in the end, the camera keeps looking at the same angle even
//         // though we did modify transform.forward.
//         // note: even though we set the rotation perfectly here, there's
//         //       still one frame where it seems to interpolate between the
//         //       new and the old rotation, which causes 1 odd camera frame.
//         //       this could be avoided by overwriting transform.forward once
//         //       more in LateUpdate.
//         look.InitializeFreeLook();
//         transform.forward = ladderCollider.transform.forward;
//     }

//     MoveState UpdateIDLE(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // QE key rotation
//         RotateWithKeys();

//         // move
//         // (moveDir.xz can be set to 0 to have an interruption when landing)
//         moveDir.x = desiredDir.x * walkSpeed;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = desiredDir.z * walkSpeed;

//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         else if (EventFalling())
//         {
//             sprintingBeforeAirborne = false;
//             return MoveState.AIRBORNE;
//         }
//         else if (EventJumpRequested())
//         {
//             // start the jump movement into Y dir, go to jumping
//             // note: no endurance>0 check because it feels odd if we can't jump
//             moveDir.y = jumpSpeed;
//             sprintingBeforeAirborne = false;
//             PlayJumpSound();
//             return MoveState.AIRBORNE;
//         }
//         else if (EventMounted())
//         {
//             return MoveState.MOUNTED;
//         }
//         else if (EventCrouchToggle())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
//             {
//                 return MoveState.CROUCHING;
//             }
//         }
//         else if (EventCrawlToggle())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.CRAWLING;
//             }
//         }
//         else if (EventLadderEnter())
//         {
//             EnterLadder();
//             return MoveState.CLIMBING;
//         }
//         else if (EventUnderWater())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.SWIMMING;
//             }
//         }
//         else if (inputDir != Vector2.zero)
//         {
//             return MoveState.WALKING;
//         }
//         else if (EventDismounted()) {} // don't care

//         return MoveState.IDLE;
//     }

//     MoveState UpdateWALKINGandRUNNING(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // QE key rotation
//         RotateWithKeys();

//         // walk or run?
//         float speed = GetWalkOrRunSpeed();

//         // move
//         moveDir.x = desiredDir.x * speed;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = desiredDir.z * speed;

//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         else if (EventFalling())
//         {
//             sprintingBeforeAirborne = speed == runSpeed;
//             return MoveState.AIRBORNE;
//         }
//         else if (EventJumpRequested())
//         {
//             // start the jump movement into Y dir, go to jumping
//             // note: no endurance>0 check because it feels odd if we can't jump
//             moveDir.y = jumpSpeed;
//             sprintingBeforeAirborne = speed == runSpeed;
//             PlayJumpSound();
//             return MoveState.AIRBORNE;
//         }
//         else if (EventMounted())
//         {
//             return MoveState.MOUNTED;
//         }
//         else if (EventCrouchToggle())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
//             {
//                 return MoveState.CROUCHING;
//             }
//         }
//         else if (EventCrawlToggle())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.CRAWLING;
//             }
//         }
//         else if (EventLadderEnter())
//         {
//             EnterLadder();
//             return MoveState.CLIMBING;
//         }
//         else if (EventUnderWater())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.SWIMMING;
//             }
//         }
//         // go to idle after fully decelerating (y doesn't matter)
//         else if (moveDir.x == 0 && moveDir.z == 0)
//         {
//             return MoveState.IDLE;
//         }
//         else if (EventDismounted()) {} // don't care

//         ProgressStepCycle(inputDir, speed);
//         return speed == walkSpeed ? MoveState.WALKING : MoveState.RUNNING;
//     }

//     MoveState UpdateCROUCHING(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // QE key rotation
//         RotateWithKeys();

//         // move
//         moveDir.x = desiredDir.x * crouchSpeed;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = desiredDir.z * crouchSpeed;

//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         else if (EventFalling())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 sprintingBeforeAirborne = false;
//                 return MoveState.AIRBORNE;
//             }
//         }
//         else if (EventJumpRequested())
//         {
//             // stop crouching when pressing jump key. this feels better than
//             // jumping from the crouching state.

//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 return MoveState.IDLE;
//             }
//         }
//         else if (EventMounted())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 return MoveState.MOUNTED;
//             }
//         }
//         else if (EventCrouchToggle())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 return MoveState.IDLE;
//             }
//         }
//         else if (EventCrawlToggle())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.CRAWLING;
//             }
//         }
//         else if (EventLadderEnter())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 EnterLadder();
//                 return MoveState.CLIMBING;
//             }
//         }
//         else if (EventUnderWater())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.SWIMMING;
//             }
//         }
//         else if (EventDismounted()) {} // don't care

//         ProgressStepCycle(inputDir, crouchSpeed);
//         return MoveState.CROUCHING;
//     }

//     MoveState UpdateCRAWLING(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // QE key rotation
//         RotateWithKeys();

//         // move
//         moveDir.x = desiredDir.x * crawlSpeed;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = desiredDir.z * crawlSpeed;

//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         else if (EventFalling())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 sprintingBeforeAirborne = false;
//                 return MoveState.AIRBORNE;
//             }
//         }
//         else if (EventJumpRequested())
//         {
//             // stop crawling when pressing jump key. this feels better than
//             // jumping from the crawling state.

//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 return MoveState.IDLE;
//             }
//         }
//         else if (EventMounted())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 return MoveState.MOUNTED;
//             }
//         }
//         else if (EventCrouchToggle())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
//             {
//                 return MoveState.CROUCHING;
//             }
//         }
//         else if (EventCrawlToggle())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 return MoveState.IDLE;
//             }
//         }
//         else if (EventLadderEnter())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 EnterLadder();
//                 return MoveState.CLIMBING;
//             }
//         }
//         else if (EventUnderWater())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.SWIMMING;
//             }
//         }
//         else if (EventDismounted()) {} // don't care

//         ProgressStepCycle(inputDir, crawlSpeed);
//         return MoveState.CRAWLING;
//     }

//     MoveState UpdateAIRBORNE(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // QE key rotation
//         RotateWithKeys();

//         // max speed depends on what we did before jumping/falling
//         float speed = sprintingBeforeAirborne ? runSpeed : walkSpeed;

//         // move
//         moveDir.x = desiredDir.x * speed;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = desiredDir.z * speed;

//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         else if (EventLanded())
//         {
//             // apply fall damage only in AIRBORNE->Landed.
//             // (e.g. not if we run face forward into a wall with high velocity)
//             ApplyFallDamage();
//             PlayLandingSound();
//             return MoveState.IDLE;
//         }
//         else if (EventLadderEnter())
//         {
//             EnterLadder();
//             return MoveState.CLIMBING;
//         }
//         else if (EventUnderWater())
//         {
//             // rescale capsule
//             if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
//             {
//                 return MoveState.SWIMMING;
//             }
//         }

//         return MoveState.AIRBORNE;
//     }

//     MoveState UpdateCLIMBING(Vector2 inputDir, Vector3 desiredDir)
//     {
//         if (EventDied())
//         {
//             // player rotation was adjusted to ladder rotation before.
//             // let's reset it, but also keep look forward
//             transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
//             ladderCollider = null;
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         // finished climbing?
//         else if (EventLadderExit())
//         {
//             // player rotation was adjusted to ladder rotation before.
//             // let's reset it, but also keep look forward
//             transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
//             ladderCollider = null;
//             return MoveState.IDLE;
//         }

//         // interpret forward/backward movement as upward/downward
//         // note: NO ACCELERATION, otherwise we would climb really fast when
//         //       sprinting towards a ladder. and the actual climb feels way too
//         //       unresponsive when accelerating.
//         moveDir.x = inputDir.x * climbSpeed;
//         moveDir.y = inputDir.y * climbSpeed;
//         moveDir.z = 0;

//         // make the direction relative to ladder rotation. so when pressing right
//         // we always climb to the right of the ladder, no matter how it's rotated
//         moveDir = ladderCollider.transform.rotation * moveDir;
//         Debug.DrawLine(transform.position, transform.position + moveDir, Color.yellow, 0.1f, false);

//         return MoveState.CLIMBING;
//     }

//     MoveState UpdateSWIMMING(Vector2 inputDir, Vector3 desiredDir)
//     {
//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         // ladder under / above water?
//         else if (EventLadderEnter())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 EnterLadder();
//                 return MoveState.CLIMBING;
//             }
//         }
//         // not under water anymore?
//         else if (!EventUnderWater())
//         {
//             // rescale capsule if possible
//             if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
//             {
//                 return MoveState.IDLE;
//             }
//         }

//         // QE key rotation
//         RotateWithKeys();

//         // move
//         moveDir.x = desiredDir.x * swimSpeed;
//         moveDir.z = desiredDir.z * swimSpeed;

//         // gravitate toward surface
//         if (waterCollider != null)
//         {
//             float surface = waterCollider.bounds.max.y;
//             float surfaceDirection = surface - controller.bounds.min.y - swimSurfaceOffset;
//             moveDir.y = surfaceDirection * swimSpeed;
//         }
//         else moveDir.y = 0;

//         return MoveState.SWIMMING;
//     }

//     MoveState UpdateMOUNTED(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // walk or run?
//         float speed = GetWalkOrRunSpeed();

//         // recalculate desired direction while ignoring inputDir horizontal part
//         // (horses can't strafe.)
//         desiredDir = GetDesiredDirection(new Vector2(0, inputDir.y));

//         // horizontal input axis rotates the character instead of strafing
//         transform.Rotate(Vector3.up * inputDir.x * mountedRotationSpeed * Time.fixedDeltaTime);

//         // move
//         moveDir.x = desiredDir.x * speed;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = desiredDir.z * speed;

//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         else if (EventFalling())
//         {
//             sprintingBeforeAirborne = speed == runSpeed;
//             return MoveState.MOUNTED_AIRBORNE;
//         }
//         else if (EventJumpRequested())
//         {
//             // start the jump movement into Y dir, go to jumping
//             // note: no endurance>0 check because it feels odd if we can't jump
//             moveDir.y = jumpSpeed;
//             sprintingBeforeAirborne = speed == runSpeed;
//             PlayJumpSound();
//             return MoveState.MOUNTED_AIRBORNE;
//         }
//         else if (EventDismounted())
//         {
//             return MoveState.IDLE;
//         }
//         else if (EventUnderWater())
//         {
//             return MoveState.MOUNTED_SWIMMING;
//         }
//         else if (EventMounted()) {} // don't care

//         return MoveState.MOUNTED;
//     }

//     MoveState UpdateMOUNTED_AIRBORNE(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // max speed depends on what we did before jumping/falling
//         float speed = sprintingBeforeAirborne ? runSpeed : walkSpeed;

//         // recalculate desired direction while ignoring inputDir horizontal part
//         // (horses can't strafe.)
//         desiredDir = GetDesiredDirection(new Vector2(0, inputDir.y));

//         // horizontal input axis rotates the character instead of strafing
//         transform.Rotate(Vector3.up * inputDir.x * mountedRotationSpeed * Time.fixedDeltaTime);

//         // move with acceleration (feels better)
//         moveDir.x = desiredDir.x * speed;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = desiredDir.z * speed;

//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         else if (EventLanded())
//         {
//             // apply fall damage only in AIRBORNE->Landed.
//             // (e.g. not if we run face forward into a wall with high velocity)
//             ApplyFallDamage();
//             PlayLandingSound();
//             return MoveState.MOUNTED;
//         }
//         else if (EventUnderWater())
//         {
//             return MoveState.MOUNTED_SWIMMING;
//         }

//         return MoveState.MOUNTED_AIRBORNE;
//     }

//     MoveState UpdateMOUNTED_SWIMMING(Vector2 inputDir, Vector3 desiredDir)
//     {
//         if (EventDied())
//         {
//             // rescale capsule
//             controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false);
//             // DEAD in any case, even if rescale failed
//             return MoveState.DEAD;
//         }
//         // not under water anymore?
//         else if (!EventUnderWater())
//         {
//             return MoveState.MOUNTED;
//         }
//         // dismounted while swimming?
//         else if (EventDismounted())
//         {
//             return MoveState.SWIMMING;
//         }

//         // recalculate desired direction while ignoring inputDir horizontal part
//         // (horses can't strafe.)
//         desiredDir = GetDesiredDirection(new Vector2(0, inputDir.y));

//         // horizontal input axis rotates the character instead of strafing
//         transform.Rotate(Vector3.up * inputDir.x * mountedRotationSpeed * Time.fixedDeltaTime);

//         // move with acceleration (feels better)
//         moveDir.x = desiredDir.x * swimSpeed;
//         moveDir.z = desiredDir.z * swimSpeed;

//         // gravitate toward surface
//         if (waterCollider != null)
//         {
//             float surface = waterCollider.bounds.max.y;
//             float surfaceDirection = surface - controller.bounds.min.y + mountUsage.seatOffsetY - swimSurfaceOffset;
//             moveDir.y = surfaceDirection * swimSpeed;
//         }
//         else moveDir.y = 0;

//         return MoveState.MOUNTED_SWIMMING;
//     }

//     MoveState UpdateDEAD(Vector2 inputDir, Vector3 desiredDir)
//     {
//         // keep falling while dead: if we get shot while falling, we shouldn't
//         // stop in mid air
//         moveDir.x = 0;
//         moveDir.y = ApplyGravity(moveDir.y);
//         moveDir.z = 0;

//         // not dead anymore?
//         if (health.current > 0)
//         {
//             // rescale capsule in any case. we don't check CanSetHeight so that
//             // no other entities can block a respawn. SetHeight will depenetrate
//             // either way.
//             controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false);
//             return MoveState.IDLE;
//         }
//         return MoveState.DEAD;
//     }

//     // use Update to check Input
//     void Update()
//     {
//         // not while typing in UI etc.
//         if (!UIUtils.AnyInputActive())
//         {
//             if (!jumpKeyPressed) jumpKeyPressed = Input.GetButtonDown("Jump");
//             if (!crawlKeyPressed) crawlKeyPressed = Input.GetKeyDown(crawlKey);
//             if (!crouchKeyPressed) crouchKeyPressed = Input.GetKeyDown(crouchKey);
//         }
//     }

//     // CharacterController movement is physics based and requires FixedUpdate.
//     // (using Update causes strange movement speeds in builds otherwise)
//     void FixedUpdate()
//     {
//         // get input and desired direction based on camera and ground
//         Vector2 inputDir = GetInputDirection();
//         Vector3 desiredDir = GetDesiredDirection(inputDir);
//         Debug.DrawLine(transform.position, transform.position + desiredDir, Color.cyan);

//         // update state machine
//         if (state == MoveState.IDLE)                  state = UpdateIDLE(inputDir, desiredDir);
//         else if (state == MoveState.WALKING)          state = UpdateWALKINGandRUNNING(inputDir, desiredDir);
//         else if (state == MoveState.RUNNING)          state = UpdateWALKINGandRUNNING(inputDir, desiredDir);
//         else if (state == MoveState.CROUCHING)        state = UpdateCROUCHING(inputDir, desiredDir);
//         else if (state == MoveState.CRAWLING)         state = UpdateCRAWLING(inputDir, desiredDir);
//         else if (state == MoveState.AIRBORNE)         state = UpdateAIRBORNE(inputDir, desiredDir);
//         else if (state == MoveState.CLIMBING)         state = UpdateCLIMBING(inputDir, desiredDir);
//         else if (state == MoveState.SWIMMING)         state = UpdateSWIMMING(inputDir, desiredDir);
//         else if (state == MoveState.MOUNTED)          state = UpdateMOUNTED(inputDir, desiredDir);
//         else if (state == MoveState.MOUNTED_AIRBORNE) state = UpdateMOUNTED_AIRBORNE(inputDir, desiredDir);
//         else if (state == MoveState.MOUNTED_SWIMMING) state = UpdateMOUNTED_SWIMMING(inputDir, desiredDir);
//         else if (state == MoveState.DEAD)             state = UpdateDEAD(inputDir, desiredDir);
//         else Debug.LogError("Unhandled Movement State: " + state);

//         // cache this move's state to detect landing etc. next time
//         if (!controller.isGrounded) lastFall = controller.velocity;

//         // move depending on latest moveDir changes
//         controller.Move(moveDir * Time.fixedDeltaTime); // note: returns CollisionFlags if needed

//         // calculate which leg is behind, so as to leave that leg trailing in the jump animation
//         // (This code is reliant on the specific run cycle offset in our animations,
//         // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
//         float runCycle = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime + runCycleLegOffset, 1);
//         jumpLeg = (runCycle < 0.5f ? 1 : -1);// * move.z;

//         // reset keys no matter what
//         jumpKeyPressed = false;
//         crawlKeyPressed = false;
//         crouchKeyPressed = false;
//     }

//     void OnGUI()
//     {
//         // show data next to player for easier debugging. this is very useful!
//         if (Debug.isDebugBuild)
//         {
//             // project player position to screen
//             Vector3 center = controllerCollider.bounds.center;
//             Vector3 point = camera.WorldToScreenPoint(center);

//             // in front of camera and in screen?
//             if (point.z >= 0 && Utils.IsPointInScreen(point))
//             {
//                 GUI.color = new Color(1, 1, 1, 0.3f);
//                 GUILayout.BeginArea(new Rect(point.x, Screen.height - point.y, 150, 100));

//                 // some info for all players, including local
//                 GUILayout.Label("grounded=" + controller.isGrounded);
//                 GUILayout.Label("groundedT=" + isGroundedWithinTolerance);
//                 GUILayout.Label("lastFall=" + lastFall);
//                 GUILayout.Label("sliding=" + controller.slidingState);

//                 GUILayout.EndArea();
//                 GUI.color = Color.white;
//             }
//         }
//     }

//     void PlayLandingSound()
//     {
//         feetAudio.clip = landSound;
//         feetAudio.Play();
//         nextStep = stepCycle + .5f;
//     }

//     void PlayJumpSound()
//     {
//         feetAudio.clip = jumpSound;
//         feetAudio.Play();
//     }

//     void ProgressStepCycle(Vector3 inputDir, float speed)
//     {
//         if (controller.velocity.sqrMagnitude > 0 && (inputDir.x != 0 || inputDir.y != 0))
//         {
//             stepCycle += (controller.velocity.magnitude + (speed*(state == MoveState.WALKING ? 1 : runStepLength)))*
//                          Time.fixedDeltaTime;
//         }

//         if (stepCycle > nextStep)
//         {
//             nextStep = stepCycle + runStepInterval;
//             PlayFootStepAudio();
//         }
//     }

//     void PlayFootStepAudio()
//     {
//         if (!controller.isGrounded) return;

//         // pick & play a random footstep sound from the array,
//         // excluding sound at index 0
//         int n = Random.Range(1, footstepSounds.Length);
//         feetAudio.clip = footstepSounds[n];
//         feetAudio.PlayOneShot(feetAudio.clip);

//         // move picked sound to index 0 so it's not picked next time
//         footstepSounds[n] = footstepSounds[0];
//         footstepSounds[0] = feetAudio.clip;
//     }

//     void OnTriggerEnter(Collider co)
//     {
//         // touching ladder? then set ladder collider
//         if (co.CompareTag("Ladder"))
//             ladderCollider = co;
//         // touching water? then set water collider
//         else if (co.CompareTag("Water"))
//             waterCollider = co;
//     }

//     void OnTriggerExit(Collider co)
//     {
//         // not touching water anymore? then clear water collider
//         if (co.CompareTag("Water"))
//             waterCollider = null;
//     }
// }
