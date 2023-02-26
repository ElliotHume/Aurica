using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloudLoadoutUIPanel : MonoBehaviour
{
    public static CloudLoadoutUIPanel Instance;
    public BindingButton bind1, bind2, bind3, bind4, bindQ, bindE, bindR, bindF;
    public BindingButton bind11, bind21, bind31, bind41, bindQ1, bindE1, bindR1, bindF1;
    public BindingButton bind12, bind22, bind32, bind42, bindQ2, bindE2, bindR2, bindF2;
    public BindingButton bind13, bind23, bind33, bind43, bindQ3, bindE3, bindR3, bindF3;
    public BindingButton bind14, bind24, bind34, bind44, bindQ4, bindE4, bindR4, bindF4;
    private Dictionary<string, BindingButton> dict, loadout0, loadout1, loadout2;
    private string bindingspell;

    void Awake() {
        CloudLoadoutUIPanel.Instance = this;
        dict = new Dictionary<string, BindingButton>();
        dict.Add("q", bindQ);
        dict.Add("e", bindE);
        dict.Add("r", bindR);
        dict.Add("f", bindF);
        dict.Add("1", bind1);
        dict.Add("2", bind2);
        dict.Add("3", bind3);
        dict.Add("4", bind4);

        dict.Add("q-1", bindQ1);
        dict.Add("e-1", bindE1);
        dict.Add("r-1", bindR1);
        dict.Add("f-1", bindF1);
        dict.Add("1-1", bind11);
        dict.Add("2-1", bind21);
        dict.Add("3-1", bind31);
        dict.Add("4-1", bind41);

        dict.Add("q-2", bindQ2);
        dict.Add("e-2", bindE2);
        dict.Add("r-2", bindR2);
        dict.Add("f-2", bindF2);
        dict.Add("1-2", bind12);
        dict.Add("2-2", bind22);
        dict.Add("3-2", bind32);
        dict.Add("4-2", bind42);

        dict.Add("q-3", bindQ3);
        dict.Add("e-3", bindE3);
        dict.Add("r-3", bindR3);
        dict.Add("f-3", bindF3);
        dict.Add("1-3", bind13);
        dict.Add("2-3", bind33);
        dict.Add("3-3", bind33);
        dict.Add("4-3", bind43);

        dict.Add("q-4", bindQ4);
        dict.Add("e-4", bindE4);
        dict.Add("r-4", bindR4);
        dict.Add("f-4", bindF4);
        dict.Add("1-4", bind14);
        dict.Add("2-4", bind34);
        dict.Add("3-4", bind34);
        dict.Add("4-4", bind44);
    }

    void Start() {
        DisplayBinds();
        gameObject.SetActive(false);
    }

    void OnEnable() {
        DisplayBinds();
    }

    public void Refresh() {
        DisplayBinds();
    }

    public void DisplayBinds() {
        if (CloudLoadoutManager.Instance == null) return;
        Dictionary<string, string> loadout = CloudLoadoutManager.Instance.GetLoadout();

        foreach(KeyValuePair<string, string> entry in loadout) {
            SetBindSlotVisuals(entry.Key, entry.Value);
        }
    }

    public void SetBindSlotVisuals(string key, string sp) {
        // Debug.Log("Try set visuals for key: "+key+"    spell: "+sp);
        if (AuricaCaster.LocalCaster == null) return;
        AuricaSpell spell = AuricaCaster.LocalCaster.CastSpellByName(sp);
        float spellStrength = AuricaCaster.LocalCaster.GetSpellStrength();
        AuricaCaster.LocalCaster.ResetCast();
        if (spell == null) return;
        // Debug.Log("Setting visuals");
        dict[key.ToLower()].SetButtonGraphics(spell, sp, spellStrength);
    }

    public void TakePersonalLoadout(int page) {
        Dictionary<string, string> loadout = CloudLoadoutManager.Instance.GetPagedLoadout(page);
        foreach(KeyValuePair<string, string> entry in loadout) {
            AuricaCaster.LocalCaster.CacheSpell(entry.Key, entry.Value);
        }
    }

    public void Bind(string key) {
        CloudLoadoutManager.Instance.Bind(key, bindingspell);
    }

    public void Cast(string key) {
        Dictionary<string, string> loadout = CloudLoadoutManager.Instance.GetLoadout();
        AuricaCaster.LocalCaster.CastSpellByName(loadout[key]);
        bindingspell = loadout[key];
    }
}
