using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StatusEffect : MonoBehaviourPunCallbacks, IOnPhotonViewPreNetDestroy {

    [HideInInspector]
    public string Identifier = "SceneObject";

    // Increase or decrease movement speed
    public bool slow;
    public float slowDuration, slowPercentage = 0f;
    public bool hasten;
    public float hastenDuration, hastenPercentage = 0f;

    // Prevent character movement
    public bool root;
    public float rootDuration;

    // Prevent being displaced
    public bool ground;
    public float groundDuration;

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

    public bool slowFall = false;
    public float slowFallDuration, slowFallPercent = 0f;

    public bool cleanse = false;
    public bool cure = false;

    public bool isContinuous = false;
    public bool canHitSelf = false;
    public bool onlyHitSelf = false;
    public bool isAffectedBySpellStrength = true;
    public bool isManualTriggerOnly = false;

    private bool isCollided = false;
    private Spell attachedSpell;
    private GameObject owner;
    private List<PhotonView> AffectedPlayers;

    void Start() {
        attachedSpell = GetComponent<Spell>();
        AffectedPlayers = new List<PhotonView>();

        if (attachedSpell != null) {
            Identifier = attachedSpell.auricaSpell.c_name;
        }
    }

    public override void OnEnable() {
        base.OnEnable();
        photonView.AddCallbackTarget(this);
    }

    public override void OnDisable() {
        base.OnDisable();
        photonView.RemoveCallbackTarget(this);
    }

    public void SetOwner(GameObject ownerGO) {
        owner = ownerGO;
    }

    public void ManualActivation(GameObject playerGO) {
        if (!photonView.IsMine) return;
        if (attachedSpell == null) attachedSpell = GetComponent<Spell>();
        PlayerManager pm = playerGO.GetComponent<PlayerManager>();
        if (pm != null) {
            PhotonView pv = PhotonView.Get(pm);
            Activate(pv);
        } else {
            Enemy enemy = playerGO.GetComponent<Enemy>();
            if (enemy != null) {
                PhotonView pv = PhotonView.Get(enemy);
                Activate(pv, true);
            }
        }    
    }

    public void ManualContinuousActivation(GameObject playerGO) {
        if (!photonView.IsMine) return;
        if (attachedSpell == null) attachedSpell = GetComponent<Spell>();
        PlayerManager pm = playerGO.GetComponent<PlayerManager>();
        if (pm != null) {
            PhotonView pv = PhotonView.Get(pm);
            ActivateContinuous(pv);
        } else {
            Enemy enemy = playerGO.GetComponent<Enemy>();
            if (enemy != null) {
                PhotonView pv = PhotonView.Get(enemy);
                ActivateContinuous(pv, true);
            }
        }   
    }

    public void ManualContinuousDeactivation(GameObject playerGO) {
        if (!photonView.IsMine || playerGO == null) return;
        if (attachedSpell == null) attachedSpell = GetComponent<Spell>();
        PlayerManager pm = playerGO.GetComponent<PlayerManager>();
        if (pm != null) {
            PhotonView pv = PhotonView.Get(pm);
            DeactivateContinuous(pv);
        } else {
            Enemy enemy = playerGO.GetComponent<Enemy>();
            if (enemy != null) {
                PhotonView pv = PhotonView.Get(enemy);
                DeactivateContinuous(pv, true, true);
            }
        }   
    }


    void OnCollisionEnter(Collision collision) {
        if (photonView.IsMine && !isCollided && !isManualTriggerOnly) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerGameObject)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    Activate(pv);
                }
            } else if (collision.gameObject.tag == "Enemy" && collision.gameObject != owner) {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null) {
                    enemy.SetLocalPlayerParticipation();
                    PhotonView pv = PhotonView.Get(enemy);
                    Activate(pv, true);
                }
            }
        }
    }

    void OnTriggerEnter(Collider collision) {
        if (photonView.IsMine && !isManualTriggerOnly) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerGameObject)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    if (!isContinuous) {
                        Activate(pv);
                    } else {
                        ActivateContinuous(pv);
                    }
                }
            } else if (collision.gameObject.tag == "Enemy" && collision.gameObject != owner) {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null) {
                    enemy.SetLocalPlayerParticipation();
                    PhotonView pv = PhotonView.Get(enemy);
                    if (!isContinuous) {
                        Activate(pv, true);
                    } else {
                        ActivateContinuous(pv, true);
                    }
                }
            }
        }
    }

    void OnTriggerStay(Collider collision) {
        if (photonView.IsMine && isContinuous && !isManualTriggerOnly) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerGameObject)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    ApplyContinuous(pv);
                }
            } else if (collision.gameObject.tag == "Enemy" && collision.gameObject != owner) {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null) {
                    PhotonView pv = PhotonView.Get(enemy);
                    ApplyContinuous(pv, true);
                }
            }
        }
    }

    void OnTriggerExit(Collider collision) {
        if (photonView.IsMine && isContinuous && !isManualTriggerOnly) {
            if (collision.gameObject.tag == "Player" && (collision.gameObject != PlayerManager.LocalPlayerGameObject || canHitSelf) && !(onlyHitSelf && collision.gameObject != PlayerManager.LocalPlayerGameObject)) {
                PlayerManager pm = collision.gameObject.GetComponent<PlayerManager>();
                if (pm != null) {
                    PhotonView pv = PhotonView.Get(pm);
                    DeactivateContinuous(pv);
                }
            }
        } else if (collision.gameObject.tag == "Enemy" && collision.gameObject != owner) {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null) {
                    PhotonView pv = PhotonView.Get(enemy);
                    DeactivateContinuous(pv, true, true);
                }
            }
    }

    public void ManualDeactivate() {
        if (photonView.IsMine && isContinuous && AffectedPlayers != null) {
            foreach (PhotonView player in AffectedPlayers) {
                DeactivateContinuous(player, false);
            }
            AffectedPlayers.Clear();
        }
    }

    public void OnPreNetDestroy(PhotonView rootView) {
        if (photonView.IsMine && isContinuous) {
            foreach (PhotonView player in AffectedPlayers) {
                DeactivateContinuous(player, false);
            }
        }
    }

    void Activate(PhotonView pv, bool isEnemy = false) {
        if (pv != null) {
            float multiplier = attachedSpell != null && isAffectedBySpellStrength ? attachedSpell.GetSpellStrength() : 1f;
            Debug.Log("Status effect for: "+gameObject+"  cast with multiplier: "+multiplier);
            if (cleanse) pv.RPC("Cleanse", RpcTarget.All);
            if (cure) pv.RPC("Cure", RpcTarget.All);
            if (silence) pv.RPC("Silence", RpcTarget.All, silenceDuration * multiplier);
            // Cap slow values at 90%, we don't want a 100% slow
            if (slow) pv.RPC("Slow", RpcTarget.All, Identifier, slowDuration * multiplier, Mathf.Clamp(slowPercentage/100f * multiplier, 0f, 90f)) ;
            if (hasten) pv.RPC("Hasten", RpcTarget.All, Identifier, hastenDuration * multiplier, hastenPercentage/100f * multiplier);
            if (root) pv.RPC("Root", RpcTarget.All, rootDuration * multiplier);
            if (stun) pv.RPC("Stun", RpcTarget.All, stunDuration * multiplier);
            if (weaken) pv.RPC("Weaken", RpcTarget.All, Identifier, weakenDuration * multiplier, weakenDistribution.ToString());
            if (strengthen) pv.RPC("Strengthen", RpcTarget.All, Identifier, strengthenDuration * multiplier, strengthenDistribution.ToString());
            if (fragile) pv.RPC("Fragile", RpcTarget.All, Identifier, fragileDuration * multiplier, fragilePercentage / 100f * multiplier);
            if (tough) pv.RPC("Tough", RpcTarget.All, Identifier, toughDuration * multiplier, toughPercentage / 100f * multiplier);
            
            if (!isEnemy) {
                if (ground) pv.RPC("Ground", RpcTarget.All, groundDuration * multiplier);
                if (healing) pv.RPC("Heal", RpcTarget.All, healFlatAmount * multiplier, healPercentAmount / 100f * multiplier);
                if (manaDrain) pv.RPC("ManaDrain", RpcTarget.All, manaDrainFlatAmount * multiplier, manaDrainPercentAmount / 100f * multiplier);
                if (camouflage) pv.RPC("Camouflage", RpcTarget.All, camouflageDuration * multiplier);
                if (slowFall) pv.RPC("SlowFall", RpcTarget.All, Identifier, slowFallDuration * multiplier, slowFallPercent / 100f * multiplier);
                if (changeManaRegen) {
                // Do not reduce the duration of a regen debuff, else people will cast spells at deliberately low strength to lessen the debuff
                if (changePercentage < 100f) {
                    pv.RPC("ManaRestoration", RpcTarget.All, Identifier, changeDuration, changePercentage / 100f * multiplier);
                } else {
                    pv.RPC("ManaRestoration", RpcTarget.All, Identifier, changeDuration * multiplier, changePercentage / 100f * multiplier);
                }
            }
            }
            
        }
    }

    void ActivateContinuous(PhotonView pv, bool isEnemy = false) {
        if (pv != null) {
            AffectedPlayers.Add(pv);
            float multiplier = attachedSpell != null && isAffectedBySpellStrength ? attachedSpell.GetSpellStrength() : 1f;
            // Debug.Log("Activate continuous   mult:" + multiplier);
            if (silence) pv.RPC("ContinuousSilence", RpcTarget.All);
            if (weaken) pv.RPC("ContinuousWeaken", RpcTarget.All, Identifier, weakenDistribution.ToString());
            if (strengthen) pv.RPC("ContinuousStrengthen", RpcTarget.All, Identifier, strengthenDistribution.ToString());
            if (slow) pv.RPC("ContinuousSlow", RpcTarget.All, Identifier, Mathf.Clamp(slowPercentage / 100f * multiplier, 0f, 90f));
            if (hasten) pv.RPC("ContinuousHasten", RpcTarget.All, Identifier, hastenPercentage / 100f * multiplier);
            if (root) pv.RPC("ContinuousRoot", RpcTarget.All);
            if (stun) pv.RPC("ContinuousStun", RpcTarget.All);
            if (fragile) pv.RPC("ContinuousFragile", RpcTarget.All, Identifier, fragilePercentage / 100f * multiplier);
            if (tough) pv.RPC("ContinuousTough", RpcTarget.All, Identifier, toughPercentage / 100f * multiplier);

            if (!isEnemy) {
                if (camouflage) pv.RPC("ContinuousCamouflage", RpcTarget.All);
                if (slowFall) pv.RPC("ContinuousSlowFall", RpcTarget.All, Identifier, slowFallPercent / 100f * multiplier);
                if (ground) pv.RPC("ContinuousGround", RpcTarget.All);
                if (changeManaRegen) pv.RPC("ContinuousManaRestoration", RpcTarget.All, Identifier, changePercentage / 100f * multiplier);
            }
        }
    }

    void ApplyContinuous(PhotonView pv, bool isEnemy = false) {
        float multiplier = attachedSpell != null && isAffectedBySpellStrength ? attachedSpell.GetSpellStrength() : 1f;
        if (cleanse) pv.RPC("Cleanse", RpcTarget.All);
        if (cure) pv.RPC("Cure", RpcTarget.All);
        if (!isEnemy) {
            if (healing) pv.RPC("Heal", RpcTarget.All, healFlatAmount * Time.deltaTime * multiplier, healPercentAmount/100f * Time.deltaTime * multiplier);
            if (manaDrain) pv.RPC("ManaDrain", RpcTarget.All, manaDrainFlatAmount * Time.deltaTime * multiplier, manaDrainPercentAmount/100f * Time.deltaTime * multiplier);
        }
    }

    void DeactivateContinuous(PhotonView pv, bool modify = true, bool isEnemy = false) {
        if (pv != null) {
            if (modify) AffectedPlayers.Remove(pv);
            float multiplier = attachedSpell != null && isAffectedBySpellStrength ? attachedSpell.GetSpellStrength() : 1f;
            if (silence) pv.RPC("EndContinuousSilence", RpcTarget.All);
            if (weaken) pv.RPC("EndContinuousWeaken", RpcTarget.All, Identifier, weakenDistribution.ToString());
            if (strengthen) pv.RPC("EndContinuousStrengthen", RpcTarget.All, Identifier, strengthenDistribution.ToString());
            if (slow) pv.RPC("EndContinuousSlow", RpcTarget.All, Identifier);
            if (hasten) pv.RPC("EndContinuousHasten", RpcTarget.All, Identifier);
            if (root) pv.RPC("EndContinuousRoot", RpcTarget.All);
            if (stun) pv.RPC("EndContinuousStun", RpcTarget.All);
            if (fragile) pv.RPC("EndContinuousFragile", RpcTarget.All, Identifier);
            if (tough) pv.RPC("EndContinuousTough", RpcTarget.All, Identifier);

            if (!isEnemy) {
                if (ground) pv.RPC("EndContinuousGround", RpcTarget.All);
                if (camouflage) pv.RPC("EndContinuousCamouflage", RpcTarget.All);
                if (slowFall) pv.RPC("EndContinuousSlowFall", RpcTarget.All);
                if (changeManaRegen) pv.RPC("EndContinuousManaRestoration", RpcTarget.All, Identifier, changePercentage / 100f * multiplier);
            }
        }
    }
}
