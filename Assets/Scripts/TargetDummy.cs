using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetDummy : MonoBehaviourPun
{
    private float aoeDamageTotal=0f, aoeDamageTick=0f, accumulatingDamageTimout=1f, accumulatingDamageTimer=0f;
    private DamagePopup accumulatingDamagePopup;

    void Update() {
        if (!photonView.IsMine) return;
        // Compute AoE tick damage and total sum, if no new damage ticks come in for a while 
        if (aoeDamageTotal == 0f && aoeDamageTick > 0f) {
            // Add damage tick to the total and reset the tick
            aoeDamageTotal += aoeDamageTick;
            aoeDamageTick = 0f;

            // Initiate an accumulating damage popup
            GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position+ (Vector3.up*2.75f), transform.rotation, 0);
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
        if (!photonView.IsMine) return;
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
    
        if (ownerID != "") Debug.Log("Target Dummy hit by ["+ownerID+"]");
    }
}
