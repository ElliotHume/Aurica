using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindingUIPanel : MonoBehaviour {

    public BindingButton bind1, bind2, bind3, bind4, bindQ, bindE, bindR, bindF;
    public static BindingUIPanel LocalInstance;
    private Dictionary<string, BindingButton> dict = new Dictionary<string, BindingButton>();
    private Dictionary<KeybindingActions, string> keyBindingDict = new Dictionary<KeybindingActions, string>();

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

        keyBindingDict.Add(KeybindingActions.SpellSlotQ, "q");
        keyBindingDict.Add(KeybindingActions.SpellSlotE, "e");
        keyBindingDict.Add(KeybindingActions.SpellSlotR, "r");
        keyBindingDict.Add(KeybindingActions.SpellSlotF, "f");
        keyBindingDict.Add(KeybindingActions.SpellSlot1, "1");
        keyBindingDict.Add(KeybindingActions.SpellSlot2, "2");
        keyBindingDict.Add(KeybindingActions.SpellSlot3, "3");
        keyBindingDict.Add(KeybindingActions.SpellSlot4, "4");
    }

    void FixedUpdate() {
        if (caster == null) {
            caster = AuricaCaster.LocalCaster;
        }
        if (caster == null || !caster.spellManasCached) return;
        float availableMana = PlayerManager.LocalInstance.Mana;
        foreach(string key in dict.Keys) {
            dict[key].CanCast(caster.CanCastCachedSpell(key, availableMana));
        }

        Dictionary<KeybindingActions, RecastSpell> activeRecastSpells = PlayerManager.LocalInstance.activeRecastSpells;
        foreach(KeyValuePair<KeybindingActions, string> entry in keyBindingDict) {
            bool recastActive = activeRecastSpells.ContainsKey(entry.Key) && activeRecastSpells[entry.Key] != null;
            bool canRecast = recastActive && activeRecastSpells[entry.Key].CanRecast();
            dict[entry.Value].CanRecast(recastActive, canRecast);
        }
    }

    public void Startup() {
        if (caster == null) {
            caster = AuricaCaster.LocalCaster;
        }
        AuricaSpell cast;
        float strength;
        if (PlayerPrefs.HasKey("CachedSpell_e")) {
            cast = caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_e"));
            strength = caster.GetSpellStrengthForSpell(PlayerPrefs.GetString("CachedSpell_e"));
            SetBind("e", cast, strength, InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotE));
        }
        if (PlayerPrefs.HasKey("CachedSpell_q")) {
            SetBind("q", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_q")), caster.GetSpellStrength(), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotQ));
        }
        if (PlayerPrefs.HasKey("CachedSpell_1")) {
            SetBind("1", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_1")), caster.GetSpellStrength(), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot1));
        }
        if (PlayerPrefs.HasKey("CachedSpell_2")) {
            SetBind("2", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_2")), caster.GetSpellStrength(), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot2));
        }
        if (PlayerPrefs.HasKey("CachedSpell_3")) {
            SetBind("3", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_3")), caster.GetSpellStrength(), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot3));
        }
        if (PlayerPrefs.HasKey("CachedSpell_4")) {
            SetBind("4", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_4")), caster.GetSpellStrength(), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlot4));
        }
        if (PlayerPrefs.HasKey("CachedSpell_f")) {
            SetBind("f", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_f")), caster.GetSpellStrength(), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotF));
        }
        if (PlayerPrefs.HasKey("CachedSpell_r")) {
            SetBind("r", caster.CastSpellByName(PlayerPrefs.GetString("CachedSpell_r")), caster.GetSpellStrength(), InputManager.Instance.GetKeyTranslationOfAction(KeybindingActions.SpellSlotR));
        }
        caster.ResetCast();
    }

    public void SetBind(string key, AuricaSpell spell, float spellStrength=-1f, string keybind="") {
        if (spell == null) return;
        dict[key].SetButtonGraphics(spell, "", spellStrength);
        if (keybind != "") dict[key].SetKeyText(keybind);
    }

    public void Bind(string key) {
        caster.CacheCurrentSpell(key);
    }
}
