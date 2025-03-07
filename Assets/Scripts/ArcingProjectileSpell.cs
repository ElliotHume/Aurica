using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ArcingProjectileSpell : Spell, IPunObservable {

    public float FiringAngle = 45f, Gravity = 9.81f;
    public Vector3 Target = Vector3.zero;
    public bool CrosshairTargeted = true;
    public float CollisionOffset = 0;
    public float CollisionDestroyTimeDelay = 5;
    public GameObject[] LocalEffectsOnCollision;
    public string[] NetworkedEffectsOnCollision;
    public bool NetworkCollisionEffectsOnlyOnHitGround = false;
    public GameObject[] DeactivateObjectsOnCollision;


    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isCollided = false, networkCollided = false;
    private Vector3 networkPosition, oldPosition, velocity;
    private Quaternion networkRotation;
    private float projectile_Velocity, flightDuration, Vx, Vy, elapsed_time;
    private new Rigidbody rigidbody;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isCollided);
            stream.SendNext(Vx);
            stream.SendNext(Vy);
            stream.SendNext(projectile_Velocity);
        } else {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkCollided = (bool)stream.ReceiveNext();
            Vx = (float)stream.ReceiveNext();
            Vy = (float)stream.ReceiveNext();
            projectile_Velocity = (float)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (velocity * lag);
        }
    }

    void Start() {
        if (photonView.IsMine) {
            if (CrosshairTargeted) GetTargetFromCrosshair();
            if (Target != null) {
                CalculateTragectory();
            }
        }
        elapsed_time = 0f;
        rigidbody = GetComponent<Rigidbody>();
    }

    void CalculateTragectory() {
        // Calculate distance to target
        float target_Distance = Vector3.Distance(transform.position, Target);

        // Calculate the velocity needed to throw the object to the target at specified angle.
        projectile_Velocity = target_Distance / (Mathf.Sin(2 * FiringAngle * Mathf.Deg2Rad) / Gravity);

        // Extract the X  Y componenent of the velocity
        Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(FiringAngle * Mathf.Deg2Rad);
        Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(FiringAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        flightDuration = target_Distance / Vx;

        // Rotate projectile to face the target.
        transform.rotation = Quaternion.LookRotation(Target - transform.position);
    }

    void FixedUpdate() {
        // Remote Behaviour
        if (!photonView.IsMine) {
            // If the network shows a collision, but no collision has happened locally, spawn the collision at current location
            if (!isCollided && networkCollided) {
                transform.position = networkPosition;
                transform.rotation = networkRotation;
                LocalCollisionBehaviour(networkPosition, Vector3.up);
                isCollided = true;
            }

            // Lag compensation
            if (!isCollided) {
                if (networkPosition.magnitude > 0.05f) transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 1000);
            }
        }

        if (!isCollided) {
            oldPosition = transform.position;
            transform.Translate(0, (Vy - (Gravity * elapsed_time)) * Time.deltaTime, Vx * Time.deltaTime);
            velocity = transform.position - oldPosition;
            elapsed_time += Time.deltaTime;
        }
    }

    public void SetTarget(Vector3 targetPos) {
        Target = targetPos;
    }

    void GetTargetFromCrosshair() {
        SetTarget(Crosshair.Instance.GetWorldPoint());
    }



    void OnCollisionEnter(Collision collision) {
        if ( isCollided ) return;
        // Debug.Log("Collision with: "+ collision.gameObject);
        
        // Prevent the projectile hitting the player who cast it if the flag is set.
        if (collision.gameObject.tag == "Player") {
            PhotonView p = PhotonView.Get(collision.gameObject);
            if (p != null && true) {
                if (p.Owner != null && p.Owner.ActorNumber == photonView.Owner.ActorNumber) {
                    return;
                }
            }
        }

        ContactPoint hit = collision.GetContact(0);
        isCollided = true;

        // Call local collision response to generate collision VFX
        LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            Invoke("DestroySelf", CollisionDestroyTimeDelay + 1f);
            if (collision.gameObject.tag == "Player") {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) { 
                        string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        FlashHitMarker(true);
                    }
                } else {
                    TargetDummy td = collision.gameObject.GetComponent<TargetDummy>();
                    if (td != null) {
                        PhotonView pv = PhotonView.Get(td);
                        if (pv != null) {
                            pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                            FlashHitMarker(true);
                        }
                    }
                }
                
            } else if (collision.gameObject.tag == "Shield") {
                ShieldSpell ss = collision.gameObject.transform.parent.gameObject.GetComponent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            } else if (collision.gameObject.tag == "Structure") {
                Structure structure = collision.gameObject.GetComponent<Structure>();
                if (structure != null) {
                    string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                    PhotonView pv = PhotonView.Get(structure);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        FlashHitMarker(!structure.IsImmune() && !structure.IsBroken());
                    }
                } else {
                    NullSphere nullSphere = collision.gameObject.GetComponent<NullSphere>();
                    if (nullSphere != null) {
                        string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                        PhotonView pv = PhotonView.Get(nullSphere);
                        if (pv != null) {
                            pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                            FlashHitMarker(true);
                        }
                    }
                }
            } else if (collision.gameObject.tag == "DamageableObject") {
                DamageableObject dmgobj = collision.gameObject.GetComponent<DamageableObject>();
                if (dmgobj != null) {
                    PhotonView pv = PhotonView.Get(dmgobj);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), "");
                        FlashHitMarker(false);
                    }
                }
            } else if (collision.gameObject.tag == "Enemy") {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
                if (enemy != null) {
                    enemy.SetLocalPlayerParticipation();
                    PhotonView pv = PhotonView.Get(enemy);
                    if (pv != null) {
                        pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
                        FlashHitMarker(false);
                    }
                }
            }
            if (NetworkCollisionEffectsOnlyOnHitGround && Vector3.Dot(hit.normal, Vector3.up) < 0.707f) return;
            foreach (string effect in NetworkedEffectsOnCollision) {
                GameObject instance = PhotonNetwork.Instantiate(effect, hit.point + hit.normal * CollisionOffset, new Quaternion());
                instance.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
                Spell instanceSpell = instance.GetComponent<Spell>();
                if (instanceSpell != null) {
                    instanceSpell.SetSpellStrength(GetSpellStrength());
                    instanceSpell.SetSpellDamageModifier(GetSpellDamageModifier());
                    instanceSpell.SetOwner(GetOwner());
                } else {
                    Enemy instancedEnemy = instance.GetComponent<Enemy>();
                    if (instancedEnemy != null) {
                        instance.transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
                        instancedEnemy.SetPlayerOwner(GetOwner());
                        instancedEnemy.SetStrength(GetSpellStrength());
                    }
                }
            }
        }
    }

    void LocalCollisionBehaviour(Vector3 hitpoint, Vector3 hitNormal) {
        foreach (var effect in DeactivateObjectsOnCollision) {
            if (effect != null) effect.SetActive(false);
        }
        foreach (var effect in LocalEffectsOnCollision) {
            GameObject instance = Instantiate(effect, hitpoint + hitNormal * CollisionOffset, new Quaternion());
            instance.transform.LookAt(hitpoint + hitNormal + hitNormal * CollisionOffset);
            Destroy(instance, CollisionDestroyTimeDelay);
        }
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }
}
