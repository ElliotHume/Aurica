using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StatusEffect : MonoBehaviourPunCallbacks, IOnPhotonViewPreNetDestroy {

    // Increase or decrease movement speed
    public bool slow;
    public float slowDuration, slowPercentage = 0f;
    public bool hasten;
    public float hastenDuration, hastenPercentage = 0f;

    // Prevent all movement, including movement spells
    public bool root;
    public float rootDuration;

    // Prevent all spellcasts
    public bool silence;
    public float silenceDuration;

    // Prevent all actions
    public bool stun;
    public float stunDuration;

    // Increase or decrease the amount of damage dealt by given mana types
    public bool weaken;
    public float weakenDuration;
    public ManaDistribution weakenDistribution;
    public bool strengthen;
    public float strengthenDuration;
    public ManaDistribution strengthenDistribution;

    // Increase or decrease the amount of damage taken
    public bool fragile;
    public float fragileDuration, fragilePercentage = 0f;
    public bool tough;
    public float toughDuration, toughPercentage = 0f;

    // Change Mana Regen of the target
    public bool changeManaRegen;
    public float changeDuration, changePercentage = 0f;

    public bool healing = false;
    public float healFlatAmount = 0f, healPercentAmount = 0f;

    public bool manaDrain = false;
    public float manaDrainFlatAmount = 0f, manaDrainPercentAmount = 0f;

    public bool camouflage = false;
    public float camouflageDuration = 0f;

    public bool cleanse = false;
    public bool cure = false;

    public bool isContinuous = false;
    public bool canHitSelf = false;
    public bool onlyHitSelf = false;

    private bool isCollided = false;
    private Spell attachedSpell;

    private List<PhotonView> AffectedPlayers;

    void Start() {
        attachedSpell = GetComponent<Spell>();
        AffectedPlayers = new List<PhotonView>();
    }

    public override void OnEnable() {
        base.OnEnable();
        photonView.AddCallbackTarget(this);
    }

    public override void OnDisable() {
        base.OnDisable();
        photonView.RemoveCallbackTarget(this);
    }

    public void ManualActivation(GameObject playerGO) {
        if (!photonView.IsMine) return;

        PlayerManager pm = playerGO.GetComponent<PlayerManager>();
        if (pm != null) {
            PhotonView pv = PhotonView.Get(pm);
            Activate(pv);
        }
    }


    void OnCollisionEnter(Collision collision) {
        if (photonView.IsMine && !isCollided) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerInstance)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    Activate(pv);
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision) {
        if (photonView.IsMine) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerInstance)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (!isContinuous) {
                        Activate(pv);
                    } else {
                        ActivateContinuous(pv);
                    }
                }
            }
        }
    }

    void OnTriggerStay(Collider collision) {
        if (photonView.IsMine && isContinuous) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerInstance)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    ApplyContinuous(pv);
                }
            }
        }
    }

    void OnTriggerExit(Collider collision) {
        if (photonView.IsMine && isContinuous) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerInstance || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerInstance)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    DeactivateContinuous(pv);
                }
            }
        }
    }

    public void OnPreNetDestroy(PhotonView rootView) {
        if (photonView.IsMine && isContinuous) {
            foreach (PhotonView player in AffectedPlayers) {
                DeactivateContinuous(player, false);
            }
        }
    }

    void Activate(PhotonView pv) {
        if (pv != null) {
            float multiplier = (attachedSpell != null) ? attachedSpell.GetSpellStrength() : 1f;
            if (cleanse) pv.RPC("Cleanse", RpcTarget.All);
            if (cure) pv.RPC("Cure", RpcTarget.All);
            if (slow) pv.RPC("Slow", RpcTarget.All, slowDuration * multiplier, slowPercentage/100f * multiplier) ;
            if (hasten) pv.RPC("Hasten", RpcTarget.All, hastenDuration * multiplier, hastenPercentage/100f * multiplier);
            if (root) pv.RPC("Root", RpcTarget.All, rootDuration * multiplier);
            if (silence) pv.RPC("Silence", RpcTarget.All, silenceDuration * multiplier);
            if (stun) pv.RPC("Stun", RpcTarget.All, stunDuration * multiplier);
            if (weaken) pv.RPC("Weaken", RpcTarget.All, weakenDuration * multiplier, weakenDistribution.ToString());
            if (strengthen) pv.RPC("Strengthen", RpcTarget.All, strengthenDuration * multiplier, strengthenDistribution.ToString());
            if (fragile) pv.RPC("Fragile", RpcTarget.All, fragileDuration * multiplier, fragilePercentage / 100f * multiplier);
            if (tough) pv.RPC("Tough", RpcTarget.All, toughDuration * multiplier, toughPercentage / 100f * multiplier);
            if (changeManaRegen) {
                // Do not reduce the duration of a regen debuff, else people will cast spells at deliberately low strength to lessen the debuff
                if (changePercentage < 100f) {
                    pv.RPC("ManaRestoration", RpcTarget.All, changeDuration, changePercentage / 100f * multiplier);
                } else {
                    pv.RPC("ManaRestoration", RpcTarget.All, changeDuration * multiplier, changePercentage / 100f * multiplier);
                }
                
            }
            if (healing) pv.RPC("Heal", RpcTarget.All, healFlatAmount * multiplier, healPercentAmount / 100f * multiplier);
            if (manaDrain) pv.RPC("ManaDrain", RpcTarget.All, manaDrainFlatAmount * multiplier, manaDrainPercentAmount / 100f * multiplier);
            if (camouflage) pv.RPC("Camouflage", RpcTarget.All, camouflageDuration * multiplier);
        }
    }

    void ActivateContinuous(PhotonView pv) {
        if (pv != null) {
            AffectedPlayers.Add(pv);
            Debug.Log("Activate continuous");
            float multiplier = (attachedSpell != null) ? attachedSpell.GetSpellStrength() : 1f;
            if (weaken) pv.RPC("ContinuousWeaken", RpcTarget.All, weakenDistribution.ToString());
            if (strengthen) pv.RPC("ContinuousStrengthen", RpcTarget.All, strengthenDistribution.ToString());
            if (changeManaRegen) pv.RPC("ContinuousManaRestoration", RpcTarget.All, changePercentage / 100f * multiplier);
            if (slow) pv.RPC("ContinuousSlow", RpcTarget.All, slowPercentage / 100f * multiplier);
            if (hasten) pv.RPC("ContinuousHasten", RpcTarget.All, hastenPercentage / 100f * multiplier);
            if (root) pv.RPC("ContinuousRoot", RpcTarget.All);
            if (silence) pv.RPC("ContinuousSilence", RpcTarget.All);
            if (stun) pv.RPC("ContinuousStun", RpcTarget.All);
            if (fragile) pv.RPC("ContinuousFragile", RpcTarget.All, fragilePercentage / 100f * multiplier);
            if (tough) pv.RPC("ContinuousTough", RpcTarget.All, toughPercentage / 100f * multiplier);
            if (camouflage) pv.RPC("ContinuousCamouflage", RpcTarget.All);
        }
    }

    void ApplyContinuous(PhotonView pv) {
        float multiplier = (attachedSpell != null) ? attachedSpell.GetSpellStrength() : 1f;
        if (cleanse) pv.RPC("Cleanse", RpcTarget.All);
        if (cure) pv.RPC("Cure", RpcTarget.All);
        if (healing) pv.RPC("Heal", RpcTarget.All, healFlatAmount * 0.002f * multiplier, healPercentAmount/100f * 0.002f * multiplier);
        if (manaDrain) pv.RPC("ManaDrain", RpcTarget.All, manaDrainFlatAmount * 0.002f * multiplier, manaDrainPercentAmount/100f * 0.002f * multiplier);
    }

    void DeactivateContinuous(PhotonView pv, bool modify = true) {
        if (pv != null) {
            if (modify) AffectedPlayers.Remove(pv);
            float multiplier = (attachedSpell != null) ? attachedSpell.GetSpellStrength() : 1f;
            if (weaken) pv.RPC("EndContinuousWeaken", RpcTarget.All, weakenDistribution.ToString());
            if (strengthen) pv.RPC("EndContinuousStrengthen", RpcTarget.All, strengthenDistribution.ToString());
            if (changeManaRegen) pv.RPC("EndContinuousManaRestoration", RpcTarget.All, changePercentage / 100f * multiplier);
            if (slow) pv.RPC("EndContinuousSlow", RpcTarget.All);
            if (hasten) pv.RPC("EndContinuousHasten", RpcTarget.All);
            if (root) pv.RPC("EndContinuousRoot", RpcTarget.All);
            if (silence) pv.RPC("EndContinuousSilence", RpcTarget.All);
            if (stun) pv.RPC("EndContinuousStun", RpcTarget.All);
            if (fragile) pv.RPC("EndContinuousFragile", RpcTarget.All);
            if (tough) pv.RPC("EndContinuousTough", RpcTarget.All);
            if (camouflage) pv.RPC("EndContinuousCamouflage", RpcTarget.All);
        }
    }
}
