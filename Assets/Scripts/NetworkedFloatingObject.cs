using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkedFloatingObject : MonoBehaviourPun, IPunObservable
{
    Vector3 resetPosition, rotationAngles;

    public bool bobs = true, rotates = true;
    public Vector3 spinVector = new Vector3(0.1f, 0.1f, 0.1f);
    public float bobbingSpeed, bobbingHeight;
    public bool delay = false;
    public float delayMinimum=1f, delayMaximum=5f;

    private bool shouldBob, shouldRotate, coinflip;
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.IsWriting) {
            if (bobs) stream.SendNext(transform.position);
            if (rotates) stream.SendNext(transform.rotation);
        } else {
            if (bobs) networkPosition = (Vector3) stream.ReceiveNext();
            if (rotates) networkRotation = (Quaternion) stream.ReceiveNext();
        }
    }

    void Start() {
        resetPosition = transform.position;
        shouldBob = bobs;
        shouldRotate = rotates;

        if (!photonView.IsMine) return;
        rotationAngles = new Vector3(Random.Range(0f, spinVector.x), Random.Range(0f, spinVector.y), Random.Range(0f, spinVector.z));
        if (delay) {
            StopMoving();
            Invoke("StartMoving", Random.Range(delayMinimum, delayMaximum));
        }
        coinflip = Random.Range(0f, 1f) >= 0.5f;
    }

    void FixedUpdate() {
        if (photonView.IsMine) {
            // Owner of the object does the moving
            if (shouldRotate) transform.Rotate(rotationAngles);
            if (shouldBob){
                //calculate what the new Y position will be
                float newY = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
                if (coinflip) newY = -newY;
                //set the object's Y to the new calculated Y
                transform.position += new Vector3(0, newY, 0) * Time.deltaTime * 0.5f;
            }
        } else {
            // Remote clients just get the updated about the movement and lerp to them.
            if (bobs && networkPosition.magnitude > 0.05f) transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime);
            if (rotates) transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRotation, Time.deltaTime * 2f);
        }
        
    }

    public void StartMoving() {
        if (bobs) shouldBob = true;
        if (rotates) shouldRotate = true;
    }

    public void StopMoving() {
        if (bobs) shouldBob = false;
        if (rotates) shouldRotate = false;
    }
}