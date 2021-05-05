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
            stream.SendNext(projectile_Velocity);
            stream.SendNext(isCollided);
        } else {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            projectile_Velocity = (float)stream.ReceiveNext();
            networkCollided = (bool)stream.ReceiveNext();

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
        elapsed_time = 0f;
    }

    void FixedUpdate() {
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

        if (elapsed_time <= flightDuration && !isCollided) {
            oldPosition = transform.position;
            transform.Translate(0, (Vy - (Gravity * elapsed_time)) * Time.deltaTime, Vx * Time.deltaTime);
            velocity = transform.position - oldPosition;
            elapsed_time += Time.deltaTime;
        }

        if (!photonView.IsMine && !isCollided) {
            if (networkPosition.magnitude > 0.05f) transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * projectile_Velocity);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 1000);
        }
    }

    public void SetTarget(Vector3 targetPos) {
        Target = targetPos;
    }

    void GetTargetFromCrosshair() {
        SetTarget(Crosshair.Instance.GetWorldPoint());
    }



    void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision with: " + collision.gameObject);
        if (collision.gameObject == PlayerManager.LocalPlayerInstance || isCollided) return;

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
            foreach (string effect in NetworkedEffectsOnCollision) {
                GameObject instance = PhotonNetwork.Instantiate(effect, hit.point + hit.normal * CollisionOffset, new Quaternion());
                instance.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
                Spell instanceSpell = instance.GetComponent<Spell>();
                if (instanceSpell != null) {
                    instanceSpell.SetSpellStrength(GetSpellStrength());
                    instanceSpell.SetSpellDamageModifier(GetSpellDamageModifier());
                    instanceSpell.SetOwner(GetOwner());
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
