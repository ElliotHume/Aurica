using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectSpell : Spell
{
    public bool canHitSelf = false;
    
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf)) {
            PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
            if (pm != null) {
                PhotonView pv = PhotonView.Get(pm);
                if (pv != null && pv.IsMine) pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson());
            }
        } else if (collision.gameObject.tag == "Shield") {
            ShieldSpell ss = collision.gameObject.transform.parent.gameObject.GetComponent<ShieldSpell>();
            if (ss != null) {
                PhotonView pv = PhotonView.Get(ss);
                if (pv != null && pv.IsMine) pv.RPC("TakeDamage", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
            } else {
                Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
            }
        }
    }
}
