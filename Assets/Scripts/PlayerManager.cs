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

    public static float MAXIMUM_MANA = 500f;

    [Tooltip("The current Health of our player")]
    public float Health = 100f;
    private float healing = 0f;

    [HideInInspector]
    public float greyHealth = 0f;

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

    [HideInInspector]
    public Dictionary<KeybindingActions, RecastSpell> activeRecastSpells = new Dictionary<KeybindingActions, RecastSpell>();

    // Is the player actively drawing glyphs
    [HideInInspector]
    public bool isDrawing = false;

    public AudioSource CastingSound, DeathSound, HitSound, HitMarkerSound, HitMarkerAoESound;

    private Animator animator;
    private string currentSpellCast = null, currentChannelledSpell = null;
    private Transform currentCastingTransform;
    private bool isChannelling = false, currentSpellIsSelfTargeted = false, currentSpellIsOpponentTargeted = false, isShielded = false, sneaking = false, respawning = false;
    private GameObject channelledSpell, spellCraftingDisplay, glyphCastingPanel;
    private PlayerMovementManager movementManager;
    private HealthBar healthBar, manaBar, boostCooldownBar1, boostCooldownBar2, greyHealthBar;
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
    private bool titleSet = false, expertiseSet = false;
    private int expertise = -1;
    private ExpertiseManager expertiseManager;
    private BindingUIPanel bindingUIPanel;
    private bool showingTabSpell = false, showingRecastTabSpell;
    private InputManager inputManager;
    private string playFabId;

    // Targeting Indicator System
    private bool preparedSpell = false;
    private AuricaSpell preparedSpellCast;
    private GameObject preparedSpellGO;
    private KeybindingActions preparedSpellKey;
    


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
    public bool manaRestorationChange, manaRestorationBuff;
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
            
            // Player Card Display Data
            stream.SendNext(hasBoost);
            stream.SendNext(greyHealth);

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
            stream.SendNext(manaRestorationBuff);
            stream.SendNext(camouflaged);
            stream.SendNext(slowFall);

            // Auxiliary effects data
            stream.SendNext(isDrawing);
            // stream.SendNext(isShielded);
        } else {
            // Network player, receive data
            // CRITICAL DATA
            this.Health = (float)stream.ReceiveNext();
            this.Mana = (float)stream.ReceiveNext();

            // Boost data
            this.hasBoost = (bool)stream.ReceiveNext();
            this.greyHealth = (float)stream.ReceiveNext();

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
            this.manaRestorationBuff = (bool)stream.ReceiveNext();
            this.camouflaged = (bool)stream.ReceiveNext();
            this.slowFall = (bool)stream.ReceiveNext();

            // Auxiliary effects data
            this.isDrawing = (bool)stream.ReceiveNext();
            // this.isShielded = (bool)stream.ReceiveNext();
        }
    }

    void Start() {
        maxMana = PlayerManager.MAXIMUM_MANA;
        Mana = maxMana;
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
                characterUI.PermanentlyHide();
            } else {
                RequestTitle();
                RequestExpertise();
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

        expertiseManager = ExpertiseManager.Instance;

        healthBar = GameObject.Find("LocalHealthBar").GetComponent<HealthBar>();
        greyHealthBar = GameObject.Find("LocalGreyHealthBar").GetComponent<HealthBar>();
        manaBar = GameObject.Find("LocalManaBar").GetComponent<HealthBar>();
        manaBar.SetMaxHealth(PlayerManager.MAXIMUM_MANA);
        boostCooldownBar1 = GameObject.Find("BoostCooldownBar1").GetComponent<HealthBar>();
        boostIndicator1 = GameObject.Find("BoostIndicator1");
        boostCooldownBar2 = GameObject.Find("BoostCooldownBar2").GetComponent<HealthBar>();
        boostIndicator2 = GameObject.Find("BoostIndicator2");
        crosshair = Object.FindObjectOfType(typeof(Crosshair)) as Crosshair;

        aimPointAnchorManager = aimPointAnchor.GetComponent<AimpointAnchor>();

        bindingUIPanel = BindingUIPanel.LocalInstance;
        inputManager = InputManager.Instance;

        if (!photonView.IsMine) {
            NetworkRequestPlayFabID();
        } else {
            playFabId = PlayerPrefs.GetString("PlayFabId");
            materialManager.CheckAdmin(GetUniqueName());
            if (MOBAMatchManager.Instance != null) MOBAMatchManager.Instance.NetworkClientPlayerJoined(GetUniqueName());
        }
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

            // // Lerp Mana regen towards default if no effects are applied. Because floating point math can create rounding error problems with statuses
            // if (!manaRestorationRoutineRunning && appliedManaRestorationChangeEffects.Count == 0 && !Mathf.Approximately(ManaRegen, defaultManaRegen)){
            //     ManaRegen = Mathf.Lerp(ManaRegen, defaultManaRegen, 0.05f * Time.deltaTime);
            //     if (Mathf.Approximately(ManaRegen, defaultManaRegen)) manaRestorationChange = false;
            // } 

            if (GameUIPanelManager.Instance && GameUIPanelManager.Instance.ShouldProcessInputs()) this.ProcessInputs();

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Backspace)) {
                auricaCaster.RemoveLastComponent();
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown("m")) {
                MasteryManager.Instance.SyncMasteries();
            }

            // If there is healing to be done, do it
            if (healing >= 0.001f && Health < maxHealth) {
                float healingDone = healing * Time.deltaTime * HealingRate;
                Health += healingDone;
                healing -= healingDone;
                if (healing < 0.01f) healing = 0f;
                if (Health >= maxHealth-greyHealth) {
                    Health = maxHealth-greyHealth;
                    healing = 0f;
                }
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
                if (expertiseManager == null) expertiseManager = ExpertiseManager.Instance;
                float expertiseMultiplier = 1f - ( 0.0075f * expertiseManager.GetExpertise());
                boostCharge1 += Time.deltaTime * boostCooldownMultiplier * expertiseMultiplier;
                if (hasBoostCharge1) {
                    boostIndicator1.SetActive(false);
                }
                hasBoostCharge1 = false;
            } else if (!hasBoostCharge1) {
                boostIndicator1.SetActive(true);
                hasBoostCharge1 = true;
            }

            if (boostCharge2 < BoostCooldown) {
                if (expertiseManager == null) expertiseManager = ExpertiseManager.Instance;
                float expertiseMultiplier = 1f - ( 0.0075f * expertiseManager.GetExpertise());
                boostCharge2 += Time.deltaTime * boostCooldownMultiplier * expertiseMultiplier;
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

            // Display grey health
            if (greyHealth > 0f) greyHealthBar.LerpTowards(greyHealth);

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
            manaRestorationBuff = ManaRegen >= defaultManaRegen;

            if (bindingUIPanel == null) bindingUIPanel = BindingUIPanel.LocalInstance;
            if (inputManager == null) inputManager = InputManager.Instance;

            if (activeRecastSpells.ContainsKey(KeybindingActions.Cast) && activeRecastSpells[KeybindingActions.Cast] != null) {
                AuricaSpell aSpell = activeRecastSpells[KeybindingActions.Cast].GetAttachedSpell().auricaSpell;
                float strength = activeRecastSpells[KeybindingActions.Cast].GetAttachedSpell().GetSpellStrength();
                bindingUIPanel.ShowTabSpell(aSpell, strength);
                showingRecastTabSpell = true;
            } else {
                if (showingRecastTabSpell) {
                    bindingUIPanel.HideTabSpell();
                    showingRecastTabSpell = false;
                }
            }

            if (!showingRecastTabSpell) {
                if (!preparedSpell && currentSpellCast == null && auricaCaster.GetCurrentSpellMatch() != null) {
                    bindingUIPanel.ShowTabSpell(auricaCaster.GetCurrentSpellMatch(), auricaCaster.GetSpellStrength());
                    showingTabSpell = true;
                } else {
                    if (showingTabSpell) {
                        bindingUIPanel.HideTabSpell();
                        showingTabSpell = false;
                    }
                }
            }   
        } else {
            if (stunned && animator.speed > 0f) {
                movementManager.Stun(true);
            } else if (!stunned && animator.speed == 0f) {
                movementManager.Stun(false);
            }
        }
    }

    private void NetworkSyncPlayFabID() {
        // Debug.LogError("TRY TO SEND PLAYFAB ID ["+playFabId+"] -- IsMine: "+photonView.IsMine);
        if (!photonView.IsMine) return;
        // Debug.LogError("SENDING PLAYFAB ID: ["+playFabId+"]");
        photonView.RPC("SetPlayFabID", RpcTarget.All, playFabId);
    }

    [PunRPC]
    private void SetPlayFabID(string id) {
        if (photonView.IsMine) return;
        playFabId = id;
        materialManager.CheckAdmin(GetUniqueName());
        // Debug.LogError("Set PlayFabId for remote client: ["+GetUniqueName()+"]");
    }

    private void NetworkRequestPlayFabID() {
        // Debug.LogError("REQUESTING PLAYFABID -- sender: "+GetUniqueName());
        photonView.RPC("RequestPlayFabID", RpcTarget.Others);
    }

    [PunRPC]
    private void RequestPlayFabID() {
        // Debug.LogError("RECIEVED REQUEST FOR PLAYFABID -- reciever: "+GetUniqueName());
        NetworkSyncPlayFabID();
    }

    public string GetUniqueName() {
        return photonView.Owner.NickName+" #"+playFabId;
    }

    public string GetName() {
        return photonView.Owner.NickName;
    }

    public bool IsHealing() {
        return healing > 0f;
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
        StartCoroutine(SendTitleWhenReady());
    }
    
    IEnumerator SendTitleWhenReady() {
        bool sent = false;
        while (!sent) {
            if (titleSet) {
                photonView.RPC("SendRemoteTitle", RpcTarget.All, playerTitle, playerTitleColour);
                sent = true;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void SendExpertise(int exp) {
        expertiseSet = true;
        expertise = exp;
        PlayerManager.LocalInstance.SetExpertiseMaterials(exp);
    }

    [PunRPC]
    public void SendRemoteExpertise(int exp) {
        if (materialManager != null) materialManager.SetExpertiseMaterials(exp);
    }

    // When spawning a remote player, ask for their expertise
    public void RequestExpertise() {
        photonView.RPC("RequestRemoteExpertise", RpcTarget.Others);
    }

    [PunRPC]
    public void RequestRemoteExpertise() {
        StartCoroutine(SendExpertiseWhenReady());
    }
    
    IEnumerator SendExpertiseWhenReady() {
        bool sent = false;
        while (!sent) {
            if (expertiseSet) {
                photonView.RPC("SendRemoteExpertise", RpcTarget.All, expertise);
                sent = true;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void HardReset() {
        if (!photonView.IsMine) return;
        // Reset health and mana
        Health = maxHealth;
        Mana = maxMana;
        greyHealth = 0f;
        greyHealthBar.SetHealth(greyHealth);
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
        greyHealthBar.SetHealth(0);
        manaBar.SetHealth(0);

        if (DeathSound != null) DeathSound.Play();

        GameManager.Instance.playerDeath(this);
        if (DeathmatchGameManager.Instance != null) {
            DeathmatchGameManager.Instance.playerDeath(this);
            if (photonView.IsMine) DeathmatchGameManager.Instance.localPlayerDeath(GetUniqueName());
        }
        if (FreeForAllGameManager.Instance != null) {
            FreeForAllGameManager.Instance.playerDeath(this);
            if (photonView.IsMine && lastPlayerToDamageSelf != GetUniqueName()) FreeForAllGameManager.Instance.localPlayerDeath(lastPlayerToDamageSelf);
        }
        if (MOBAMatchManager.Instance != null) {
            MOBAMatchManager.Instance.PlayerDeath(this);
            if (photonView.IsMine && lastPlayerToDamageSelf != GetUniqueName()) MOBAMatchManager.Instance.NetworkPlayerDeath(lastPlayerToDamageSelf);
        }

        if (photonView.IsMine) photonView.RPC("SendDeathEvent", RpcTarget.All, lastPlayerToDamageSelf);

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
    public void SendDeathEvent(string killerID) {
        Debug.Log("Player ["+killerID+"] got a kill!");
        if (killerID != GetUniqueName() && killerID == PlayerManager.LocalInstance.GetUniqueName()) {
            PlayerManager.LocalInstance.GetKillHealing();
        }
    }

    public void GetKillHealing() {
        greyHealth = Mathf.Max(0f, greyHealth-25f);
        healing += 10f;
    }

    [PunRPC]
    public void TeleportEffect(Vector3 newPosition) {
        if (!photonView.IsMine || grounded) return;
        Debug.Log("Teleporting " + gameObject + "  to " + newPosition);
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

    public void FuzzyTeleport(Transform newPosition, float radius = 1f) {
        if (!photonView.IsMine) return;
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
        Debug.Log("Fuzzy Teleporting " + gameObject + "  to pos: " + newPosition.position+" with random offset: "+randomOffset);
        transform.position = newPosition.position+randomOffset;
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

    public void SetExpertiseMaterials(int expertise) {
        materialManager.SetExpertiseMaterials(expertise);
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

        if (activeRecastSpells.Count > 0) {
            foreach(KeyValuePair<KeybindingActions, RecastSpell> entry in activeRecastSpells) {
                if (inputManager.GetKeyDown(entry.Key)) {
                    if (entry.Value != null) {
                        if (entry.Value.CanRecast()) {
                            entry.Value.InitiateRecast();
                        } else {
                            continue;
                        }
                    }
                    activeRecastSpells.Remove(entry.Key);
                    return;
                }
            }
        }
        
        if (movementManager.CanCast()) {
            if (Input.GetKey(KeyCode.LeftAlt)) {
                // Ctrl casting casts the components for the bound spell but doesnt actually fire off the spell 
                if (inputManager.GetKeyDown(KeybindingActions.SpellSlot1)) {
                    auricaCaster.CastBindSlot("1");
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlot2)) {
                    auricaCaster.CastBindSlot("2");
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlot3)) {
                    auricaCaster.CastBindSlot("3");
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlot4)) {
                    auricaCaster.CastBindSlot("4");
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotE)) {
                    auricaCaster.CastBindSlot("e");
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotQ)) {
                    auricaCaster.CastBindSlot("q");
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotR)) {
                    auricaCaster.CastBindSlot("r");
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotF)) {
                    auricaCaster.CastBindSlot("f");
                }
            } else {
                if (inputManager.GetKeyDown(KeybindingActions.Cast)) {
                    PrepareSpellCast(auricaCaster.CastFinal(), KeybindingActions.Cast);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlot1)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("1"), KeybindingActions.SpellSlot1);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlot2)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("2"), KeybindingActions.SpellSlot2);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlot3)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("3"), KeybindingActions.SpellSlot3);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlot4)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("4"), KeybindingActions.SpellSlot4);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotE)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("e"), KeybindingActions.SpellSlotE);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotQ)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("q"), KeybindingActions.SpellSlotQ);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotR)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("r"), KeybindingActions.SpellSlotR);
                } else if (inputManager.GetKeyDown(KeybindingActions.SpellSlotF)) {
                    PrepareSpellCast(auricaCaster.CastBindSlot("f"), KeybindingActions.SpellSlotF);
                }

                if (preparedSpell) {
                    if (!inputManager.GetKey(preparedSpellKey)) {
                        CastAuricaSpell(preparedSpellCast);
                    }
                }
            }
        } else if (isChannelling && (
            inputManager.GetKeyDown(KeybindingActions.SpellSlot1) ||
            inputManager.GetKeyDown(KeybindingActions.SpellSlot2) ||
            inputManager.GetKeyDown(KeybindingActions.SpellSlot3) ||
            inputManager.GetKeyDown(KeybindingActions.SpellSlot4) ||
            inputManager.GetKeyDown(KeybindingActions.SpellSlotQ) ||
            inputManager.GetKeyDown(KeybindingActions.SpellSlotE) ||
            inputManager.GetKeyDown(KeybindingActions.SpellSlotR) ||
            inputManager.GetKeyDown(KeybindingActions.SpellSlotF) ||
            inputManager.GetKeyDown(KeybindingActions.Cast))) {
            StopChannelling();
            auricaCaster.ResetCast();
        } else {
            if (preparedSpell) {
                if (!inputManager.GetKey(preparedSpellKey)) {
                    CancelPreparedCast();
                }
            }
        }

        if (inputManager.GetKeyDown(KeybindingActions.Ping)) {
            PhotonNetwork.Instantiate("Ping", crosshair.GetWorldPoint(), Quaternion.identity);
        }
        

        // Use Boost
        if (inputManager.GetKeyDown(KeybindingActions.Boost) || Input.GetKeyDown(KeyCode.Mouse2)) {
            if (hasBoost && !stunned && !silenced && !isShielded) {
                movementManager.Boost();
                particleManager.PlayBoostParticles();
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

    void PrepareSpellCast(AuricaSpell spell, KeybindingActions key) {
        if (isChannelling) {
            StopChannelling();
            auricaCaster.ResetCast();
            return;
        }
        if (spell == null) {
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

        if (activeRecastSpells.ContainsKey(key)) {
            return;
        }

        // Get spell strength
        float currentSpellStrength = auricaCaster.GetSpellStrength();
        // Debug.Log("Spell strength: "+currentSpellStrength);

        if (spell.difficultyRank == AuricaSpell.DifficultyRank.Rank3 && currentSpellStrength < AuricaSpell.RANK3_SPELL_STRENGTH_CUTOFF) {
            TipWindow.Instance.ShowTip("Spell Failure", "To cast a rank 3 difficulty spell, you must cast it at a spell strength of 50% or higher. Use the spell crafting menu to improve the spell strength.", 2f);
            CastFizzle();
            auricaCaster.ResetCast();
            return;
        }
        if (spell.difficultyRank == AuricaSpell.DifficultyRank.Rank4 && currentSpellStrength < AuricaSpell.RANK4_SPELL_STRENGTH_CUTOFF) {
            TipWindow.Instance.ShowTip("Spell Failure", "To cast a rank 4 difficulty spell, you must cast it at a spell strength of 35% or higher.", 2f);
            CastFizzle();
            auricaCaster.ResetCast();
            return;
        }

        // If another key is pressed while there is a spell prepared, cancel previous preparation
        if (key != preparedSpellKey && preparedSpellCast != null) {
            preparedSpellCast = null;
            preparedSpellGO = null;
        }
        preparedSpellKey = key;
        preparedSpell = true;

        string linkedSpellResource = spell.linkedSpellResource;
        // Check if the spell is a mastery spell, and if it is check if the player has sufficient mastery
        if (spell.isMasterySpell && !MasteryManager.Instance.HasMasteryForSpell(spell)) {
            linkedSpellResource = spell.masteryFailedSpellResource != "" ? spell.masteryFailedSpellResource : "XCollision_Fizzle";
            TipWindow.Instance.ShowTip("Not Enough Mastery", "To cast this spell properly you need more mastery with "+spell.masteryCategory.ToString()+" spells", 4f);
            auricaCaster.ResetCast();
        }

        // Load spell resource
        GameObject dataObject = Resources.Load<GameObject>(linkedSpellResource);
        preparedSpellCast = spell;
        preparedSpellGO = dataObject;
        Spell foundSpell = dataObject != null ? dataObject.GetComponent<Spell>() : null;

        AoESpell aoe = dataObject.GetComponent<AoESpell>();
        TargetedSpell ts = dataObject.GetComponent<TargetedSpell>();
        SummonSpell ss = dataObject.GetComponent<SummonSpell>();


       
        if ( aoe != null) {
            crosshair.ActivateTargetingIndicator(aoe.GetTargetingIndicatorScale(currentSpellStrength), aoe.UseAimPointNormal, aoe.IsSelfTargeted || aoe.CastingAnchor == "transform", aoe.IsOpponentTargeted, aoe.PositionOffset);
        } else if ( ts != null) {
            crosshair.ActivateTargetingIndicator(Vector3.one * 2f, false, ts.IsSelfTargeted, ts.IsOpponentTargeted, Vector3.up);
        } else if ( ss != null) {
            crosshair.ActivateTargetingIndicator(ss.GetTargetingIndicatorScale(), true, ss.CastingAnchor == "transform", ss.IsOpponentTargeted, Vector3.zero);
        } else {
            CastAuricaSpell(spell);
            preparedSpell = false;
            return;
        }

        // Play hand casting particles while holding a spell
        particleManager.PlayHandParticle(foundSpell.CastAnimationType, spell.manaType);
    }

    void CancelPreparedCast() {
        auricaCaster.ResetCast();
        crosshair.DeactivateTargetingIndicator();
        preparedSpell = false;
    }

    void CastAuricaSpell(AuricaSpell spell) {
        if (!photonView.IsMine) return;

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
        if (spell.isMasterySpell && !MasteryManager.Instance.HasMasteryForSpell(spell)) {
            linkedSpellResource = spell.masteryFailedSpellResource;
        }

        // Load spell resource
        GameObject dataObject = Resources.Load<GameObject>(linkedSpellResource);
        Spell foundSpell = dataObject != null ? dataObject.GetComponent<Spell>() : null;

        if (foundSpell == null) {
            if (linkedSpellResource.StartsWith("XCollision")) PhotonNetwork.Instantiate(linkedSpellResource, transform.position, transform.rotation);
            return;
        }

        // if (foundSpell.Damage > 0f) Debug.Log("Spell " + spell.c_name+" Adjusted Damage/Mana: "+((foundSpell.Damage * auricaCaster.GetSpellStrength()) / auricaCaster.GetManaCost())+" Base Damage/Mana: "+(foundSpell.Damage / auricaCaster.GetManaCost()));

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
                if (foundSpell.UseAimPointNormal) {
                    aimPointAnchor.rotation = Quaternion.LookRotation(aimPointAnchorManager.GetHitNormal(), (aimPointAnchor.transform.position - transform.position));
                } else {
                    aimPointAnchor.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                }
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

        // Stop any targeting indicators
        crosshair.DeactivateTargetingIndicator();
        preparedSpell = false;

        // ADD MASTERY
        if (MasteryManager.Instance != null) MasteryManager.Instance.AddMasteries(spell.masteries, spell.difficultyRank);

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

                Spell[] spells = newSpell.GetComponentsInChildren<Spell>();
                foreach(Spell spell in spells) {
                    // Spell spell = newSpell.GetComponent<Spell>();
                    if (spell != null) {
                        spell.SetSpellStrength(auricaCaster.GetSpellStrength());
                        spell.SetSpellDamageModifier(aura.GetInnateStrength() + strengths - weaknesses);
                        spell.SetOwner(gameObject);
                        spell.SetExpertiseParameters(ExpertiseManager.Instance.GetExpertise());
                        Mana += spell.ManaRefund;
                    } else {
                        Debug.Log("Could not grab <Spell> Object from newly instantiated spell");
                    }
                }

                
                RecastSpell[] recastSpells = newSpell.GetComponentsInChildren<RecastSpell>();
                foreach(RecastSpell newRecastSpell in recastSpells) {
                    // RecastSpell newRecastSpell = newSpell.GetComponent<RecastSpell>();
                    if (newRecastSpell != null) {
                        newRecastSpell.SetSpellStrength(auricaCaster.GetSpellStrength());
                        newRecastSpell.SetSpellDamageModifier(aura.GetInnateStrength() + strengths - weaknesses);
                        newRecastSpell.SetOwner(gameObject);
                        newRecastSpell.SetExpertiseParameters(ExpertiseManager.Instance.GetExpertise());

                        activeRecastSpells.Add(preparedSpellKey, newRecastSpell);
                    }
                }

                if (currentSpellIsSelfTargeted) {
                    currentSpellIsSelfTargeted = false;
                    TargetedSpell[] targetedSpells = newSpell.GetComponentsInChildren<TargetedSpell>();
                    AoESpell[] aoeSpells = newSpell.GetComponentsInChildren<AoESpell>();
                    foreach(TargetedSpell targetedSpell in targetedSpells) targetedSpell.SetTarget(gameObject);
                    foreach(AoESpell aoeSpell in aoeSpells) aoeSpell.SetTarget(gameObject);
                } else if (currentSpellIsOpponentTargeted) {
                    currentSpellIsOpponentTargeted = false;
                    TargetedSpell[] targetedSpells = newSpell.GetComponentsInChildren<TargetedSpell>();
                    AoESpell[] aoeSpells = newSpell.GetComponentsInChildren<AoESpell>();

                    GameObject target = GetPlayerWithinAimTolerance(2f);
                    if (target != null) {
                        foreach(TargetedSpell targetedSpell in targetedSpells) targetedSpell.SetTarget(target);
                        foreach(AoESpell aoeSpell in aoeSpells) aoeSpell.SetTarget(target);
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

    public void ToggleDrawingEffects(bool flag) {
        isDrawing = flag;
    }

    // public void StartDrawing() {
    //     particleManager.StartDrawingParticles();
    // }

    // public void StopDrawing() {
    //     particleManager.StopDrawingParticles();
    // }


    private float GetHighestEffectValue(Dictionary<string, float> effects) {
        float highest = 0f;
        foreach(KeyValuePair<string, float> effect in effects) {
            if (effect.Value > highest) {
                highest = effect.Value;
            }
        }
        return highest;
    }










































































    /*  --------------------  STATUS EFFECTS ------------------------ */


    // Heal - restore health by a flat value and/or a percentage of missing health
    [PunRPC]
    void Heal(float flat, float percentage, float greyHealthMultiplier) {
        if (photonView.IsMine) {
            // Debug.Log("HEAL FOR: ["+flat+"] flat + ["+percentage+"] percent missing health");
            float newHeal = (flat + ((maxHealth - Health) * percentage)) * GameManager.GLOBAL_SPELL_HEALING_MULTIPLIER;
            healing += newHeal;

            // Increase grey health proportional to how much healing needs to be applied
            if (Health < maxHealth-greyHealth){
                greyHealth += Mathf.Min(newHeal * greyHealthMultiplier, maxHealth-greyHealth-Health);
            } 
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
        particleManager.PlayManaDrainFX();
        if (photonView.IsMine) {
            // Debug.Log("Draining Mana by - flat: "+flat+" & percentage: "+percentage+" = "+(Mana * percentage));
            Mana -= flat + (Mana * percentage);
        }
    }







    // Slow - Decrease animation speed
    Dictionary<string, float> appliedSlowEffects = new Dictionary<string, float>();

    [PunRPC]
    void Slow(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            // If a status effect from the same Identifier has already been applied, do not apply another.
            if (appliedSlowEffects.ContainsKey(Identifier)) return;
            appliedSlowEffects.Add(Identifier, percentage);
            slowRoutine = StartCoroutine(SlowRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator SlowRoutine(string Identifier, float duration, float percentage) {
        slowRoutineRunning = true;
        ApplyCombinedMovementEffects();
        yield return new WaitForSeconds(duration);
        appliedSlowEffects.Remove(Identifier);
        ApplyCombinedMovementEffects();
        slowRoutineRunning = false;

    }
    [PunRPC]
    public void ContinuousSlow(string Identifier, float percentage) {
        if (photonView.IsMine && !isShielded) {
            // If a status effect from the same Identifier has already been applied, do not apply another.
            if (appliedSlowEffects.ContainsKey(Identifier)) return;
            appliedSlowEffects.Add(Identifier, percentage);
            ApplyCombinedMovementEffects();
        }
    }
    [PunRPC]
    public void EndContinuousSlow(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedSlowEffects.ContainsKey(Identifier)) return;
            appliedSlowEffects.Remove(Identifier);
            ApplyCombinedMovementEffects();
        }
    }






    // Hasten - Increase animation speed
    Dictionary<string, float> appliedHasteEffects = new Dictionary<string, float>();

    [PunRPC]
    void Hasten(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedHasteEffects.ContainsKey(Identifier)) return;
            appliedHasteEffects.Add(Identifier, percentage);
            hasteRoutine = StartCoroutine(HastenRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator HastenRoutine(string Identifier, float duration, float percentage) {
        hasteRoutineRunning = true;
        ApplyCombinedMovementEffects();
        yield return new WaitForSeconds(duration);
        appliedHasteEffects.Remove(Identifier);
        ApplyCombinedMovementEffects();
        hasteRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousHasten(string Identifier, float percentage) {
        if (photonView.IsMine) {
            if (appliedHasteEffects.ContainsKey(Identifier)) return;
            appliedHasteEffects.Add(Identifier, percentage);
            ApplyCombinedMovementEffects();
        }
    }
    [PunRPC]
    public void EndContinuousHasten(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedHasteEffects.ContainsKey(Identifier)) return;
            appliedHasteEffects.Remove(Identifier);
            ApplyCombinedMovementEffects();
        }
    }

    private float GetCombinedMovementEffects() {
        float highestSlow = GetHighestEffectValue(appliedSlowEffects);
        float highestHaste = GetHighestEffectValue(appliedHasteEffects);
        return highestHaste - highestSlow;
    }

    private void ApplyCombinedMovementEffects() {
        hastened = appliedHasteEffects.Count > 0;
        slowed = appliedSlowEffects.Count > 0;
        if (!hastened && !slowed) {
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            movementManager.ResetMovementSpeed();
            return;
        }
        float combinedEffects = GetCombinedMovementEffects();
        animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER + combinedEffects;
        movementManager.ChangeMovementSpeed(1f + combinedEffects);
    }






    // Rooted - Prevent movement, including movement spells
    List<string> appliedRootEffects = new List<string>();

    [PunRPC]
    void Root(string Identifier, float duration) {
        if (photonView.IsMine && !isShielded) {
            if (appliedRootEffects.Contains(Identifier)) return;
            appliedRootEffects.Add(Identifier);
            rootRoutine = StartCoroutine(RootRoutine(Identifier, duration));
        }
    }
    IEnumerator RootRoutine(string Identifier, float duration) {
        rootRoutineRunning = true;
        ApplyRootEffects();
        yield return new WaitForSeconds(duration);
        if (appliedRootEffects.Contains(Identifier)) appliedRootEffects.Remove(Identifier);
        ApplyRootEffects();
        rootRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousRoot(string Identifier) {
        if (photonView.IsMine && !isShielded) {
            if (appliedRootEffects.Contains(Identifier)) return;
            appliedRootEffects.Add(Identifier);
            ApplyRootEffects();
        }
    }
    [PunRPC]
    public void EndContinuousRoot(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedRootEffects.Contains(Identifier)) return;
            appliedRootEffects.Remove(Identifier);
            ApplyRootEffects();
        }
    }
    private void ApplyRootEffects() {
        rooted = appliedRootEffects.Count > 0;
        movementManager.Root(rooted);
    }

    
    
    
    // Grounded - Cannot be displaced
    List<string> appliedGroundEffects = new List<string>();

    [PunRPC]
    void Ground(string Identifier, float duration) {
        if (photonView.IsMine && !isShielded) {
            if (appliedGroundEffects.Contains(Identifier)) return;
            appliedGroundEffects.Add(Identifier);
            groundedRoutine = StartCoroutine(GroundedRoutine(Identifier, duration));
        }
    }
    IEnumerator GroundedRoutine(string Identifier, float duration) {
        groundedRoutineRunning = true;
        ApplyGroundedEffects();
        yield return new WaitForSeconds(duration);
        if (appliedGroundEffects.Contains(Identifier)) appliedGroundEffects.Remove(Identifier);
        ApplyGroundedEffects();
        groundedRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousGround(string Identifier) {
        if (photonView.IsMine && !isShielded) {
            if (appliedGroundEffects.Contains(Identifier)) return;
            appliedGroundEffects.Add(Identifier);
            ApplyGroundedEffects();
        }
    }
    [PunRPC]
    public void EndContinuousGround(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedGroundEffects.Contains(Identifier)) return;
            appliedGroundEffects.Remove(Identifier);
            ApplyGroundedEffects();
        }
    }
    private void ApplyGroundedEffects() {
        grounded = appliedGroundEffects.Count > 0;
        movementManager.Ground(grounded);
    }







    // Stunned - Prevent moving and spellcasting, basically a stacked root and silence
    List<string> appliedStunEffects = new List<string>();

    [PunRPC]
    void Stun(string Identifier, float duration) {
        if (photonView.IsMine && !isShielded) {
            if (appliedStunEffects.Contains(Identifier)) return;
            appliedStunEffects.Add(Identifier);
            StopChannelling();
            slowRoutine = StartCoroutine(StunRoutine(Identifier, duration));
        }
    }
    IEnumerator StunRoutine(string Identifier, float duration) {
        stunRoutineRunning = true;
        ApplyStunEffects();
        yield return new WaitForSeconds(duration);
        if (appliedStunEffects.Contains(Identifier)) appliedStunEffects.Remove(Identifier);
        ApplyStunEffects();
        stunRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousStun(string Identifier) {
        if (photonView.IsMine && !isShielded) {
            if (appliedStunEffects.Contains(Identifier)) return;
            appliedStunEffects.Add(Identifier);
            StopChannelling();
            ApplyStunEffects();
        }
    }
    [PunRPC]
    public void EndContinuousStun(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedStunEffects.Contains(Identifier)) return;
            appliedStunEffects.Remove(Identifier);
            ApplyStunEffects();
        }
    }
    private void ApplyStunEffects() {
        stunned = appliedStunEffects.Count > 0;
        movementManager.Stun(stunned);
    }






    // Silence - Prevent spellcasting
    List<string> appliedSilenceEffects = new List<string>();

    [PunRPC]
    void Silence(string Identifier, float duration) {
        if (photonView.IsMine && !silenced) {
            if (appliedSilenceEffects.Contains(Identifier)) return;
            appliedSilenceEffects.Add(Identifier);
            silenceRoutine = StartCoroutine(SilenceRoutine(Identifier, duration));
        }
    }
    IEnumerator SilenceRoutine(string Identifier, float duration) {
        silenceRoutineRunning = true;
        StopChannelling();
        ApplySilenceEffects();
        yield return new WaitForSeconds(duration);
        if (appliedSilenceEffects.Contains(Identifier)) appliedSilenceEffects.Remove(Identifier);
        ApplySilenceEffects();
        silenceRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousSilence(string Identifier) {
        if (photonView.IsMine) {
            if (appliedSilenceEffects.Contains(Identifier)) return;
            appliedSilenceEffects.Add(Identifier);
            StopChannelling();
            ApplySilenceEffects();
        }
    }

    [PunRPC]
    public void EndContinuousSilence(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedSilenceEffects.Contains(Identifier)) return;
            appliedSilenceEffects.Remove(Identifier);
            ApplySilenceEffects();
        }
    }
    private void ApplySilenceEffects() {
        silenced = appliedSilenceEffects.Count > 0;
    }






    // Weaken - Deal reduced damage of given mana types
    Dictionary<string, ManaDistribution> appliedWeakenEffects = new Dictionary<string, ManaDistribution>();

    [PunRPC]
    void Weaken(string Identifier, float duration, string weaknessString) {
        if (photonView.IsMine && !isShielded) {
            if (appliedWeakenEffects.ContainsKey(Identifier)) return;
            ManaDistribution weaknessDist = new ManaDistribution(weaknessString);
            appliedWeakenEffects.Add(Identifier, weaknessDist);
            weakenRoutine = StartCoroutine(WeakenRoutine(Identifier, duration));
        }
    }
    IEnumerator WeakenRoutine(string Identifier, float duration) {
        weakenRoutineRunning = true;
        ApplyCombinedDamageOutputEffects();
        yield return new WaitForSeconds(duration);
        if (appliedWeakenEffects.ContainsKey(Identifier)) {
            appliedWeakenEffects.Remove(Identifier);
        }
        ApplyCombinedDamageOutputEffects();
        weakenRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousWeaken(string Identifier, string weakString) {
        if (photonView.IsMine && !isShielded) {
            if (appliedWeakenEffects.ContainsKey(Identifier)) return;
            ManaDistribution weakDist = new ManaDistribution(weakString);
            appliedWeakenEffects.Add(Identifier, weakDist);
            ApplyCombinedDamageOutputEffects();
        }
    }

    [PunRPC]
    public void EndContinuousWeaken(string Identifier, string weakString) {
        if (photonView.IsMine) {
            if (!appliedWeakenEffects.ContainsKey(Identifier)) return;
            appliedWeakenEffects.Remove(Identifier);
            ApplyCombinedDamageOutputEffects();
        }
    }






    // Strengthen - Deal increased damage of given mana types
    Dictionary<string, ManaDistribution> appliedStrengthenEffects = new Dictionary<string, ManaDistribution>();

    [PunRPC]
    void Strengthen(string Identifier, float duration, string strengthString) {
        if (photonView.IsMine && !isShielded) {
            if (appliedStrengthenEffects.ContainsKey(Identifier)) return;
            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            appliedStrengthenEffects.Add(Identifier, strengthDist);
            strengthenRoutine = StartCoroutine(StrengthenRoutine(Identifier, duration, strengthDist));
        }
    }
    IEnumerator StrengthenRoutine(string Identifier, float duration, ManaDistribution strengthDist) {
        strengthenRoutineRunning = true;
        ApplyCombinedDamageOutputEffects();
        yield return new WaitForSeconds(duration);
        if (appliedStrengthenEffects.ContainsKey(Identifier)) {
            appliedStrengthenEffects.Remove(Identifier);
        }
        ApplyCombinedDamageOutputEffects();
        strengthenRoutineRunning = false;
    }

    [PunRPC]
    public void ContinuousStrengthen(string Identifier, string strengthString) {
        if (photonView.IsMine) {
            if (appliedStrengthenEffects.ContainsKey(Identifier)) return;
            ManaDistribution strengthDist = new ManaDistribution(strengthString);
            appliedStrengthenEffects.Add(Identifier, strengthDist);
            ApplyCombinedDamageOutputEffects();
        }
    }

    [PunRPC]
    public void EndContinuousStrengthen(string Identifier, string strengthString) {
        if (photonView.IsMine) {
            if (!appliedStrengthenEffects.ContainsKey(Identifier)) return;
            appliedStrengthenEffects.Remove(Identifier);
            ApplyCombinedDamageOutputEffects();
        }
    }

    private void ApplyCombinedDamageOutputEffects() {
        strengthened = appliedStrengthenEffects.Count > 0;
        if (!strengthened) strengths = new ManaDistribution();
        
        weakened = appliedWeakenEffects.Count > 0;
        if (!weakened) weaknesses = new ManaDistribution();

        if (!strengthened && !weakened) {
            return;
        }

        ManaDistribution combinedStrengths = new ManaDistribution();
        foreach(KeyValuePair<string, ManaDistribution> effect in appliedStrengthenEffects) {
            combinedStrengths += effect.Value;
        }
        strengths = combinedStrengths;

        ManaDistribution combinedWeaknesses = new ManaDistribution();
        foreach(KeyValuePair<string, ManaDistribution> effect in appliedWeakenEffects) {
            combinedWeaknesses += effect.Value;
        }
        weaknesses = combinedWeaknesses;
    }






    // Fragile - Take increased damage from all sources
    Dictionary<string, float> appliedFragileEffects = new Dictionary<string, float>();

    [PunRPC]
    void Fragile(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedFragileEffects.ContainsKey(Identifier)) return;
            appliedFragileEffects.Add(Identifier, percentage);
            fragileRoutine = StartCoroutine(FragileRoutine(Identifier, duration));
        }
    }
    IEnumerator FragileRoutine(string Identifier, float duration) {
        fragileRoutineRunning = true;
        ApplyCombinedResistanceEffects();
        yield return new WaitForSeconds(duration);
        if (appliedFragileEffects.ContainsKey(Identifier)) appliedFragileEffects.Remove(Identifier);
        ApplyCombinedResistanceEffects();
        fragileRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousFragile(string Identifier, float percentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedFragileEffects.ContainsKey(Identifier)) return;
            appliedFragileEffects.Add(Identifier, percentage);
            ApplyCombinedResistanceEffects();
        }
    }

    [PunRPC]
    public void EndContinuousFragile(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedFragileEffects.ContainsKey(Identifier)) return;
            appliedFragileEffects.Remove(Identifier);
            ApplyCombinedResistanceEffects();
        }
    }






    // Toughen - Take decreased damage from all sources
    Dictionary<string, float> appliedToughEffects = new Dictionary<string, float>();

    [PunRPC]
    void Tough(string Identifier, float duration, float percentage) {
        if (photonView.IsMine && !isShielded) {
            if (appliedToughEffects.ContainsKey(Identifier)) return;
            appliedToughEffects.Add(Identifier, percentage);
            toughRoutine = StartCoroutine(ToughRoutine(Identifier, duration));
        }
    }
    IEnumerator ToughRoutine(string Identifier, float duration) {
        toughRoutineRunning = true;
        ApplyCombinedResistanceEffects();
        yield return new WaitForSeconds(duration);
        if (appliedToughEffects.ContainsKey(Identifier)) appliedToughEffects.Remove(Identifier);
        ApplyCombinedResistanceEffects();
        toughRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousTough(string Identifier, float percentage) {
        if (photonView.IsMine) {
            if (appliedToughEffects.ContainsKey(Identifier)) return;
            appliedToughEffects.Add(Identifier, percentage);
            ApplyCombinedResistanceEffects();
        }
    }
    [PunRPC]
    public void EndContinuousTough(string Identifier) {
        if (photonView.IsMine) {
            if (!appliedToughEffects.ContainsKey(Identifier)) return;
            appliedToughEffects.Remove(Identifier);
            ApplyCombinedResistanceEffects();
        }
    }

    private void ApplyCombinedResistanceEffects() {
        tough = appliedToughEffects.Count > 0;
        if (!tough) toughPercentage = 0f;

        fragile = appliedFragileEffects.Count > 0;
        if (!fragile) fragilePercentage = 0f;

        if (!tough && !fragile) {
            return;
        }
        toughPercentage = GetHighestEffectValue(appliedToughEffects);
        fragilePercentage = GetHighestEffectValue(appliedFragileEffects);
    }






    // ManaRestoration - change the mana regen rate by a multiplier value
    Dictionary<string, float> appliedManaRestorationChangeEffects = new Dictionary<string, float>();

    [PunRPC]
    void ManaRestoration(string Identifier, float duration, float restorationPercentage) {
        if (photonView.IsMine && (!isShielded || restorationPercentage >= 1f)) {
            if (appliedManaRestorationChangeEffects.ContainsKey(Identifier)) return;
            appliedManaRestorationChangeEffects.Add(Identifier, restorationPercentage);
            manaRestorationRoutine = StartCoroutine(ManaRestorationRoutine(Identifier, duration, restorationPercentage));
        }
    }
    IEnumerator ManaRestorationRoutine(string Identifier, float duration, float restorationPercentage) {
        manaRestorationRoutineRunning = true;
        ApplyCombinedManaRestorationEffects();
        yield return new WaitForSeconds(duration);
        if (appliedManaRestorationChangeEffects.ContainsKey(Identifier)) appliedManaRestorationChangeEffects.Remove(Identifier);
        manaRestorationRoutineRunning = false;
        ApplyCombinedManaRestorationEffects();
    }

    [PunRPC]
    public void ContinuousManaRestoration(string Identifier, float restorationPercentage) {
        if (photonView.IsMine && (!isShielded || restorationPercentage >= 1f)) {
            if (appliedManaRestorationChangeEffects.ContainsKey(Identifier)) return;
            appliedManaRestorationChangeEffects.Add(Identifier, restorationPercentage);
            ApplyCombinedManaRestorationEffects();
        }
    }

    [PunRPC]
    public void EndContinuousManaRestoration(string Identifier, float restorationPercentage) {
        if (photonView.IsMine) {
            if (!appliedManaRestorationChangeEffects.ContainsKey(Identifier)) return;
            appliedManaRestorationChangeEffects.Remove(Identifier);
            ApplyCombinedManaRestorationEffects();
        }
    }

    private void ApplyCombinedManaRestorationEffects() {
        manaRestorationChange = appliedManaRestorationChangeEffects.Count > 0;
        if (!manaRestorationChange) {
            manaRestorationPercentage = 0f;
            ManaRegen = defaultManaRegen;
            return;
        }
        manaRestorationPercentage = 1f;
        foreach(KeyValuePair<string, float> effect in appliedManaRestorationChangeEffects) {
            manaRestorationPercentage *= effect.Value;
        }
        ManaRegen = defaultManaRegen * manaRestorationPercentage;
    }






    // Camouflage
    List<string> appliedCamouflageEffects = new List<string>();

    [PunRPC]
    void Camouflage(string Identifier, float duration) {
        if (appliedCamouflageEffects.Contains(Identifier)) return;
        appliedCamouflageEffects.Add(Identifier);
        camouflageRoutine = StartCoroutine(CamouflageRoutine(Identifier, duration));
    }
    IEnumerator CamouflageRoutine(string Identifier, float duration) {
        camouflagedRoutineRunning = true;
        ApplyCamouflageEffects();
        yield return new WaitForSeconds(duration);
        if (appliedCamouflageEffects.Contains(Identifier)) appliedCamouflageEffects.Remove(Identifier);
        ApplyCamouflageEffects();
        camouflagedRoutineRunning = false;
    }

    [PunRPC]
    public void ContinuousCamouflage(string Identifier) {
        if (appliedCamouflageEffects.Contains(Identifier)) return;
        appliedCamouflageEffects.Add(Identifier);
        ApplyCamouflageEffects();
    }

    [PunRPC]
    public void EndContinuousCamouflage(string Identifier) {
        if (!appliedCamouflageEffects.Contains(Identifier)) return;
        appliedCamouflageEffects.Remove(Identifier);
        ApplyCamouflageEffects();
    }

    private void ApplyCamouflageEffects() {
        camouflaged = appliedCamouflageEffects.Count > 0;
        if (!camouflaged) {
            materialManager.ResetMaterial();
            EndSneak();
            return;
        }
        materialManager.GoInvisible();
        Sneak();
    }





    // SlowFall - Take decreased damage from all sources
    Dictionary<string, float> appliedSlowFallEffects = new Dictionary<string, float>();

    [PunRPC]
    void SlowFall(string Identifier, float duration, float percentage) {
        if (photonView.IsMine) {
            if (appliedSlowFallEffects.ContainsKey(Identifier)) return;
            appliedSlowFallEffects.Add(Identifier, percentage);
            slowFallRoutine = StartCoroutine(SlowFallRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator SlowFallRoutine(string Identifier, float duration, float percentage) {
        slowFallRoutineRunning = true;
        ApplyHighestSlowFallEffect();
        yield return new WaitForSeconds(duration);
        if (appliedSlowFallEffects.ContainsKey(Identifier)) appliedSlowFallEffects.Remove(Identifier);
        slowFallRoutineRunning = false;
        ApplyHighestSlowFallEffect();
    }
    [PunRPC]
    public void ContinuousSlowFall(string Identifier, float percentage) {
        if (photonView.IsMine) {
            if (appliedSlowFallEffects.ContainsKey(Identifier)) return;
            appliedSlowFallEffects.Add(Identifier, percentage);
            ApplyHighestSlowFallEffect();
        }
    }
    [PunRPC]
    public void EndContinuousSlowFall(string Identifier) {
        if (photonView.IsMine) {
            if (appliedSlowFallEffects.ContainsKey(Identifier)) return;
            appliedSlowFallEffects.Remove(Identifier);
            ApplyHighestSlowFallEffect();
        }
    }

    private void ApplyHighestSlowFallEffect() {
        slowFall = appliedSlowFallEffects.Count > 0;
        if (!slowFall) {
            movementManager.SlowFall(false);
            return;
        }
        float highestSlowfall = GetHighestEffectValue(appliedSlowFallEffects);
        movementManager.SlowFall(true, highestSlowfall);
    }


    // Cleanse
    [PunRPC]
    void Cleanse() {
        particleManager.PlayCleanseFX();
        if (!photonView.IsMine) return;
        try {
            if (silenced) {
                if (silenceRoutineRunning) StopCoroutine(silenceRoutine);
                appliedSilenceEffects.Clear();
                ApplySilenceEffects();
            }
            if (slowed) {
                if (slowRoutineRunning) StopCoroutine(slowRoutine);
                appliedSlowEffects.Clear();
                ApplyCombinedMovementEffects();
            }
            if (hastened) {
                if (hasteRoutineRunning) StopCoroutine(hasteRoutine);
                appliedHasteEffects.Clear();
                ApplyCombinedMovementEffects();
            }
            if (rooted) {
                if (rootRoutineRunning) StopCoroutine(rootRoutine);
                appliedRootEffects.Clear();
                ApplyRootEffects();
            }
            if (grounded) {
                if (groundedRoutineRunning) StopCoroutine(rootRoutine);
                appliedGroundEffects.Clear();
                ApplyGroundedEffects();
            }
            if (stunned) {
                if (stunRoutineRunning) StopCoroutine(stunRoutine);
                appliedStunEffects.Clear();
                ApplyStunEffects();
            }
            if (fragile) {
                if (fragileRoutineRunning) StopCoroutine(fragileRoutine);
                appliedFragileEffects.Clear();
                ApplyCombinedResistanceEffects();
            }
            if (tough) {
                if (toughRoutineRunning) StopCoroutine(toughRoutine);
                appliedToughEffects.Clear();
                ApplyCombinedResistanceEffects();
            }
            if (weakened) {
                if (weakenRoutineRunning) StopCoroutine(weakenRoutine);
                appliedWeakenEffects.Clear();
                ApplyCombinedDamageOutputEffects();
            }
            if (strengthened) {
                if (strengthenRoutineRunning) StopCoroutine(strengthenRoutine);
                appliedStrengthenEffects.Clear();
                ApplyCombinedDamageOutputEffects();
            }
            if (manaRestorationChange) {
                if (manaRestorationRoutineRunning) StopCoroutine(manaRestorationRoutine);
                appliedManaRestorationChangeEffects.Clear();
                ApplyCombinedManaRestorationEffects();
            }
            if (camouflaged) {
                if (camouflagedRoutineRunning) StopCoroutine(camouflageRoutine);
                appliedCamouflageEffects.Clear();
                ApplyCamouflageEffects();
            }
            if (slowFall) {
                if (slowFallRoutineRunning) StopCoroutine(slowFallRoutine);
                appliedSlowFallEffects.Clear();
                ApplyHighestSlowFallEffect();
            }
        } catch (System.Exception e) {
            Debug.Log("Error trying to cleanse: "+e.Message);
        }
        
    }


    // Cure
    // Removes all negative status effects
    [PunRPC]
    void Cure() {
        particleManager.PlayCureFX();
        if (!photonView.IsMine) return;
        try {
            if (slowed) {
                if (slowRoutineRunning) StopCoroutine(slowRoutine);
                appliedSlowEffects.Clear();
                ApplyCombinedMovementEffects();
            }
            if (silenced) {
                if (silenceRoutineRunning) StopCoroutine(silenceRoutine);
                appliedSilenceEffects.Clear();
                ApplySilenceEffects();
            }
            if (rooted) {
                if (rootRoutineRunning) StopCoroutine(rootRoutine);
                appliedRootEffects.Clear();
                ApplyRootEffects();
            }
            if (grounded) {
                if (groundedRoutineRunning) StopCoroutine(rootRoutine);
                appliedGroundEffects.Clear();
                ApplyGroundedEffects();
            }
            if (stunned) {
                if (stunRoutineRunning) StopCoroutine(stunRoutine);
                appliedStunEffects.Clear();
                ApplyStunEffects();
            }
            if (fragile) {
                if (fragileRoutineRunning) StopCoroutine(fragileRoutine);
                appliedFragileEffects.Clear();
                ApplyCombinedResistanceEffects();
            }
            if (weakened) {
                if (weakenRoutineRunning) StopCoroutine(weakenRoutine);
                appliedWeakenEffects.Clear();
                ApplyCombinedDamageOutputEffects();
            }
            if (manaRestorationChange) {
                if (manaRestorationRoutineRunning) StopCoroutine(manaRestorationRoutine);
                List<string> toBeRemoved = new List<string>();
                foreach(KeyValuePair<string, float> effect in appliedManaRestorationChangeEffects) {
                    if (effect.Value < 1f) toBeRemoved.Add(effect.Key);
                }
                foreach(string Identifier in toBeRemoved) {
                    appliedManaRestorationChangeEffects.Remove(Identifier);
                }
                ApplyCombinedManaRestorationEffects();
            }
        } catch (System.Exception e) {
            Debug.Log("Error trying to cleanse: "+e.Message);
        }
    }
}