using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetedSpell : Spell {
    public bool OneShotEffect = true, UNIMPLEMENTEDLastingEffect = false, FollowsTarget = true, SpellStrengthChangesDuration = true;
    public float DestroyTimeDelay = 15f;
    public GameObject[] DeactivateObjectsAfterDuration;
    public Vector3 PositionOffset = Vector3.zero;

    private GameObject TargetGO;
    private PlayerManager TargetPM;
    private StatusEffect statusEffect;
    private MovementEffect movementEffect;

    private bool hasActivated = false, durationEnded = false;

    void Awake() {
        statusEffect = GetComponent<StatusEffect>();
        movementEffect = GetComponent<MovementEffect>();
        if (SpellStrengthChangesDuration) {
            Duration *= GetSpellStrength();
            DestroyTimeDelay *= GetSpellStrength();
        }
        Duration *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
        DestroyTimeDelay *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;

        if (photonView.IsMine) {
            if (OneShotEffect && TargetGO != null) OneShot();
            Invoke("DestroySelf", DestroyTimeDelay);
        }
        Invoke("EndSpell", Duration);
    }

    // Update is called once per frame
    void Update() {
        if (!photonView.IsMine) return;

        if (!hasActivated && OneShotEffect && TargetGO != null) {
            OneShot();
        }

        if (UNIMPLEMENTEDLastingEffect) {
            Lasting();
        }

        if (FollowsTarget && TargetGO != null && !durationEnded) {
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
        if (Damage > 0f && TargetPM != null) {
            PhotonView pv = PhotonView.Get(TargetPM);
            string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
            if (pv != null)
                pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
        }
        Debug.Log("TARGETED SPELL STRENGTH "+GetSpellStrength());
        if (statusEffect != null) statusEffect.ManualActivation(TargetGO);
        if (movementEffect != null) movementEffect.ManualActivation(TargetGO);
    }

    void Lasting() {
        // TODO: Do nothing, so far unneeded
    }

    void DestroySelf() {
        PhotonNetwork.Destroy(gameObject);
    }

    void EndSpell() {
        photonView.RPC("StopParticles", RpcTarget.All);
    }

    [PunRPC]
    public void StopParticles() {
        DisableParticlesAfterDuration();
    }

    void DisableParticlesAfterDuration() {
        durationEnded = true;
        foreach (var effect in DeactivateObjectsAfterDuration) {
            if (effect != null) effect.SetActive(false);
        }
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var effect in particles) {
            if (effect != null) effect.Stop();
        }
    }
}
