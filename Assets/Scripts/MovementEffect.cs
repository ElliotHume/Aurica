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
    public bool isVacuum = false;
    public bool isFixed = false;
    public bool isContinuous = false;
    public AudioSource clip;

    private bool isCollided = false;
    private Spell attachedSpell;

    void Start() {
        attachedSpell = GetComponent<Spell>();
        if (isFixed) displacementDirection = transform.forward;
        if (isVacuum) {
            isKnockback = false;
        }
    }

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
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf)) {
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
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    if (isKnockback) displacementDirection = (collision.gameObject.transform.position - transform.position).normalized;
                    if (isVacuum) displacementDirection = (transform.position - collision.gameObject.transform.position).normalized;
                    Activate(pm);
                }
            }
        }
    }

    void OnTriggerStay(Collider collision) {
        if (photonView.IsMine && isContinuous) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    if (isKnockback) displacementDirection = (collision.gameObject.transform.position - transform.position).normalized;
                    if (isVacuum) displacementDirection = (transform.position - collision.gameObject.transform.position).normalized;
                    Activate(pm);
                }
            }
        }
    }

    void Activate(PlayerManager pm) {
        PhotonView pv = PhotonView.Get(pm);
        if (pv != null) {
            float multiplier = (attachedSpell != null) ? attachedSpell.GetSpellStrength() : 1f;
            pv.RPC("Displace", RpcTarget.All, displacementDirection, displacementDistance * multiplier, displacementSpeed, isKnockback || isVacuum || isFixed);
            if (clip != null) clip.Play();
        }
    }
}
