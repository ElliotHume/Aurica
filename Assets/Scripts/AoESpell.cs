using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AoESpell : Spell {
    public float LastingDamage = 0f;
    public bool OneShotEffect = true, LastingEffect = false, attachToTarget = false, canHitSelf = false, growBeforeStart = false, SpellStrengthChangesDuration = true, SpellStrengthChangesScale = false;
    public float DestroyTimeDelay = 15f, StartTimeDelay = 0f;
    public float ScalingFactor = 0f, ScalingLimit = 0f;
    public Vector3 PositionOffset = Vector3.zero;
    public GameObject[] DeactivateObjectsAfterDuration;
    public ParticleSystem[] EffectsOnDelayedStartup;

    private float amountOfScalingApplied = 0f;
    private bool active = true;

    void Start() {
        float spellStrength = GetSpellStrength();
        if (SpellStrengthChangesDuration) {
            Duration *= spellStrength;
            DestroyTimeDelay *= spellStrength;
            StartTimeDelay *= spellStrength;
        }
        if (SpellStrengthChangesScale) {
            transform.localScale *= spellStrength;
        }
        Duration *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
        DestroyTimeDelay *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
        if (photonView.IsMine) {
            Invoke("DestroySelf", DestroyTimeDelay+StartTimeDelay);
            Invoke("DisableCollisions", Duration+StartTimeDelay);
        }
        if (StartTimeDelay > 0f) {
            active = false;
            DisableCollisions();
            Invoke("Enable", StartTimeDelay);
        }
        Invoke("DisableParticlesAfterDuration", Duration+StartTimeDelay);

        if (!attachToTarget && PositionOffset != Vector3.zero) transform.position += PositionOffset;
    }

    void FixedUpdate() {
        if ((active || growBeforeStart) && ScalingFactor != 0f && (ScalingLimit == 0f || amountOfScalingApplied < ScalingLimit)) {
            transform.localScale += transform.localScale * ScalingFactor * Time.deltaTime;
            if (ScalingLimit != 0f) amountOfScalingApplied += Mathf.Abs(ScalingFactor * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!OneShotEffect || Damage == 0f) return;

        // TODO: Call local collision response to generate collision VFX
        // ContactPoint hit = collision.GetContact(0);
        // LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            if (other.gameObject.tag == "Player" && (other.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf)) {
                PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) {
                        string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        FlashHitMarker(true);
                    }
                } else {
                    TargetDummy td = other.gameObject.GetComponent<TargetDummy>();
                    if (td != null) {
                        PhotonView pv = PhotonView.Get(td);
                        if (pv != null) {
                            pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson());
                            FlashHitMarker(true);
                        }
                    }
                }
            } else if (other.gameObject.tag == "Shield") {
                ShieldSpell ss = other.gameObject.GetComponentInParent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            } else if (other.gameObject.tag == "DamageableObject") {
                DamageableObject dmgobj = other.gameObject.GetComponent<DamageableObject>();
                if (dmgobj != null) {
                    PhotonView pv = PhotonView.Get(dmgobj);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                        FlashHitMarker(true);
                    }
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
            if (other.gameObject.tag == "Player" && (other.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf)) {
                PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();
                string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                    FlashHitMarker(false);
                } else {
                    TargetDummy td = other.gameObject.GetComponent<TargetDummy>();
                    if (td != null) {
                        PhotonView pv = PhotonView.Get(td);
                        if (pv != null) {
                            pv.RPC("OnSpellCollide", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                            FlashHitMarker(false);
                        }
                    }
                }
            } else if (other.gameObject.tag == "Shield") {
                // Same as HitShield but with LastingDamage instead
                ShieldSpell ss = other.gameObject.GetComponentInParent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            } else if (other.gameObject.tag == "DamageableObject") {
                DamageableObject dmgobj = other.gameObject.GetComponent<DamageableObject>();
                if (dmgobj != null) {
                    PhotonView pv = PhotonView.Get(dmgobj);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                        FlashHitMarker(true);
                    }
                }
            }
        }
    }

    public void SetTarget(GameObject targetGO) {
        transform.position = targetGO.transform.position + PositionOffset;
        transform.rotation = targetGO.transform.rotation;

        if (attachToTarget) {
            transform.parent = targetGO.transform;
        }
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        active = false;
        GetComponent<Collider>().enabled = false;
    }

    void Enable() {
        active = true;
        GetComponent<Collider>().enabled = true;
        foreach(var effect in EffectsOnDelayedStartup) {
            effect.Play();
        }
        foreach(var audio in GetComponents<AudioSource>()) {
            audio.Play();
        }
    }

    void DisableParticlesAfterDuration() {
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
    }
}
