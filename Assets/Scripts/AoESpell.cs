using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AoESpell : Spell {
    public float LastingDamage = 0f;
    public bool OneShotEffect = true, LastingEffect = false, canHitSelf = false;
    public float DestroyTimeDelay = 15f, CollisionTimeDelay = 0f;
    public float ScalingFactor = 0f, ScalingLimit = 0f;
    public GameObject[] DeactivateObjectsAfterDuration;

    private float amountOfScalingApplied = 0f;

    void Awake() {
        if (photonView.IsMine) {
            Invoke("DestroySelf", DestroyTimeDelay);
            Invoke("DisableCollisions", Duration);
        }
        if (CollisionTimeDelay > 0f) {
            DisableCollisions();
            Invoke("EnableCollisions", CollisionTimeDelay);
        }
        Invoke("DisableParticlesAfterDuration", Duration);
    }

    void FixedUpdate() {
        if (ScalingFactor != 0f && (ScalingLimit == 0f || amountOfScalingApplied < ScalingLimit)) {
            transform.localScale += transform.localScale * ScalingFactor * Time.deltaTime;
            if (ScalingLimit != 0f) amountOfScalingApplied += Mathf.Abs(ScalingFactor * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!OneShotEffect) return;

        // TODO: Call local collision response to generate collision VFX
        // ContactPoint hit = collision.GetContact(0);
        // LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            if (other.gameObject.tag == "Player" && (other.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf)) {
                PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength(), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson());
                }
            } else if (other.gameObject.tag == "Shield") {
                ShieldSpell ss = other.gameObject.transform.parent.gameObject.GetComponent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, Damage * GetSpellStrength(), auricaSpell.targetDistribution.GetJson());
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (!LastingEffect) return;
        // TODO: Call local collision response to generate collision VFX
        // ContactPoint hit = collision.GetContact(0);
        // LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            if (other.gameObject.tag == "Player" && (other.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf)) {
                PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength(), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson());
                }
            } else if (other.gameObject.tag == "Shield") {
                // Same as HitShield but with LastingDamage instead
                ShieldSpell ss = other.gameObject.transform.parent.gameObject.GetComponent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength(), auricaSpell.targetDistribution.GetJson());
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            }
        }
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        GetComponent<Collider>().enabled = false;
    }

    void EnableCollisions() {
        GetComponent<Collider>().enabled = true;
    }

    void DisableParticlesAfterDuration() {
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
    }
}
