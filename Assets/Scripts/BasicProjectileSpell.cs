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
    public bool CanHitSelf = true, ContinuesPastCollision = false;
    public GameObject Target;
    public bool HomingProjectile = false;
    public float HomingDetectionSphereRadius = 1f;
    public bool TrackingProjectile = false;
    public float TrackingTurnSpeed = 0.1f;
    public float CollisionOffset = 0;
    public float CollisionDestroyTimeDelay = 5;
    public float MaxDistance = 0f, AirDrag = 0f, MinSpeed = 0f;
    public bool ExplodeOnReachMaxDistance = false;
    public GameObject[] EffectsOnCollision;
    public string[] NetworkedEffectsOnCollision;
    public GameObject[] DeactivateObjectsOnCollision;

    private Vector3 startPosition;
    private Vector3 travelDistance = Vector3.zero;
    private Quaternion startRotation;
    private bool isCollided = false, networkCollided = false;
    private Transform targetT;
    private bool targetIsPlayer;
    private Vector3 randomTimeOffset;
    private Crosshair crosshair;
    private int homingLayerMask = 1 << 3;

    private Vector3 networkPosition, oldPosition, velocity;
    private Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(Speed);
            stream.SendNext(isCollided);
        } else {
            networkPosition = (Vector3) stream.ReceiveNext();
            networkRotation = (Quaternion) stream.ReceiveNext();
            Speed = (float) stream.ReceiveNext();
            networkCollided = (bool) stream.ReceiveNext();

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
        if (TrackingProjectile) crosshair = Crosshair.Instance;
    }


    public void FixedUpdate() {
        // Check for network collision
        if (!photonView.IsMine) {
            // If no collision has happened locally, but the network shows a collision, spawn the collision at current location
            if (!isCollided && networkCollided) {
                transform.position = networkPosition;
                transform.rotation = networkRotation;
                LocalCollisionBehaviour(networkPosition, -transform.forward);
                isCollided = true;
            }
        }

        // Local Behaviour
        oldPosition = transform.position;
        UpdateWorldPosition();
        velocity = transform.position - oldPosition;

        // Remote movement compensation
        if (!photonView.IsMine) {
            if (!isCollided) {
                if (networkPosition.magnitude > 0.05f) transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime * Speed);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 1000);
            }
        }
        
    }

    void OnCollisionEnter(Collision collision) {
        if ( isCollided ) return;
        Debug.Log("Collision with: "+collision.gameObject);
        
        if (!CanHitSelf) {
            PhotonView p = PhotonView.Get(collision.gameObject);
            if (p != null) {
                if (p.Owner.ActorNumber == photonView.Owner.ActorNumber) {
                    return;
                }
            }
        }

        ContactPoint hit = collision.GetContact(0);
        if (!ContinuesPastCollision) isCollided = true;

        // Call local collision response to generate collision VFX
        LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            Invoke("DestroySelf", CollisionDestroyTimeDelay+1f);
            if (collision.gameObject.tag == "Player") {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson());
                }
            } else if (collision.gameObject.tag == "Shield") {
                ShieldSpell ss = collision.gameObject.transform.parent.gameObject.GetComponent<ShieldSpell>();
                if (ss != null) {
                    PhotonView pv = PhotonView.Get(ss);
                    if (pv != null) pv.RPC("TakeDamage", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), auricaSpell.targetDistribution.GetJson());
                } else {
                    Debug.Log("Spell has hit a shield but cannot find ShieldSpell Component");
                }
            }
            NetworkCollisionBehaviour(hit.point, hit.normal);
        }
    }

    void LocalCollisionBehaviour(Vector3 hitpoint, Vector3 hitNormal) {
        foreach (var effect in DeactivateObjectsOnCollision) {
            if (effect != null) effect.SetActive(false);
        }
        foreach (var effect in EffectsOnCollision) {
            GameObject instance = Instantiate(effect, hitpoint + hitNormal * CollisionOffset, new Quaternion());
            instance.transform.LookAt(hitpoint + hitNormal + hitNormal * CollisionOffset);
            Destroy(instance, CollisionDestroyTimeDelay);
        }
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void NetworkCollisionBehaviour(Vector3 hitPoint, Vector3 hitNormal) {
        foreach(string effect in NetworkedEffectsOnCollision) {
            GameObject instance = PhotonNetwork.Instantiate(effect, hitPoint + hitNormal * CollisionOffset, transform.rotation);
            instance.transform.LookAt(hitPoint + hitNormal + hitNormal * CollisionOffset);
            instance.transform.Rotate(Vector3.forward, transform.eulerAngles.y);
            Spell instanceSpell = instance.GetComponent<Spell>();
            if (instanceSpell != null) {
                instanceSpell.SetSpellStrength(GetSpellStrength());
                instanceSpell.SetSpellDamageModifier(GetSpellDamageModifier());
                instanceSpell.SetOwner(GetOwner());
            }
        }
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void UpdateWorldPosition() {
        if (!photonView.IsMine || isCollided) return;
        if (Target != null && targetT == null) targetT = Target.transform;
        if ((MaxDistance > 0f && travelDistance.magnitude > MaxDistance) || Speed < 0.1f) {
            Debug.Log("Stopping due to max distance");
            if (ExplodeOnReachMaxDistance) {
                LocalCollisionBehaviour(transform.position, transform.forward);
                NetworkCollisionBehaviour(transform.position, transform.forward);
            } else {
                // Similar to LocalCollision Behaviour, but do not spawn any effects
                foreach (var effect in DeactivateObjectsOnCollision) {
                    if (effect != null) effect.SetActive(false);
                }
                GetComponent<Collider>().enabled = false;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
                GetComponent<Rigidbody>().isKinematic = true;
                Invoke("DestroySelf", CollisionDestroyTimeDelay+1f);
            }
            isCollided = true;
        }

        Vector3 randomOffset = Vector3.zero;
        if (RandomMoveRadius > 0) {
            randomOffset = GetRadiusRandomVector() * RandomMoveRadius;
            if (Target != null) {
                var fade = Vector3.Distance(transform.position, targetT.position) / Vector3.Distance(startPosition, targetT.position);
                randomOffset *= fade;
            }
        }

        // If a homing projectile, check for a player in the radius and set it as target
        if (HomingProjectile) {
            if (Target == null){
                Collider[] hits = Physics.OverlapSphere(transform.position, HomingDetectionSphereRadius, homingLayerMask);
                if (hits.Length > 0) {
                    foreach( var hit in hits ) {
                        if (hit.gameObject != PlayerManager.LocalPlayerInstance) {
                            SetTarget(hit.gameObject, true);
                            break;
                        }
                    }
                }
            }
        }

        var frameMoveOffsetWorld = Vector3.zero;
        if (Target == null) {
            if (TrackingProjectile) startRotation = Quaternion.RotateTowards(startRotation, Quaternion.LookRotation(crosshair.GetWorldPoint() - transform.position), TrackingTurnSpeed * Time.deltaTime);
            var currentForwardVector = (Vector3.forward + randomOffset) * Speed * Time.deltaTime;
            frameMoveOffsetWorld = startRotation * currentForwardVector;
        } else {
            Vector3 targetOffset = targetIsPlayer ? new Vector3(0,1,0) : Vector3.zero;
            var forwardVec = ((targetT.position + targetOffset) - transform.position).normalized;
            var currentForwardVector = (forwardVec + randomOffset) * Speed * Time.deltaTime;
            frameMoveOffsetWorld = currentForwardVector;
        }

        if (TrackingProjectile) transform.rotation = startRotation;
        if (AirDrag > 0f) Speed = Mathf.Lerp(Speed, MinSpeed, AirDrag/100f);
        transform.position += frameMoveOffsetWorld;
        travelDistance += frameMoveOffsetWorld;
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

    public void SetTarget(GameObject go, bool isPlayerCharacter=false) {
        Debug.Log("Locking on to target "+go);
        Target = go;
        targetT = go.transform;
        targetIsPlayer = isPlayerCharacter;
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, HomingDetectionSphereRadius);
    }
}
