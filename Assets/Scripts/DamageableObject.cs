using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class DamageableObject : MonoBehaviourPun
{
    public float damageThreshold = 10f;
    public UnityEvent OnDamage, OnDamageThresholdReached;

    private float health;
    private bool hasPopped = false;

    private float aoeDamageTotal=0f, aoeDamageTick=0f, accumulatingDamageTimout=1f, accumulatingDamageTimer=0f;
    private DamagePopup accumulatingDamagePopup;

    // Start is called before the first frame update
    void Start() {
        health = damageThreshold;
    }

    void Update() {
        if (!photonView.IsMine) return;
        // Compute AoE tick damage and total sum, if no new damage ticks come in for a while 
        if (aoeDamageTotal == 0f && aoeDamageTick > 0f) {
            // Add damage tick to the total and reset the tick
            aoeDamageTotal += aoeDamageTick;
            aoeDamageTick = 0f;

            // Initiate an accumulating damage popup
            GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position+ (Vector3.up * 0.5f), transform.rotation, 0);
            newPopup.transform.SetParent(gameObject.transform);
            DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
            if (dmgPopup != null) {
                dmgPopup.AccumulatingDamagePopup(aoeDamageTotal);
                accumulatingDamagePopup = dmgPopup;
            }
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

    [PunRPC]
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID = "") {
        if (hasPopped) return;
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);

        // Apply the damage
        float finalDamage = Damage * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER;

        // Create damage popup
        if (finalDamage > 1.5f) {
            GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position + (Vector3.up*2.75f), transform.rotation, 0);

            DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
            if (dmgPopup != null) {
                dmgPopup.ShowDamage(finalDamage);
                dmgPopup.isSceneObject = true;
            }
        } else {
            aoeDamageTick += finalDamage;
        }

        health -= finalDamage;
            
        if (health <= 0f) {
            OnDamageThresholdReached.Invoke();
            hasPopped = true;
        } else {
            OnDamage.Invoke();
        }
    }

    public void ResetHealthPool() {
        health = damageThreshold;
        hasPopped = false;
    }
}
