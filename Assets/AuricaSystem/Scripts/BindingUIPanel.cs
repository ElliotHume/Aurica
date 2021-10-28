using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindingUIPanel : MonoBehaviour
{
    public BindingButton bind1, bind2, bind3, bind4, bindQ, bindE, bindR, bindF;
    public static BindingUIPanel LocalInstance;
    private Dictionary<string, BindingButton> dict = new Dictionary<string, BindingButton>();

    void Awake() {
        BindingUIPanel.LocalInstance = this;

        dict.Add("q", bindQ);
        dict.Add("e", bindE);
        dict.Add("r", bindR);
        dict.Add("f", bindF);
        dict.Add("1", bind1);
        dict.Add("2", bind2);
        dict.Add("3", bind3);
        dict.Add("4", bind4);
    }

    void FixedUpdate() {
        if (AuricaCaster.LocalCaster == null || !AuricaCaster.LocalCaster.spellManasCached) return;
        float availableMana = PlayerManager.LocalInstance.Mana;
        foreach(string key in dict.Keys) {
            dict[key].CanCast(AuricaCaster.LocalCaster.CanCastCachedSpell(key, availableMana));
        }
    }

    public void Startup() {
        if (PlayerPrefs.HasKey("CachedSpell_e")) {
            SetBind("e", AuricaCaster.LocalCaster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_e")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_q")) {
            SetBind("q", AuricaCaster.LocalCaster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_q")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_1")) {
            SetBind("1", AuricaCaster.LocalCaster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_1")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_2")) {
            SetBind("2", AuricaCaster.LocalCaster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_2")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_3")) {
            SetBind("3", AuricaCaster.LocalCaster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_3")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_4")) {
            SetBind("4", AuricaCaster.LocalCaster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_4")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_f")) {
            SetBind("f", AuricaCaster.LocalCaster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_f")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_r")) {
            string componentString = PlayerPrefs.GetString("CachedSpell_r");
            // Debug.Log("Try to find spell name for: "+componentString);
            AuricaSpell spell = AuricaCaster.LocalCaster.CastSpellByName(componentString);
            // Debug.Log("Found spell name: "+spell.c_name);
            SetBind("r", spell);
        }
        AuricaCaster.LocalCaster.ResetCast();
    }

    public void SetBind(string key, AuricaSpell spell) {
        if (spell == null) return;
        dict[key].SetButtonGraphics(spell);
        Debug.Log("Set graphics for key: "+key+" with spell: "+spell.c_name);
    }

    public void Bind(string key) {
        AuricaCaster.LocalCaster.CacheCurrentSpell(key);
    }
}
