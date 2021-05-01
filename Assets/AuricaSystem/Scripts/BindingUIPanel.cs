using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindingUIPanel : MonoBehaviour
{
    public BindingButton bind1, bind2, bind3, bindQ, bindE, bindR;
    public static BindingUIPanel LocalInstance;
    private Dictionary<string, BindingButton> dict = new Dictionary<string, BindingButton>();

    void Awake() {
        BindingUIPanel.LocalInstance = this;
    }

    public void Startup() {
        dict.Add("q", bindQ);
        dict.Add("e", bindE);
        dict.Add("r", bindR);
        dict.Add("1", bind1);
        dict.Add("2", bind2);
        dict.Add("3", bind3);

        if (PlayerPrefs.HasKey("CachedSpell_e")) {
            SetBind("e", AuricaCaster.LocalCaster.GetSpellMatchFromString(PlayerPrefs.GetString("CachedSpell_e")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_q")) {
            SetBind("q", AuricaCaster.LocalCaster.GetSpellMatchFromString(PlayerPrefs.GetString("CachedSpell_q")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_1")) {
            SetBind("1", AuricaCaster.LocalCaster.GetSpellMatchFromString(PlayerPrefs.GetString("CachedSpell_1")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_2")) {
            SetBind("2", AuricaCaster.LocalCaster.GetSpellMatchFromString(PlayerPrefs.GetString("CachedSpell_2")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_3")) {
            SetBind("3", AuricaCaster.LocalCaster.GetSpellMatchFromString(PlayerPrefs.GetString("CachedSpell_3")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_r")) {
            string componentString = PlayerPrefs.GetString("CachedSpell_r");
            // Debug.Log("Try to find spell name for: "+componentString);
            AuricaSpell spell = AuricaCaster.LocalCaster.GetSpellMatchFromString(componentString);
            // Debug.Log("Found spell name: "+spell.c_name);
            SetBind("r", spell);
        }
    }

    public void SetBind(string key, AuricaSpell spell) {
        Debug.Log("Set graphics for key: "+key+" with spell: "+spell.c_name);
        dict[key].SetButtonGraphics(spell);
    }

    public void Bind(string key) {
        AuricaCaster.LocalCaster.CacheCurrentSpell(key);
    }
}
