using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ChannelledSpell : Spell {

    // AoE fields
    public bool isAoE = true;
    public float LastingDamage = 0f;
    public bool attachToTarget = false, moveTowardsAimpoint = false, canHitSelf = false, growBeforeStart = false, DamageScaling = false, SpellStrengthChangesScale = false;
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

    private float effectStartTimer = 0f, effectTimer = 0f;
    private int numberOfEffectsSpawned = 0;
    private bool spawnEffectsStarted = false;
    Crosshair crosshair;

    void Start() {
        float spellStrength = GetSpellStrength();
        if (SpellStrengthChangesScale) {
            transform.localScale *= spellStrength;
        }

        if (StartTimeDelay > 0f) {
            active = false;
            DisableCollisions();
            Invoke("Enable", StartTimeDelay);
        }

        if (!attachToTarget && PositionOffset != Vector3.zero) transform.position += PositionOffset;

        if (moveTowardsAimpoint) {
            crosshair = Crosshair.Instance;
        }
    }

    void Update() {
        if (!photonView.IsMine) return;
        if (moveTowardsAimpoint) {
            transform.position = Vector3.MoveTowards(transform.position, crosshair.GetWorldPoint(), MoveSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate() {
        if (!photonView.IsMine) return;
        if ((active || growBeforeStart) && ScalingFactor != 0f && (ScalingLimit == 0f || amountOfScalingApplied < ScalingLimit)) {
            transform.localScale += transform.localScale * ScalingFactor * Time.deltaTime;
            if (ScalingLimit != 0f) amountOfScalingApplied += Mathf.Abs(ScalingFactor * Time.deltaTime);
            Debug.Log("SCALING "+amountOfScalingApplied);
        }

        if (active && DamageScaling) {
            LastingDamage += Time.deltaTime * DamageScalingRate;
            Debug.Log("Lasting damage: "+LastingDamage);
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
