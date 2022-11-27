using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellLoadoutDropSlot : MonoBehaviour {

    public string BindKeyID;
    public bool CloudLoadoutSlot;

    public void OnSpellDrop(string SpellText) {
        // Debug.Log("OnSpellDrop: "+SpellText);
        if (CloudLoadoutSlot) {
            CloudLoadoutManager.Instance.Bind(BindKeyID, SpellText);
        } else {
            AuricaCaster.LocalCaster.CacheSpell(BindKeyID, SpellText);
        }
    }
}
