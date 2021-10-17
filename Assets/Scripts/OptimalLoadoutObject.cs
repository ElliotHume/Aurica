using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class OptimalLoadoutObject : MonoBehaviour
{
    public List<AuricaSpell> excludedSpells, allSpellsList;
    public UnityEvent OnBind;

    public List<AuricaSpell> GenerateOptimalSpells() {
        AuricaSpell[] allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpellsList = new List<AuricaSpell>(allSpells);
        foreach ( AuricaSpell spell in allSpellsList) {
            if ((spell.keyComponents.Count == 0 || spell.isAuric) && !excludedSpells.Contains(spell) ) excludedSpells.Add(spell);
        }
        if (excludedSpells.Count > 0) {
            foreach (AuricaSpell spell in excludedSpells) allSpellsList.Remove(spell);
        }
        allSpellsList = allSpellsList.OrderBy((spell) => 2f-AuricaCaster.LocalCaster.GetSpellStrengthForSpell(spell)).ToList();
        // int i = 1;
        // foreach ( AuricaSpell s in allSpellsList ) {
        //     Debug.Log(""+i+". "+s.c_name+"  with strength of: "+AuricaCaster.LocalCaster.GetSpellStrengthForSpell(s));
        //     i++;
        // }
        return allSpellsList;
    }

    public void BindLoadout() {
        GenerateOptimalSpells();
        AuricaCaster.LocalCaster.CacheSpell("1", allSpellsList[0].ToString());
        AuricaCaster.LocalCaster.CacheSpell("2", allSpellsList[1].ToString());
        AuricaCaster.LocalCaster.CacheSpell("3", allSpellsList[2].ToString());
        AuricaCaster.LocalCaster.CacheSpell("4", allSpellsList[3].ToString());
        AuricaCaster.LocalCaster.CacheSpell("q", allSpellsList[4].ToString());
        AuricaCaster.LocalCaster.CacheSpell("e", allSpellsList[5].ToString());
        AuricaCaster.LocalCaster.CacheSpell("r", allSpellsList[6].ToString());
        AuricaCaster.LocalCaster.CacheSpell("f", allSpellsList[7].ToString());
        OnBind.Invoke();
    }
}
