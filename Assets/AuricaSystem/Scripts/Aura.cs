using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Aura : MonoBehaviourPun {

    public bool usePercentStrength = true;

    private ManaDistribution AuraDistribution, InnateStrength;
    private PlayerManager player;
    private string playerName;

    void Start() {
        player = GetComponent<PlayerManager>();
        if (PlayerPrefs.HasKey("Aura")) {
            Debug.Log("Found aura in PlayerPrefs");
            AuraDistribution = new ManaDistribution(PlayerPrefs.GetString("Aura"));
        } else {
            playerName = player.photonView.Owner.NickName;
            Debug.Log("Trying to load aura file: " + playerName + "-aura");
            TextAsset auraFile = Resources.Load<TextAsset>("Auras/" + playerName + "-aura");

            // No personal aura was found, use default
            if (auraFile == null) auraFile = Resources.Load<TextAsset>("Auras/default-aura");

            if (auraFile != null) {
                AuraDistribution = JsonUtility.FromJson<ManaDistribution>(auraFile.text);
            }
        }
        

        InnateStrength = CalculateInnateStrengths();

        Debug.Log("AURA:  " + AuraDistribution.ToString());
        player.SetMaxMana(AuraDistribution.GetAggregate() * 100f * GameManager.GLOBAL_PLAYER_MAX_MANA_MULTIPLIER);
        player.ConfirmAura();
        AuricaCaster.LocalCaster.CacheSpellManas();

        if (photonView.IsMine) {
            if (AuraUIPanel.Instance != null) {
                AuraUIPanel.Instance.gameObject.SetActive(true);
                AuraUIPanel.Instance.SetAura(this);
            }
        }
    }

    public void SetAura(ManaDistribution newAura) {
        AuraDistribution = newAura;
        PlayerPrefs.SetString("Aura", newAura.ToString());

        InnateStrength = CalculateInnateStrengths();

        Debug.Log("NEW AURA SET:  " + AuraDistribution.ToString());
        player.SetMaxMana(AuraDistribution.GetAggregate() * 100f * GameManager.GLOBAL_PLAYER_MAX_MANA_MULTIPLIER);
        player.ConfirmAura();
        AuricaCaster.LocalCaster.CacheSpellManas();

        if (photonView.IsMine) {
            if (AuraUIPanel.Instance != null) {
                AuraUIPanel.Instance.gameObject.SetActive(true);
                AuraUIPanel.Instance.SetAura(this);
            }
        }
    }

    ManaDistribution CalculateInnateStrengths() {
        if (usePercentStrength) {
            // Making sure that the sign (+/-) of the aligned mana is preserved
            ManaDistribution strengths = new ManaDistribution();
            List<float> percents = AuraDistribution.GetAsPercentages();
            strengths.structure = percents[0];// * (AuraDistribution.structure / Mathf.Abs(AuraDistribution.structure));
            strengths.essence = percents[1];// * (AuraDistribution.essence / Mathf.Abs(AuraDistribution.essence));
            strengths.fire = percents[2];
            strengths.water = percents[3];
            strengths.earth = percents[4];
            strengths.air = percents[5];
            strengths.nature = percents[6];// * (AuraDistribution.nature / Mathf.Abs(AuraDistribution.nature));
            // Debug.Log("Innate Strengths: "+strengths.ToString());
            return strengths;
        } else {
            return new ManaDistribution(AuraDistribution.ToString());
        }

    }

    //  Damage to the caster is calculated as follows:
    //  -    The damage distribution is calculated as percentages of the aggregate, so if the distribution is 60% fire mana, 60% of the damage is dealt as fire damage
    //  -    The aura distribution is calculated as percentages as well, and used to reduce the damage dealt
    //          This means that if the casters aura is 70% fire mana, they will take 70% reduced fire damage
    //              Aligned mana percentages are absolute, so if the casters aura is 20% structured mana of either order or chaos,
    //              they will take 20% reduced structured damage instead of taking more damage if the alignments are opposing
    public float GetDamage(float damage, ManaDistribution damageDist) {
        List<float> percents = damageDist.GetAsPercentages();
        List<float> auraPercents = AuraDistribution.GetAsPercentages();
        if (percents.Count == 0) return damage;
        for (var i = 0; i < 7; i++) {
            percents[i] = percents[i] * damage * (1f - auraPercents[i]);
        }
        // Log Damages
        // foreach (var x in percents) Debug.Log(x.ToString());

        float sum = 0;
        foreach (var element in percents) {
            sum += element;
        }

        return sum;
    }

    public float GetDamage(float damage, ManaDistribution damageDist, ManaDistribution damageModifiers) {
        List<float> percents = damageDist.GetAsPercentages();
        List<float> auraPercents = AuraDistribution.GetAsPercentages();
        List<float> damageModifiersList = damageModifiers.ToList();
        if (percents.Count == 0) return damage;
        for (var i = 0; i < 7; i++) {
            percents[i] = percents[i] * damage * (1f - auraPercents[i]) * (1 - damageModifiersList[i]);
        }
        // Log Damages
        // foreach (var x in percents) Debug.Log(x.ToString());

        float sum = 0;
        foreach (var element in percents) {
            sum += element;
        }

        return sum;
    }

    public ManaDistribution GetAura() {
        return AuraDistribution;
    }

    public ManaDistribution GetInnateStrength() {
        return InnateStrength;
    }

    public float GetAggregatePower() {
        return AuraDistribution.GetAggregate();
    }
}
