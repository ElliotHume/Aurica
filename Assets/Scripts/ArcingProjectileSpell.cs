using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ArcingProjectileSpell : Spell, IPunObservable
{

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
    private bool isCollided = false, networkCollided = false, setTarget=false, controlled = true;
    private Vector3 networkPosition, oldPosition, velocity;
    private Quaternion networkRotation;
    private float projectile_Velocity, flightDuration, Vx, Vy, elapsed_time;
    private Rigidbody rigidbody;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(isCollided);
        } else {
            networkPosition = (Vector3) stream.ReceiveNext();
            networkRotation = (Quaternion) stream.ReceiveNext();
            networkCollided = (bool) stream.ReceiveNext();

            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.timestamp));
            networkPosition += (velocity * lag);
        }
    }

    void Start() {
        if (CrosshairTargeted) GetTargetFromCrosshair();
        if (Target != null) {
            CalculateTragectory();
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

    void FixedUpdate()
    {
        if (elapsed_time <= flightDuration){
            oldPosition = transform.position;
            transform.Translate(0, (Vy - (Gravity * elapsed_time)) * Time.deltaTime, Vx * Time.deltaTime);
            velocity = transform.position - oldPosition;
            elapsed_time += Time.deltaTime;
        } else if (controlled) {
            controlled = false;
            rigidbody.useGravity = true;
        }
    }

    public void SetTarget(Vector3 targetPos){
        Target = targetPos;
        setTarget = true;
        controlled = true;
    }

    void GetTargetFromCrosshair() {
        SetTarget(Crosshair.Instance.GetWorldPoint());
    }



    void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision with: "+collision.gameObject);
        if (collision.gameObject == PlayerManager.LocalPlayerInstance) return;

        ContactPoint hit = collision.GetContact(0);
        isCollided = true;

        // Call local collision response to generate collision VFX
        LocalCollisionBehaviour(hit.point, hit.normal);

        if (photonView.IsMine) {
            Invoke("DestroySelf", CollisionDestroyTimeDelay+1f);
            if (collision.gameObject.tag == "Player") {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (pv != null) pv.RPC("OnSpellCollide", RpcTarget.All, Damage, ManaDamageType, SpellEffectType, Duration);
                }
            }
            foreach(string effect in NetworkedEffectsOnCollision) {
                GameObject instance = PhotonNetwork.Instantiate(effect, hit.point + hit.normal * CollisionOffset, new Quaternion());
                instance.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
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
