using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class ObjectiveSphere : MonoBehaviourPun
{
    public Vector3 holdingPositionOffset;
    public Transform originPoint;
    public StatusEffect appliedStatusEffect;
    public AudioSource GrabSound, ResetSound, IsHeldLoopSound;
    public UnityEvent OnGrab, OnReset, OnObjectiveComplete;

    private bool isHeld = false;
    private GameObject holdingPlayerGO;
    private PlayerManager holdingPlayerManager;

    // Update is called once per frame
    void Update() {
        if (!photonView.IsMine) return;
        if (isHeld && holdingPlayerGO != null) {
            transform.position = holdingPlayerGO.transform.position + holdingPositionOffset;
            transform.rotation = holdingPlayerGO.transform.rotation;
        }
    }

    void OnTriggerEnter(Collider collider) {
        if (!photonView.IsMine) return;

        if (!isHeld && collider.gameObject.tag == "Player") {
            AttachToPlayer(collider.gameObject);
        }
        if (isHeld && collider.gameObject.tag == "Spell") {
            Reset();
        }
    }

    public void AttachToPlayer(GameObject player){
        holdingPlayerGO = player;
        holdingPlayerManager = player.GetComponent<PlayerManager>();
        Debug.Log("Attach objective ball to player: "+holdingPlayerManager.GetUniqueName());
        if (appliedStatusEffect != null) appliedStatusEffect.ManualContinuousActivation(player);
        isHeld = true;
        if (GrabSound != null) GrabSound.Play();
        if (IsHeldLoopSound != null) IsHeldLoopSound.Play();
    }

    public void Reset() {
        Debug.Log("Reset object ball");
        isHeld = false;
        if (appliedStatusEffect != null) appliedStatusEffect.ManualContinuousDeactivation(holdingPlayerGO);
        holdingPlayerGO = null;
        holdingPlayerManager = null;
        transform.position = originPoint.position;
        if (ResetSound != null) ResetSound.Play();
        if (IsHeldLoopSound != null) IsHeldLoopSound.Stop();
    }
}
