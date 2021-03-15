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
    public bool Rising = false;
    public float TimeToRise = 1f;
    public float StartingOffset = 0f;
    public GameObject[] RisingParticles, DestructionParticles, DeactivateObjectsAfterDuration;


    private Vector3 startPosition = Vector3.zero, Destination = Vector3.zero;
    private float amountOfScalingApplied = 0f;
    private bool active = true, doneMoving = true;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(transform.localScale);
        } else {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            transform.localScale = (Vector3)stream.ReceiveNext();
        }
    }

    void Start() {
        if (photonView.IsMine) {
            if (SpellStrengthChangesDuration) {
                Duration *= GetSpellStrength();
                DestroyTimeDelay *= GetSpellStrength();
            }
            Duration *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
            DestroyTimeDelay *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
            if (DestroyTimeDelay > 0f) Invoke("DestroySelf", DestroyTimeDelay);
            if (Duration > 0f) Invoke("DisableCollisions", Duration);
        }
        if (StartTimeDelay > 0f) {
            active = false;
            DisableCollisions();
            Invoke("Enable", StartTimeDelay);
        }
        if (Duration > 0f) Invoke("DisableParticlesAfterDuration", Duration);
        if (DestroyTimeDelay > 0f) Invoke("PlayDestructionParticles", DestroyTimeDelay - 0.1f);
        //if (RotationOffset != Vector3.zero) transform.Rotate(RotationOffset);

        foreach (var effect in RisingParticles) {
            GameObject newEffect = Instantiate(effect, transform.position, transform.rotation);
            Destroy(newEffect, DestroyTimeDelay);
        }

        if (Rising) {
            if (Destination == Vector3.zero) Destination = transform.localPosition;
            transform.position -= transform.forward * StartingOffset;
            startPosition = transform.position;

            StartCoroutine(Rise());
        }


    }

    void FixedUpdate() {
        if (active && doneMoving && ScalingFactor != 0f && (ScalingLimit == 0f || amountOfScalingApplied < ScalingLimit)) {
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
            transform.localPosition = Vector3.Lerp(currentPos, Destination, t);
            yield return new WaitForFixedUpdate();
        }
        doneMoving = true;
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        active = false;
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
    }

    void Enable() {
        active = true;
        GetComponent<Collider>().enabled = true;
    }

    void PlayDestructionParticles() {
        foreach (var effect in DestructionParticles) {
            GameObject newEffect = Instantiate(effect, transform.position, transform.rotation);
            Destroy(newEffect, DestroyTimeDelay);
        }
    }

    void DisableParticlesAfterDuration() {
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
    }
}
