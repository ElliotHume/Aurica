using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Aura : MonoBehaviourPun {

    private ManaDistribution AuraDistribution;
    private PlayerManager player;
    private string playerName;

    void Start() {
        player = GetComponent<PlayerManager>();
        playerName = player.photonView.Owner.NickName;
        Debug.Log("Trying to load aura file: " + playerName + "-aura");
        TextAsset auraFile = Resources.Load<TextAsset>("Auras/" + playerName + "-aura");

        // No personal aura was found, use default
        if (auraFile == null) auraFile = Resources.Load<TextAsset>("Auras/default-aura");

        if (auraFile != null) {
            AuraDistribution = JsonUtility.FromJson<ManaDistribution>(auraFile.text);
        }

        Debug.Log("AURA:  " + AuraDistribution.ToString());

        // GetDamage(200f, new ManaDistribution("1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0"));
    }

    public void GetDamage(float damage, ManaDistribution damageDist) {
        List<float> percents = damageDist.GetAsPercentages();
        List<float> auraList = AuraDistribution.ToList();
        float structureDiff = damageDist.structure - AuraDistribution.structure;
        float essenceDiff = damageDist.essence - AuraDistribution.essence;
        float natureDiff = damageDist.nature - AuraDistribution.nature;
        percents[0] = percents[0] * damage * (1f - (structureDiff < 0 ? structureDiff : Mathf.Abs(AuraDistribution.structure * 0.75f)));
        percents[1] = percents[1] * damage * (1f - (essenceDiff < 0 ? essenceDiff : Mathf.Abs(AuraDistribution.essence * 0.75f)));
        percents[2] = percents[2] * damage * (1f - (AuraDistribution.fire * 0.75f));
        percents[3] = percents[3] * damage * (1f - (AuraDistribution.water * 0.75f));
        percents[4] = percents[4] * damage * (1f - (AuraDistribution.earth * 0.75f));
        percents[5] = percents[5] * damage * (1f - (AuraDistribution.air * 0.75f));
        percents[6] = percents[6] * damage * (1f - (natureDiff < 0 ? natureDiff : Mathf.Abs(AuraDistribution.nature * 0.75f)));

        // Log Damages
        // foreach (var x in percents) Debug.Log(x.ToString());
    }

    public ManaDistribution GetAura() {
        return AuraDistribution;
    }
}
