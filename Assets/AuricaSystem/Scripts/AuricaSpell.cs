using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "AuricaSpell", menuName = "Aurica/AuricaSpell", order = 1)]
public class AuricaSpell : ScriptableObject {
    public enum ManaType {
        Auric, Order, Chaos, Life, Death, Fire, Water, Earth, Air, Divine, Demonic
    };

    public string c_name;
    public ManaType manaType;
    [TextArea(15, 3)]
    public string description;
    public ManaDistribution targetDistribution;
    public List<AuricaSpellComponent> keyComponents;
    public float baseManaCost = 20f, componentManaMultiplier = 0.5f, errorThreshold = 3.0f;
    public bool isAuric = false;
    public List<MasteryManager.MasteryCategories> masteries;
    public string linkedSpellResource = "";


    public bool CheckComponents(List<AuricaSpellComponent> components) {
        List<int> indexList = new List<int>();
        List<string> componentNames = new List<string>();
        List<string> keyComponentNames = new List<string>();
        foreach (var item in components) {
            componentNames.Add(item.c_name);
        }
        foreach (var item in keyComponents) {
            keyComponentNames.Add(item.c_name);
        }

        foreach (string keyComp in keyComponentNames) {
            int ind = componentNames.IndexOf(keyComp);
            if (ind == -1) return false;
            indexList.Add(ind);
        }

        List<int> sortedList = new List<int>(indexList);
        sortedList.Sort();
        return indexList.Count == keyComponents.Count;
    }

    public int GetNumberOfMatchingComponents(List<AuricaSpellComponent> components) {
        List<string> componentNames = new List<string>();
        List<string> keyComponentNames = new List<string>();
        foreach (var item in components) {
            componentNames.Add(item.c_name);}
        foreach (var item in keyComponents) {
            keyComponentNames.Add(item.c_name);
        }

        int matchingComponents = 0;
        foreach (string keyComp in keyComponentNames) {
            int ind = componentNames.IndexOf(keyComp);
            if (ind != -1) matchingComponents += 1;
        }

        return matchingComponents;
    }

    public float GetError(ManaDistribution targetDist) {
        return !isAuric ? targetDistribution.CheckDistError(targetDist) : 0f;
    }

    public float GetSpellDamageModifier(ManaDistribution damageMod) {
        float sum = 0f;
        List<float> percents = targetDistribution.GetAsPercentages();
        List<float> modPercents = damageMod.ToList();
        if (percents.Count == 0 || modPercents.Count == 0) {
            Debug.Log("Could not find percentages, or modifier percentages");
            return 1f;
        }

        for (int i = 0; i < 7; i++) {
            sum += percents[i] * (1+modPercents[i]);
            // Debug.Log("Percent "+i+"  "+ percents[i]+" * "+ (1+modPercents[i]));
        }

        // Debug.Log("Spell damage modifier by: x"+sum);
        return sum;
    }

    public ManaDistribution IdealAuraCalculation() {
        ManaDistribution basicDist = new ManaDistribution();
        ManaDistribution auricDist = new ManaDistribution();
        foreach(AuricaSpellComponent component in keyComponents) {
            basicDist += component.basicDistribution;
            auricDist += component.auricDistribution;
        }
        
        float structure = Mathf.Clamp(auricDist.structure != 0 ? (targetDistribution.structure - basicDist.structure) / auricDist.structure : 0, -1, 1);
        float essence = Mathf.Clamp(auricDist.essence != 0 ? (targetDistribution.essence - basicDist.essence) / auricDist.essence : 0, -1, 1);
        float fire = Mathf.Clamp(auricDist.fire != 0 ? (targetDistribution.fire - basicDist.fire) / auricDist.fire : 0, 0, 1);
        float water = Mathf.Clamp(auricDist.water != 0 ? (targetDistribution.water - basicDist.water) / auricDist.water : 0, 0, 1);
        float earth = Mathf.Clamp(auricDist.earth != 0 ? (targetDistribution.earth - basicDist.earth) / auricDist.earth : 0, 0, 1);
        float air = Mathf.Clamp(auricDist.air != 0 ? (targetDistribution.air - basicDist.air) / auricDist.air : 0, 0, 1);
        float nature = Mathf.Clamp(auricDist.nature != 0 ? (targetDistribution.nature - basicDist.nature) / auricDist.nature : 0, -1, 1);

        return new ManaDistribution(structure, essence, fire, water, earth, air, nature);
    }

    public ManaDistribution GetCombinedAuricDistribution() {
        ManaDistribution sum = new ManaDistribution();
        if (keyComponents.Count == 0) return sum;
        foreach (var component in keyComponents) {
            if (component.hasAuricDistribution) sum += component.auricDistribution;
        }
        return sum;
    }

    public ManaDistribution GetCombinedBasicDistribution() {
        ManaDistribution sum = new ManaDistribution();
        if (keyComponents.Count == 0) return sum;
        foreach (var component in keyComponents) {
            if (component.hasBasicDistribution) sum += component.basicDistribution;
        }
        return sum;
    }

    public override string ToString() {
        string componentString = "";
        foreach(AuricaSpellComponent c in keyComponents) {
            componentString += c.c_name+", ";
        }
        componentString = componentString.Substring(0, componentString.Length-2);
        return componentString;
    }
}