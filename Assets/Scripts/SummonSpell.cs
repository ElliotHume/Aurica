using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SummonSpell : Spell, IPunObservable {

    public bool OneShotEffect = true, LastingEffect = false, canHitSelf = false;
    public float DestroyTimeDelay = 15f, StartTimeDelay = 0f;
    public float ScalingFactor = 0f, ScalingLimit = 0f;
    public Vector3 RotationOffset = Vector3.zero;
    public bool Rising = false;
    public float TimeToRise = 1f;
    public Vector3 StartingOffset = Vector3.zero, Destination = Vector3.zero;
    public GameObject[] RisingParticles, DeactivateObjectsAfterDuration;


    private Vector3 startPosition;
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
            if (DestroyTimeDelay > 0f) Invoke("DestroySelf", DestroyTimeDelay);
            if (Duration > 0f) Invoke("DisableCollisions", Duration);
        }
        if (StartTimeDelay > 0f) {
            active = false;
            DisableCollisions();
            Invoke("Enable", StartTimeDelay);
        }
        if (Duration > 0f) Invoke("DisableParticlesAfterDuration", Duration);
        //if (RotationOffset != Vector3.zero) transform.Rotate(RotationOffset);

        if (Rising) {
            if (Destination == Vector3.zero) Destination = transform.position;
            transform.position -= StartingOffset;
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
        var currentPos = transform.position;
        var t = 0f;
        while (t < 1) {
            t += Time.deltaTime / TimeToRise;
            transform.position = Vector3.Lerp(currentPos, Destination, t);
            yield return new WaitForFixedUpdate();
        }
        doneMoving = true;
        AudioSource asrce = GetComponent<AudioSource>();
        if (asrce != null) asrce.Play();
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableCollisions() {
        active = false;
        GetComponent<Collider>().enabled = false;
    }

    void Enable() {
        active = true;
        GetComponent<Collider>().enabled = true;
    }

    void DisableParticlesAfterDuration() {
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
    }
}
