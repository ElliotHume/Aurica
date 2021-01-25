using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetedSpell : Spell {
    public bool OneShotEffect = true, LastingEffect = false, FollowsTarget = true;
    public float DestroyTimeDelay = 15f;
    public GameObject[] DeactivateObjectsAfterDuration;
    public Vector3 PositionOffset = Vector3.zero;

    private GameObject TargetGO;
    private PlayerManager TargetPM;
    private StatusEffect statusEffect;
    private MovementEffect movementEffect;

    private bool hasActivated = false;

    void Awake() {
        statusEffect = GetComponent<StatusEffect>();
        movementEffect = GetComponent<MovementEffect>();
        if (photonView.IsMine) {
            if (OneShotEffect && TargetGO != null) OneShot();
            Invoke("DestroySelf", DestroyTimeDelay);
        }
        Invoke("DisableParticlesAfterDuration", Duration);
    }

    // Update is called once per frame
    void Update() {
        if (!photonView.IsMine) return;

        if (!hasActivated && OneShotEffect && TargetGO != null) {
            OneShot();
        }

        if (LastingEffect) {
            Lasting();
        }

        if (FollowsTarget && TargetGO != null) {
            transform.position = TargetGO.transform.position + PositionOffset;
        }
    }

    public void SetTarget(GameObject targetGO) {
        TargetGO = targetGO;
        TargetPM = targetGO.GetComponent<PlayerManager>();

        transform.position = targetGO.transform.position + PositionOffset;
        transform.rotation = targetGO.transform.rotation;

        if (!hasActivated && OneShotEffect) {
            OneShot();
        }
    }

    void OneShot() {
        hasActivated = true;
        if (statusEffect != null) statusEffect.ManualActivation(TargetGO);
        if (movementEffect != null) movementEffect.ManualActivation(TargetGO);
    }

    void Lasting() {
        // Do nothing, so far unneeded
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void DisableParticlesAfterDuration() {
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
    }
}
