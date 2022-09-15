using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;
using Photon.Realtime;


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

    [HideInInspector]
    public bool hasBoost = true;
    [HideInInspector]
    public float BoostCooldown = 100f;
    public float boostCooldownMultiplier = 5f;

    [HideInInspector]
    public Aura aura;

    public AudioSource CastingSound, DeathSound, HitSound, HitMarkerSound, HitMarkerAoESound;

    private Animator animator;
    private string currentSpellCast = "", currentChannelledSpell = "";
    private Transform currentCastingTransform;
    private bool isChannelling = false, currentSpellIsSelfTargeted = false, currentSpellIsOpponentTargeted = false, isShielded = false, sneaking = false, respawning = false;
    private GameObject channelledSpell, spellCraftingDisplay, glyphCastingPanel;
    private PlayerMovementManager movementManager;
    private HealthBar healthBar, manaBar, boostCooldownBar1, boostCooldownBar2;
    private GameObject boostIndicator1, boostIndicator2;
    private bool hasBoostCharge1, hasBoostCharge2;
    private float boostCharge1, boostCharge2;
    private Crosshair crosshair;
    private float maxMana, maxHealth, defaultManaRegen;
    private Spell cachedSpellComponent;
    private CharacterUI characterUI;
    private AuricaCaster auricaCaster;
    private ShieldSpell currentShield;
    private CustomCameraWork cameraWorker;
    private AimpointAnchor aimPointAnchorManager;
    private float aoeDamageTotal=0f, aoeDamageTick=0f, accumulatingDamageTimout=1f, accumulatingDamageTimer=0f;
    private DamagePopup accumulatingDamagePopup;
    private string lastPlayerToDamageSelf;
    private float immunityTimer = 0f;
    private string playerTitle, playerTitleColour;
    private bool titleSet = false;
    


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

    // Prevent moving, not including displacement
    [HideInInspector]
    public bool rooted;
    Coroutine rootRoutine;
    bool rootRoutineRunning;

    // Prevent displacement
    [HideInInspector]
    public bool grounded;
    Coroutine groundedRoutine;
    bool groundedRoutineRunning;

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
    private float manaRestorationDuration, manaRestorationPercentage;
    Coroutine manaRestorationRoutine;
    bool manaRestorationRoutineRunning;

    [HideInInspector]
    public bool camouflaged = false;
    Coroutine camouflageRoutine;
    bool camouflagedRoutineRunning;

     [HideInInspector]
    public bool slowFall;
    private float slowFallDuration;
    Coroutine slowFallRoutine;
    bool slowFallRoutineRunning;



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            // CRITICAL DATA
            stream.SendNext(Health);
            stream.SendNext(Mana);

            // Boost data
            stream.SendNext(hasBoost);

            // Status Effect data
            stream.SendNext(slowed);
            stream.SendNext(hastened);
            stream.SendNext(rooted);
            stream.SendNext(grounded);
            stream.SendNext(silenced);
            stream.SendNext(stunned);
            stream.SendNext(weakened);
            stream.SendNext(strengthened);
            stream.SendNext(fragile);
            stream.SendNext(tough);
            stream.SendNext(manaRestorationChange);
            stream.SendNext(camouflaged);
            stream.SendNext(slowFall);
        } else {
            // Network player, receive data
            // CRITICAL DATA
            this.Health = (float)stream.ReceiveNext();
            this.Mana = (float)stream.ReceiveNext();

            // Boost data
            this.hasBoost = (bool)stream.ReceiveNext();

            // Status Effect data
            this.slowed = (bool)stream.ReceiveNext();
            this.hastened = (bool)stream.ReceiveNext();
            this.rooted = (bool)stream.ReceiveNext();
            this.grounded = (bool)stream.ReceiveNext();
            this.silenced = (bool)stream.ReceiveNext();
            this.stunned = (bool)stream.ReceiveNext();
            this.weakened = (bool)stream.ReceiveNext();
            this.strengthened = (bool)stream.ReceiveNext();
            this.fragile = (bool)stream.ReceiveNext();
            this.tough = (bool)stream.ReceiveNext();
            this.manaRestorationChange = (bool)stream.ReceiveNext();
            this.camouflaged = (bool)stream.ReceiveNext();
            this.slowFall = (bool)stream.ReceiveNext();
        }
    }

    void Start() {
        maxMana = Mana;
        maxHealth = Health;

        boostCharge1 = BoostCooldown;
        boostCharge2 = BoostCooldown;

        ManaRegen *= GameManager.GLOBAL_PLAYER_MANA_REGEN_MULTIPLIER;
        ManaRegenGrowthRate *= GameManager.GLOBAL_PLAYER_MANA_GROWTH_MULTIPLIER;

        defaultManaRegen = ManaRegen;

        // Follow the local player character with the camera
        if (photonView.IsMine) {
            cameraWorker = this.gameObject.GetComponent<CustomCameraWork>();
            if (cameraWorker != null) {
                cameraWorker.OnStartFollowing();
            } else {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }
        }

        if (PlayerUiPrefab != null) {
            GameObject _uiGo = Instantiate(PlayerUiPrefab, transform.position + new Vector3(0f, 2.15f, 0f), transform.rotation);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            _uiGo.transform.SetParent(transform);
            characterUI = _uiGo.GetComponent<CharacterUI>();
            materialManager.SetUI(characterUI);

            if (photonView.IsMine) {
                _uiGo.SetActive(false);
            } else {
                RequestTitle();
            }
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
        boostCooldownBar1 = GameObject.Find("BoostCooldownBar1").GetComponent<HealthBar>();
        boostIndicator1 = GameObject.Find("BoostIndicator1");
        boostCooldownBar2 = GameObject.Find("BoostCooldownBar2").GetComponent<HealthBar>();
        boostIndicator2 = GameObject.Find("BoostIndicator2");
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
        if (photonView.IsMine) {
            // Allowed to look at and craft spells while dead, but nothing else
            if (Health <= 0f) return;

            // Compute AoE tick damage and total sum, if no new damage ticks come in for a while 
            if (aoeDamageTotal == 0f && aoeDamageTick > 0f) {
                // Add damage tick to the total and reset the tick
                aoeDamageTotal += aoeDamageTick;
                aoeDamageTick = 0f;

                // Initiate an accumulating damage popup
                accumulatingDamagePopup = characterUI.CreateAccumulatingDamagePopup(aoeDamageTotal);
            } else if (aoeDamageTotal > 0f && aoeDamageTick > 0f) {
                // Add damage tick to the total and reset the tick
                aoeDamageTotal += aoeDamageTick;
                aoeDamageTick = 0f;

                // Update the accumulating damage popup
                accumulatingDamagePopup.AccumulatingDamagePopup(aoeDamageTotal);

                // Reset the tick timout timer
                accumulatingDamageTimer = 0f;
            } else if (aoeDamageTotal > 0f && aoeDamageTick == 0f && accumulatingDamageTimer < accumulatingDamageTimout) {
                // If there is a running total but no new damage tick, start the timer to end the accumulating process
                accumulatingDamageTimer += Time.deltaTime;
            } else if (aoeDamageTotal > 0f && aoeDamageTick == 0f && accumulatingDamageTimer >= accumulatingDamageTimout) {
                // Timout has been reached for new damage ticks, end the accumulation process and reset all variables
                accumulatingDamagePopup.EndAccumulatingDamagePopup();
                aoeDamageTotal = 0f;
                aoeDamageTick = 0f;
                accumulatingDamageTimer = 0f;
            }

            // Lerp Mana regen towards default if no effects are applied. Because floating point math can create rounding error problems with statuses
            if (!manaRestorationRoutineRunning && appliedManaRestorationChangeEffects.Count == 0 && !Mathf.Approximately(ManaRegen, defaultManaRegen)){
                ManaRegen = Mathf.Lerp(ManaRegen, defaultManaRegen, 0.05f * Time.deltaTime);
                if (Mathf.Approximately(ManaRegen, defaultManaRegen)) manaRestorationChange = false;
            } 

            if (GameUIPanelManager.Instance && GameUIPanelManager.Instance.ShouldProcessInputs()) this.ProcessInputs();

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Backspace)) {
                AuricaCaster.LocalCaster.RemoveLastComponent();
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("m")) {
                MasteryManager.Instance.SyncMasteries();
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
                    StopChannelling();
                    CastFizzle();
                }
            }

            // Regen mana if below maxMana
            if (Mana < maxMana) {
                Mana += ManaRegen * Time.deltaTime * ((1.1f - Mana / maxMana) * ManaRegenGrowthRate);
                if (Mana > maxMana) Mana = maxMana;
            }

            if (Mana < 0f) Mana = 0f;

            // Reduce cooldown for boost charges
            if (boostCharge1 < BoostCooldown) {
                boostCharge1 += Time.deltaTime * boostCooldownMultiplier;
                if (hasBoostCharge1) {
                    boostIndicator1.SetActive(false);
                }
                hasBoostCharge1 = false;
            } else if (!hasBoostCharge1) {
                boostIndicator1.SetActive(true);
                hasBoostCharge1 = true;
            }

            if (boostCharge2 < BoostCooldown) {
                boostCharge2 += Time.deltaTime * boostCooldownMultiplier;
                if (hasBoostCharge2) {
                    boostIndicator2.SetActive(false);
                }
                hasBoostCharge2 = false;
            } else if (!hasBoostCharge2) {
                boostIndicator2.SetActive(true);
                hasBoostCharge2 = true;
            }

            hasBoost = hasBoostCharge1 || hasBoostCharge2;


            // Display health and mana values
            healthBar.SetHealth(Health);
            manaBar.SetHealth(Mana);

            // Display Boost cooldowns
            boostCooldownBar1.SetHealth(boostCharge1);
            boostCooldownBar2.SetHealth(boostCharge2);
        }
    }

    public void FixedUpdate () {
        if (Health <= 0f && !dead) {
            Die();
        }
        if (photonView.IsMine) {
            if (respawning && !dead && immunityTimer <= 2f) {
                immunityTimer += Time.deltaTime;
            } else if (respawning && !dead && immunityTimer > 2f) {
                respawning = false;
                immunityTimer = 0f;
            }
        } else {
            if (stunned && animator.speed > 0f) {
                movementManager.Stun(true);
            } else if (!stunned && animator.speed == 0f) {
                movementManager.Stun(false);
            }
        }
    }

    public string GetUniqueName() {
        return photonView.Owner.NickName+"-|-"+photonView.ViewID;
    }

    // When the local players title has been retreived from PlayFab, send their title to everyone
    public void SendTitle(string title, string titleColour) {
        titleSet = true;
        playerTitle = title;
        playerTitleColour = titleColour;        
    }

    [PunRPC]
    public void SendRemoteTitle(string title, string titleColour) {
        if (characterUI != null) characterUI.SetTitle(title, titleColour);
    }

    // When spawning a remote player, ask for their title
    public void RequestTitle() {
        photonView.RPC("RequestRemoteTitle", RpcTarget.Others);
    }

    [PunRPC]
    public void RequestRemoteTitle() {
        if (titleSet) photonView.RPC("SendRemoteTitle", RpcTarget.All, playerTitle, playerTitleColour);;
    }

    public void HardReset() {
        if (!photonView.IsMine) return;
        // Reset health and mana
        Health = maxHealth;
        Mana = maxMana;
        healthBar.SetHealth(Health);
        manaBar.SetHealth(Mana);

        boostCharge1 = BoostCooldown;
        boostCharge2 = BoostCooldown;

        // Reset any spell casts
        auricaCaster.ResetCast();
        if (spellCraftingDisplay != null) {
            SpellCraftingUIDisplay sp = spellCraftingDisplay.GetComponent<SpellCraftingUIDisplay>();
            if (sp != null) sp.ClearSpell();
        }
        StopChannelling();

        // TODO: Clear any status effects
        Cleanse();
    }

    public void Die() {
        if (dead) return;
        dead = true;
        respawning = true;
        animator.enabled = false;
        RootBone.transform.parent = null;
        healthBar.SetHealth(0);
        manaBar.SetHealth(0);

        if (DeathSound != null) DeathSound.Play();

        GameManager.Instance.playerDeath(this);
        if (DeathmatchGameManager.Instance != null) {
            DeathmatchGameManager.Instance.playerDeath(this);
            if (photonView.IsMine) DeathmatchGameManager.Instance.localPlayerDeath(GetUniqueName());
        }
        if (FreeForAllGameManager.Instance != null) {
            FreeForAllGameManager.Instance.playerDeath(this);
            if (photonView.IsMine) FreeForAllGameManager.Instance.localPlayerDeath(lastPlayerToDamageSelf);
        }

        ObjectiveSphere[] objectiveSpheres = FindObjectsOfType<ObjectiveSphere>();
        foreach( ObjectiveSphere os in objectiveSpheres) os.DropIfHolding(this); 
    }

    public void Respawn() {
        // if (!photonView.IsMine) return;
        dead = false;
        animator.enabled = true;
        RootBone.transform.position = transform.position;
        RootBone.transform.parent = transform;

        HardReset();
    }

    [PunRPC]
    public void TeleportEffect(Vector3 newPosition) {
        if (!photonView.IsMine) return;
        // Debug.Log("Teleporting " + gameObject + "  to " + newPosition);
        transform.position = newPosition;
    }

    public void Teleport(Vector3 newPosition) {
        if (!photonView.IsMine) return;
        // Debug.Log("Teleporting " + gameObject + "  to " + newPosition);
        transform.position = newPosition;
    }

    public void Teleport(Transform newPosition) {
        if (!photonView.IsMine) return;
        // Debug.Log("Teleporting " + gameObject + "  to pos: " + newPosition.position + "with rotation");
        transform.position = newPosition.position;
        transform.rotation = newPosition.rotation;
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
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID="") {
        if (!photonView.IsMine || isShielded) return;
        if (ownerID != "") {
            lastPlayerToDamageSelf = ownerID;
            // Debug.Log("Took damage from "+ownerID);
        }
        
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
        if (respawning) return;
        float finalDamage = GetFinalDamage(damage, spellDistribution);
        Health -= finalDamage;

        // Play hit effects
        DamageVignette.Instance.FlashDamage(finalDamage);
        if (cameraWorker != null) cameraWorker.Shake(finalDamage / 100f, 0.198f);
        if (HitSound != null && finalDamage > 2f) HitSound.Play();

        // Create damage popup
        if (finalDamage > 5f) {
            characterUI.CreateDamagePopup(finalDamage);
        } else {
            // For an AoE spell tick we do something different
            aoeDamageTick += finalDamage;
        }

        if (finalDamage > 2f) Debug.Log("Take Damage --  pre-resistance: " + damage + "    post-resistance: " + finalDamage + "     resistance total: " + finalDamage / damage);
    }

    IEnumerator TakeDirectDoTDamage(float damage, float duration, ManaDistribution spellDistribution) {
        float damagePerSecond = damage / duration;
        // Debug.Log("Take dot damage: "+damage+" duration: "+duration+ "     resistance total: " + aura.GetDamage(damage, spellDistribution) / damage);
        while (duration > 0f) {
            TakeDamage(damagePerSecond * Time.deltaTime, spellDistribution);
            duration -= Time.deltaTime;
            if (Health <= 0) break;
            yield return new WaitForFixedUpdate();
        }
    }

    float GetFinalDamage(float damage, ManaDistribution spellDistribution) {
        // Modify the damage if the player is fragile or tough
        if (fragile) damage *= (1 + fragilePercentage);
        if (tough) damage *= Mathf.Max(0, (1 - toughPercentage));

        // Apply the damage
        return aura.GetDamage(damage, spellDistribution) * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER;
    }

    public void FlashHitMarker(bool majorDamage) {
        crosshair.FlashHitMarker(majorDamage);
        if (majorDamage && HitMarkerSound != null) HitMarkerSound.Play();
        if (!majorDamage && HitMarkerAoESound != null && !HitMarkerAoESound.isPlaying) HitMarkerAoESound.Play();
    }

    public void PlayCultivationEffect() {
        PhotonNetwork.Instantiate("XCollision_Cultivate", transform.position, transform.rotation);
    }

































    /*  --------------------  SPELLCASTING ------------------------ */

    void ProcessInputs() {
        if (silenced || stunned || !photonView.IsMine) return;

        if (movementManager.CanCast()) {
            if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot1)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("1"));
            } else if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot2)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("2"));
            } else if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot3)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("3"));
            } else if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot4)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("4"));
            } else if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotE)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("e"));
            } else if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotQ)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("q"));
            } else if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotR)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("r"));
            } else if (InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotF)) {
                CastAuricaSpell(auricaCaster.CastBindSlot("f"));
            }
        } else if (isChannelling && (
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot1) ||
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot2) ||
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot3) ||
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlot4) ||
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotQ) ||
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotE) ||
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotR) ||
            InputManager.Instance.GetKeyDown(KeybindingActions.SpellSlotF))) {
            StopChannelling();
            auricaCaster.ResetCast();
        }

        // Use Boost
        if (InputManager.Instance.GetKeyDown(KeybindingActions.Boost) || Input.GetKeyDown(KeyCode.Mouse2)) {
            if (hasBoost && !stunned && !silenced && !isShielded) {
                movementManager.Boost();
                PhotonNetwork.Instantiate("XCollision_Boost", transform.position, transform.rotation);
                if (hasBoostCharge1) {
                    boostCharge1 = 0f;
                } else if (hasBoostCharge2) {
                    boostCharge2 = 0f;
                }
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown("j")) {
            boostCharge1 = BoostCooldown;
            boostCharge2 = BoostCooldown;
        }

        if (Input.GetKeyDown("\\")) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                Debug.LogWarning("Slowed: "+slowed+"\n Hastened: "+hastened+"\n Weakenesses: "+weaknesses.ToString()+"\n Strengths: "+strengths.ToString()+"\n Fragile: "+fragilePercentage+"\n Tough: "+toughPercentage+"\n Mana Altered: "+manaRestorationPercentage);
            } else {
                Mana += 400;
            }
        }

        if (InputManager.Instance.GetKeyDown(KeybindingActions.Cast)) {
            if (!isChannelling) {
                if (movementManager.CanCast()) CastAuricaSpell(auricaCaster.CastFinal());
            } else {
                StopChannelling();
                auricaCaster.ResetCast();
            }
        }
    }

    public void SetMaxMana(float newMax) {
        if (photonView.IsMine) {
            maxMana = newMax;
            Mana = newMax;
            manaBar.SetMaxHealth(maxMana);

            // Debug.Log("New Max mana: " + maxMana + " " + Mana);
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

    void StopChannelling() {
        movementManager.StopChannelling();
        EndChannel();
    }

    [PunRPC]
    public void BreakShield() {
        StopChannelling();
    }

    void CastFizzle() {
        PhotonNetwork.Instantiate("XCollision_Fizzle", transform.position, transform.rotation);
    }

    void CastAuricaSpell(AuricaSpell spell) {
        if (!photonView.IsMine) return;
        if (isChannelling) {
            StopChannelling();
            auricaCaster.ResetCast();
            return;
        }
        if (spell == null) {
            // Debug.Log("Invalid cast");
            CastFizzle();
            auricaCaster.ResetCast();
            return;
        }

        // Fail before animation if the player does not have sufficient mana.
        if (Mana - auricaCaster.GetManaCost() < 0f) {
            // Debug.Log("Insufficient Mana for spell!");
            CastFizzle();
            manaBar.BlinkText();
            auricaCaster.ResetCast();
            return;
        }

        string linkedSpellResource = spell.linkedSpellResource;

        // Check if the spell is a mastery spell, and if it is check if the player has sufficient mastery
        if (spell.isMasterySpell) {
            if (!MasteryManager.Instance.HasMasteryForSpell(spell)) {
                linkedSpellResource = spell.masteryFailedSpellResource;
                TipWindow.Instance.ShowTip("Not Enough Mastery", "To cast this spell properly you need more mastery with "+spell.masteryCategory.ToString()+" spells", 4f);
                //Debug.Log("Mastery check failed");
            } else {
                //Debug.Log("Mastery check passed");
            }

        }

        // Load spell resource
        GameObject dataObject = Resources.Load<GameObject>(linkedSpellResource);
        Spell foundSpell = dataObject != null ? dataObject.GetComponent<Spell>() : null;

        if (foundSpell == null) {
            if (linkedSpellResource.StartsWith("XCollision")) PhotonNetwork.Instantiate(linkedSpellResource, transform.position, transform.rotation);
            return;
        }

        if (foundSpell.Damage > 0f) Debug.Log("Spell " + spell.c_name+" Adjusted Damage/Mana: "+((foundSpell.Damage * auricaCaster.GetSpellStrength()) / auricaCaster.GetManaCost())+" Base Damage/Mana: "+(foundSpell.Damage / auricaCaster.GetManaCost()));

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
            currentSpellCast = linkedSpellResource;
            movementManager.PlayCastingAnimation(foundSpell.CastAnimationType);
            particleManager.PlayHandParticle(foundSpell.CastAnimationType, spell.manaType);
            if (CastingSound != null) CastingSound.Play();
        } else {
            // If the spell is channelled, channel it immediately if its a shield, else start the channelling animation
            currentChannelledSpell = linkedSpellResource;
            ShieldSpell shield = dataObject.GetComponent<ShieldSpell>(); 
            if (shield != null) {
                ChannelShield();
            } else {
                movementManager.PlayCastingAnimation(foundSpell.CastAnimationType);
                particleManager.PlayHandParticle(foundSpell.CastAnimationType, spell.manaType);
                if (CastingSound != null) CastingSound.Play();
            }
        }

        // ADD MASTERY
        if (MasteryManager.Instance != null) MasteryManager.Instance.AddMasteries(spell.masteries);

        if (spellCraftingDisplay != null) {
            SpellCraftingUIDisplay sp = spellCraftingDisplay.GetComponent<SpellCraftingUIDisplay>();
            if (sp != null) sp.ClearSpell();
        }
    }

    void CastSpell() {
        if (photonView.IsMine && !silenced && !stunned) {
            if (Mana - auricaCaster.GetManaCost() > 0f) {
                Transform aimTransform = !currentSpellIsOpponentTargeted ? currentCastingTransform : GetPlayerWithinAimTolerance(2f) != null ? GetPlayerWithinAimTolerance(2f).transform : null;
                if (aimTransform == null) {
                    CastFizzle();
                    auricaCaster.ResetCast();
                    TipWindow.Instance.ShowTip("No Target Found", "This spell is a targeted spell, look at a player or monster for this spell to target them.", 3f);
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

                    GameObject target = GetPlayerWithinAimTolerance(2f);
                    if (target != null) {
                        // Debug.Log("Target found: " + target);
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

    void ChannelSpell() {
        if (photonView.IsMine) {
            if (!isChannelling && currentChannelledSpell != null && !silenced && !stunned) {
                if (Mana - auricaCaster.GetManaCost() > 0f) {
                    Transform aimTransform = !currentSpellIsOpponentTargeted ? currentCastingTransform : GetPlayerWithinAimTolerance(2f) != null ? GetPlayerWithinAimTolerance(2f).transform : null;
                    if (aimTransform == null) {
                        CastFizzle();
                        auricaCaster.ResetCast();
                        return;
                    }
                    isChannelling = true;
                    channelledSpell = PhotonNetwork.Instantiate(currentChannelledSpell, aimTransform.position, aimTransform.rotation);
                    Mana -= auricaCaster.GetManaCost();

                    Spell foundSpell = channelledSpell.GetComponent<Spell>();
                    if (foundSpell != null) {
                        foundSpell.SetSpellStrength(auricaCaster.GetSpellStrength());
                        foundSpell.SetSpellDamageModifier(aura.GetInnateStrength() + strengths - weaknesses);
                        foundSpell.SetOwner(gameObject);
                        Mana += foundSpell.ManaRefund;
                    }

                    ChannelledSpell channelSpell = channelledSpell.GetComponent<ChannelledSpell>();
                    if (channelledSpell != null) {
                        if (currentSpellIsSelfTargeted) {
                            currentSpellIsSelfTargeted = false;
                            channelSpell.SetTarget(gameObject);
                        } else if (currentSpellIsOpponentTargeted) {
                            currentSpellIsOpponentTargeted = false;
                            GameObject target = GetPlayerWithinAimTolerance(2f);
                            if (target != null) {
                                channelSpell.SetTarget(target);
                            }
                        }
                    }

                    movementManager.StartChannelling(foundSpell.CastAnimationType, false);

                    
                } else {
                    CastFizzle();
                }
                auricaCaster.ResetCast();
            }
        }
    }

    void ChannelShield() {
        if (photonView.IsMine) {
            if (!isChannelling && currentChannelledSpell != null && !silenced && !stunned) {
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

                    movementManager.StartChannelling(foundSpell.CastAnimationType, currentShield != null);
                } else {
                    CastFizzle();
                }
                auricaCaster.ResetCast();
            }
        }
    }

    public void EndChannel() {
        if (!photonView.IsMine) return;
        isChannelling = false;
        isShielded = false;
        try {
            if (currentShield != null) {
                currentShield.Break();
                currentShield = null;
            } else if (channelledSpell != null) {
                ChannelledSpell spellChannel = channelledSpell.GetComponent<ChannelledSpell>();
                if (spellChannel != null) {
                    spellChannel.EndChannel();
                } else {
                    PhotonNetwork.Destroy(channelledSpell);
                }
            }
        } catch (System.Exception e){
            Debug.LogError("ERROR STOPPING CHANNEL: "+e);
        }
        channelledSpell = null;
    }

    public void EndCast() {
        particleManager.StopHandParticles(!sneaking);
    }























    /*  --------------------  STATUS EFFECTS ------------------------ */


    // Heal - restore health by a flat value and/or a percentage of missing health
    [PunRPC]
    void Heal(float flat, float percentage) {
        if (photonView.IsMine) {
            // Debug.Log("HEAL FOR: ["+flat+"] flat + ["+percentage+"] percent missing health");
            healing += (flat + ((maxHealth - Health) * percentage)) * GameManager.GLOBAL_SPELL_HEALING_MULTIPLIER;
        }
    }






    // Displace - move the player along a local or worldspace direction vector
    [PunRPC]
    void Displace(Vector3 direction, float distance, float speed, bool isWorldSpaceDirection) {
        if (photonView.IsMine) {
            StopChannelling();
            movementManager.Displace(direction, distance, speed, isWorldSpaceDirection);
        }
    }






    // ManaDrain - drain mana by a flat value and/or a percentage of missing health
    [PunRPC]
    void ManaDrain(float flat, float percentage) {
        if (photonView.IsMine) {
            // Debug.Log("Draining Mana by - flat: "+flat+" & percentage: "+percentage+" = "+(Mana * percentage));
            Mana -= flat + (Mana * percentage);
        }
    }







    // Slow - Decrease animation speed
    List<string> appliedSlowEffects = new List<string>();

    [PunRPC]
    void Slow(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            // If a status effect from the same Identifier has already been applied, do not apply another.
            if (appliedSlowEffects.Contains(Identifier)) {
                // Debug.Log("Nullify duplicate {SLOW} from ["+Identifier+"].");
                return;
            }
            appliedSlowEffects.Add(Identifier);

            slowed = true;
            slowRoutine = StartCoroutine(SlowRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator SlowRoutine(string Identifier, float duration, float percentage) {
        slowRoutineRunning = true;
        animator.speed *= 1f - percentage;
        movementManager.ChangeMovementSpeed(1f - percentage);
        yield return new WaitForSeconds(duration);
        animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        movementManager.ResetMovementSpeed();
        slowed = false;
        slowRoutineRunning = false;

        // Remove the Identifier from the applied effects list
        if (appliedSlowEffects.Contains(Identifier)) appliedSlowEffects.Remove(Identifier);
    }
    [PunRPC]
    public void ContinuousSlow(string Identifier, float percentage) {
        if (photonView.IsMine) {
            // If a status effect from the same Identifier has already been applied, do not apply another.
            if (appliedSlowEffects.Contains(Identifier)) {
                // Debug.Log("Nullify duplicate {SLOW} from ["+Identifier+"].");
                return;
            }
            Debug.Log("Apply slow: "+percentage);
            appliedSlowEffects.Add(Identifier);

            slowed = true;
            animator.speed *= 1f - percentage;
            movementManager.ChangeMovementSpeed(1f - percentage);
        }
    }
    [PunRPC]
    public void EndContinuousSlow(string Identifier) {
        if (photonView.IsMine) {
            if (appliedSlowEffects.Contains(Identifier)){
                appliedSlowEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            slowed = false;
        }
    }






    // Hasten - Increase animation speed
    List<string> appliedHasteEffects = new List<string>();

    [PunRPC]
    void Hasten(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedHasteEffects.Contains(Identifier)) return;
            appliedHasteEffects.Add(Identifier);

            hastened = true;
            hasteRoutine = StartCoroutine(HastenRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator HastenRoutine(string Identifier, float duration, float percentage) {
        hasteRoutineRunning = true;
        animator.speed *= 1f + percentage;
        movementManager.ChangeMovementSpeed(1f + percentage);
        yield return new WaitForSeconds(duration);
        animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        movementManager.ResetMovementSpeed();
        hastened = false;
        hasteRoutineRunning = false;

        if (appliedHasteEffects.Contains(Identifier)) appliedHasteEffects.Remove(Identifier);
    }
    [PunRPC]
    public void ContinuousHasten(string Identifier, float percentage) {
        if (photonView.IsMine) {
            if (appliedHasteEffects.Contains(Identifier)) return;
            appliedHasteEffects.Add(Identifier);

            hastened = true;
            animator.speed *= 1f + percentage;
            movementManager.ChangeMovementSpeed(1f + percentage);
        }
    }
    [PunRPC]
    public void EndContinuousHasten(string Identifier) {
        if (photonView.IsMine) {
            if (appliedHasteEffects.Contains(Identifier)){
                appliedHasteEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            hastened = false;
        }
    }






    // Rooted - Prevent movement, including movement spells
    [PunRPC]
    void Root(float duration) {
        if (photonView.IsMine && !rooted && !isShielded) {
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

    
    
    
    // Grounded - Cannot be displaced
    [PunRPC]
    void Ground(float duration) {
        if (photonView.IsMine && !isShielded) {
            groundedRoutine = StartCoroutine(GroundedRoutine(duration));
        }
    }
    IEnumerator GroundedRoutine(float duration) {
        groundedRoutineRunning = true;
        grounded = true;
        movementManager.Ground(true);
        yield return new WaitForSeconds(duration);
        grounded = false;
        groundedRoutineRunning = false;
        movementManager.Ground(false);
    }
    [PunRPC]
    public void ContinuousGround() {
        if (photonView.IsMine) {
            grounded = true;
            movementManager.Ground(true);
        }
    }
    [PunRPC]
    public void EndContinuousGround() {
        if (photonView.IsMine) {
            grounded = false;
            movementManager.Ground(false);
        }
    }







    // Stunned - Prevent moving and spellcasting, basically a stacked root and silence
    [PunRPC]
    void Stun(float duration) {
        if (photonView.IsMine && !stunned && !isShielded) {
            stunned = true;
            StopChannelling();
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
            StopChannelling();
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
        if (photonView.IsMine && !silenced && !isShielded) {
            silenced = true;
            StopChannelling();
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
            StopChannelling();
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
    List<string> appliedWeakenEffects = new List<string>();

    [PunRPC]
    void Weaken(string Identifier, float duration, string weaknessString) {
        if (photonView.IsMine && !isShielded) {
            if (appliedWeakenEffects.Contains(Identifier)) return;
            appliedWeakenEffects.Add(Identifier);

            ManaDistribution weaknessDist = new ManaDistribution(weaknessString);
            weakenRoutine = StartCoroutine(WeakenRoutine(Identifier, duration, weaknessDist));
        }
    }
    IEnumerator WeakenRoutine(string Identifier, float duration, ManaDistribution weaknessDist) {
        weakenRoutineRunning = true;
        weakened = true;
        weaknesses += weaknessDist;
        yield return new WaitForSeconds(duration);
        weaknesses -= weaknessDist;
        if (weaknesses.GetAggregate() <= 0.1f) weakened = false;
        weakenRoutineRunning = false;

        if (appliedWeakenEffects.Contains(Identifier)) appliedWeakenEffects.Remove(Identifier);
    }
    [PunRPC]
    public void ContinuousWeaken(string Identifier, string weakString) {
        if (photonView.IsMine) {
            if (appliedWeakenEffects.Contains(Identifier)) return;
            appliedWeakenEffects.Add(Identifier);
            
            ManaDistribution weakDist = new ManaDistribution(weakString);
            weakened = true;
            weaknesses += weakDist;
            //Debug.Log("New Strength: " + weaknesses.ToString());
        }
    }

    [PunRPC]
    public void EndContinuousWeaken(string Identifier, string weakString) {
        if (photonView.IsMine) {
            if (appliedWeakenEffects.Contains(Identifier)){
                appliedWeakenEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            ManaDistribution weakDist = new ManaDistribution(weakString);
            weaknesses -= weakDist;
            if (weaknesses.GetAggregate() <= 0.1f) weakened = false;
            //Debug.Log("New Strength after end: " + weaknesses.ToString());
        }
    }






    // Strengthen - Deal increased damage of given mana types
    List<string> appliedStrengthenEffects = new List<string>();

    [PunRPC]
    void Strengthen(string Identifier, float duration, string strengthString) {
        if (photonView.IsMine && !isShielded) {
            if (appliedStrengthenEffects.Contains(Identifier)) {
                // Debug.Log("Nullify duplicate {STRENGTHEN} from ["+Identifier+"].");
                return;
            }
            appliedStrengthenEffects.Add(Identifier);

            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            strengthenRoutine = StartCoroutine(StrengthenRoutine(Identifier, duration, strengthDist));
        }
    }
    IEnumerator StrengthenRoutine(string Identifier, float duration, ManaDistribution strengthDist) {
        strengthenRoutineRunning = true;
        strengthened = true;
        strengths += strengthDist;
        //Debug.Log("New Strength: " + strengths.ToString());
        yield return new WaitForSeconds(duration);
        strengths -= strengthDist;
        if (strengths.GetAggregate() <= 0.1f) strengthened = false;
        strengthenRoutineRunning = false;

        if (appliedStrengthenEffects.Contains(Identifier)) appliedStrengthenEffects.Remove(Identifier);
    }

    [PunRPC]
    public void ContinuousStrengthen(string Identifier, string strengthString) {
        if (photonView.IsMine) {
            if (appliedStrengthenEffects.Contains(Identifier)) {
                // Debug.Log("Nullify duplicate {STRENGTHEN} from ["+Identifier+"].");
                return;
            }
            appliedStrengthenEffects.Add(Identifier);

            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            strengthened = true;
            strengths += strengthDist;
            //Debug.Log("New Strength: " + strengths.ToString());
        }
    }

    [PunRPC]
    public void EndContinuousStrengthen(string Identifier, string strengthString) {
        if (photonView.IsMine) {
            if (appliedStrengthenEffects.Contains(Identifier)){
                appliedStrengthenEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            strengths -= strengthDist;
            if (strengths.GetAggregate() <= 0.1f) strengthened = false;
            //Debug.Log("New Strength after end: " + strengths.ToString());
        }
    }






    // Fragile - Take increased damage from all sources
    List<string> appliedFragileEffects = new List<string>();

    [PunRPC]
    void Fragile(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedFragileEffects.Contains(Identifier)) return;
            appliedFragileEffects.Add(Identifier);

            fragile = true;
            fragilePercentage = percentage;
            fragileRoutine = StartCoroutine(FragileRoutine(Identifier, duration));
        }
    }
    IEnumerator FragileRoutine(string Identifier, float duration) {
        fragileRoutineRunning = true;
        yield return new WaitForSeconds(duration);
        fragile = false;
        fragilePercentage = 0f;
        fragileRoutineRunning = false;

        if (appliedFragileEffects.Contains(Identifier)) appliedFragileEffects.Remove(Identifier);
    }
    [PunRPC]
    public void ContinuousFragile(string Identifier, float percentage) {
        if (photonView.IsMine) {
            if (appliedFragileEffects.Contains(Identifier)) return;
            appliedFragileEffects.Add(Identifier);

            fragile = true;
            fragilePercentage = percentage;
        }
    }

    [PunRPC]
    public void EndContinuousFragile(string Identifier) {
        if (photonView.IsMine) {
            if (appliedFragileEffects.Contains(Identifier)){
                appliedFragileEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            fragile = false;
            fragilePercentage = 0f;
        }
    }






    // Toughen - Take decreased damage from all sources
    List<string> appliedToughEffects = new List<string>();

    [PunRPC]
    void Tough(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedToughEffects.Contains(Identifier)) return;
            appliedToughEffects.Add(Identifier);

            toughRoutine = StartCoroutine(ToughRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator ToughRoutine(string Identifier, float duration, float percentage) {
        toughRoutineRunning = true;
        tough = true;
        toughPercentage = percentage;
        yield return new WaitForSeconds(duration);
        tough = false;
        toughPercentage = 0f;
        toughRoutineRunning = false;

        if (appliedToughEffects.Contains(Identifier)) appliedToughEffects.Remove(Identifier);
    }
    [PunRPC]
    public void ContinuousTough(string Identifier, float percentage) {
        if (photonView.IsMine) {
            if (appliedToughEffects.Contains(Identifier)) return;
            appliedToughEffects.Add(Identifier);

            tough = true;
            toughPercentage = percentage;
        }
    }
    [PunRPC]
    public void EndContinuousTough(string Identifier) {
        if (photonView.IsMine) {
            if (appliedToughEffects.Contains(Identifier)){
                appliedToughEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            tough = false;
            toughPercentage = 0f;
        }
    }






    // ManaRestoration - change the mana regen rate by a multiplier value
    List<string> appliedManaRestorationChangeEffects = new List<string>();

    [PunRPC]
    void ManaRestoration(string Identifier, float duration, float restorationPercentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedManaRestorationChangeEffects.Contains(Identifier)) return;
            appliedManaRestorationChangeEffects.Add(Identifier);

            manaRestorationRoutine = StartCoroutine(ManaRestorationRoutine(Identifier, duration, restorationPercentage));
        }
    }
    IEnumerator ManaRestorationRoutine(string Identifier, float duration, float restorationPercentage) {
        // Debug.Log("Mana restoration mult: "+restorationPercentage+"     for: "+duration+" seconds.");
        manaRestorationRoutineRunning = true;
        manaRestorationChange = true;
        ManaRegen *= restorationPercentage;
        manaRestorationPercentage = restorationPercentage;
        // Debug.Log("New Mana Regen : " + ManaRegen);
        yield return new WaitForSeconds(duration);
        if (ManaRegen != defaultManaRegen) ManaRegen /= restorationPercentage;
        if (Mathf.Approximately(ManaRegen, defaultManaRegen)) manaRestorationChange = false;
        // Debug.Log("New Mana Regen after end: " + ManaRegen);
        manaRestorationRoutineRunning = false;
        manaRestorationPercentage = 0f;

        if (appliedManaRestorationChangeEffects.Contains(Identifier)) appliedManaRestorationChangeEffects.Remove(Identifier);
    }

    [PunRPC]
    public void ContinuousManaRestoration(string Identifier, float restorationPercentage) {
        if (photonView.IsMine) {
            if (appliedManaRestorationChangeEffects.Contains(Identifier)) return;
            appliedManaRestorationChangeEffects.Add(Identifier);

            manaRestorationChange = true;
            manaRestorationPercentage = restorationPercentage;
            ManaRegen *= restorationPercentage;
            // Debug.Log("New Mana Regen : " + ManaRegen);
        }
    }

    [PunRPC]
    public void EndContinuousManaRestoration(string Identifier, float restorationPercentage) {
        if (photonView.IsMine) {
            if (appliedManaRestorationChangeEffects.Contains(Identifier)){
                appliedManaRestorationChangeEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            if (ManaRegen != defaultManaRegen) ManaRegen /= restorationPercentage;
            manaRestorationPercentage = 0f;
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





    // SlowFall - Take decreased damage from all sources
    List<string> appliedSlowFallEffects = new List<string>();

    [PunRPC]
    void SlowFall(string Identifier, float duration, float percentage) {
        if (photonView.IsMine) {
            if (appliedSlowFallEffects.Contains(Identifier)){
                // Debug.Log("Nullify duplicate {SLOW FALL} from ["+Identifier+"].");
                return;
            } 
            appliedSlowFallEffects.Add(Identifier);

            slowFallRoutine = StartCoroutine(SlowFallRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator SlowFallRoutine(string Identifier, float duration, float percentage) {
        slowFallRoutineRunning = true;
        slowFall = true;
        movementManager.SlowFall(true, percentage);
        yield return new WaitForSeconds(duration);
        if (appliedSlowFallEffects.Contains(Identifier)) appliedSlowFallEffects.Remove(Identifier);
        slowFall = appliedSlowFallEffects.Count > 0;
        slowFallRoutineRunning = false;
        movementManager.SlowFall(appliedSlowFallEffects.Count > 0);
    }
    [PunRPC]
    public void ContinuousSlowFall(string Identifier, float percentage) {
        if (photonView.IsMine) {
            if (appliedSlowFallEffects.Contains(Identifier)) return;
            appliedSlowFallEffects.Add(Identifier);

            slowFall = true;
            movementManager.SlowFall(true, percentage);
        }
    }
    [PunRPC]
    public void EndContinuousSlowFall(string Identifier) {
        if (photonView.IsMine) {
            if (appliedSlowFallEffects.Contains(Identifier)){
                appliedSlowFallEffects.Remove(Identifier);
            } else {
                // Don't remove the effect if it isn't being applied anymore
                return;
            }

            slowFall = appliedSlowFallEffects.Count > 0;
            movementManager.SlowFall(appliedSlowFallEffects.Count > 0);
        }
    }


    // Cleanse
    [PunRPC]
    void Cleanse() {
        try {
            if (silenced) {
                if (silenceRoutineRunning) StopCoroutine(silenceRoutine);
                silenced = false;
            }
            if (slowed) {
                if (slowRoutineRunning) StopCoroutine(slowRoutine);
                animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
                movementManager.ResetMovementSpeed();
                appliedSlowEffects.Clear();
                slowed = false;
            }
            if (hastened) {
                if (hasteRoutineRunning) StopCoroutine(hasteRoutine);
                animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
                movementManager.ResetMovementSpeed();
                appliedHasteEffects.Clear();
                hastened = false;
            }
            if (rooted) {
                if (rootRoutineRunning) StopCoroutine(rootRoutine);
                movementManager.Root(false);
                rooted = false;
            }
            if (grounded) {
                if (groundedRoutineRunning) StopCoroutine(rootRoutine);
                movementManager.Ground(false);
                grounded = false;
            }
            if (stunned) {
                if (stunRoutineRunning) StopCoroutine(stunRoutine);
                movementManager.Stun(false);
                stunned = false;
            }
            if (fragile) {
                if (fragileRoutineRunning) StopCoroutine(fragileRoutine);
                fragilePercentage = 0f;
                appliedFragileEffects.Clear();
                fragile = false;
            }
            if (tough) {
                if (toughRoutineRunning) StopCoroutine(toughRoutine);
                toughPercentage = 0f;
                appliedToughEffects.Clear();
                tough = false;
            }
            if (weakened) {
                if (weakenRoutineRunning) StopCoroutine(weakenRoutine);
                weaknesses = new ManaDistribution();
                appliedWeakenEffects.Clear();
                weakened = false;
            }
            if (strengthened) {
                if (strengthenRoutineRunning) StopCoroutine(strengthenRoutine);
                strengths = new ManaDistribution();
                appliedStrengthenEffects.Clear();
                strengthened = false;
            }
            if (manaRestorationChange) {
                if (manaRestorationRoutineRunning) StopCoroutine(manaRestorationRoutine);
                ManaRegen = defaultManaRegen;
                manaRestorationPercentage = 0f;
                appliedManaRestorationChangeEffects.Clear();
                manaRestorationChange = false;
            }
            if (camouflaged) {
                if (camouflagedRoutineRunning) StopCoroutine(camouflageRoutine);
                materialManager.ResetMaterial();
                camouflaged = false;
            }
            if (slowFall) {
                if (slowFallRoutineRunning) StopCoroutine(slowFallRoutine);
                movementManager.SlowFall(false);
                appliedSlowFallEffects.Clear();
                slowFall = false;
            }
        } catch (System.Exception e) {
            Debug.Log("Error trying to cleanse: "+e.Message);
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
            appliedSlowEffects.Clear();
        }
        if (rooted) {
            if (rootRoutineRunning) StopCoroutine(rootRoutine);
            movementManager.Root(false);
            rooted = false;
        }
        if (grounded) {
            if (groundedRoutineRunning) StopCoroutine(rootRoutine);
            movementManager.Ground(false);
            grounded = false;
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
            appliedFragileEffects.Clear();
        }
        if (weakened) {
            if (weakenRoutineRunning) StopCoroutine(weakenRoutine);
            weaknesses = new ManaDistribution();
            weakened = false;
            appliedWeakenEffects.Clear();
        }
        if (manaRestorationChange) {
            if (ManaRegen < defaultManaRegen) {
                if (manaRestorationRoutineRunning) StopCoroutine(manaRestorationRoutine);
                ManaRegen = defaultManaRegen;
                manaRestorationPercentage = 0f;
                manaRestorationChange = false;
                appliedManaRestorationChangeEffects.Clear();
            }
        }
    }
}