using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
public class MobAI : Enemy, IPunObservable {
    float maxHealth;

    public LayerMask whatIsGround, whatIsPlayer, sightBlockingMask;
    public EnemyCharacterUI enemyUI;
    public AudioSource hitSound;
    GameObject closestPlayer;
    Vector3 playerPos;
    NavMeshAgent agent;
    Animator anim;
    private Vector3 networkPosition, oldPosition, velocity;
    private Quaternion networkRotation;
    private float aoeDamageTotal=0f, aoeDamageTick=0f, accumulatingDamageTimout=1f, accumulatingDamageTimer=0f;
    private DamagePopup accumulatingDamagePopup;


    // Patrolling
    public bool doesPatrol = true;
    Vector3 walkPoint, startPoint, currentNavAgentDestination, networkNavAgentDestination;
    bool walkPointSet;
    public float walkPointRange = 10f, idleTime = 6f;

    //Attacking
    public float attackCooldown = 4f;
    public string attackPrefabID = "Spell_Slash";
    public Vector3 attackOffset = Vector3.zero;
    public GameObject attackParticlesParent;
    bool alreadyAttacked;

    //States
    public float sightRange = 10f, attackRange = 3f, walkingSpeed = 2f, runningSpeed = 4f;
    bool targetInSightRange, targetInAttackRange, canSeeTarget, idling, walking = false, inCombat = false, slowed = false;

    // Enemy Specific
    public ManaDistribution aura;
    public UnityEvent onDeath;


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            // CRITICAL DATA
            stream.SendNext(Health);
            stream.SendNext(inCombat);

            stream.SendNext(currentNavAgentDestination);

            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        } else {
            // Network player, receive data
            // CRITICAL DATA
            this.Health = (float)stream.ReceiveNext();
            this.inCombat = (bool)stream.ReceiveNext();

            this.networkNavAgentDestination = (Vector3)stream.ReceiveNext();

            this.networkPosition = (Vector3)stream.ReceiveNext();
            this.networkRotation = (Quaternion)stream.ReceiveNext();

            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (velocity * lag);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        startPoint = transform.position;
        agent.speed = walkingSpeed;
        maxHealth = Health;
        oldPosition = transform.position;
    }

    public GameObject FindClosestPlayer() {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Player");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos) {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance) {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    void Update() {
        velocity = transform.position - oldPosition;
        oldPosition = transform.position;

        // Remote movement compensation
        if (!photonView.IsMine) {
            if (networkNavAgentDestination == Vector3.zero) {
                agent.ResetPath();
                currentNavAgentDestination = Vector3.zero;
                // Debug.Log("Network Reset Path");
            } else if (currentNavAgentDestination != networkNavAgentDestination) {
                agent.SetDestination(networkNavAgentDestination);
                currentNavAgentDestination = networkNavAgentDestination;
                // Debug.Log("Network Set Path: "+networkNavAgentDestination);
            }
            if (networkPosition.magnitude > 0.05f) transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime * (inCombat ? runningSpeed : walkingSpeed) );
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 350);
            return;
        }

        // AOE damage popup calculator
        // Compute AoE tick damage and total sum, if no new damage ticks come in for a while 
        if (aoeDamageTotal == 0f && aoeDamageTick > 0f) {
            // Add damage tick to the total and reset the tick
            aoeDamageTotal += aoeDamageTick;
            aoeDamageTick = 0f;

            // Initiate an accumulating damage popup
            accumulatingDamagePopup = enemyUI.CreateAccumulatingDamagePopup(aoeDamageTotal);
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
    }

    void FixedUpdate() {
        // Get closest player
        closestPlayer = FindClosestPlayer();

        // Get player controller position in worldspace
        playerPos = closestPlayer.transform.position + new Vector3(0f,1f,0f);

        // Check if the target is in sight range
        targetInSightRange = Vector3.Distance(transform.position, playerPos) <= sightRange;

        // Check if target is in attack range
        targetInAttackRange = Vector3.Distance(transform.position, playerPos) <= attackRange;

        // If in sight range, check if the target is obscured
        if (targetInSightRange || targetInAttackRange) {
            RaycastHit raycastHit;
            if( Physics.SphereCast(transform.position+transform.up, 0.5f, (playerPos - (transform.position+transform.up)), out raycastHit, 100f, sightBlockingMask) ) {
                // Debug.Log("Can see: "+raycastHit.transform.gameObject);
                canSeeTarget = raycastHit.transform.gameObject.tag == "Player";
            }
        }

        // Debug.Log("InSightRange: "+targetInSightRange+"    InAttackRange: "+targetInAttackRange+"     CanSeeTarget: "+canSeeTarget);

        // Set animation variable(s)
        anim.SetBool("Moving", walking);
        anim.SetBool("InCombat", inCombat);

        // Thinking logic
        if (Health > 0f) {
            if (!inCombat) {
                if (!targetInSightRange || (targetInSightRange && !canSeeTarget)) {
                    if (doesPatrol) Patrolling();
                } else {
                    inCombat = true;
                    agent.speed = runningSpeed;
                    ChaseTarget();
                }
            } else {
                if ((!targetInAttackRange && !alreadyAttacked) || (targetInAttackRange && !canSeeTarget && !alreadyAttacked)) {
                    ChaseTarget();
                } else {
                    AttackTarget();
                }
            }
        } else if (agent.hasPath) {
            agent.ResetPath();
            currentNavAgentDestination = Vector3.zero;
        }
    }

    void Patrolling() {
        // If: no walkpoint is set and not already idling, idle
        // Else: walk to the set walkpoint
        if (!walkPointSet) {
            walking = false;
            if (!idling) {
                idling = true;
                Idle();
            }
        } else {
            walking = true;
            agent.SetDestination(walkPoint);
            currentNavAgentDestination = walkPoint;
        }

        // If target is at the set walkpoint, find a new walkpoint
        if ((transform.position - walkPoint).magnitude < 1f) {
            walkPointSet = false;
        }
    }

    void Idle() {
        // Stop moving, and after a time, look for a new walkpoint
        agent.ResetPath();
        currentNavAgentDestination = Vector3.zero;
        Invoke(nameof(SearchWalkPoint), idleTime);
    }

    void ChaseTarget() {
        // Walk towards target
        walking = true;

        // Find closest point on navmesh from the player controllers center, check if it is reachable
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.SamplePosition(playerPos, out hit, 2f, NavMesh.AllAreas)){
            agent.CalculatePath(hit.position, path);
            if (path.status == NavMeshPathStatus.PathComplete) {
                agent.SetDestination(hit.position);
                currentNavAgentDestination = hit.position;
            } else {
                OutOfReach();
            }
        } else {
            OutOfReach();
        }
    }

    void AttackTarget() {
        // Stop moving and look at the target
        walking = false;
        agent.ResetPath();
        currentNavAgentDestination = Vector3.zero;
        transform.LookAt(new Vector3 (playerPos.x, transform.position.y, playerPos.z));

        // If you are not currently attacking, attack target
        if (!alreadyAttacked) {
            anim.SetBool("Attack", true);
            alreadyAttacked = true;
            // Start attack particles
            if (attackParticlesParent != null) {
                ParticleSystem[] particles = attackParticlesParent.GetComponentsInChildren<ParticleSystem>();
                foreach (var effect in particles) {
                    if (effect != null) effect.Play();
                }
            }
            
        }
    }

    public void CreateAttack() {
        GameObject newSpell = PhotonNetwork.Instantiate(attackPrefabID, transform.position + attackOffset, transform.rotation);
        Spell spell = newSpell.GetComponent<Spell>();
        if (spell != null) {
            spell.SetSpellDamageModifier(aura);
            spell.SetOwner(gameObject, false);
        } else {
            Debug.Log("Could not grab <Spell> Object from newly instantiated spell");
        }
    }

    public void CompleteAttack() {
        Debug.Log("Complete Attack");
        anim.SetBool("Attack", false);
        // Stop attack particles
        if (attackParticlesParent != null) {
            ParticleSystem[] particles = attackParticlesParent.GetComponentsInChildren<ParticleSystem>();
            foreach (var effect in particles) {
                if (effect != null) effect.Stop();
            }
        }
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack() {
        alreadyAttacked = false;
    }

    [PunRPC]
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID="") {
        if (!photonView.IsMine) return;
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);
        switch (SpellEffectType) {
            case "dot":
                if (hitSound) hitSound.Play();
                StartCoroutine(TakeDirectDoTDamage(Damage, Duration, spellDistribution));
                break;
            default:
                // Debug.Log("Default Spell effect --> Take direct damage");
                TakeDamage(Damage, spellDistribution);
                break;
        }
        // Debug.Log("Current Health: "+Health);
    }

    public void TakeDamage(float damage, ManaDistribution damageDistribution) {
        float finalDamage = GetAdjustedDamage(damage, damageDistribution);
        inCombat = true;
        agent.speed = runningSpeed;
        Health -= finalDamage;

        if (finalDamage >= 1.5f) {
            if (hitSound) hitSound.Play();
            if (enemyUI != null) {
                enemyUI.CreateDamagePopup(finalDamage);
            }
            Slow(1f);
        } else {
            // For an AoE spell tick we do something different
            aoeDamageTick += finalDamage;
        }

        if (Health <= 0f) {
            Die();
        }
    }

    public float GetAdjustedDamage(float damage, ManaDistribution damageDist) {
        List<float> percents = damageDist.GetAsPercentages();
        List<float> auraPercents = aura.ToList();
        if (percents.Count == 0) return damage;
        for (var i = 0; i < 7; i++) {
            percents[i] = percents[i] * damage * (1f - auraPercents[i]);
        }

        float sum = 0;
        foreach (var element in percents) {
            sum += element;
        }

        return sum;
    }

    IEnumerator TakeDirectDoTDamage(float damage, float duration, ManaDistribution spellDistribution) {
        // Play hit sounds
        float damagePerSecond = damage / duration;
        // Debug.Log("Take dot damage: "+damage+" duration: "+duration+ "     resistance total: " + aura.GetDamage(damage, spellDistribution) / damage);
        while (duration > 0f) {
            TakeDamage(damagePerSecond * Time.deltaTime, spellDistribution);
            duration -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public void OutOfReach() {
        if (targetInAttackRange) {
            AttackTarget();
        } else {
            walking = false;
            agent.ResetPath();
            currentNavAgentDestination = Vector3.zero;
        }
    }

    void ResumeMovement() {
        agent.speed = inCombat ? runningSpeed : walkingSpeed;
        slowed = false;
    }

    public void Slow(float duration){
        if (!slowed) {
            agent.speed /= 5f;
            Invoke(nameof(ResumeMovement), duration);
        }
    }

    public void Die() {
        anim.Play("Dead");
        walking = false;
        onDeath.Invoke();
        GetComponent<CapsuleCollider>().enabled = false;
        Destroy(gameObject, 30f);
    }

    void SearchWalkPoint() {
        // Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(startPoint.x + randomX, transform.position.y, startPoint.z + randomZ);

        // Check if the random point is on valid ground and is reachable by the agent
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(walkPoint, path);
        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround) && path.status == NavMeshPathStatus.PathComplete) {
            walkPointSet = true;
        }

        idling = false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere for the sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draw a red sphere for the attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}