using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;

using System.Collections;

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [Tooltip("The current Health of our player")]
    public float Health = 100f;

    [Tooltip("Where spells witll spawn from when being cast forwards")]
    public Transform frontCastingAnchor;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    private Animator animator;
    private string currentSpellCast = "";
    private Transform currentCastingTransform;
    private PlayerMovementManager movementManager;
    private HealthBar healthBar;
    private Crosshair crosshair;


    /* ----------------- STATUS EFFECTS ---------------------- */

    // Increase or decrease movement speed
    private bool slowed;
    private float slowedDuration, slowedPercentage = 0f;
    private bool hastened;
    private float hastenedDuration, hastenedPercentage = 0f;

    // Prevent all movement, including movement spells
    private bool rooted;
    private float rootedDuration;

    // Prevent all spellcasts
    private bool silenced;
    private float silencedDuration;

    // Prevent all actions
    private bool stunned;
    private float stunnedDuration;

    // Lower or Raise Damage/Health of spells
    private bool weakened;
    private float weakenedDuration, weakenedPercentage = 0f;
    private bool strengthened;
    private float strengthenedDuration, strengthenedPercentage = 0f;

    // Increase or decrease the amount of damage taken
    private bool fragile;
    private float fragileDuration, fragilePercentage = 0f;
    private bool tough;
    private float toughDuration, toughPercentage = 0f;




    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            stream.SendNext(Health);
        } else {
            // Network player, receive data
            this.Health = (float)stream.ReceiveNext();
        }
    }

    void Start() {
        // Follow the player character with the camera
        CustomCameraWork _cameraWork = this.gameObject.GetComponent<CustomCameraWork>();
        if (_cameraWork != null) {
            if (photonView.IsMine){
                _cameraWork.OnStartFollowing();
            }
        } else {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        // Get animator for casting
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        // Get movement manager
        movementManager = GetComponent<PlayerMovementManager>();

        healthBar = Object.FindObjectOfType(typeof(HealthBar)) as HealthBar;

        crosshair = Object.FindObjectOfType(typeof(Crosshair)) as Crosshair;
    }

    void Awake() {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine) {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    void Update() {
        if (photonView.IsMine) {
            this.ProcessInputs();
            if (Health <= 0f) {
                GameManager.Instance.LeaveRoom();
            }
        }
    }



    /* ------------------------ SPELL COLLISION HANDLING ----------------------- */

    [PunRPC]
    void OnSpellCollide(float Damage, string ManaDamageType, string SpellEffectType, float Duration) {
        if (!photonView.IsMine) return;
        Debug.Log("Collision with spell of type: "+ManaDamageType);
        // Spell spell = spellGO.GetComponent<Spell>();
        switch (SpellEffectType) {
            case "dotprojectile":
                StartCoroutine(TakeDirectDoTDamage(Damage, ManaDamageType, Duration));
                break;
            default:
                Debug.Log("Default Spell effect --> Take direct damage");
                TakeDamage(Damage);
                break;
        }

        healthBar.SetHealth(Health);
        Debug.Log("Current Health: "+Health);
    }

    void TakeDamage(float damage) {
        if (fragile) damage *= (1+fragilePercentage);
        if (tough) damage *= (1-toughPercentage);
        Health -= damage;
    }

    IEnumerator TakeDirectDoTDamage(float damage, string damageType, float duration){
        float damagePerSecond = damage / duration;
        while (duration > 0f) {
            TakeDamage(damagePerSecond * Time.deltaTime);
            duration -= Time.deltaTime;
            healthBar.SetHealth(Health);
            yield return new WaitForFixedUpdate();
        }
    }






    /*  --------------------  SPELLCASTING ------------------------ */

    void ProcessInputs() {
        if (Input.GetKeyDown("1")) {
            StartCastFireball();
        } else if (Input.GetKeyDown("2")) {
            StartCastShadeSmoke();
        } else if (Input.GetKeyDown("3")) {
            StartCastArcaneThrow();
        } else if (Input.GetKeyDown("4")) {
            StartCastCondense();
        }
    }

    void TurnCastingAnchorDirectionToAimPoint() {
        Vector3 aimPoint = crosshair.GetWorldPoint();
        currentCastingTransform.LookAt(aimPoint);
    }

    Vector3 GetCrosshairAimPoint() {
        return crosshair.GetWorldPoint();
    }







    /*  --------------------  SPELLS ------------------------ */

    void StartCastFireball() {
        currentSpellCast = "Spell_Fireball";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(1);
    }

    void StartCastShadeSmoke() {
        currentSpellCast = "Spell_ShadeSmoke";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(1);
    }

    void StartCastArcaneThrow() {
        currentSpellCast = "Spell_ArcaneThrow";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(1);
    }

    void StartCastCondense() {
        currentSpellCast = "Spell_Condense";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(1);
    }

    void CastSpell() {
        if (photonView.IsMine && !silenced) {
            PhotonNetwork.Instantiate(currentSpellCast, currentCastingTransform.position, currentCastingTransform.rotation);
        }
    }






    /*  --------------------  STATUS EFFECTS ------------------------ */

    // Slow + Hasten
    [PunRPC]
    void Slow(float duration, float percentage) {
        if (photonView.IsMine && !slowed) {
            slowed = true;
            StartCoroutine(SlowRoutine(duration, percentage));
        }
    }
    IEnumerator SlowRoutine(float duration, float percentage) {
        float startSpeed = animator.speed;
        animator.speed *= 1f - percentage;
        yield return new WaitForSeconds(duration);
        animator.speed = startSpeed;
        slowed = false;
    }


    [PunRPC]
    void Hasten(float duration, float percentage) {
        if (photonView.IsMine && !hastened) {
            hastened = true;
            StartCoroutine(HastenRoutine(duration, percentage));
        }
    }
    IEnumerator HastenRoutine(float duration, float percentage) {
        float startSpeed = animator.speed;
        animator.speed *= 1f + percentage;
        yield return new WaitForSeconds(duration);
        animator.speed = startSpeed;
        hastened = false;
    }
}