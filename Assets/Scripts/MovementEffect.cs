using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MovementEffect : MonoBehaviourPun {

    // Displace the target along a local direction vector
    public float displacementDistance = 0f, displacementSpeed = 1f;
    public Vector3 displacementDirection = Vector3.zero;

    public bool canHitSelf = false;
    public bool isKnockback = false;
    public bool isContinuous = false;


    private bool isCollided = false;

    public void ManualActivation(GameObject playerGO) {
        if (!photonView.IsMine) return;

        PlayerManager pm = playerGO.GetComponent<PlayerManager>();
        if (pm != null) {
            if (isKnockback) displacementDirection = transform.forward;
            Activate(pm);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (photonView.IsMine && !isCollided) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    if (isKnockback) displacementDirection = transform.forward;
                    Activate(pm);
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision) {
        if (photonView.IsMine) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    if (isKnockback) displacementDirection = transform.forward;
                    Activate(pm);
                }
            }
        }
    }

    void OnTriggerStay(Collider collision) {
        if (photonView.IsMine && isContinuous) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    if (isKnockback) displacementDirection = transform.forward;
                    Activate(pm);
                }
            }
        }
    }

    void Activate(PlayerManager pm) {
        PhotonView pv = PhotonView.Get(pm);
        if (pv != null) {
            pv.RPC("Displace", RpcTarget.All, displacementDirection, displacementDistance, displacementSpeed, isKnockback);
        }
    }
}
