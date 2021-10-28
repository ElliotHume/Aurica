using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CachedSpell {
    public string componentsByName;

    public CachedSpell(string components) {
        componentsByName = components;
    }

    public void AddComponents(AuricaCaster caster) {
        string[] componentSeperator = new string[] { ", " };
        string[] splitComponents = componentsByName.Split(componentSeperator, System.StringSplitOptions.None);
        foreach (string item in splitComponents) {
            caster.AddComponent(item);
        }
    }

    public float CalculateManaCost(AuricaCaster caster) {
        caster.CastSpellByName(componentsByName);
        float manaCost = caster.GetManaCost();
        caster.ResetCast();
        return manaCost;
    }
}
