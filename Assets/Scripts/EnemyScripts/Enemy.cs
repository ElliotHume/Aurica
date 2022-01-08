using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class Enemy : MonoBehaviourPunCallbacks {
    public string c_name = "";
    public float Health = 100f;
    protected float maxHealth;
    public LayerMask whatIsGround, whatIsPlayer, sightBlockingMask;
    public EnemyCharacterUI enemyUI;
    public AudioSource hurtSound, attackWindupSound, aggroSound, breathingSound;

    protected GameObject closestPlayer;
    protected Vector3 playerPos;
    protected UnityEngine.AI.NavMeshAgent agent;
    protected Animator animator;

    protected Vector3 networkPosition, oldPosition, velocity;
    protected Quaternion networkRotation;
    protected float aoeDamageTotal=0f, aoeDamageTick=0f, accumulatingDamageTimout=1f, accumulatingDamageTimer=0f;
    protected DamagePopup accumulatingDamagePopup;

    // States
    public float sightRange = 10f, attackRange = 3f, walkingSpeed = 2f, runningSpeed = 4f;
    protected bool targetInSightRange, targetInAttackRange, canSeeTarget, idling, walking = false, inCombat = false, dead = false;

    // Patrolling
    public bool doesPatrol = false;
    public float walkPointRange = 10f, idleTime = 6f;
    protected Vector3 walkPoint, startPoint;
    protected bool walkPointSet;

    // Enemy Specific
    public ManaDistribution aura;
    public UnityEvent onDeath;



    /* ----------------- STATUS EFFECT VARIABLES ---------------------- */

    // Increase or decrease movement speed
    [HideInInspector]
    public bool slowed;
    protected Coroutine slowRoutine;
    protected bool slowRoutineRunning;

    [HideInInspector]
    public bool hastened;
    protected Coroutine hasteRoutine;
    protected bool hasteRoutineRunning;

    // Prevent moving, not including displacement
    [HideInInspector]
    public bool rooted;
    protected Coroutine rootRoutine;
    protected bool rootRoutineRunning;

    // Prevent all spellcasts
    [HideInInspector]
    public bool silenced;
    protected Coroutine silenceRoutine;
    protected bool silenceRoutineRunning;

    // Prevent all actions
    [HideInInspector]
    public bool stunned;
    protected Coroutine stunRoutine;
    protected bool stunRoutineRunning;

    // Do less damage of given mana types
    [HideInInspector]
    public bool weakened;
    protected ManaDistribution weaknesses;
    protected Coroutine weakenRoutine;
    protected bool weakenRoutineRunning;

    // Do more damage of given mana types
    [HideInInspector]
    public bool strengthened;
    protected ManaDistribution strengths;
    protected Coroutine strengthenRoutine;
    protected bool strengthenRoutineRunning;

    // Increase or decrease the amount of damage taken
    [HideInInspector]
    public bool fragile;
    protected float fragileDuration, fragilePercentage = 0f;
    protected Coroutine fragileRoutine;
    protected bool fragileRoutineRunning;

    [HideInInspector]
    public bool tough;
    protected float toughDuration, toughPercentage = 0f;
    protected Coroutine toughRoutine;
    protected bool toughRoutineRunning;



    /* ---------------------- DAMAGE MANAGER ---------------------- */

    [PunRPC]
    protected void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID="") {
        if (!photonView.IsMine) return;
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);
        switch (SpellEffectType) {
            case "dot":
                if (hurtSound) hurtSound.Play();
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
        float finalDamage = GetFinalDamage(damage, damageDistribution);

        if (!inCombat && aggroSound != null) aggroSound.Play();
        inCombat = true;
        if (!slowed && !hastened && !rooted && !stunned) agent.speed = runningSpeed;
        Health -= finalDamage;

        if (finalDamage >= 1.5f) {
            if (hurtSound) hurtSound.Play();
            if (enemyUI != null) {
                enemyUI.CreateDamagePopup(finalDamage);
            }
        } else {
            // For an AoE spell tick we do something different
            aoeDamageTick += finalDamage;
        }
    }

    public float GetFinalDamage(float initialDamage, ManaDistribution damageDist) {
        float damage = GetAdjustedDamage(initialDamage, damageDist);
        if (fragile) damage *= (1 + fragilePercentage);
        if (tough) damage *= Mathf.Max(0, (1 - toughPercentage));

        return damage;
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

    /* ---------------------- MOVEMENT MANAGER ---------------------- */

    public void ChangeMovementSpeed(float multiplier) {
        agent.speed *= multiplier;
    }

    public void ResetMovementSpeed() {
        agent.speed = runningSpeed;
    }

    public void Root(bool rooted) {
        if (rooted) {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        } else {
            agent.isStopped = stunned;
        }
        Debug.Log("ROOT ENEMY: "+agent.isStopped);
    }

    public void Stun(bool stunned) {
        if (stunned) {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        } else {
            agent.isStopped = rooted;
        }
        animator.speed = stunned ? 0f : 1f;
    }


    /*  --------------------  STATUS EFFECTS ------------------------ */

    // Slow - Decrease animation speed
    List<string> appliedSlowEffects = new List<string>();

    [PunRPC]
    public void Slow(string Identifier, float duration, float percentage) {
        if (photonView.IsMine) {
            // If a status effect from the same Identifier has already been applied, do not apply another.
            if (appliedSlowEffects.Contains(Identifier)) {
                Debug.Log("Nullify duplicate {SLOW} from ["+Identifier+"].");
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
        ChangeMovementSpeed(1f - percentage);
        yield return new WaitForSeconds(duration);
        animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        ResetMovementSpeed();
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
                Debug.Log("Nullify duplicate {SLOW} from ["+Identifier+"].");
                return;
            }
            appliedSlowEffects.Add(Identifier);

            slowed = true;
            animator.speed *= 1f - percentage;
            ChangeMovementSpeed(1f - percentage);
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
            ResetMovementSpeed();
            slowed = false;
        }
    }






    // Hasten - Increase animation speed
    List<string> appliedHasteEffects = new List<string>();

    [PunRPC]
    public void Hasten(string Identifier, float duration, float percentage) {
        if (photonView.IsMine) {
            if (appliedHasteEffects.Contains(Identifier)) return;
            appliedHasteEffects.Add(Identifier);

            hastened = true;
            hasteRoutine = StartCoroutine(HastenRoutine(Identifier, duration, percentage));
        }
    }
    IEnumerator HastenRoutine(string Identifier, float duration, float percentage) {
        hasteRoutineRunning = true;
        animator.speed *= 1f + percentage;
        ChangeMovementSpeed(1f + percentage);
        yield return new WaitForSeconds(duration);
        animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
        ResetMovementSpeed();
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
            ChangeMovementSpeed(1f + percentage);
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
            ResetMovementSpeed();
            hastened = false;
        }
    }






    // Rooted - Prevent movement, including movement spells
    [PunRPC]
    public void Root(float duration) {
        if (photonView.IsMine && !rooted) {
            rooted = true;
            rootRoutine = StartCoroutine(RootRoutine(duration));
        }
    }
    IEnumerator RootRoutine(float duration) {
        rootRoutineRunning = true;
        Root(true);
        yield return new WaitForSeconds(duration);
        Root(false);
        rooted = false;
        rootRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousRoot() {
        if (photonView.IsMine) {
            rooted = true;
            Root(true);
        }
    }
    [PunRPC]
    public void EndContinuousRoot() {
        if (photonView.IsMine) {
            rooted = false;
            Root(false);
        }
    }



    // Stunned - Prevent moving and spellcasting, basically a stacked root and silence
    [PunRPC]
    public void Stun(float duration) {
        if (photonView.IsMine && !stunned) {
            stunned = true;
            slowRoutine = StartCoroutine(StunRoutine(duration));
        }
    }
    IEnumerator StunRoutine(float duration) {
        stunRoutineRunning = true;
        Stun(true);
        yield return new WaitForSeconds(duration);
        Stun(false);
        stunned = false;
        stunRoutineRunning = false;
    }
    [PunRPC]
    public void ContinuousStun() {
        if (photonView.IsMine) {
            stunned = true;
            Stun(true);
        }
    }
    [PunRPC]
    public void EndContinuousStun() {
        if (photonView.IsMine) {
            stunned = false;
            Stun(false);
        }
    }






    // Silence - Prevent spellcasting
    [PunRPC]
    public void Silence(float duration) {
        if (photonView.IsMine && !silenced) {
            silenced = true;
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
    public void Weaken(string Identifier, float duration, string weaknessString) {
        if (photonView.IsMine) {
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
    public void Strengthen(string Identifier, float duration, string strengthString) {
        if (photonView.IsMine) {
            if (appliedStrengthenEffects.Contains(Identifier)) {
                Debug.Log("Nullify duplicate {STRENGTHEN} from ["+Identifier+"].");
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
                Debug.Log("Nullify duplicate {STRENGTHEN} from ["+Identifier+"].");
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
    public void Fragile(string Identifier, float duration, float percentage) {
        if (photonView.IsMine) {
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
    public void Tough(string Identifier, float duration, float percentage) {
        if (photonView.IsMine) {
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

    // Cleanse
    [PunRPC]
    public void Cleanse() {
        if (slowed) {
            if (slowRoutineRunning) StopCoroutine(slowRoutine);
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            ResetMovementSpeed();
            appliedSlowEffects.Clear();
            slowed = false;
        }
        if (hastened) {
            if (hasteRoutineRunning) StopCoroutine(hasteRoutine);
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            ResetMovementSpeed();
            appliedHasteEffects.Clear();
            hastened = false;
        }
        if (rooted) {
            if (rootRoutineRunning) StopCoroutine(rootRoutine);
            Root(false);
            rooted = false;
        }
        if (silenced) {
            if (silenceRoutineRunning) StopCoroutine(silenceRoutine);
            silenced = false;
        }
        if (stunned) {
            if (stunRoutineRunning) StopCoroutine(stunRoutine);
            Stun(false);
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
    }





    // Cure
    // Removes all negative status effects
    [PunRPC]
    public void Cure() {
        if (slowed) {
            if (slowRoutineRunning) StopCoroutine(slowRoutine);
            animator.speed = GameManager.GLOBAL_ANIMATION_SPEED_MULTIPLIER;
            ResetMovementSpeed();
            slowed = false;
            appliedSlowEffects.Clear();
        }
        if (rooted) {
            if (rootRoutineRunning) StopCoroutine(rootRoutine);
            Root(false);
            rooted = false;
        }
        if (silenced) {
            if (silenceRoutineRunning) StopCoroutine(silenceRoutine);
            silenced = false;
        }
        if (stunned) {
            if (stunRoutineRunning) StopCoroutine(stunRoutine);
            Stun(false);
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
    }
}
