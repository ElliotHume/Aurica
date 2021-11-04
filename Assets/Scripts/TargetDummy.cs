using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetDummy : MonoBehaviour
{
    [PunRPC]
    void OnSpellCollide(float Damage, string SpellEffectType, float Duration, string spellDistributionJson, string ownerID = "") {
        ManaDistribution spellDistribution = JsonUtility.FromJson<ManaDistribution>(spellDistributionJson);

        // Apply the damage
        float finalDamage = Damage * GameManager.GLOBAL_SPELL_DAMAGE_MULTIPLIER;

        // Create damage popup
        if (finalDamage > 3f) {
            GameObject newPopup = PhotonNetwork.Instantiate("ZZZ Damage Popup Canvas", transform.position + (Vector3.up*2.75f), transform.rotation, 0);

            DamagePopup dmgPopup = newPopup.GetComponent<DamagePopup>();
            if (dmgPopup != null) {
                dmgPopup.ShowDamage(finalDamage);
                dmgPopup.isSceneObject = true;
            }
        }
    
        if (ownerID != "") Debug.Log("Target Dummy hit by ["+ownerID+"]");
    }
}
