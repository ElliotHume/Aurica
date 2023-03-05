using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SummonSpell : Spell, IPunObservable {

    public bool OneShotEffect = true, LastingEffect = false, canHitSelf = false, SpellStrengthChangesDuration = true;
    public float DestroyTimeDelay = 15f, StartTimeDelay = 0f;
    public float ScalingFactor = 0f, ScalingLimit = 0f;
    //public Vector3 RotationOffset = Vector3.zero;
    public bool Rising = false, AlignToZAxis = true;
    public float TimeToRise = 1f;
    public float StartingOffset = 0f;
    public Vector3 TargetingIndicatorScale = Vector3.zero;
    public GameObject[] RisingParticles, DestructionParticles, DeactivateObjectsAfterDuration;


    private Vector3 startPosition = Vector3.zero, Destination = Vector3.zero;
    private float amountOfScalingApplied = 0f;
    private bool active = true, doneMoving = true;
    private Vector3 networkPosition, oldPosition, velocity;
    private Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(transform.localScale);
        } else {
            networkPosition = (Vector3) stream.ReceiveNext();
            networkRotation = (Quaternion) stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
            float lag = Mathf.Abs((float) (PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (velocity * lag);
        }
    }

    void Start() {
        if (photonView.IsMine) {
            Duration *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
            DestroyTimeDelay *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
            if (SpellStrengthChangesDuration) {
                Duration *= GetSpellStrength();
                DestroyTimeDelay *= GetSpellStrength();
            }
            Invoke("DestroySelf", DestroyTimeDelay+StartTimeDelay);
            Invoke("DisableCollisions", Duration+StartTimeDelay);
            Invoke("EndSpell", Duration+StartTimeDelay);
        }

        if (Rising) {
            if (Destination == Vector3.zero) Destination = transform.localPosition;
            transform.position -= AlignToZAxis ? transform.forward * StartingOffset : transform.up * StartingOffset;
            startPosition = transform.position;
            if (photonView.IsMine) StartCoroutine(Rise());
        }
        if (StartTimeDelay > 0f) {
            active = false;
            DisableCollisions();
            Invoke("Enable", StartTimeDelay);
        }
        //if (RotationOffset != Vector3.zero) transform.Rotate(RotationOffset);

        foreach (var effect in RisingParticles) {
            GameObject newEffect = Instantiate(effect, transform.position, transform.rotation);
            Destroy(newEffect, DestroyTimeDelay);
        }


    }

    void Update() {
        if (!photonView.IsMine) {
            if (networkPosition.magnitude > 0.05f) transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime * 2f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 1000);
        }
    }

    void FixedUpdate() {
        if (photonView.IsMine && active && doneMoving && ScalingFactor != 0f && (ScalingLimit == 0f || amountOfScalingApplied < ScalingLimit)) {
            transform.localScale += transform.localScale * ScalingFactor * Time.deltaTime;
            if (ScalingLimit != 0f) amountOfScalingApplied += Mathf.Abs(ScalingFactor * Time.deltaTime);
        }
    }

    IEnumerator Rise() {
        doneMoving = false;
        var currentPos = transform.localPosition;
        var t = 0f;
        while (t < 1) {
            t += Time.deltaTime / TimeToRise;
            oldPosition = transform.position;
            transform.localPosition = Vector3.Lerp(currentPos, Destination, t);
            velocity = transform.position - oldPosition;
            yield return null;
        }
        velocity = Vector3.zero;
        doneMoving = true;
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        active = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach(Collider collider in colliders ) collider.enabled = false;
    }

    void Enable() {
        active = true;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach(Collider collider in colliders ) collider.enabled = true;
    }

    void EndSpell() {
        photonView.RPC("StopParticles", RpcTarget.All);
    }

    [PunRPC]
    public void StopParticles() {
        DisableParticlesAfterDuration();
        PlayDestructionParticles();
    }

    void PlayDestructionParticles() {
        foreach (var effect in DestructionParticles) {
            GameObject newEffect = Instantiate(effect, transform.position, transform.rotation);
            Destroy(newEffect, 5f);
        }
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
}
