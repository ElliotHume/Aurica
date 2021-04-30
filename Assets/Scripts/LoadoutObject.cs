using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LoadoutObject : MonoBehaviour
{
    public bool key1,key2,key3,keyQ,keyE,keyR;
    public AuricaSpell spell1, spell2, spell3, spellQ, spellE, spellR;
    public string spell1Text, spell2Text, spell3Text, spellQText, spellEText, spellRText;
    public UnityEvent OnBind;

    public void BindLoadout() {
        if (key1) AuricaCaster.LocalCaster.CacheSpell("1", spell1Text == "" ? spell1.ToString() : spell1Text);
        if (key2) AuricaCaster.LocalCaster.CacheSpell("2", spell2Text == "" ? spell2.ToString() : spell2Text);
        if (key3) AuricaCaster.LocalCaster.CacheSpell("3", spell3Text == "" ? spell3.ToString() : spell3Text);
        if (keyQ) AuricaCaster.LocalCaster.CacheSpell("q", spellQText == "" ? spellQ.ToString() : spellQText);
        if (keyE) AuricaCaster.LocalCaster.CacheSpell("e", spellEText == "" ? spellE.ToString() : spellEText);
        if (keyR) AuricaCaster.LocalCaster.CacheSpell("r", spellRText == "" ? spellR.ToString() : spellRText);
        OnBind.Invoke();
    }
}
