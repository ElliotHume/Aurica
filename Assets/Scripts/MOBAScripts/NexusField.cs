using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NexusField : MonoBehaviourPun {
    [Tooltip("Radius in which the nexus field will apply the buffs/debuffs")]
    [SerializeField]
    private float Radius = 6f;

    [Tooltip("Radius offset for where to detect players from")]
    [SerializeField]
    private Vector3 Offset = Vector3.up;

    [Tooltip("Resource name of the network effect to spawn when a spell is destroyed")]
    [SerializeField]
    private string NetworkEffectOnDestroy;

    [Tooltip("Objects to hide when the nexus field is being disabled")]
    [SerializeField]
    private List<GameObject> HideObjectsOnDisable;

    [Tooltip("Objects to toggle the active state when the nexus field is being disable specifically by a null sphere")]
    [SerializeField]
    private List<GameObject> EnableObjectsOnNullSphereDisable;


    [Tooltip("Status effect to apply as the nexus buff to allied players")]
    [SerializeField]
    private StatusEffect NexusBuff;

    [Tooltip("Status effect to apply as the nexus debuff to opposing players")]
    [SerializeField]
    private StatusEffect NexusDebuff;

    private MOBATeam.Team Team;
    private bool enabled = true, disabledByNullSphere = false, playingDisableParticles = false;
    private List<GameObject> playersInRadius;
    private List<GameObject> buffingPlayers, debuffingPlayers;

    void Start() {
        playersInRadius = new List<GameObject>();
        buffingPlayers = new List<GameObject>();
        debuffingPlayers = new List<GameObject>();
    }

    void FixedUpdate() {
        // Check if null sphere is in range, disable attacks if it is
        disabledByNullSphere = Structure.IsNullSphereInRadius(transform, Radius, Offset);

        // Play the null sphere disabled particles if we are not already doing so
        if (disabledByNullSphere && !playingDisableParticles) {
            foreach(GameObject obj in HideObjectsOnDisable){ obj.SetActive(false); }
            foreach(GameObject obj in EnableObjectsOnNullSphereDisable){ obj.SetActive(true); }
            playingDisableParticles = true;
        } else if (!disabledByNullSphere && playingDisableParticles) {
            if (enabled) foreach(GameObject obj in HideObjectsOnDisable){ obj.SetActive(true); }
            foreach(GameObject obj in EnableObjectsOnNullSphereDisable) { obj.SetActive(false); }
            playingDisableParticles = false;
        }

        if (photonView.IsMine) {
            playersInRadius = Structure.GetPlayersInRadius(transform, Radius, Offset);
            if (enabled && !disabledByNullSphere) {
                // For each player in the radius, if we dont already have an attack being fired at them, create the attack and set them as the target.
                foreach(GameObject playerGO in playersInRadius) {
                    MOBAPlayer mp = playerGO.GetComponent<MOBAPlayer>();
                    PlayerManager pm = playerGO.GetComponent<PlayerManager>();
                    // Do not deal with dead players
                    if (pm != null && pm.dead) continue;

                    if (mp != null && mp.Side == Team) {
                        // Buff allied players
                        if (!buffingPlayers.Contains(playerGO)) {
                            buffingPlayers.Add(playerGO);
                            NexusBuff.ManualContinuousActivation(playerGO);
                        }
                    } else {
                        // Debuff opposing players
                        if (!debuffingPlayers.Contains(playerGO)) {
                            debuffingPlayers.Add(playerGO);
                            NexusDebuff.ManualContinuousActivation(playerGO);
                        }
                    }
                }
            }
            foreach(GameObject playerGO in buffingPlayers) {
                PlayerManager pm = playerGO.GetComponent<PlayerManager>();
                if (!playersInRadius.Contains(playerGO) || pm.dead || disabledByNullSphere || !enabled) {
                    buffingPlayers.Remove(playerGO);
                    NexusBuff.ManualContinuousDeactivation(playerGO);
                    break;
                }
            }
            foreach(GameObject playerGO in debuffingPlayers) {
                PlayerManager pm = playerGO.GetComponent<PlayerManager>();
                if (!playersInRadius.Contains(playerGO) || pm.dead || disabledByNullSphere || !enabled) {
                    debuffingPlayers.Remove(playerGO);
                    NexusDebuff.ManualContinuousDeactivation(playerGO);
                    break;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        if (photonView.IsMine && enabled && !disabledByNullSphere) {
            if (other.gameObject.tag == "Spell") {
                Spell sp = other.gameObject.GetComponent<Spell>();
                BasicProjectileSpell bps = other.gameObject.GetComponent<BasicProjectileSpell>();
                ArcingProjectileSpell aps = other.gameObject.GetComponent<ArcingProjectileSpell>();
                if (sp != null && bps == null && aps == null) {
                    PhotonView pv = PhotonView.Get(sp);
                    if (pv != null) pv.RPC("DestroySpell", RpcTarget.All);
                    if (NetworkEffectOnDestroy != "") PhotonNetwork.Instantiate(NetworkEffectOnDestroy, other.gameObject.transform.position, other.gameObject.transform.rotation);
                }
            }
        }
    }

    void OnTriggerStay(Collider other) {
        if (photonView.IsMine && enabled && !disabledByNullSphere) {
            if (other.gameObject.tag == "Spell") {
                Spell sp = other.gameObject.GetComponent<Spell>();
                BasicProjectileSpell bps = other.gameObject.GetComponent<BasicProjectileSpell>();
                ArcingProjectileSpell aps = other.gameObject.GetComponent<ArcingProjectileSpell>();
                if (sp != null && bps == null && aps == null) {
                    PhotonView pv = PhotonView.Get(sp);
                    if (pv != null) pv.RPC("DestroySpell", RpcTarget.All);
                    if (NetworkEffectOnDestroy != "") PhotonNetwork.Instantiate(NetworkEffectOnDestroy, other.gameObject.transform.position, other.gameObject.transform.rotation);
                }
            }
        }
    }

    public void SetTeam(MOBATeam.Team team) {
        Team = team;
    }

    public void Disable() {
        enabled = false;
        foreach(GameObject obj in HideObjectsOnDisable){ obj.SetActive(false); }
    }

    public void Enable() {
        enabled = true;
        foreach(GameObject obj in HideObjectsOnDisable){ obj.SetActive(true); }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position+Offset, Radius);
    }
}
