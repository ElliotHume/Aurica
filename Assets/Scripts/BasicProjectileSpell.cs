using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;

public class BasicProjectileSpell : Spell
{
    public float Speed = 1f;
    public float RandomMoveRadius = 0f;
    public float RandomMoveSpeedScale = 0f;
    public GameObject Target;
    public float CollisionOffset = 0;
    public float CollisionDestroyTimeDelay = 5;
    public GameObject[] EffectsOnCollision;
    public GameObject[] DeactivateObjectsOnCollision;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool isCollided = false;
    private Transform targetT;
    private Vector3 randomTimeOffset;

    private const byte PlayerProjectileCollideEventCode = 1;

    void Awake() {
        startPosition = transform.position;
        startRotation = transform.rotation;
        randomTimeOffset = Random.insideUnitSphere * 10;
    }

    // Update is called once per frame
    void Update() {
        UpdateWorldPosition();
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log("Collision with: "+collision.gameObject);
        isCollided = true;
        ContactPoint hit = collision.GetContact(0);

        foreach (var effect in DeactivateObjectsOnCollision) {
            if (effect != null) effect.SetActive(false);
        }
        foreach (var effect in EffectsOnCollision) {
            GameObject instance = Instantiate(effect, hit.point + hit.normal * CollisionOffset, new Quaternion());
            instance.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
            Destroy(instance, CollisionDestroyTimeDelay);
        }

        if (photonView.IsMine) {
            if (collision.gameObject.tag == "Player") {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                PhotonView pv = PhotonView.Get(pm);
                pv.RPC("OnSpellCollide", RpcTarget.All, Damage, ManaDamageType, SpellEffectType);
            }

            Invoke("DestroySelf", CollisionDestroyTimeDelay+1f);
        }
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void UpdateWorldPosition() {
        if (Target != null && targetT == null) targetT = Target.transform;

        Vector3 randomOffset = Vector3.zero;
        if (RandomMoveRadius > 0) {
            randomOffset = GetRadiusRandomVector() * RandomMoveRadius;
            if (Target != null) {
                var fade = Vector3.Distance(transform.position, targetT.position) / Vector3.Distance(startPosition, targetT.position);
                randomOffset *= fade;
            }
        }

        var frameMoveOffsetWorld = Vector3.zero;
        if (!isCollided) {
            if (Target == null) {
                var currentForwardVector = (Vector3.forward + randomOffset) * Speed * Time.deltaTime;
                frameMoveOffsetWorld = startRotation * currentForwardVector;
            } else {
                var forwardVec = (targetT.position - transform.position).normalized;
                var currentForwardVector = (forwardVec + randomOffset) * Speed * Time.deltaTime;
                frameMoveOffsetWorld = currentForwardVector;
            }
        }

        transform.position += frameMoveOffsetWorld;
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
}
