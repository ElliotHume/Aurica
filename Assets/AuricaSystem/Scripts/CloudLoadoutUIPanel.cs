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

    // void Update() {
    //     if (Input.GetKeyDown("1")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_1");
    //         AuricaCaster.LocalCaster.CastBindSlot("1");
    //     } else if (Input.GetKeyDown("2")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_2");
    //         AuricaCaster.LocalCaster.CastBindSlot("2");
    //     } else if (Input.GetKeyDown("3")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_3");
    //         AuricaCaster.LocalCaster.CastBindSlot("3");
    //     } else if (Input.GetKeyDown("4")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_4");
    //         AuricaCaster.LocalCaster.CastBindSlot("4");
    //     } else if (Input.GetKeyDown("e")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_e");
    //         AuricaCaster.LocalCaster.CastBindSlot("e");
    //     } else if (Input.GetKeyDown("q")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_q");
    //         AuricaCaster.LocalCaster.CastBindSlot("q");
    //     } else if (Input.GetKeyDown("r")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_r");
    //         AuricaCaster.LocalCaster.CastBindSlot("r");
    //     } else if (Input.GetKeyDown("f")) {
    //         bindingspell = PlayerPrefs.GetString("CachedSpell_f");
    //         AuricaCaster.LocalCaster.CastBindSlot("f");
    //     } else if (Input.GetKeyDown(KeyCode.Tab)) {
    //         AuricaCaster.LocalCaster.ResetCast();
    //     }
    // }

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
