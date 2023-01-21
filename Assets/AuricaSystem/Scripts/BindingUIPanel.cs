using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindingUIPanel : MonoBehaviour {

    public BindingButton bind1, bind2, bind3, bind4, bindQ, bindE, bindR, bindF;
    public static BindingUIPanel LocalInstance;
    private Dictionary<string, BindingButton> dict = new Dictionary<string, BindingButton>();

    private AuricaCaster caster;

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
        if (caster == null) {
            caster = AuricaCaster.LocalCaster;
        }
        if (caster == null || !caster.spellManasCached) return;
        float availableMana = PlayerManager.LocalInstance.Mana;
        Dictionary<KeybindingActions, RecastSpell> activeRecastSpells = PlayerManager.LocalInstance.activeRecastSpells;
        foreach(string key in dict.Keys) {
            dict[key].CanCast(caster.CanCastCachedSpell(key, availableMana));
        }
    }

    public void Startup() {
        if (caster == null) {
            caster = AuricaCaster.LocalCaster;
        }
        if (PlayerPrefs.HasKey("CachedSpell_e")) {
            SetBind("e", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_e")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotE));
        }
        if (PlayerPrefs.HasKey("CachedSpell_q")) {
            SetBind("q", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_q")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotQ));
        }
        if (PlayerPrefs.HasKey("CachedSpell_1")) {
            SetBind("1", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_1")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot1));
        }
        if (PlayerPrefs.HasKey("CachedSpell_2")) {
            SetBind("2", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_2")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot2));
        }
        if (PlayerPrefs.HasKey("CachedSpell_3")) {
            SetBind("3", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_3")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot3));
        }
        if (PlayerPrefs.HasKey("CachedSpell_4")) {
            SetBind("4", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_4")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot4));
        }
        if (PlayerPrefs.HasKey("CachedSpell_f")) {
            SetBind("f", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_f")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotF));
        }
        if (PlayerPrefs.HasKey("CachedSpell_r")) {
            SetBind("r", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_r")), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotR));
        }
        caster.ResetCast();
    }

    public void SetBind(string key, AuricaSpell spell, string keybind="") {
        if (spell == null) return;
        dict[key].SetButtonGraphics(spell);
        if (keybind != "") dict[key].SetKeyText(keybind);
    }

    public void Bind(string key) {
        caster.CacheCurrentSpell(key);
    }
}
