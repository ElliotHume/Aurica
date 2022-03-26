using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChannelledSpell : Spell {

    // AoE fields
    public bool isAoE = true;
    public float LastingDamage = 0f;
    public bool attachToTarget = false, moveTowardsAimpoint = false, turnTowardsAimpoint = false, canHitSelf = false, growBeforeStart = false, DamageScaling = false, SpellStrengthChangesScale = false, SpellStrengthChangesScalingSpeed=false;
    public float StartTimeDelay = 0f, DestroyTimeDelay = 3f, MoveSpeed = 5f, DamageScalingRate = 1f;
    public float ScalingFactor = 0f, ScalingLimit = 0f;
    public Vector3 PositionOffset = Vector3.zero;
    public GameObject[] DeactivateObjectsAfterChannel;
    public ParticleSystem[] EffectsOnDelayedStartup;

    private float amountOfScalingApplied = 0f;
    private bool active = true;

    // Channel specific fields
    public bool spawnsEffects = false;
    public string[] NetworkedSpawnEffects;
    public float SpawnEffectsDelay = 0.5f, TimeBetweenSpawnEffects = 1f;
    public int MaxNumberOfEffectsSpawned = 0;
    public float RandomSpawnSphereSize = 0f;
    public bool SpawnedEffectsFaceAwayFromCenter = false;

    public bool ParticleCollisions = false;
    public float DamagePerParticle = 1f;
    public ParticleSystem collisionParticles;
    public AudioClip particleCollisionSound;
    public float clipVolume;
    private List<ParticleCollisionEvent> collisionEvents;

    private float effectStartTimer = 0f, effectTimer = 0f;
    private int numberOfEffectsSpawned = 0;
    private bool spawnEffectsStarted = false;
    Crosshair crosshair;

    void Start() {
        collisionEvents = new List<ParticleCollisionEvent>();
        float spellStrength = GetSpellStrength();
        if (SpellStrengthChangesScale) {
            transform.localScale *= spellStrength;
        }
        if (SpellStrengthChangesScalingSpeed) {
            ScalingFactor *= Mathf.Max(0.66f, 0.1f + GetSpellStrength());
        }

        if (StartTimeDelay > 0f) {
            active = false;
            DisableCollisions();
            Invoke("Enable", StartTimeDelay);
        }

        if (!attachToTarget && PositionOffset != Vector3.zero) transform.position += PositionOffset;

        if (moveTowardsAimpoint || turnTowardsAimpoint) {
            crosshair = Crosshair.Instance;
        }
    }

    void Update() {
        if (!photonView.IsMine) return;
        if (active && moveTowardsAimpoint) {
            transform.position = Vector3.MoveTowards(transform.position, crosshair.GetWorldPoint(), MoveSpeed * Time.deltaTime);
        }
        if (active && turnTowardsAimpoint) {
            Vector3 direction = crosshair.GetWorldPoint() - transform.position;
            Quaternion toRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, MoveSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate() {
        if (!photonView.IsMine) return;
        if ((active || growBeforeStart) && ScalingFactor != 0f && (ScalingLimit == 0f || amountOfScalingApplied < ScalingLimit)) {
            transform.localScale += transform.localScale * ScalingFactor * Time.deltaTime;
            if (ScalingLimit != 0f) amountOfScalingApplied += Mathf.Abs(ScalingFactor * Time.deltaTime);
        }

        if (active && DamageScaling) {
            LastingDamage += Time.deltaTime * DamageScalingRate;
            if (ParticleCollisions) DamagePerParticle += Time.deltaTime * DamageScalingRate;
        }

        if (active && spawnsEffects) {
            if (!spawnEffectsStarted) {
                // Run a timer until the spawn effects should start.
                if (SpawnEffectsDelay == 0) spawnEffectsStarted = true;
                effectStartTimer += Time.deltaTime;
                if (effectStartTimer >= SpawnEffectsDelay) {
                    spawnEffectsStarted = true;
                }
            } else {
                if (MaxNumberOfEffectsSpawned != 0 && numberOfEffectsSpawned >= MaxNumberOfEffectsSpawned) return;

                effectTimer += Time.deltaTime;
                if (effectTimer >= TimeBetweenSpawnEffects) {
                    SpawnNetworkEffects();
                    effectTimer = 0f;
                    numberOfEffectsSpawned++;
                }
            }
        }
    }

    public void EndChannel() {
        Invoke("DestroySelf", DestroyTimeDelay);
        DisableCollisions();
        transform.parent = null;
        active = false;
        photonView.RPC("StopParticles", RpcTarget.All);
    }

    [PunRPC]
    public void StopParticles() {
        DisableParticlesAfterChannel();
    }

    void OnTriggerStay(Collider other) {
        if (photonView.IsMine && isAoE) {
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
        if (pv != null && !pv.IsMine) return;
        
        int numCollisionEvents = collisionParticles.GetCollisionEvents(other, collisionEvents);
        if (particleCollisionSound) {
            foreach(var collision in collisionEvents) {
                AudioSource.PlayClipAtPoint(particleCollisionSound, collision.intersection, clipVolume);
            }
        }

        if (other.tag == "Player" && (!photonView.IsMine || canHitSelf)) {
            Debug.Log("Player hit: "+other);
            PlayerManager pm = other.GetComponent<PlayerManager>();
            if (pm != null) {
                if (pv != null) {
                    string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                    pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerParticle * numCollisionEvents * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                    FlashHitMarker(false);
                }
            } else {
                TargetDummy td = other.GetComponent<TargetDummy>();
                if (td != null) {
                    if (pv != null) {
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
                if (pv != null) {
                    pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerParticle * numCollisionEvents * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                    FlashHitMarker(false);
                }
            }
        } else if (other.tag == "Shield") {
            ShieldSpell ss = other.transform.parent.gameObject.GetComponent<ShieldSpell>();
            if (ss != null) {
                if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, DamagePerParticle * numCollisionEvents * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
            } else {
                Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
            }
        } else if (other.tag == "DamageableObject") {
            DamageableObject dmgobj = other.GetComponent<DamageableObject>();
            if (dmgobj != null) {
                if (pv != null) {
                    pv.RPC("OnSpellCollide", RpcTarget.All, DamagePerParticle * numCollisionEvents * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                    FlashHitMarker(false);
                }
            }
        }
    }

    void SpawnNetworkEffects() {
        foreach(string effect in NetworkedSpawnEffects) {
            Vector3 spawnPoint = transform.position;
            Quaternion spawnRotation = transform.rotation;
            if (RandomSpawnSphereSize > 0f) {
                spawnPoint += Random.insideUnitSphere * RandomSpawnSphereSize;
            }
            if (SpawnedEffectsFaceAwayFromCenter) {
                Vector3 direction = transform.position - spawnPoint;
                spawnRotation = Quaternion.LookRotation(direction);
            }
            GameObject instance = PhotonNetwork.Instantiate(effect, spawnPoint, spawnRotation);
            Spell instanceSpell = instance.GetComponent<Spell>();
            if (instanceSpell != null) {
                instanceSpell.SetSpellStrength(GetSpellStrength());
                instanceSpell.SetSpellDamageModifier(GetSpellDamageModifier());
                instanceSpell.SetOwner(GetOwner());
            }
        }
    }

    public void SetTarget(GameObject targetGO) {
        transform.position = targetGO.transform.position + PositionOffset;
        transform.rotation = targetGO.transform.rotation;

        if (attachToTarget) {
            transform.parent = targetGO.transform;
            transform.localPosition = PositionOffset;
            transform.localRotation = Quaternion.identity;
        }
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        active = false;
        Collider coll = GetComponent<Collider>();
        if (coll != null) coll.enabled = false;
    }

    void Enable() {
        active = true;
        Collider coll = GetComponent<Collider>();
        if (coll != null) coll.enabled = true;

        foreach(var effect in EffectsOnDelayedStartup) {
            effect.Play();
        }
        foreach(var audio in GetComponents<AudioSource>()) {
            audio.Play();
        }
    }

    void DisableParticlesAfterChannel() {
        transform.parent = null;
        active=false;
        foreach (var effect in DeactivateObjectsAfterChannel) {
            if (effect != null) effect.SetActive(false);
        }
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var effect in particles) {
            if (effect != null) effect.Stop();
        }
    }

    void OnDrawGizmosSelected() {
        if (RandomSpawnSphereSize > 0f) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, RandomSpawnSphereSize);
        }
    }
}
