using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;

using System.Collections;

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable {
    [Tooltip("The current Health of our player")]
    public float Health = 100f;
    private float healing = 0f;

    [Tooltip("The current Mana pool of our player")]
    public float Mana = 100f;

    [Tooltip("The rate at which Mana will regenerate (Mana/second)")]
    public float ManaRegen = 2.5f;

    [Tooltip("The rate at which Mana will regenerate (Health/second)")]
    public float HealthRegen = 0.025f;

    [Tooltip("The multiplying rate at which Healing will be applied")]
    public float HealingRate = 1f;

    [Tooltip("The rate at which Mana regen will increase if a spell hasnt been cast recently")]
    public float ManaRegenGrowthRate = 0.005f;

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
    private string currentSpellCast = "", currentChannelledSpell = "";
    private Transform currentCastingTransform;
    private bool isChannelling = false, currentSpellIsSelfTargeted = false, currentSpellIsOpponentTargeted = false;
    private GameObject channelledSpell;
    private PlayerMovementManager movementManager;
    private HealthBar healthBar, manaBar;
    private Crosshair crosshair;
    private float maxMana, maxHealth;
    private Spell cachedSpellComponent;
    private CharacterUI characterUI;
    private Aura aura;


    /* ----------------- STATUS EFFECTS ---------------------- */

    // Increase or decrease movement speed
    [HideInInspector]
    public bool slowed;
    [HideInInspector]
    public bool hastened;

    // Prevent all movement, including movement spells
    [HideInInspector]
    public bool rooted;

    // Prevent all spellcasts
    [HideInInspector]
    public bool silenced;

    // Prevent all actions
    [HideInInspector]
    public bool stunned;

    // TODO: Lower or Raise Damage/Health of spells
    // [HideInInspector]
    // public bool weakened;
    // private float weakenedDuration, weakenedPercentage = 0f;
    // [HideInInspector]
    // private bool strengthened;
    // private float strengthenedDuration, strengthenedPercentage = 0f;

    // Increase or decrease the amount of damage taken
    [HideInInspector]
    public bool fragile;
    private float fragileDuration, fragilePercentage = 0f;
    [HideInInspector]
    public bool tough;
    private float toughDuration, toughPercentage = 0f;




    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            // CRITICAL DATA
            stream.SendNext(Health);
            stream.SendNext(Mana);

            // Auxiliary data
            stream.SendNext(slowed);
            stream.SendNext(hastened);
            stream.SendNext(rooted);
            stream.SendNext(silenced);
            stream.SendNext(stunned);
            stream.SendNext(fragile);
            stream.SendNext(tough);
        } else {
            // Network player, receive data
            // CRITICAL DATA
            this.Health = (float)stream.ReceiveNext();
            this.Mana = (float)stream.ReceiveNext();

            // Auxiliary data
            this.slowed = (bool)stream.ReceiveNext();
            this.hastened = (bool)stream.ReceiveNext();
            this.rooted = (bool)stream.ReceiveNext();
            this.silenced = (bool)stream.ReceiveNext();
            this.stunned = (bool)stream.ReceiveNext();
            this.fragile = (bool)stream.ReceiveNext();
            this.tough = (bool)stream.ReceiveNext();
        }
    }

    void Start() {
        maxMana = Mana;
        maxHealth = Health;

        // Follow the player character with the camera
        CustomCameraWork _cameraWork = this.gameObject.GetComponent<CustomCameraWork>();
        if (_cameraWork != null) {
            if (photonView.IsMine) {
                _cameraWork.OnStartFollowing();
            }
        } else {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        if (PlayerUiPrefab != null) {
            GameObject _uiGo = Instantiate(PlayerUiPrefab, transform.position + new Vector3(0f, 2f, 0f), transform.rotation);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            _uiGo.transform.SetParent(transform);
            characterUI = _uiGo.GetComponent<CharacterUI>();
        } else {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

        // Get animator for casting
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        aura = GetComponent<Aura>();

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

            // If there is healing to be done, do it
            if (healing > 0f && Health < maxHealth) {
                float healingDone = healing * Time.deltaTime * HealingRate;
                Health += healingDone;
                healing -= healingDone;
            } else {
                healing = 0f;
            }

            // If health should regen, do it
            if (HealthRegen > 0f && Health < maxHealth) Health += HealthRegen * Time.deltaTime;

            // If you are channelling a spell, reduce mana and reset mana regrowth
            if (isChannelling && channelledSpell != null) {
                if (Mana > 0f) {
                    if (cachedSpellComponent == null) cachedSpellComponent = channelledSpell.GetComponent<Spell>();
                    Mana -= cachedSpellComponent.ManaChannelCost * Time.deltaTime;
                } else {
                    StopBlocking();
                }
            }

            // Regen mana if below maxMana
            if (Mana < maxMana) {
                Mana += ManaRegen * Time.deltaTime * ((1.1f - Mana / maxMana) * ManaRegenGrowthRate);
                if (Mana > maxMana) Mana = maxMana;
            }

            // Display health and mana values
            healthBar.SetHealth(Health);
            manaBar.SetHealth(Mana);
        }
    }

    /* ------------------------ SPELL COLLISION HANDLING ----------------------- */

    [PunRPC]
    void OnSpellCollide(float Damage, string ManaDamageType, string SpellEffectType, float Duration, string spellDistributionJson) {
        if (!photonView.IsMine) return;
        Debug.Log("Collision with spell of type: " + ManaDamageType);
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);
        // Spell spell = spellGO.GetComponent<Spell>();
        switch (SpellEffectType) {
            case "dotprojectile":
                StartCoroutine(TakeDirectDoTDamage(Damage, ManaDamageType, Duration, spellDistribution));
                break;
            default:
                Debug.Log("Default Spell effect --> Take direct damage");
                TakeDamage(Damage, spellDistribution);
                break;
        }
        // Debug.Log("Current Health: "+Health);
    }

    void TakeDamage(float damage, ManaDistribution spellDistribution) {
        if (fragile) damage *= (1 + fragilePercentage);
        if (tough) damage *= (1 - toughPercentage);
        Health -= aura.GetDamage(damage, spellDistribution);
        Debug.Log("Take Damage --  pre-resistance: " + damage + "    post-resistance: " + aura.GetDamage(damage, spellDistribution));
    }

    IEnumerator TakeDirectDoTDamage(float damage, string damageType, float duration, ManaDistribution spellDistribution) {
        float damagePerSecond = damage / duration;
        while (duration > 0f) {
            TakeDamage(damagePerSecond * Time.deltaTime, spellDistribution);
            duration -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }






    /*  --------------------  SPELLCASTING ------------------------ */

    void ProcessInputs() {
        if (silenced || stunned) return;

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
        } else if (Input.GetKeyDown("0")) {
            StartCastMinorHeal();
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

    public void SetMaxMana(float newMax) {
        if (photonView.IsMine) {
            maxMana = newMax;
            Mana = newMax;
            manaBar.SetMaxHealth(maxMana);

            Debug.Log("New Max mana: " + maxMana + " " + Mana);
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

    GameObject GetPlayerWithinAimTolerance(float tolerance) {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, tolerance, (transform.position - GetCrosshairAimPoint()), out hit, 1000, 1 << 3)) {
            Debug.Log("HIT PLAYER!");
            return hit.collider.gameObject;
        }
        return null;
    }

    void StopBlocking() {
        movementManager.StopBlock();
        ChannelSpell(false);
    }

    void CastSpell() {
        if (photonView.IsMine && currentSpellCast != null && !silenced && !stunned) {
            GameObject dataObject = Resources.Load<GameObject>(currentSpellCast);
            Debug.Log("Spell object grabbed: " + dataObject);
            Spell foundSpell = dataObject.GetComponent<Spell>();
            if (Mana - foundSpell.ManaCost > 0f) {
                GameObject newSpell = PhotonNetwork.Instantiate(currentSpellCast, currentCastingTransform.position, currentCastingTransform.rotation);
                Mana -= foundSpell.ManaCost;

                if (currentSpellIsSelfTargeted) {
                    currentSpellIsSelfTargeted = false;
                    TargetedSpell ts = newSpell.GetComponent<TargetedSpell>();
                    if (ts != null) ts.SetTarget(gameObject);
                } else if (currentSpellIsOpponentTargeted) {
                    currentSpellIsOpponentTargeted = false;
                    TargetedSpell ts = newSpell.GetComponent<TargetedSpell>();
                    GameObject target = GetPlayerWithinAimTolerance(3f);
                    if (ts != null && target != null) {
                        ts.SetTarget(target);
                    }
                }
            }
        }
        currentSpellCast = null;
    }

    void ChannelSpell(bool start = true) {
        if (photonView.IsMine) {
            if (start && !isChannelling && currentChannelledSpell != null && !silenced && !stunned) {
                isChannelling = true;
                GameObject dataObject = Resources.Load<GameObject>(currentChannelledSpell);
                Debug.Log("Spell object grabbed: " + dataObject);
                Spell foundSpell = dataObject.GetComponent<Spell>();
                if (Mana - foundSpell.ManaCost > 0f) {
                    channelledSpell = PhotonNetwork.Instantiate(currentChannelledSpell, currentCastingTransform.position, currentCastingTransform.rotation);
                    channelledSpell.transform.SetParent(gameObject.transform);
                    Mana -= foundSpell.ManaCost;
                }
            } else if ((!start && isChannelling) || silenced || stunned) {
                isChannelling = false;
                PhotonNetwork.Destroy(channelledSpell);
                channelledSpell = null;
            }
        }
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

    void StartCastMinorHeal() {
        currentSpellCast = "Spell_MinorHeal";
        currentCastingTransform = transform;
        currentSpellIsSelfTargeted = true;
        movementManager.PlayCastingAnimation(0);
    }



    /*  --------------------  STATUS EFFECTS ------------------------ */

    // Heal - restore health by a flat value and/or a percentage of missing health
    [PunRPC]
    void Heal(float flat, float percentage) {
        if (photonView.IsMine) {
            healing += flat + ((maxHealth - Health) * percentage);
        }
    }

    // Displace - move the player along a local or worldspace direction vector
    [PunRPC]
    void Displace(Vector3 direction, float distance, float speed, bool isWorldSpaceDirection) {
        if (photonView.IsMine) {
            movementManager.Displace(direction, distance, speed, isWorldSpaceDirection);
        }
    }

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