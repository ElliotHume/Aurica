using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AoESpell : Spell {
    public float LastingDamage = 0f;
    public bool OneShotEffect = true, LastingEffect = false, attachToTarget = false, canHitSelf = false, growBeforeStart = false, SpellStrengthChangesDuration = true, SpellStrengthChangesScale = false;
    public float DestroyTimeDelay = 15f, StartTimeDelay = 0f;
    public float ScalingFactor = 0f, ScalingLimit = 0f;
    public Vector3 TargetingIndicatorScale = Vector3.zero;
    public Vector3 PositionOffset = Vector3.zero;
    public GameObject[] DeactivateObjectsAfterDuration;
    public ParticleSystem[] EffectsOnDelayedStartup;
    public string[] NetworkedEffectsOnDelay;
    public float NetworkedEffectsDelay;
    public Vector3 NetworkedEffectsOffset;

    public bool ParticleCollisions = false;
    public float DamagePerParticle = 1f;
    public ParticleSystem collisionParticles;
    public AudioClip particleCollisionSound;
    public float clipVolume;
    private List<ParticleCollisionEvent> collisionEvents;

    private float amountOfScalingApplied = 0f;
    private bool active = true;

    void Start() {
        collisionEvents = new List<ParticleCollisionEvent>();
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

            if (NetworkedEffectsOnDelay.Length > 0) {
                Invoke("CreateNetworkedEffects", NetworkedEffectsDelay);
            }
        }
        if (StartTimeDelay > 0f) {
            active = false;
            DisableCollisions();
            Invoke("Enable", StartTimeDelay);
        }
        Invoke("EndSpell", Duration+StartTimeDelay);

        if (!attachToTarget && PositionOffset != Vector3.zero) transform.position += (transform.forward * PositionOffset.z + transform.right * PositionOffset.x + transform.up * PositionOffset.y);
    }

    void FixedUpdate() {
        if ((active || growBeforeStart) && ScalingFactor != 0f && (ScalingLimit == 0f || amountOfScalingApplied < ScalingLimit)) {
            transform.localScale += transform.localScale * ScalingFactor * Time.deltaTime;
            if (ScalingLimit != 0f) amountOfScalingApplied += Mathf.Abs(ScalingFactor * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!OneShotEffect || Damage == 0f || (!GetCanHitOwner() && other.gameObject == GetOwner())) return;

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
                            pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
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
            } else if (other.gameObject.tag == "Enemy") {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                if (enemy != null) {
                    PhotonView pv = PhotonView.Get(enemy);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        FlashHitMarker(true);
                    }
                }
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (!LastingEffect || (!GetCanHitOwner() && other.gameObject == GetOwner())) return;
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
                        FlashHitMarker(false);
                    }
                }
            } else if (other.gameObject.tag == "Enemy") {
                Enemy enemy = other.gameObject.GetComponent<Enemy>();
                string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                if (enemy != null) {
                    enemy.SetLocalPlayerParticipation();
                    PhotonView pv = PhotonView.Get(enemy);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, LastingDamage * 0.002f * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        FlashHitMarker(false);
                    }
                }
            }
        }
    }

    void OnParticleCollision(GameObject other) {
        // Particle collisions are locally authoratative for players, if on YOUR screen you are hit by a particle you take the damage.
        if (!ParticleCollisions || (!GetCanHitOwner() && other == GetOwner())) return;
        PhotonView pv = PhotonView.Get(other);
        int numCollisionEvents = collisionParticles.GetCollisionEvents(other, collisionEvents);
        if (particleCollisionSound) {
            foreach(var collision in collisionEvents) {
                AudioSource.PlayClipAtPoint(particleCollisionSound, collision.intersection, clipVolume);
            }
        }

        if (pv != null && pv.IsMine){
            if (other.tag == "Player") {
                PlayerManager pm = other.GetComponent<PlayerManager>();
                if (pm != null && (!photonView.IsMine || canHitSelf)) {
                    string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                    pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerParticle * numCollisionEvents * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                    FlashHitMarker(false);
                } else {
                    TargetDummy td = other.GetComponent<TargetDummy>();
                    if (td != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerParticle * numCollisionEvents * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                        FlashHitMarker(false);
                    }
                }
            }
        }

        // Anything other than players is handled by the spell owner
        if (!photonView.IsMine) return;    
        if (other.tag == "Enemy") {
            Enemy enemy = other.GetComponent<Enemy>();
            string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
            if (enemy != null) {
                enemy.SetLocalPlayerParticipation();
                pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerParticle * numCollisionEvents * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                FlashHitMarker(false);
            }
        } else if (other.tag == "Shield") {
            ShieldSpell ss = other.transform.parent.gameObject.GetComponent<ShieldSpell>();
            if (ss != null) {
                pv = PhotonView.Get(ss);
                if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, DamagePerParticle * numCollisionEvents * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
            } else {
                Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
            }
        } else if (other.tag == "DamageableObject") {
            DamageableObject dmgobj = other.GetComponent<DamageableObject>();
            if (dmgobj != null) {
                pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerParticle * numCollisionEvents * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                FlashHitMarker(false);
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

    public Vector3 GetTargetingIndicatorScale() {
        if (TargetingIndicatorScale == Vector3.zero || TargetingIndicatorScale == Vector3.one) {
            TargetingIndicatorScale = Vector3.one;
            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            if (sphereCollider != null) {
                TargetingIndicatorScale = (sphereCollider.radius * 2) * transform.lossyScale;
            } else {
                CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
                if (capsuleCollider != null) {
                    TargetingIndicatorScale = (capsuleCollider.radius * 2) * transform.lossyScale;
                } else {
                    BoxCollider boxCollider = GetComponent<BoxCollider>();
                    if (boxCollider != null) {
                        TargetingIndicatorScale = Vector3.Scale(boxCollider.size, transform.lossyScale);
                    }
                }
            }
        }
        return TargetingIndicatorScale;
    }

    void CreateNetworkedEffects() {
        foreach(string effect in NetworkedEffectsOnDelay) {
            GameObject instance = PhotonNetwork.Instantiate(effect, transform.position + (transform.forward * NetworkedEffectsOffset.z) + (transform.right * NetworkedEffectsOffset.x) + (transform.up * NetworkedEffectsOffset.y), transform.rotation);
            Enemy instancedEnemy = instance.GetComponent<Enemy>();
            if (instancedEnemy != null) {
                instance.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                instancedEnemy.SetPlayerOwner(GetOwner());
                instancedEnemy.SetStrength(GetSpellStrength());
            } else {
                Spell instanceSpell = instance.GetComponent<Spell>();
                if (instanceSpell != null) {
                    instanceSpell.SetSpellStrength(GetSpellStrength());
                    instanceSpell.SetSpellDamageModifier(GetSpellDamageModifier());
                    instanceSpell.SetOwner(GetOwner());
                }
            }
        }
    }

    void DestroySelf() {
        StatusEffect[] statusEffects = GetComponents<StatusEffect>();
        foreach(StatusEffect status in statusEffects) status.ManualDeactivate();
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        // Debug.Log("Disable Collisions");
        active = false;
        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;
        StatusEffect se = GetComponent<StatusEffect>();
        if (se) se.ManualDeactivate();
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

    void EndSpell() {
        photonView.RPC("StopParticles", RpcTarget.All);
    }

    [PunRPC]
    public void StopParticles() {
        DisableParticlesAfterDuration();
    }

    void DisableParticlesAfterDuration() {
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var effect in particles) {
            if (effect != null) effect.Stop();
        }
    }
}
