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

    //Attacking
    public float attackCooldown = 4f;
    public string attackPrefabID = "Spell_Slash";
    public Vector3 attackOffset = Vector3.zero;
    public bool turnAttackToLookAtTarget = true;
    public GameObject attackParticlesParent;

    // Unreachable attack
    // for when the targeted player is off of the navmesh and out of attack range.
    public string unreachableAttackID;
    public float unreachableAttackRange = 80f;
    public Vector3 unreachableAttackOffset = Vector3.zero;
    public bool turnUnreachableAttackToLookAtTarget = true;
    bool targetInUnreachableAttackRange = false;

    bool alreadyAttacked = false, attacking = false, targetUnreachable = false, attackStartedAsUnreachable = false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            // CRITICAL DATA
            stream.SendNext(Health);
            stream.SendNext(inCombat);
            stream.SendNext(playerOwned);

            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

            // Status Effect data
            stream.SendNext(slowed);
            stream.SendNext(hastened);
            stream.SendNext(rooted);
            stream.SendNext(silenced);
            stream.SendNext(stunned);
            stream.SendNext(weakened);
            stream.SendNext(strengthened);
            stream.SendNext(fragile);
            stream.SendNext(tough);
        } else {
            // Network player, receive data
            // CRITICAL DATA
            this.Health = (float)stream.ReceiveNext();
            bool networkCombat = (bool)stream.ReceiveNext();
            bool playerOwned = (bool)stream.ReceiveNext();

            this.networkPosition = (Vector3)stream.ReceiveNext();
            this.networkRotation = (Quaternion)stream.ReceiveNext();

            // Status Effect data
            this.slowed = (bool)stream.ReceiveNext();
            this.hastened = (bool)stream.ReceiveNext();
            this.rooted = (bool)stream.ReceiveNext();
            this.silenced = (bool)stream.ReceiveNext();
            this.stunned = (bool)stream.ReceiveNext();
            this.weakened = (bool)stream.ReceiveNext();
            this.strengthened = (bool)stream.ReceiveNext();
            this.fragile = (bool)stream.ReceiveNext();
            this.tough = (bool)stream.ReceiveNext();

            if (networkCombat && !inCombat) {
                inCombat = true;
                if (aggroSound != null) aggroSound.Play();
            }

            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (velocity * lag);
        }
    }

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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
            if (go == playerOwner) continue;
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance && go.GetComponents<TargetDummy>().Length == 0) {
                closest = go;
                distance = curDistance;
            }
        }
        // Debug.Log("Closest player: "+closest+"     enemyOwner:"+playerOwner+"   isTargetOwner: "+(closest == playerOwner));
        return closest;
    }

    void Update() {
        velocity = transform.position - oldPosition;
        oldPosition = transform.position;

        // Remote movement compensation
        if (!photonView.IsMine) {
            if (networkPosition.magnitude > 0.05f) transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * runningSpeed * 3f);
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

        // Set animator variable(s)
        animator.SetBool("Moving", (walking && (agent.hasPath && !((transform.position - agent.destination).magnitude < 0.2f))) && !rooted && !stunned);
        animator.SetBool("InCombat", inCombat);

        // Check health
        if (Health <= 0f) {
            if (!dead) Die();
            if (agent.hasPath) {
                agent.ResetPath();
                agent.isStopped = true;
            }
            return;
        }
        if (playerOwned) Health -= ((1f - (Health/maxHealth)) + (1f * Mathf.Clamp(maxHealth/100f, 0f, 1f))) * Time.deltaTime;

        // Don't do anything if stunned or there are no valid player targets
        if (stunned || closestPlayer == null) return;

        // Get player controller position in worldspace
        playerPos = closestPlayer.transform.position + new Vector3(0f,1f,0f);

        // Check if the target is in sight range
        targetInSightRange = Vector3.Distance(transform.position, playerPos) <= sightRange;

        // Check if target is in attack range
        targetInAttackRange = Vector3.Distance(transform.position, playerPos) <= attackRange;

        // Check if target is in attack range
        targetInUnreachableAttackRange = Vector3.Distance(transform.position, playerPos) <= unreachableAttackRange;

        // If in sight range, check if the target is obscured
        if (targetInSightRange || targetInAttackRange || targetInUnreachableAttackRange) {
            RaycastHit raycastHit;
            if( Physics.SphereCast(transform.position+transform.up, 0.2f, (playerPos - (transform.position+transform.up)), out raycastHit, 100f, sightBlockingMask) ) {
                // Debug.Log("Can see: "+raycastHit.transform.gameObject);
                canSeeTarget = raycastHit.transform.gameObject.tag == "Player";
            }
        }

        // Debug.Log("InSightRange: "+targetInSightRange+"    InAttackRange: "+targetInAttackRange+"     CanSeeTarget: "+canSeeTarget);

        // Thinking logic
        if (!inCombat) {
            if (!targetInSightRange || (targetInSightRange && !canSeeTarget)) {
                if (doesPatrol) Patrolling();
            } else {
                inCombat = true;
                if (!slowed && !hastened && !rooted && !stunned) agent.speed = runningSpeed;
                if (aggroSound != null) aggroSound.Play();
                ChaseTarget();
            }
        } else {
            if ((!targetInAttackRange && !attacking) || (targetInAttackRange && !canSeeTarget && !attacking) || (targetUnreachable && !targetInAttackRange)) {
                ChaseTarget();
            } else {
                targetUnreachable = false;
                AttackTarget();
            }
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
        } else if (!rooted && !stunned) {
            walking = true;
            agent.SetDestination(walkPoint);
        }

        // If target is at the set walkpoint, find a new walkpoint
        if ((transform.position - walkPoint).magnitude < 1f) {
            walkPointSet = false;
        }
    }

    void Idle() {
        // Stop moving, and after a time, look for a new walkpoint
        agent.ResetPath();
        Invoke(nameof(SearchWalkPoint), idleTime);
    }

    void ChaseTarget() {
        // Find closest point on navmesh from the player controllers center, check if it is reachable
        NavMeshHit hit;
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.SamplePosition(playerPos, out hit, 3f, NavMesh.AllAreas)){
            agent.CalculatePath(hit.position, path);
            if (path.status == NavMeshPathStatus.PathComplete && !rooted) {
                // Walk towards target
                targetUnreachable = false;
                agent.SetDestination(hit.position);
                walking = true;
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
        transform.LookAt(new Vector3 (playerPos.x, transform.position.y, playerPos.z));

        // If you are not currently attacking, attack target
        if (!alreadyAttacked) {
            attacking = true;
            animator.SetBool("Attack", true);
            alreadyAttacked = true;
            attackStartedAsUnreachable = targetUnreachable;
            // Start attack particles
            if (attackParticlesParent != null) {
                ParticleSystem[] particles = attackParticlesParent.GetComponentsInChildren<ParticleSystem>();
                foreach (var effect in particles) {
                    if (effect != null) effect.Play();
                }
            }
            
        }
    }

    public void PlayAttackSound() {
        if (attackWindupSound != null) attackWindupSound.Play();
    }

    public void CreateAttack() {
        if (!photonView.IsMine || silenced || stunned) return;
        GameObject newSpell;
        if (!attackStartedAsUnreachable) {
            newSpell = PhotonNetwork.Instantiate("Enemies/EnemySpells/"+attackPrefabID, transform.position + (transform.right * attackOffset.x) +(transform.forward * attackOffset.z) + (transform.up * attackOffset.y), transform.rotation);
            if (turnAttackToLookAtTarget) newSpell.transform.LookAt(playerPos);
        } else {
            newSpell = PhotonNetwork.Instantiate("Enemies/EnemySpells/"+unreachableAttackID, transform.position + (transform.right * unreachableAttackOffset.x) +(transform.forward * unreachableAttackOffset.z) + (transform.up * unreachableAttackOffset.y), transform.rotation);
            if (turnUnreachableAttackToLookAtTarget) newSpell.transform.LookAt(playerPos);
        }
        
        Spell spell = newSpell.GetComponent<Spell>();
        if (spell != null) {
            spell.SetSpellDamageModifier(new ManaDistribution() + strengths - weaknesses);
            spell.SetOwner(gameObject, false);
            if (spellStrength != 0f) spell.SetSpellStrength(spellStrength);
        } else {
            Debug.Log("Could not grab <Spell> Object from newly instantiated spell");
        }

        if (spell.IsSelfTargeted) {
            TargetedSpell targetedSpell = newSpell.GetComponent<TargetedSpell>();
            if (targetedSpell != null) targetedSpell.SetTarget(gameObject);
            AoESpell aoeSpell = newSpell.GetComponent<AoESpell>();
            if (aoeSpell != null) aoeSpell.SetTarget(gameObject);
        } else if (spell.IsOpponentTargeted) {
            TargetedSpell ts = newSpell.GetComponent<TargetedSpell>();
            if (ts != null) ts.SetTarget(closestPlayer);
            AoESpell aoeSpell = newSpell.GetComponent<AoESpell>();
            if (aoeSpell != null) aoeSpell.SetTarget(closestPlayer);
        }

        BasicProjectileSpell bps = newSpell.GetComponent<BasicProjectileSpell>();
        if (bps != null) {
            bps.SetEnemyAttack();
            bps.SetAimAssistTarget(closestPlayer);
            bps.SetHomingTarget(closestPlayer);
        }
        StatusEffect se = newSpell.GetComponent<StatusEffect>();
        if (se != null) {
            se.SetOwner(gameObject);
        }
    }

    public void CompleteAttack() {
        animator.SetBool("Attack", false);
        // Stop attack particles
        if (attackParticlesParent != null) {
            ParticleSystem[] particles = attackParticlesParent.GetComponentsInChildren<ParticleSystem>();
            foreach (var effect in particles) {
                if (effect != null) effect.Stop();
            }
        }
        attacking = false;
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack() {
        alreadyAttacked = false;
    }


    public void OutOfReach() {
        if (targetInAttackRange) {
            AttackTarget();
        } else if (unreachableAttackID != "" && targetInUnreachableAttackRange && canSeeTarget) {
            targetUnreachable = true;
            AttackTarget();
        } else if (walking) {
            walking = false;
            NavMeshHit hit;
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas)){
                agent.CalculatePath(hit.position, path);
                if (path.status == NavMeshPathStatus.PathComplete) {
                    agent.SetDestination(hit.position);
                }
            }
        }
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

        if (unreachableAttackID != "") {
            // Draw a blue sphere for the attack range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, unreachableAttackRange);
        }
    }
}