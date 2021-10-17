using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OptimalLoadoutObject : LoadoutObject
{
    public List<AuricaSpell> excludedSpells;

    public void BindOptimalSpells() {
        AuricaSpell[] allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        List<AuricaSpell> allSpellsList = new List<AuricaSpell>(allSpells);

        List<AuricaSpell> optimalSpells = new List<AuricaSpell>();
        
    }
}
