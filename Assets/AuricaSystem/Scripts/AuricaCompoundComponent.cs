using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AuricaCompoundComponent", menuName = "Aurica/AuricaCompoundComponent", order = 3)]
public class AuricaCompoundComponent : ScriptableObject {
    public string c_name;
    public bool needsMastery = false;
    public MasteryManager.MasteryLevel masteryLevel = MasteryManager.MasteryLevel.Novice;
    public List<AuricaSpellComponent> components;
}