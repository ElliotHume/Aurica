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

    public GameObject spawnPrefab;

    public bool CheckComponents(List<AuricaSpellComponent> components) {
        List<int> indexList = new List<int>();
        foreach (AuricaSpellComponent keyComp in keyComponents) {
            int ind = components.IndexOf(keyComp);
            if (ind == -1) return false;
            indexList.Add(ind);
        }

        List<int> sortedList = indexList;
        sortedList.Sort();
        return sortedList.SequenceEqual(indexList) && indexList.Count == keyComponents.Count;
        /*
        indexList = []
        for keyComp in self.keyComponents:
            try:
                indexList.append(components.index(keyComp))
            except:
                None ## do nothing
        return indexList == sorted(indexList) and len(indexList) == len(self.keyComponents)
        */
    }
}