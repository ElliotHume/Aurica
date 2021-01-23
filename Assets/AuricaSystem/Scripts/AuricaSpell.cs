using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AuricaSpell", menuName = "Aurica/AuricaSpell", order = 1)]
public class AuricaSpell : ScriptableObject {
    public string c_name;
    [TextArea(15, 3)]
    public string description;
    public ManaDistribution targetDistribution;
    public List<AuricaSpellComponent> keyComponents;
    public float errorThreshold = 3.0f;
    public bool isAuric = false;
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
        return sortedList.SequenceEqual(indexList) && indexList.Count == keyComponents.Count;
    }

    public float GetError(ManaDistribution targetDist) {
        return targetDistribution.CheckDistError(targetDist);
    }

    public static bool operator ==(AuricaSpell a, AuricaSpell b) {
        return a.c_name == b.c_name;
    }
    public static bool operator !=(AuricaSpell a, AuricaSpell b) {
        return a.c_name != b.c_name;
    }
}