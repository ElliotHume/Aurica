using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloudLoadoutUIPanel : MonoBehaviour
{
    public static CloudLoadoutUIPanel Instance;
    public BindingButton bind1, bind2, bind3, bind4, bindQ, bindE, bindR, bindF;
    private Dictionary<string, BindingButton> dict;
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
        if (AuricaCaster.LocalCaster == null) return;
        AuricaSpell spell = AuricaCaster.LocalCaster.CastSpellByName(sp);
        AuricaCaster.LocalCaster.ResetCast();
        if (spell == null) return;
        dict[key.ToLower()].SetButtonGraphics(spell, sp);
    }

    public void TakePersonalLoadout() {
        Dictionary<string, string> loadout = CloudLoadoutManager.Instance.GetLoadout();
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
