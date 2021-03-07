using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadoutObject : MonoBehaviour
{
    public bool key1,key2,key3,keyQ,keyE,keyR;
    public AuricaSpell spell1, spell2, spell3, spellQ, spellE, spellR;

    public void BindLoadout() {
        if (key1) AuricaCaster.LocalCaster.CacheSpell("1", spell1.ToString());
        if (key2) AuricaCaster.LocalCaster.CacheSpell("2", spell2.ToString());
        if (key3) AuricaCaster.LocalCaster.CacheSpell("3", spell3.ToString());
        if (keyQ) AuricaCaster.LocalCaster.CacheSpell("q", spellQ.ToString());
        if (keyE) AuricaCaster.LocalCaster.CacheSpell("e", spellE.ToString());
        if (keyR) AuricaCaster.LocalCaster.CacheSpell("r", spellR.ToString());
    }
}
