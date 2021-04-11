using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoadoutObject : MonoBehaviour
{
    public bool useString = false,key1,key2,key3,keyQ,keyE,keyR;
    public AuricaSpell spell1, spell2, spell3, spellQ, spellE, spellR;
    public string spell1Text, spell2Text, spell3Text, spellQText, spellEText, spellRText;
    public UnityEvent OnBind;

    public void BindLoadout() {
        if (!useString) {
            if (key1) AuricaCaster.LocalCaster.CacheSpell("1", spell1.ToString());
            if (key2) AuricaCaster.LocalCaster.CacheSpell("2", spell2.ToString());
            if (key3) AuricaCaster.LocalCaster.CacheSpell("3", spell3.ToString());
            if (keyQ) AuricaCaster.LocalCaster.CacheSpell("q", spellQ.ToString());
            if (keyE) AuricaCaster.LocalCaster.CacheSpell("e", spellE.ToString());
            if (keyR) AuricaCaster.LocalCaster.CacheSpell("r", spellR.ToString());
        } else {
            if (key1) AuricaCaster.LocalCaster.CacheSpell("1", spell1Text);
            if (key2) AuricaCaster.LocalCaster.CacheSpell("2", spell2Text);
            if (key3) AuricaCaster.LocalCaster.CacheSpell("3", spell3Text);
            if (keyQ) AuricaCaster.LocalCaster.CacheSpell("q", spellQText);
            if (keyE) AuricaCaster.LocalCaster.CacheSpell("e", spellEText);
            if (keyR) AuricaCaster.LocalCaster.CacheSpell("r", spellRText);
        }
        OnBind.Invoke();
    }
}
