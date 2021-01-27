using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindingUIPanel : MonoBehaviour
{
    public Text bindQ, bindE, bind1, bind2, bind3, bindR;
    public static BindingUIPanel LocalInstance;
    private Dictionary<string, Text> dict = new Dictionary<string, Text>();

    void Start() {
        dict.Add("q", bindQ);
        dict.Add("e", bindE);
        dict.Add("r", bindR);
        dict.Add("1", bind1);
        dict.Add("2", bind2);
        dict.Add("3", bind3);

        BindingUIPanel.LocalInstance = this;
        if (PlayerPrefs.HasKey("CachedSpell_e")) {
            SetBindText("e", AuricaCaster.LocalCaster.GetSpellMatchString(PlayerPrefs.GetString("CachedSpell_e")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_q")) {
            SetBindText("q", AuricaCaster.LocalCaster.GetSpellMatchString(PlayerPrefs.GetString("CachedSpell_q")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_1")) {
            SetBindText("1", AuricaCaster.LocalCaster.GetSpellMatchString(PlayerPrefs.GetString("CachedSpell_1")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_2")) {
            SetBindText("2", AuricaCaster.LocalCaster.GetSpellMatchString(PlayerPrefs.GetString("CachedSpell_2")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_3")) {
            SetBindText("3", AuricaCaster.LocalCaster.GetSpellMatchString(PlayerPrefs.GetString("CachedSpell_3")));
        }
        if (PlayerPrefs.HasKey("CachedSpell_r")) {
            string componentString = PlayerPrefs.GetString("CachedSpell_r");
            Debug.Log("Try to find spell name for: "+componentString);
            string spellName = AuricaCaster.LocalCaster.GetSpellMatchString(componentString);
            Debug.Log("Found spell name: "+spellName);
            SetBindText("r", spellName);
        }
    }

    public void SetBindText(string key, string name) {
        dict[key].text = "Bound: "+name;
    }

    public void Bind(string key) {
        AuricaCaster.LocalCaster.CacheCurrentSpell(key);
    }
}
