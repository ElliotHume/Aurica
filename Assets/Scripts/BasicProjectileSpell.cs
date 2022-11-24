using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;

public class BasicProjectileSpell : Spell, IPunObservable
{
    public float Speed = 20f;
    public float RandomMoveRadius = 0f;
    public float RandomMoveSpeedScale = 0f;
    public bool CanHitSelf = true, IgnoreNonPlayerCollision = false;
    public bool AimAssistedProjectile = true;
    public float AimAssistTurningSpeed = 15f;
    public bool PerfectHomingProjectile = false;
    public float HomingDetectionSphereRadius = 1.5f;
    public bool TrackingProjectile = false;
    public float TrackingTurnSpeed = 10f;
    public float CollisionOffset = 0;
    public float CollisionDestroyTimeDelay = 5;
    public float MaxDistance = 0f, AirDrag = 0f, MinSpeed = 0f;
    public bool ExplodeOnReachMaxDistance = false;
    public GameObject[] EffectsOnCollision;
    public string[] NetworkedEffectsOnCollision;
    public bool LocalCollisionEffectsUseHitNormal = true, NetworkCollisionEffectsUseHitNormal = true, NetworkCollisionEffectsOnlyOnHitGround = false;
    public GameObject[] DeactivateObjectsOnCollision;

    public bool ParticleCollisions = false;
    public float DamagePerParticle = 1f;
    public ParticleSystem collisionParticles;
    public AudioClip particleCollisionSound;
    public float clipVolume;

    private List<ParticleCollisionEvent> collisionEvents;
    private Vector3 startPosition;
    private Vector3 travelDistance = Vector3.zero;
    private Quaternion startRotation;
    private bool isCollided = false, networkCollided = false, enemyAttack = false, collidersDisabled = false, layerSwitched = false;
    private GameObject HomingTarget, AimAssistTarget;
    private Transform homingTargetT, aimAssistTargetT;
    private Vector3 randomTimeOffset, playerOffset = new Vector3(0f, 1f, 0f);
    private Crosshair crosshair;
    // private int playerDetectingLayerMask = 1 << 3;
    private int collidedViewId = -1, networkCollidedViewId = -1;
    private Vector3 networkPosition, oldPosition, velocity;
    private Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(Speed);
            stream.SendNext(isCollided);
            stream.SendNext(collidedViewId);
        } else {
            networkPosition = (Vector3) stream.ReceiveNext();
            networkRotation = (Quaternion) stream.ReceiveNext();
            Speed = (float) stream.ReceiveNext();
            networkCollided = (bool) stream.ReceiveNext();
            networkCollidedViewId = (int) stream.ReceiveNext();

            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (velocity * lag);
        }
    }


    void Start() {
        startPosition = transform.position;
        startRotation = transform.rotation;
        transform.parent = null;
        randomTimeOffset = Random.insideUnitSphere * 10;
        Speed *= GameManager.GLOBAL_SPELL_SPEED_MULTIPLIER;
        crosshair = Crosshair.Instance;
        collisionEvents = new List<ParticleCollisionEvent>();

        if (!TrackingProjectile && AimAssistedProjectile && AimAssistTurningSpeed < Speed/3f) {
            AimAssistTurningSpeed = Speed/3f;
        }
    }

    public void Update() {
        if (!photonView.IsMine) {
            if (!layerSwitched) {
                int SpellCollisionLayer = LayerMask.NameToLayer("SpellCollider");
                gameObject.layer = SpellCollisionLayer;
                layerSwitched = true;
            }
            if (!isCollided) {
                if (networkPosition.magnitude > 0.05f) transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime * Speed) + (velocity * Time.deltaTime);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 1000);

                // If no collision has happened locally, but the network shows a collision, determine where the collision should occur and spawn it.
                if (networkCollided) {
                    Vector3 hitPosition = networkPosition;
                    if (networkCollidedViewId != -1) {
                        // The spell has collided with a player, we want to make sure this is reflected clientside by moving the projectile towards the player that is hit
                        Transform hitPlayer = PhotonView.Find(networkCollidedViewId).gameObject.transform;
                        hitPosition = ((hitPlayer.position+new Vector3(0f,1f,0f)) + networkPosition)/2f;
                        transform.position = Vector3.MoveTowards(transform.position, hitPosition, Time.deltaTime * Speed * 2f);
                    } else {
                        // The spell has collided with a non-player object, move it to the networked position on the assumption that the hit object is stationary.
                        transform.position = Vector3.MoveTowards(transform.position, hitPosition, Time.deltaTime * Speed * 2f);
                    }
                    if (Vector3.Distance(transform.position, hitPosition) <= 0.1f) {
                        transform.rotation = networkRotation;
                        LocalCollisionBehaviour(transform.position, -transform.forward);
                        isCollided = true;
                    }
                }
            }
        }

        oldPosition = transform.position;
        UpdateWorldPosition();
        velocity = transform.position - oldPosition;
    }

    void OnCollisionEnter(Collision collision) {
        if ( isCollided || !photonView.IsMine ) return;

        // Debug.Log(""+gameObject+"  Collided with  "+collision.gameObject);
        
        // Prevent the projectile hitting the player who cast it if the flag is set.
        if (!CanHitSelf && collision.gameObject.tag == "Player") {
            PhotonView p = PhotonView.Get(collision.gameObject);
            if (p != null && true) {
                if (p.Owner != null && p.Owner.ActorNumber == photonView.Owner.ActorNumber) {
                    return;
                }
            }
        }

        if (enemyAttack && collision.gameObject == GetOwner()) return;

        if (IgnoreNonPlayerCollision && collision.gameObject.tag != "Player" && collision.gameObject.tag != "Untagged" && collision.gameObject.tag != "Enemy" ) return;

        ContactPoint hit = collision.GetContact(0);
        isCollided = true;

        // Call local collision response to generate collision VFX
        LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            Invoke("DestroySelf", CollisionDestroyTimeDelay+1f);
            if (collision.gameObject.tag == "Player") {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        collidedViewId = pv.ViewID;
                        FlashHitMarker(true);
                    }
                } else {
                    TargetDummy td = collision.gameObject.GetComponent<TargetDummy>();
                    if (td != null) {
                        PhotonView pv = PhotonView.Get(td);
                        if (pv != null) {
                            pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                            collidedViewId = pv.ViewID;
                            FlashHitMarker(true);
                        }
                    }
                }
            } else if (collision.gameObject.tag == "Enemy") {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                if (enemy != null) {
                    PhotonView pv = PhotonView.Get(enemy);
                    if (pv != null) {
                        enemy.SetLocalPlayerParticipation();
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        collidedViewId = pv.ViewID;
                        FlashHitMarker(true);
                    }
                }
            }
            if (!IgnoreNonPlayerCollision) {
                if (collision.gameObject.tag == "Shield") {
                    ShieldSpell ss = collision.gameObject.transform.parent.gameObject.GetComponent<ShieldSpell>();
                    if (ss != null) {
                        PhotonView pv = PhotonView.Get(ss);
                        if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
                    } else {
                        Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                    }
                } else if (collision.gameObject.tag == "Spell") {
                    // This is required for the case when a remote client shoots a spell out of the air.
                    // Without this RPC the spell will turn off its collision on remote clients before those remote clients register the collision on the local spell that is hit.
                    BasicProjectileSpell spell = collision.gameObject.GetComponent<BasicProjectileSpell>();
                    if (spell != null) {
                        PhotonView pv = PhotonView.Get(spell);
                        if (pv != null) {
                            pv.RPC("SpellCollision", RpcTarget.All);
                        }
                    }
                } else if (collision.gameObject.tag == "DamageableObject") {
                    DamageableObject dmgobj = collision.gameObject.GetComponent<DamageableObject>();
                    if (dmgobj != null) {
                        PhotonView pv = PhotonView.Get(dmgobj);
                        if (pv != null) {
                            pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                            collidedViewId = pv.ViewID;
                            FlashHitMarker(true);
                        }
                    }
                }
            }
            
            NetworkCollisionBehaviour(hit.point, hit.normal);
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
                if (pm != null && (!photonView.IsMine || CanHitSelf)) {
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

    [PunRPC]
    public void SpellCollision() {
        if (IgnoreNonPlayerCollision) return;
        Debug.Log(gameObject+" hit by spell");
        LocalCollisionBehaviour(transform.position, -transform.forward);
        isCollided = true;
        if (photonView.IsMine) {
            Invoke("DestroySelf", CollisionDestroyTimeDelay+1f);
            NetworkCollisionBehaviour(transform.position, Vector3.up);
        }
    }

    void LocalCollisionBehaviour(Vector3 hitpoint, Vector3 hitNormal) {
        Vector3 normal = LocalCollisionEffectsUseHitNormal ? hitNormal : Vector3.up;
        foreach (var effect in DeactivateObjectsOnCollision) {
            if (effect != null) effect.SetActive(false);
        }
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var effect in particles) {
            if (effect != null) effect.Stop();
        }
        foreach (var effect in EffectsOnCollision) {
            GameObject instance = Instantiate(effect, hitpoint + normal * CollisionOffset, new Quaternion());
            instance.transform.LookAt(hitpoint + normal + normal * CollisionOffset);
            Destroy(instance, CollisionDestroyTimeDelay);
        }
        DisableCollisions();
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void NetworkCollisionBehaviour(Vector3 hitPoint, Vector3 hitNormal) {
        Vector3 normal = NetworkCollisionEffectsUseHitNormal ? hitNormal : Vector3.up;
        if (NetworkCollisionEffectsOnlyOnHitGround && Vector3.Dot(hitNormal, Vector3.up) < 0.707f) return;
        foreach(string effect in NetworkedEffectsOnCollision) {
            GameObject instance = PhotonNetwork.Instantiate(effect, hitPoint + normal * CollisionOffset, transform.rotation);
            Enemy instancedEnemy = instance.GetComponent<Enemy>();
            if (instancedEnemy != null) {
                instance.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                instancedEnemy.SetPlayerOwner(GetOwner());
                instancedEnemy.SetStrength(GetSpellStrength());
            } else {
                instance.transform.LookAt(hitPoint + normal + normal * CollisionOffset);
                instance.transform.Rotate(Vector3.forward, transform.eulerAngles.y);
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
        PhotonNetwork.Destroy(gameObject);
    }

    void UpdateWorldPosition() {
        if (!photonView.IsMine || isCollided) return;
        if (HomingTarget != null && homingTargetT == null) homingTargetT = HomingTarget.transform;
        if ((MaxDistance > 0f && travelDistance.magnitude > MaxDistance) || Speed < 0.1f) {
            if (ExplodeOnReachMaxDistance) {
                LocalCollisionBehaviour(transform.position, transform.forward);
                NetworkCollisionBehaviour(transform.position, transform.forward);
            } else {
                // Similar to LocalCollision Behaviour, but do not spawn any effects
                foreach (var effect in DeactivateObjectsOnCollision) {
                    if (effect != null) effect.SetActive(false);
                }
                DisableCollisions();
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().isKinematic = true;
                Invoke("DestroySelf", CollisionDestroyTimeDelay+1f);
            }
            isCollided = true;
            return;
        }

        // If aim assisted, get the player that is being aimed at from the crosshair and set them as the target
        if (AimAssistedProjectile && !enemyAttack) {
            GameObject crossHairHit = crosshair.GetPlayerHit(1f);
            SetAimAssistTarget(crossHairHit);
            if (PerfectHomingProjectile && crossHairHit != null && (crossHairHit.transform.position-transform.position).magnitude <= HomingDetectionSphereRadius) {
                // Debug.Log("Set homing target to aim targeted player.");
                SetHomingTarget(crossHairHit);
            }
        }

        Vector3 randomOffset = Vector3.zero;
        if (RandomMoveRadius > 0) {
            randomOffset = GetRadiusRandomVector() * RandomMoveRadius;
            if (HomingTarget != null) {
                var fade = Vector3.Distance(transform.position, homingTargetT.position) / Vector3.Distance(startPosition, homingTargetT.position);
                randomOffset *= fade;
            }
        }

        // // DEPRECATED, now use the crosshair aim assist targeting to find homing targets.
        // If a homing projectile, check for a player in the radius and set them as them target
        // if (PerfectHomingProjectile && HomingTarget == null) {
        //     Collider[] hits = Physics.OverlapSphere(transform.position, HomingDetectionSphereRadius, playerDetectingLayerMask);
        //     if (hits.Length > 0) {
        //         foreach( var hit in hits ) {
        //             if (hit.gameObject != PlayerManager.LocalPlayerGameObject) {
        //                 Debug.Log("Set homing target to player found in radius");
        //                 SetHomingTarget(hit.gameObject);
        //                 break;
        //             }
        //         }
        //     }
        // }

        var frameMoveOffsetWorld = Vector3.zero;
        if (PerfectHomingProjectile && HomingTarget != null) {
            var forwardVec = ((homingTargetT.position + playerOffset) - transform.position).normalized;
            var currentForwardVector = (forwardVec + randomOffset) * Speed * Time.deltaTime;
            frameMoveOffsetWorld = currentForwardVector;
        } else {
            if (AimAssistedProjectile && AimAssistTarget != null) {
                startRotation = Quaternion.RotateTowards(startRotation, Quaternion.LookRotation((aimAssistTargetT.position + playerOffset * aimAssistTargetT.localScale.y) - transform.position), (AimAssistTurningSpeed + TrackingTurnSpeed) * Time.deltaTime);
            } else if (TrackingProjectile) {
                startRotation = Quaternion.RotateTowards(startRotation, Quaternion.LookRotation(crosshair.GetWorldPoint() - transform.position), TrackingTurnSpeed * Time.deltaTime);
            }
            
            var currentForwardVector = (Vector3.forward + randomOffset) * Speed * Time.deltaTime;
            frameMoveOffsetWorld = startRotation * currentForwardVector;
        }

        if (TrackingProjectile) transform.rotation = startRotation;
        if (AirDrag > 0f) Speed = Mathf.Lerp(Speed, MinSpeed, AirDrag/100f);
        transform.position += frameMoveOffsetWorld;
        travelDistance += frameMoveOffsetWorld;

        if (HomingTarget != null && ((homingTargetT.position + playerOffset) - transform.position).magnitude < 1.25f) {
            transform.position = homingTargetT.position + playerOffset;
        }
    }

    Vector3 GetRadiusRandomVector() {
        var x = Time.time * RandomMoveSpeedScale + randomTimeOffset.x;
        var vecX = Mathf.Sin(x / 7 + Mathf.Cos(x / 2)) * Mathf.Cos(x / 5 + Mathf.Sin(x));

        x = Time.time * RandomMoveSpeedScale + randomTimeOffset.y;
        var vecY = Mathf.Cos(x / 8 + Mathf.Sin(x / 2)) * Mathf.Sin(Mathf.Sin(x / 1.2f) + x * 1.2f);

        x = Time.time * RandomMoveSpeedScale + randomTimeOffset.z;
        var vecZ = Mathf.Cos(x * 0.7f + Mathf.Cos(x * 0.5f)) * Mathf.Cos(Mathf.Sin(x * 0.8f) + x * 0.3f);

        return new Vector3(vecX, vecY, vecZ);
    }

    public void SetHomingTarget(GameObject go) {
        HomingTarget = go;
        homingTargetT = go.transform;
    }

    public void SetAimAssistTarget(GameObject go) {
        if (go == null) {
            AimAssistTarget = null;
            aimAssistTargetT = null;
            return;
        }
        AimAssistTarget = go;
        aimAssistTargetT = go.transform;
    }

    public void SetEnemyAttack() {
        enemyAttack = true;
    }

    public void DisableCollisions() {
        foreach(Collider col in GetComponents<Collider>()) col.enabled = false;
        collidersDisabled = true;
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, HomingDetectionSphereRadius);
    }
}
