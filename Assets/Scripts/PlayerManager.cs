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

    [Tooltip("The current Mana pool of our player")]
    public float Mana = 100f;

    [Tooltip("The rate at which Mana will regenerate (Mana/second)")]
    public float ManaRegen = 10;

    [Tooltip("Where spells witll spawn from when being cast forwards")]
    public Transform frontCastingAnchor;
    [Tooltip("Where spells witll spawn from when being cast upwards")]
    public Transform topCastingAnchor;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    private Animator animator;
    private string currentSpellCast = "", currentChannelledSpell= "";
    private Transform currentCastingTransform;
    private bool isChannelling = false;
    private GameObject channelledSpell;
    private PlayerMovementManager movementManager;
    private HealthBar healthBar, manaBar;
    private Crosshair crosshair;
    private float maxMana;


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

    // TODO: Lower or Raise Damage/Health of spells
    // private bool weakened;
    // private float weakenedDuration, weakenedPercentage = 0f;
    // private bool strengthened;
    // private float strengthenedDuration, strengthenedPercentage = 0f;

    // Increase or decrease the amount of damage taken
    private bool fragile;
    private float fragileDuration, fragilePercentage = 0f;
    private bool tough;
    private float toughDuration, toughPercentage = 0f;




    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            stream.SendNext(Health);
            stream.SendNext(Mana);
        } else {
            // Network player, receive data
            this.Health = (float)stream.ReceiveNext();
            this.Mana = (float)stream.ReceiveNext();
        }
    }

    void Start() {
        maxMana = Mana;

        // Follow the player character with the camera
        CustomCameraWork _cameraWork = this.gameObject.GetComponent<CustomCameraWork>();
        if (_cameraWork != null) {
            if (photonView.IsMine){
                _cameraWork.OnStartFollowing();
            }
        } else {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        if (PlayerUiPrefab != null) {
            GameObject _uiGo =  Instantiate(PlayerUiPrefab, transform.position+new Vector3(0f, 2f, 0f), transform.rotation);
            _uiGo.SendMessage ("SetTarget", this, SendMessageOptions.RequireReceiver);
            _uiGo.transform.SetParent(transform);
        } else {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

        // Get animator for casting
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        // Get movement manager
        movementManager = GetComponent<PlayerMovementManager>();

        healthBar = GameObject.Find("LocalHealthBar").GetComponent<HealthBar>();
        manaBar = GameObject.Find("LocalManaBar").GetComponent<HealthBar>();
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
            if (Mana < maxMana) {
                Mana += ManaRegen * Time.deltaTime;
                if (Mana > maxMana) Mana = maxMana;
            }
            manaBar.SetHealth(Mana);
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
        // Debug.Log("Current Health: "+Health);
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
        } else if (Input.GetKeyDown("5")) {
            StartCastAngelWisp();
        } else if (Input.GetKeyDown("6")) {
            StartCastSoulStrike();
        } else if (Input.GetKeyDown("7")) {
            StartCastAuricBolt();
        } else if (Input.GetKeyDown("8")) {
            StartCastEmberSphere();
        } else if (Input.GetKeyDown("9")) {
            StartCastEarthBound();
        } 
        
        if (Input.GetKey("e")) {
            if (!isChannelling) StartChannelAuricBarrier();
        } else {
            if (isChannelling && currentChannelledSpell == "Spell_AuricBarrier") StopBlocking();
        }

        if (Input.GetKey("q")) {
            if (!isChannelling) StartChannelForceShield();
        } else {
            if (isChannelling && currentChannelledSpell == "Spell_ForceShield") StopBlocking();
        }
    }

    void TurnCastingAnchorDirectionToAimPoint() {
        Vector3 aimPoint = crosshair.GetWorldPoint();
        currentCastingTransform.LookAt(aimPoint);
    }

    void ResetCastingAnchorDirection() {
        currentCastingTransform.rotation = transform.rotation;
    }

    Vector3 GetCrosshairAimPoint() {
        return crosshair.GetWorldPoint();
    }

    void StopBlocking() {
        movementManager.StopBlock();
        ChannelSpell(false);
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

    void StartCastAngelWisp() {
        currentSpellCast = "Spell_AngelWisp";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(10);
    }

    void StartCastSoulStrike() {
        currentSpellCast = "Spell_SoulStrike";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(1);
    }

    void StartCastAuricBolt() {
        currentSpellCast = "Spell_AuricBolt";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(1);
    }

    void StartCastEmberSphere() {
        currentSpellCast = "Spell_EmberSphere";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(2);
    }

    void StartCastEarthBound() {
        currentSpellCast = "Spell_EarthBound";
        currentCastingTransform = frontCastingAnchor;
        TurnCastingAnchorDirectionToAimPoint();
        movementManager.PlayCastingAnimation(10);
    }

    void StartChannelAuricBarrier() {
        currentChannelledSpell = "Spell_AuricBarrier";
        currentCastingTransform = frontCastingAnchor;
        ResetCastingAnchorDirection();
        ChannelSpell();
        movementManager.StartBlock();
    }

    void StartChannelForceShield() {
        currentChannelledSpell = "Spell_ForceShield";
        currentCastingTransform = transform;
        ResetCastingAnchorDirection();
        ChannelSpell();
        movementManager.StartBlock();
    }

    void CastSpell() {
        if (photonView.IsMine && currentSpellCast != null && !silenced && !stunned) {
            GameObject dataObject = Resources.Load<GameObject>(currentSpellCast);
            Debug.Log("Spell object grabbed: "+dataObject);
            GameObject newSpell = PhotonNetwork.Instantiate(currentSpellCast, currentCastingTransform.position, currentCastingTransform.rotation);
        }
        currentSpellCast = null;
    }

    void ChannelSpell(bool start = true) {
        if (photonView.IsMine) {
            if (start && !isChannelling && currentChannelledSpell != null && !silenced && !stunned) {
                isChannelling = true;
                channelledSpell = PhotonNetwork.Instantiate(currentChannelledSpell, currentCastingTransform.position, currentCastingTransform.rotation);
                channelledSpell.transform.SetParent(gameObject.transform);
            } else if ((!start && isChannelling) || silenced || stunned) {
                isChannelling = false;
                PhotonNetwork.Destroy(channelledSpell);
            }
        }
    }






    /*  --------------------  STATUS EFFECTS ------------------------ */

    // Slow - Decrease animation speed
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
        animator.speed = 1f * GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        slowed = false;
    }

    // Hasten - Increase animation speed
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
        animator.speed = 1f * GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        hastened = false;
    }

    // Rooted - Prevent movement, including movement spells
    [PunRPC]
    void Root(float duration) {
        if (photonView.IsMine && !rooted) {
            rooted = true;
            StartCoroutine(RootRoutine(duration));
        }
    }
    IEnumerator RootRoutine(float duration) {
        movementManager.Root(true);
        yield return new WaitForSeconds(duration);
        movementManager.Root(false);
        rooted = false;
    }

    // Stunned - Prevent all movement and spellcasting
    [PunRPC]
    void Stun(float duration) {
        if (photonView.IsMine && !stunned) {
            stunned = true;
            StartCoroutine(StunRoutine(duration));
        }
    }
    IEnumerator StunRoutine(float duration) {
        movementManager.Stun(true);
        yield return new WaitForSeconds(duration);
        movementManager.Stun(false);
        stunned = false;
    }

    // Silence - Prevent spellcasting
    [PunRPC]
    void Silence(float duration) {
        if (photonView.IsMine && !silenced) {
            silenced = true;
            StartCoroutine(SilenceRoutine(duration));
        }
    }
    IEnumerator SilenceRoutine(float duration) {
        yield return new WaitForSeconds(duration);
        silenced = false;
    }

    // Fragile - Take increased damage from all sources
    [PunRPC]
    void Fragile(float duration, float percentage) {
        if (photonView.IsMine && !fragile) {
            fragile = true;
            fragilePercentage = percentage;
            StartCoroutine(FragileRoutine(duration));
        }
    }
    IEnumerator FragileRoutine(float duration) {
        yield return new WaitForSeconds(duration);
        fragile = false;
        fragilePercentage = 0f;
    }

    // Toughen - Take decreased damage from all sources
    [PunRPC]
    void Tough(float duration, float percentage) {
        if (photonView.IsMine && !tough) {
            tough = true;
            toughPercentage = percentage;
            StartCoroutine(ToughRoutine(duration));
        }
    }
    IEnumerator ToughRoutine(float duration) {
        yield return new WaitForSeconds(duration);
        tough = false;
        toughPercentage = 0f;
    }
}