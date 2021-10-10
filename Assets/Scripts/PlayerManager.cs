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

    [Tooltip("Where spells will spawn from when being cast forwards")]
    public Transform frontCastingAnchor;
    [Tooltip("Where spells will spawn from when being cast upwards")]
    public Transform topCastingAnchor;
    [Tooltip("Where spells will spawn from when being cast at aimpoint")]
    public Transform aimPointAnchor;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerGameObject;

    public static PlayerManager LocalInstance;

    [Tooltip("The Player's UI GameObject Prefab")]
    [SerializeField]
    public GameObject PlayerUiPrefab;

    [Tooltip("The root bone of the character model, used for animations and ragdolling")]
    public GameObject RootBone;

    public PlayerParticleManager particleManager;
    public CharacterMaterialManager materialManager;

    [HideInInspector]
    public bool dead = false;

    public AudioSource CastingSound, DeathSound, HitSound;

    private Animator animator;
    private string currentSpellCast = "", currentChannelledSpell = "";
    private Transform currentCastingTransform;
    private bool isChannelling = false, currentSpellIsSelfTargeted = false, currentSpellIsOpponentTargeted = false, isShielded = false, sneaking = false;
    private GameObject channelledSpell, spellCraftingDisplay, glyphCastingPanel;
    private PlayerMovementManager movementManager;
    private HealthBar healthBar, manaBar;
    private Crosshair crosshair;
    private float maxMana, maxHealth, defaultManaRegen;
    private Spell cachedSpellComponent;
    private CharacterUI characterUI;
    private Aura aura;
    private AuricaCaster auricaCaster;
    private ShieldSpell currentShield;
    private CustomCameraWork cameraWorker;
    private AimpointAnchor aimPointAnchorManager;


    /* ----------------- STATUS EFFECTS ---------------------- */

    // Increase or decrease movement speed
    [HideInInspector]
    public bool slowed;
    Coroutine slowRoutine;
    bool slowRoutineRunning;

    [HideInInspector]
    public bool hastened;
    Coroutine hasteRoutine;
    bool hasteRoutineRunning;

    // Prevent all movement, including movement spells
    [HideInInspector]
    public bool rooted;
    Coroutine rootRoutine;
    bool rootRoutineRunning;

    // Prevent all spellcasts
    [HideInInspector]
    public bool silenced;
    Coroutine silenceRoutine;
    bool silenceRoutineRunning;

    // Prevent all actions
    [HideInInspector]
    public bool stunned;
    Coroutine stunRoutine;
    bool stunRoutineRunning;

    // Do less damage of given mana types
    [HideInInspector]
    public bool weakened;
    private ManaDistribution weaknesses;
    Coroutine weakenRoutine;
    bool weakenRoutineRunning;

    // Do more damage of given mana types
    [HideInInspector]
    public bool strengthened;
    private ManaDistribution strengths;
    Coroutine strengthenRoutine;
    bool strengthenRoutineRunning;

    // Increase or decrease the amount of damage taken
    [HideInInspector]
    public bool fragile;
    private float fragileDuration, fragilePercentage = 0f;
    Coroutine fragileRoutine;
    bool fragileRoutineRunning;

    [HideInInspector]
    public bool tough;
    private float toughDuration, toughPercentage = 0f;
    Coroutine toughRoutine;
    bool toughRoutineRunning;

    [HideInInspector]
    public bool manaRestorationChange;
    private float manaRestorationDuration;
    Coroutine manaRestorationRoutine;
    bool manaRestorationRoutineRunning;

    [HideInInspector]
    public bool camouflaged = false;
    Coroutine camouflageRoutine;
    bool camouflagedRoutineRunning;




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
        defaultManaRegen = ManaRegen;

        // Follow the player character with the camera
        cameraWorker = this.gameObject.GetComponent<CustomCameraWork>();
        if (cameraWorker != null) {
            if (photonView.IsMine) {
                cameraWorker.OnStartFollowing();
            }
        } else {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        if (PlayerUiPrefab != null) {
            GameObject _uiGo = Instantiate(PlayerUiPrefab, transform.position + new Vector3(0f, 2f, 0f), transform.rotation);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            _uiGo.transform.SetParent(transform);
            characterUI = _uiGo.GetComponent<CharacterUI>();
            materialManager.SetUI(characterUI);
        } else {
            Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
        }

        // Get animator for casting
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        aura = GetComponent<Aura>();
        auricaCaster = GetComponent<AuricaCaster>();

        // Get movement manager
        movementManager = GetComponent<PlayerMovementManager>();

        healthBar = GameObject.Find("LocalHealthBar").GetComponent<HealthBar>();
        manaBar = GameObject.Find("LocalManaBar").GetComponent<HealthBar>();
        crosshair = Object.FindObjectOfType(typeof(Crosshair)) as Crosshair;

        aimPointAnchorManager = aimPointAnchor.GetComponent<AimpointAnchor>();
    }

    void Awake() {
        if (photonView.IsMine) {
            PlayerManager.LocalInstance = this;
            PlayerManager.LocalPlayerGameObject = this.gameObject;
        }

        // Unparent the aimpoint anchor so that when the player moves the anchor wont move with the player
        if (aimPointAnchor != null) aimPointAnchor.transform.parent = null;
    }

    void Update() {
        if (Health <= 0f && !dead) {
            Die();
        }
        if (photonView.IsMine) {
            // Allowed to look at and craft spells while dead, but nothing else
            if (Health <= 0f) return;

            if (spellCraftingDisplay != null && !spellCraftingDisplay.activeInHierarchy) this.ProcessInputs();

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

            if (Mana < 0f) Mana = 0f;

            // Display health and mana values
            healthBar.SetHealth(Health);
            manaBar.SetHealth(Mana);
        }
    }

    public string GetUniqueName() {
        return photonView.Owner.NickName+"-|-"+photonView.ViewID;
    }

    public void HardReset() {
        if (!photonView.IsMine) return;
        // Reset health and mana
        Health = maxHealth;
        Mana = maxMana;
        healthBar.SetHealth(Health);
        manaBar.SetHealth(Mana);

        // Reset any spell casts
        auricaCaster.ResetCast();
        if (spellCraftingDisplay != null) {
            SpellCraftingUIDisplay sp = spellCraftingDisplay.GetComponent<SpellCraftingUIDisplay>();
            if (sp != null) sp.ClearSpell();
        }
        StopBlocking();

        // TODO: Clear any status effects
        Cleanse();
    }

    public void Die() {
        if (dead) return;
        dead = true;
        animator.enabled = false;
        RootBone.transform.parent = null;
        healthBar.SetHealth(0);
        manaBar.SetHealth(0);

        if (DeathSound != null) DeathSound.Play();

        GameManager.Instance.playerDeath(this);
        if (DeathmatchGameManager.Instance != null) DeathmatchGameManager.Instance.playerDeath(this);
    }

    public void Respawn() {
        // if (!photonView.IsMine) return;
        dead = false;
        animator.enabled = true;
        RootBone.transform.position = transform.position;
        RootBone.transform.parent = transform;

        HardReset();
    }

    public void Teleport(Vector3 newPosition) {
        if (!photonView.IsMine) return;
        Debug.Log("Teleporting " + gameObject + "  to " + newPosition);
        transform.position = newPosition;
    }

    public void Sneak() {
        if (sneaking) return;
        particleManager.StopDefaultParticles();
        materialManager.HideCharacterUI();
        materialManager.HideOutline();
        sneaking = true;
    }

    public void EndSneak() {
        if (!sneaking || camouflaged) return;
        particleManager.StartDefaultParticles();
        materialManager.ShowCharacterUI();
        materialManager.ShowOutline();
        sneaking = false;
    }

    public void SetNameColor(Color color) {
        materialManager.SetNameColor(color);
    }

    public void ResetNameColor() {
        materialManager.ResetNameColor();
    }

    public void SetPlayerMaterial(Material mat) {
        materialManager.SetPlayerMaterial(mat);
    }

    public void ResetPlayerMaterial() {
        materialManager.ResetPlayerMaterial();
    }

    public void SetPlayerOutline(Color color) {
        materialManager.SetOutline(color);
    }

    public void ResetPlayerOutline() {
        materialManager.ResetOutline();
    }

    /* ------------------------ SPELL COLLISION HANDLING ----------------------- */

    [PunRPC]
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson) {
        if (!photonView.IsMine) return;
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);
        // Spell spell = spellGO.GetComponent<Spell>();
        switch (SpellEffectType) {
            case "dot":
                StartCoroutine(TakeDirectDoTDamage(Damage, Duration, spellDistribution));
                break;
            default:
                // Debug.Log("Default Spell effect --> Take direct damage");
                TakeDamage(Damage, spellDistribution);
                break;
        }
        // Debug.Log("Current Health: "+Health);
    }

    public void TakeDamage(float damage, ManaDistribution spellDistribution) {
        // Modify the damage if the player is fragile or tough
        if (fragile) damage *= (1 + fragilePercentage);
        if (tough) damage *= (1 - toughPercentage);

        // If you are shielded, negate all but a fraction of the damage.
        if (isShielded) damage *= 0.1f;

        // Apply the damage
        float finalDamage = aura.GetDamage(damage, spellDistribution) * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER;
        Health -= finalDamage;

        // Play hit effects
        DamageVignette.Instance.FlashDamage(finalDamage);
        if (cameraWorker != null) cameraWorker.Shake(finalDamage / 100f, 0.198f);
        if (HitSound != null && finalDamage > 3f) HitSound.Play();

        if (damage > 1f) Debug.Log("Take Damage --  pre-resistance: " + damage + "    post-resistance: " + finalDamage + "     resistance total: " + finalDamage / damage);
    }

    IEnumerator TakeDirectDoTDamage(float damage, float duration, ManaDistribution spellDistribution) {
        float damagePerSecond = damage / duration;
        // Debug.Log("Take dot damage: "+damage+" duration: "+duration+ "     resistance total: " + aura.GetDamage(damage, spellDistribution) / damage);
        while (duration > 0f) {
            TakeDamage(damagePerSecond * Time.deltaTime, spellDistribution);
            duration -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }






    /*  --------------------  SPELLCASTING ------------------------ */

    void ProcessInputs() {
        if (silenced || stunned || !photonView.IsMine) return;

        if (movementManager.CanCast()) {
            if (Input.GetKeyDown("1")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("1"));
            } else if (Input.GetKeyDown("2")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("2"));
            } else if (Input.GetKeyDown("3")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("3"));
            } else if (Input.GetKeyDown("4")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("4"));
            } else if (Input.GetKeyDown("e")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("e"));
            } else if (Input.GetKeyDown("q")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("q"));
            } else if (Input.GetKeyDown("r")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("r"));
            } else if (Input.GetKeyDown("f")) {
                CastAuricaSpell(auricaCaster.CastBindSlot("f"));
            }
        }

        if (Input.GetKeyDown("\\")) {
            Mana += 400;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (!isChannelling) {
                if (movementManager.CanCast()) CastAuricaSpell(auricaCaster.CastFinal());
            } else {
                StopBlocking();
                auricaCaster.ResetCast();
            }
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

    public void ConfirmAura() {
        if (!photonView.IsMine) return;
        spellCraftingDisplay = GameManager.Instance.GetSpellCraftingPanel();
        glyphCastingPanel = GameManager.Instance.GetGlyphCastingPanel();
        // glyphCastingPanel.SetActive(false);
        var gc = spellCraftingDisplay.GetComponent<SpellCraftingUIDisplay>();
        if (gc != null) gc.SendAura(aura.GetAura());
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
        return crosshair.GetPlayerHit(tolerance);
    }

    void StopBlocking() {
        movementManager.StopBlock();
        ChannelSpell(false);
    }

    [PunRPC]
    public void BreakShield() {
        StopBlocking();
    }

    void CastFizzle() {
        PhotonNetwork.Instantiate("XCollision_Fizzle", transform.position, transform.rotation);
    }

    void CastAuricaSpell(AuricaSpell spell) {
        if (!photonView.IsMine) return;
        if (isChannelling) {
            StopBlocking();
            auricaCaster.ResetCast();
            return;
        }
        if (spell == null) {
            Debug.Log("Invalid cast");
            CastFizzle();
            auricaCaster.ResetCast();
            return;
        }
        Debug.Log("Spell Match: " + spell.c_name);
        // Load spell resource
        GameObject dataObject = Resources.Load<GameObject>(spell.linkedSpellResource);
        Spell foundSpell = dataObject != null ? dataObject.GetComponent<Spell>() : null;

        if (foundSpell == null) {
            return;
        }

        // Change the casting anchor (where the spell spawns from)
        switch (foundSpell.CastingAnchor) {
            case "transform":
                currentCastingTransform = transform;
                break;
            case "top":
                currentCastingTransform = topCastingAnchor;
                break;
            case "aimpoint":
                aimPointAnchor.position = GetCrosshairAimPoint();
                aimPointAnchor.rotation = Quaternion.LookRotation(aimPointAnchorManager.GetHitNormal(), (aimPointAnchor.transform.position - transform.position));
                currentCastingTransform = aimPointAnchor;
                break;
            default:
                currentCastingTransform = frontCastingAnchor;
                break;
        }

        // Turn the casting anchor in the direction we want the spell to face
        if (foundSpell.CastingAnchor != "transform") {
            if (foundSpell.TurnToAimPoint) {
                TurnCastingAnchorDirectionToAimPoint();
            } else {
                ResetCastingAnchorDirection();
            }
        }
        
        currentSpellIsSelfTargeted = foundSpell.IsSelfTargeted;
        currentSpellIsOpponentTargeted = foundSpell.IsOpponentTargeted;

        // Set the spell to cast, and start the animation
        if (!foundSpell.IsChannel) {
            currentSpellCast = spell.linkedSpellResource;
            movementManager.PlayCastingAnimation(foundSpell.CastAnimationType);
            particleManager.PlayHandParticle(foundSpell.CastAnimationType, spell.manaType);
            if (CastingSound != null) CastingSound.Play();
        } else {
            // If the spell is channelled, channel it immediately
            currentChannelledSpell = spell.linkedSpellResource;
            ChannelSpell();
        }

        if (spellCraftingDisplay != null) {
            SpellCraftingUIDisplay sp = spellCraftingDisplay.GetComponent<SpellCraftingUIDisplay>();
            if (sp != null) sp.ClearSpell();
        }
    }

    void CastSpell() {
        if (photonView.IsMine && !silenced && !stunned) {
            if (Mana - auricaCaster.GetManaCost() > 0f) {
                Transform aimTransform = !currentSpellIsOpponentTargeted ? currentCastingTransform : GetPlayerWithinAimTolerance(5f) != null ? GetPlayerWithinAimTolerance(5f).transform : null;
                if (aimTransform == null) {
                    CastFizzle();
                    return;
                }
                GameObject newSpell = PhotonNetwork.Instantiate(currentSpellCast, aimTransform.position, aimTransform.rotation);
                Mana -= auricaCaster.GetManaCost();

                Spell spell = newSpell.GetComponent<Spell>();
                if (spell != null) {
                    spell.SetSpellStrength(auricaCaster.GetSpellStrength());
                    spell.SetSpellDamageModifier(aura.GetInnateStrength() + strengths - weaknesses);
                    spell.SetOwner(gameObject);
                    Mana += spell.ManaRefund;
                } else {
                    Debug.Log("Could not grab <Spell> Object from newly instantiated spell");
                }

                if (currentSpellIsSelfTargeted) {
                    currentSpellIsSelfTargeted = false;
                    TargetedSpell targetedSpell = newSpell.GetComponent<TargetedSpell>();
                    if (targetedSpell != null) targetedSpell.SetTarget(gameObject);
                    AoESpell aoeSpell = newSpell.GetComponent<AoESpell>();
                    if (aoeSpell != null) aoeSpell.SetTarget(gameObject);
                } else if (currentSpellIsOpponentTargeted) {
                    currentSpellIsOpponentTargeted = false;
                    TargetedSpell ts = newSpell.GetComponent<TargetedSpell>();
                    AoESpell aoeSpell = newSpell.GetComponent<AoESpell>();

                    GameObject target = GetPlayerWithinAimTolerance(10f);
                    if (target != null) {
                        Debug.Log("Target found: " + target);
                        if (ts != null) ts.SetTarget(target);
                        if (aoeSpell != null) aoeSpell.SetTarget(target);
                    }
                    
                }
            } else {
                CastFizzle();
                manaBar.BlinkText();
            }
            auricaCaster.ResetCast();
        }
        currentSpellCast = null;
    }

    void ChannelSpell(bool start = true) {
        if (photonView.IsMine) {
            if (start && !isChannelling && currentChannelledSpell != null && !silenced && !stunned) {
                if (Mana - auricaCaster.GetManaCost() > 0f) {
                    isChannelling = true;
                    channelledSpell = PhotonNetwork.Instantiate(currentChannelledSpell, currentCastingTransform.position, currentCastingTransform.rotation);
                    channelledSpell.transform.SetParent(gameObject.transform);
                    Mana -= auricaCaster.GetManaCost();

                    Spell foundSpell = channelledSpell.GetComponent<Spell>();
                    if (foundSpell != null) {
                        foundSpell.SetSpellStrength(auricaCaster.GetSpellStrength());
                        foundSpell.SetSpellDamageModifier(aura.GetInnateStrength() + strengths - weaknesses);
                        foundSpell.SetOwner(gameObject);
                    }

                    currentShield = channelledSpell.GetComponent<ShieldSpell>();
                    if (currentShield != null) {
                        currentShield.SetShieldStrength(auricaCaster.GetSpellStrength());
                        if (currentShield.SpellEffectType == "shield") isShielded = true;
                    }

                    movementManager.StartBlock();
                } else {
                    CastFizzle();
                }
                auricaCaster.ResetCast();
            } else if ((!start && isChannelling) || silenced || stunned) {
                isChannelling = false;
                isShielded = false;
                try {
                    if (currentShield != null) {
                        currentShield.Break();
                    } else if (channelledSpell != null) {
                        PhotonNetwork.Destroy(channelledSpell);
                    }
                } catch {
                    // Do nothing, likely has already been cleaned up
                }
                channelledSpell = null;
            }
        }
    }

    public void EndCast() {
        particleManager.StopHandParticles(!sneaking);
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






    // ManaDrain - drain mana by a flat value and/or a percentage of missing health
    [PunRPC]
    void ManaDrain(float flat, float percentage) {
        if (photonView.IsMine) {
            Debug.Log("Draining Mana by "+(flat + ((maxMana - Mana) * percentage)));
            Mana -= flat + ((maxMana - Mana) * percentage);
        }
    }







    // Slow - Decrease animation speed
    [PunRPC]
    void Slow(float duration, float percentage) {
        if (photonView.IsMine && !slowed) {
            slowed = true;
            slowRoutine = StartCoroutine(SlowRoutine(duration, percentage));
        }
    }
    IEnumerator SlowRoutine(float duration, float percentage) {
        slowRoutineRunning = true;
        animator.speed *= 1f - percentage;
        movementManager.ChangeMovementSpeed(1f - percentage);
        yield return new WaitForSeconds(duration);
        animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        movementManager.ResetMovementSpeed();
        slowed = false;
        slowRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousSlow(float percentage) {
        if (photonView.IsMine) {
            slowed = true;
            animator.speed *= 1f - percentage;
            movementManager.ChangeMovementSpeed(1f - percentage);
        }
    }
    [PunRPC]
    public void EndContinuousSlow() {
        if (photonView.IsMine) {
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            slowed = false;
        }
    }






    // Hasten - Increase animation speed
    [PunRPC]
    void Hasten(float duration, float percentage) {
        if (photonView.IsMine && !hastened) {
            hastened = true;
            hasteRoutine = StartCoroutine(HastenRoutine(duration, percentage));
        }
    }
    IEnumerator HastenRoutine(float duration, float percentage) {
        hasteRoutineRunning = true;
        animator.speed *= 1f + percentage;
        movementManager.ChangeMovementSpeed(1f + percentage);
        yield return new WaitForSeconds(duration);
        animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        movementManager.ResetMovementSpeed();
        hastened = false;
        hasteRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousHasten(float percentage) {
        if (photonView.IsMine) {
            hastened = true;
            animator.speed *= 1f + percentage;
            movementManager.ChangeMovementSpeed(1f + percentage);
        }
    }
    [PunRPC]
    public void EndContinuousHasten() {
        if (photonView.IsMine) {
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            hastened = false;
        }
    }






    // Rooted - Prevent movement, including movement spells
    [PunRPC]
    void Root(float duration) {
        if (photonView.IsMine && !rooted) {
            rooted = true;
            rootRoutine = StartCoroutine(RootRoutine(duration));
        }
    }
    IEnumerator RootRoutine(float duration) {
        rootRoutineRunning = true;
        movementManager.Root(true);
        yield return new WaitForSeconds(duration);
        movementManager.Root(false);
        rooted = false;
        rootRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousRoot() {
        if (photonView.IsMine) {
            rooted = true;
            movementManager.Root(true);
        }
    }
    [PunRPC]
    public void EndContinuousRoot() {
        if (photonView.IsMine) {
            rooted = false;
            movementManager.Root(false);
        }
    }






    // Stunned - Prevent all movement and spellcasting
    [PunRPC]
    void Stun(float duration) {
        if (photonView.IsMine && !stunned) {
            stunned = true;
            ChannelSpell(false);
            slowRoutine = StartCoroutine(StunRoutine(duration));
        }
    }
    IEnumerator StunRoutine(float duration) {
        stunRoutineRunning = true;
        movementManager.Stun(true);
        yield return new WaitForSeconds(duration);
        movementManager.Stun(false);
        stunned = false;
        stunRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousStun() {
        if (photonView.IsMine) {
            ChannelSpell(false);
            stunned = true;
            movementManager.Stun(true);
        }
    }
    [PunRPC]
    public void EndContinuousStun() {
        if (photonView.IsMine) {
            stunned = false;
            movementManager.Stun(false);
        }
    }






    // Silence - Prevent spellcasting
    [PunRPC]
    void Silence(float duration) {
        if (photonView.IsMine && !silenced) {
            silenced = true;
            ChannelSpell(false);
            silenceRoutine = StartCoroutine(SilenceRoutine(duration));
        }
    }
    IEnumerator SilenceRoutine(float duration) {
        silenceRoutineRunning = true;
        yield return new WaitForSeconds(duration);
        silenced = false;
        silenceRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousSilence() {
        if (photonView.IsMine) {
            ChannelSpell(false);
            silenced = true;
        }
    }

    [PunRPC]
    public void EndContinuousSilence() {
        if (photonView.IsMine) {
            silenced = false;
        }
    }






    // Weaken - Deal reduced damage of given mana types
    [PunRPC]
    void Weaken(float duration, string weaknessString) {
        if (photonView.IsMine) {
            ManaDistribution weaknessDist = new ManaDistribution(weaknessString);
            weakenRoutine = StartCoroutine(WeakenRoutine(duration, weaknessDist));
        }
    }
    IEnumerator WeakenRoutine(float duration, ManaDistribution weaknessDist) {
        weakenRoutineRunning = true;
        weakened = true;
        weaknesses += weaknessDist;
        yield return new WaitForSeconds(duration);
        weaknesses -= weaknessDist;
        if (weaknesses.GetAggregate() <= 0.1f) weakened = false;
        weakenRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousWeaken(string weakString) {
        if (photonView.IsMine) {
            ManaDistribution weakDist = new ManaDistribution(weakString);
            weakened = true;
            weaknesses += weakDist;
            //Debug.Log("New Strength: " + weaknesses.ToString());
        }
    }

    [PunRPC]
    public void EndContinuousWeaken(string weakString) {
        if (photonView.IsMine) {
            ManaDistribution weakDist = new ManaDistribution(weakString);
            weaknesses -= weakDist;
            if (weaknesses.GetAggregate() <= 0.1f) weakened = false;
            //Debug.Log("New Strength after end: " + weaknesses.ToString());
        }
    }






    // Strengthen - Deal increased damage of given mana types
    [PunRPC]
    void Strengthen(float duration, string strengthString) {
        if (photonView.IsMine) {
            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            strengthenRoutine = StartCoroutine(StrengthenRoutine(duration, strengthDist));
        }
    }
    IEnumerator StrengthenRoutine(float duration, ManaDistribution strengthDist) {
        strengthenRoutineRunning = true;
        strengthened = true;
        strengths += strengthDist;
        //Debug.Log("New Strength: " + strengths.ToString());
        yield return new WaitForSeconds(duration);
        strengths -= strengthDist;
        if (strengths.GetAggregate() <= 0.1f) strengthened = false;
        strengthenRoutineRunning = false;
    }

    [PunRPC]
    public void ContinuousStrengthen(string strengthString) {
        if (photonView.IsMine) {
            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            strengthened = true;
            strengths += strengthDist;
            //Debug.Log("New Strength: " + strengths.ToString());
        }
    }

    [PunRPC]
    public void EndContinuousStrengthen(string strengthString) {
        if (photonView.IsMine) {
            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            strengths -= strengthDist;
            if (strengths.GetAggregate() <= 0.1f) strengthened = false;
            //Debug.Log("New Strength after end: " + strengths.ToString());
        }
    }






    // Fragile - Take increased damage from all sources
    [PunRPC]
    void Fragile(float duration, float percentage) {
        if (photonView.IsMine && !fragile) {
            fragile = true;
            fragilePercentage = percentage;
            fragileRoutine = StartCoroutine(FragileRoutine(duration));
        }
    }
    IEnumerator FragileRoutine(float duration) {
        fragileRoutineRunning = true;
        yield return new WaitForSeconds(duration);
        fragile = false;
        fragilePercentage = 0f;
        fragileRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousFragile(float percentage) {
        if (photonView.IsMine) {
            fragile = true;
            fragilePercentage = percentage;
        }
    }

    [PunRPC]
    public void EndContinuousFragile() {
        if (photonView.IsMine) {
            fragile = false;
            fragilePercentage = 0f;
        }
    }






    // Toughen - Take decreased damage from all sources
    [PunRPC]
    void Tough(float duration, float percentage) {
        if (photonView.IsMine && !tough) {
            toughRoutine = StartCoroutine(ToughRoutine(duration, percentage));
        }
    }
    IEnumerator ToughRoutine(float duration, float percentage) {
        toughRoutineRunning = true;
        tough = true;
        toughPercentage = percentage;
        yield return new WaitForSeconds(duration);
        tough = false;
        toughPercentage = 0f;
        toughRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousTough(float percentage) {
        if (photonView.IsMine) {
            tough = true;
            toughPercentage = percentage;
        }
    }
    [PunRPC]
    public void EndContinuousTough() {
        if (photonView.IsMine) {
            tough = false;
            toughPercentage = 0f;
        }
    }






    // ManaRestoration - change the mana regen rate by a multiplier value
    [PunRPC]
    void ManaRestoration(float duration, float restorationPercentage) {
        if (photonView.IsMine) {
            manaRestorationRoutine = StartCoroutine(ManaRestorationRoutine(duration, restorationPercentage));
        }
    }
    IEnumerator ManaRestorationRoutine(float duration, float restorationPercentage) {
        Debug.Log("Mana restoration mult: "+restorationPercentage+"     for: "+duration+" seconds.");
        manaRestorationRoutineRunning = true;
        manaRestorationChange = true;
        ManaRegen *= restorationPercentage;
        Debug.Log("New Mana Regen : " + ManaRegen);
        yield return new WaitForSeconds(duration);
        ManaRegen /= restorationPercentage;
        if (ManaRegen == defaultManaRegen) manaRestorationChange = false;
        Debug.Log("New Mana Regen after end: " + ManaRegen);
        manaRestorationRoutineRunning = false;
    }

    [PunRPC]
    public void ContinuousManaRestoration(float restorationPercentage) {
        if (photonView.IsMine) {
            manaRestorationChange = true;
            ManaRegen *= restorationPercentage;
            // Debug.Log("New Mana Regen : " + ManaRegen);
        }
    }

    [PunRPC]
    public void EndContinuousManaRestoration(float restorationPercentage) {
        if (photonView.IsMine) {
            ManaRegen /= restorationPercentage;
            if (ManaRegen == defaultManaRegen) manaRestorationChange = false;
            //Debug.Log("New Mana Regen after end: " + ManaRegen);
        }
    }






    // Camouflage
    [PunRPC]
    void Camouflage(float duration) {
        camouflageRoutine = StartCoroutine(CamouflageRoutine(duration));
    }
    IEnumerator CamouflageRoutine(float duration) {
        camouflagedRoutineRunning = true;
        camouflaged = true;
        materialManager.GoInvisible();
        Sneak();
        yield return new WaitForSeconds(duration);
        materialManager.ResetMaterial();
        camouflaged = false;
        EndSneak();
        camouflagedRoutineRunning = false;
    }

    [PunRPC]
    public void ContinuousCamouflage() {
        camouflaged = true;
        materialManager.GoInvisible();
        Sneak();
    }

    [PunRPC]
    public void EndContinuousCamouflage() {
        camouflaged = false;
        materialManager.ResetMaterial();
        EndSneak();
    }


    // Cleanse
    [PunRPC]
    void Cleanse() {
        if (slowed) {
            if (slowRoutineRunning) StopCoroutine(slowRoutine);
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            slowed = false;
        }
        if (hastened) {
            if (hasteRoutineRunning) StopCoroutine(hasteRoutine);
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            hastened = false;
        }
        if (rooted) {
            if (rootRoutineRunning) StopCoroutine(rootRoutine);
            movementManager.Root(false);
            rooted = false;
        }
        if (silenced) {
            if (silenceRoutineRunning) StopCoroutine(silenceRoutine);
            silenced = false;
        }
        if (stunned) {
            if (stunRoutineRunning) StopCoroutine(stunRoutine);
            movementManager.Stun(false);
            stunned = false;
        }
        if (fragile) {
            if (fragileRoutineRunning) StopCoroutine(fragileRoutine);
            fragilePercentage = 0f;
            fragile = false;
        }
        if (tough) {
            if (toughRoutineRunning) StopCoroutine(toughRoutine);
            toughPercentage = 0f;
            tough = false;
        }
        if (weakened) {
            if (weakenRoutineRunning) StopCoroutine(weakenRoutine);
            weaknesses = new ManaDistribution();
            weakened = false;
        }
        if (strengthened) {
            if (strengthenRoutineRunning) StopCoroutine(strengthenRoutine);
            strengths = new ManaDistribution();
            strengthened = false;
        }
        if (manaRestorationChange) {
            if (manaRestorationRoutineRunning) StopCoroutine(manaRestorationRoutine);
            ManaRegen = defaultManaRegen;
            manaRestorationChange = false;
        }
        if (camouflaged) {
            if (camouflagedRoutineRunning) StopCoroutine(camouflageRoutine);
            materialManager.ResetMaterial();
            camouflaged = false;
        }
    }





    // Cure
    // Removes all negative status effects
    [PunRPC]
    void Cure() {
        if (slowed) {
            if (slowRoutineRunning) StopCoroutine(slowRoutine);
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            slowed = false;
        }
        if (fragile) {
            if (fragileRoutineRunning) StopCoroutine(fragileRoutine);
            fragilePercentage = 0f;
            fragile = false;
        }
        if (weakened) {
            if (weakenRoutineRunning) StopCoroutine(weakenRoutine);
            weaknesses = new ManaDistribution();
            weakened = false;
        }
        if (manaRestorationChange) {
            if (ManaRegen < defaultManaRegen) {
                if (manaRestorationRoutineRunning) StopCoroutine(manaRestorationRoutine);
                ManaRegen = defaultManaRegen;
                manaRestorationChange = false;
            }
        }
    }
}