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
    private bool isChannelling = false, currentSpellIsSelfTargeted = false, currentSpellIsOpponentTargeted = false, isShielded = false;
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

    // Do less damage of given mana types
    [HideInInspector]
    public bool weakened;
    private ManaDistribution weaknesses;

    // Do more damage of given mana types
    [HideInInspector]
    public bool strengthened;
    private ManaDistribution strengths;

    // Increase or decrease the amount of damage taken
    [HideInInspector]
    public bool fragile;
    private float fragileDuration, fragilePercentage = 0f;
    [HideInInspector]
    public bool tough;
    private float toughDuration, toughPercentage = 0f;

    [HideInInspector]
    public bool manaRestorationChange;
    private float manaRestorationDuration, manaRestorationPercentage = 0f;




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
    }

    void Awake() {
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);

        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine) {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }
    }

    void Update() {
        if (photonView.IsMine) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                spellCraftingDisplay.SetActive(!spellCraftingDisplay.activeInHierarchy);
                glyphCastingPanel.SetActive(!glyphCastingPanel.activeInHierarchy);
            }
            if (!spellCraftingDisplay.activeInHierarchy) this.ProcessInputs();

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
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson) {
        if (!photonView.IsMine) return;
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);
        // Spell spell = spellGO.GetComponent<Spell>();
        switch (SpellEffectType) {
            case "dotprojectile":
                StartCoroutine(TakeDirectDoTDamage(Damage, Duration, spellDistribution));
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
        if (cameraWorker != null) cameraWorker.Shake(damage/100f, 0.198f);
        if (isShielded || damage == 0f) return;
        Health -= aura.GetDamage(damage, spellDistribution);
        Debug.Log("Take Damage --  pre-resistance: " + damage + "    post-resistance: " + aura.GetDamage(damage, spellDistribution));
    }

    IEnumerator TakeDirectDoTDamage(float damage, float duration, ManaDistribution spellDistribution) {
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

        if (Input.GetKeyDown("0")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("0"));
        } else if (Input.GetKeyDown("1")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("1"));
        } else if (Input.GetKeyDown("2")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("2"));
        } else if (Input.GetKeyDown("3")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("3"));
        } else if (Input.GetKeyDown("4")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("4"));
        } else if (Input.GetKeyDown("5")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("5"));
        } else if (Input.GetKeyDown("6")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("6"));
        } else if (Input.GetKeyDown("7")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("7"));
        } else if (Input.GetKeyDown("8")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("8"));
        } else if (Input.GetKeyDown("9")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("9"));
        } else if (Input.GetKeyDown("e")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("e"));
        } else if (Input.GetKeyDown("q")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("q"));
        } else if (Input.GetKeyDown("r")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("r"));
        } else if (Input.GetKeyDown("z")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("z"));
        } else if (Input.GetKeyDown("x")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("x"));
        } else if (Input.GetKeyDown("c")) {
            CastAuricaSpell(auricaCaster.CastBindSlot("c"));
        }



        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (!isChannelling) {
                CastAuricaSpell(auricaCaster.Cast());
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
        spellCraftingDisplay = GameObject.Find("SpellCraftingPanel");
        glyphCastingPanel = GameObject.Find("ComponentCastingPanel");
        glyphCastingPanel.SetActive(false);
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
        Debug.Log("Spell Match: " + spell.c_name);
        // Load spell resource
        GameObject dataObject = Resources.Load<GameObject>(spell.linkedSpellResource);
        Spell foundSpell = dataObject.GetComponent<Spell>();

        // Change the casting anchor (where the spell spawns from)
        switch (foundSpell.CastingAnchor) {
            case "transform":
                currentCastingTransform = transform;
                break;
            case "top":
                currentCastingTransform = topCastingAnchor;
                break;
            default:
                currentCastingTransform = frontCastingAnchor;
                break;
        }

        // Turn the casting anchor in the direction we want the spell to face
        if (foundSpell.TurnToAimPoint) {
            TurnCastingAnchorDirectionToAimPoint();
        } else {
            ResetCastingAnchorDirection();
        }

        if (foundSpell.IsSelfTargeted) currentSpellIsSelfTargeted = true;
        if (foundSpell.IsOpponentTargeted) currentSpellIsOpponentTargeted = true;

        // Set the spell to cast, and start the animation
        if (!foundSpell.IsChannel) {
            currentSpellCast = spell.linkedSpellResource;
            movementManager.PlayCastingAnimation(foundSpell.CastAnimationType);
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
                GameObject newSpell = PhotonNetwork.Instantiate(currentSpellCast, currentCastingTransform.position, currentCastingTransform.rotation);
                Mana -= auricaCaster.GetManaCost();

                Spell spell = newSpell.GetComponent<Spell>();
                if (spell != null) {
                    spell.SetSpellStrength(auricaCaster.GetSpellStrength());
                    if (strengthened || weakened) spell.SetSpellDamageModifier(strengths - weaknesses);
                    spell.SetOwner(gameObject);
                }

                if (currentSpellIsSelfTargeted) {
                    currentSpellIsSelfTargeted = false;
                    TargetedSpell ts = newSpell.GetComponent<TargetedSpell>();
                    if (ts != null) ts.SetTarget(gameObject);
                } else if (currentSpellIsOpponentTargeted) {
                    currentSpellIsOpponentTargeted = false;
                    TargetedSpell ts = newSpell.GetComponent<TargetedSpell>();
                    GameObject target = GetPlayerWithinAimTolerance(10f);
                    if (ts != null && target != null) {
                        Debug.Log("Target found: " + target);
                        ts.SetTarget(target);
                    }
                }
            } else {
                CastFizzle();
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
                        foundSpell.SetSpellDamageModifier(strengths - weaknesses);
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
            } else if ((!start && isChannelling) || silenced || stunned) {
                isChannelling = false;
                isShielded = false;
                try {
                    if (currentShield != null) {
                        currentShield.Break();
                    } else {
                        PhotonNetwork.Destroy(channelledSpell);
                    }
                } catch {
                    // Do nothing, likely has already been cleaned up
                }
                channelledSpell = null;
            }
        }
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
            Mana -= flat + ((maxMana - Mana) * percentage);
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






    // Weaken - Deal reduced damage of given mana types
    [PunRPC]
    void Weaken(float duration, string weaknessString) {
        if (photonView.IsMine) {
            ManaDistribution weaknessDist = new ManaDistribution(weaknessString);
            StartCoroutine(WeakenRoutine(duration, weaknessDist));
        }
    }
    IEnumerator WeakenRoutine(float duration, ManaDistribution weaknessDist) {
        weakened = true;
        weaknesses += weaknessDist;
        yield return new WaitForSeconds(duration);
        weaknesses -= weaknessDist;
        if (weaknesses.GetAggregate() <= 0.1f) weakened = false;
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
            StartCoroutine(StrengthenRoutine(duration, strengthDist));
        }
    }
    IEnumerator StrengthenRoutine(float duration, ManaDistribution strengthDist) {
        strengthened = true;
        strengths += strengthDist;
        //Debug.Log("New Strength: " + strengths.ToString());
        yield return new WaitForSeconds(duration);
        strengths -= strengthDist;
        if (strengths.GetAggregate() <= 0.1f) strengthened = false;
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






    // ManaRestoration - change the mana regen rate by a multiplier value
    [PunRPC]
    void ManaRestoration(float duration, float restorationPercentage) {
        if (photonView.IsMine) {
            StartCoroutine(ManaRestorationRoutine(duration, restorationPercentage));
        }
    }
    IEnumerator ManaRestorationRoutine(float duration, float restorationPercentage) {
        manaRestorationChange = true;
        ManaRegen *= restorationPercentage;
        yield return new WaitForSeconds(duration);
        ManaRegen /= restorationPercentage;
        if (ManaRegen == defaultManaRegen) manaRestorationChange = false;
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
}