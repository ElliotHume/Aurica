using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TargetedSpell : Spell {
    public bool OneShotEffect = true, FollowsTarget = true, SpellStrengthChangesDuration = true;
    public float DestroyTimeDelay = 15f, StartTimeDelay = 0f;
    public GameObject[] DeactivateObjectsAfterDuration;
    public Vector3 PositionOffset = Vector3.zero;
    public StatusEffect statusEffect;
    public MovementEffect movementEffect;

    private GameObject TargetGO = null;
    private PlayerManager TargetPM;
    private Enemy TargetEM;
    private TargetDummy TargetTD;

    private bool hasActivated = false, durationEnded = false, active = true;

    void Start() {
        statusEffect = GetComponent<StatusEffect>();
        movementEffect = GetComponent<MovementEffect>();
        if (SpellStrengthChangesDuration) {
            Duration *= GetSpellStrength();
            DestroyTimeDelay *= GetSpellStrength();
            StartTimeDelay *= GetSpellStrength();
        }
        Duration *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;
        DestroyTimeDelay *= GameManager.GLOBAL_SPELL_DURATION_MULTIPLIER;

        if (photonView.IsMine) {
            Invoke("DestroySelf", DestroyTimeDelay+StartTimeDelay);
        }
        if (StartTimeDelay > 0f) {
            active = false;
            Invoke("Activate", StartTimeDelay);
        }
        Invoke("EndSpell", Duration+StartTimeDelay);
    }

    // Update is called once per frame
    void Update() {
        if (FollowsTarget && TargetGO != null && !durationEnded) {
            transform.position = TargetGO.transform.position + PositionOffset;
        }
    }

    void FixedUpdate() {
        if (!photonView.IsMine) return;
        if (active && !hasActivated && OneShotEffect && TargetGO != null ) {
            OneShot();
        }
    }

    private void Activate() {
        active = true;
    }

    public void SetTarget(GameObject targetGO) {
        TargetGO = targetGO;
        TargetPM = targetGO.GetComponent<PlayerManager>();
        TargetEM = targetGO.GetComponent<Enemy>();
        TargetTD = targetGO.GetComponent<TargetDummy>();

        transform.position = targetGO.transform.position + PositionOffset;
        transform.rotation = targetGO.transform.rotation;

        if (TargetPM != null) photonView.RPC("NetworkSetPlayerTarget", RpcTarget.All, TargetPM.GetUniqueName());
    }

    [PunRPC]
    public void NetworkSetPlayerTarget(string PlayerID) {
        if (photonView.IsMine) return;
        PlayerManager pm = GameManager.GetPlayerFromID(PlayerID);

        TargetGO = pm.gameObject;
        TargetPM = pm;

        transform.position = TargetGO.transform.position + PositionOffset;
        transform.rotation = TargetGO.transform.rotation;
    }

    void OneShot() {
        if (hasActivated) return;
        hasActivated = true;
        if (Damage > 0f && TargetPM != null) {
            PhotonView pv = PhotonView.Get(TargetPM);
            string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
            if (pv != null)
                pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
        }
        if (Damage > 0f && TargetEM != null) {
            TargetEM.SetLocalPlayerParticipation();
            PhotonView pv = PhotonView.Get(TargetEM);
            string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
            if (pv != null)
                pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
        }
        if (Damage > 0f && TargetTD != null) {
            PhotonView pv = PhotonView.Get(TargetTD);
            string ownerID = GetOwnerPM() != null ? GetOwnerPM().GetUniqueName() : "";
            if (pv != null)
                pv.RPC("OnSpellCollide", RpcTarget.All, Damage * GetSpellStrength() * auricaSpell.GetSpellDamageModifier(GetSpellDamageModifier()), SpellEffectType, Duration, auricaSpell.targetDistribution.GetJson(), ownerID);
        }
        if (statusEffect != null) statusEffect.ManualActivation(TargetGO);
        if (movementEffect != null) movementEffect.ManualActivation(TargetGO);
    }

    void DestroySelf() {
        StatusEffect[] statusEffects = GetComponents<StatusEffect>();
        foreach(StatusEffect status in statusEffects) status.ManualDeactivate();
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
