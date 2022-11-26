using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class CloudLoadoutManager : MonoBehaviour {

    public static CloudLoadoutManager Instance;

    private bool fetched = false, fetching = false;
    private AuricaSpell[] allSpells;
    private List<AuricaSpell> allSpellsList;

    [HideInInspector]
    public string page0Key1, page0Key2, page0Key3, page0Key4, page0KeyQ, page0KeyE, page0KeyR, page0KeyF;

    [HideInInspector]
    public string page1Key1, page1Key2, page1Key3, page1Key4, page1KeyQ, page1KeyE, page1KeyR, page1KeyF;

    [HideInInspector]
    public string page2Key1, page2Key2, page2Key3, page2Key4, page2KeyQ, page2KeyE, page2KeyR, page2KeyF;
    

    void Start() {
        CloudLoadoutManager.Instance = this;
    }

    void FixedUpdate() {
        if (!fetched && !fetching) FetchCloudLoadout();
    }

    public Dictionary<string, string> GetLoadout() {
        Dictionary<string, string> loadout = new Dictionary<string, string>();
        loadout.Add("1", page0Key1);
        loadout.Add("2", page0Key2);
        loadout.Add("3", page0Key3);
        loadout.Add("4", page0Key4);
        loadout.Add("Q", page0KeyQ);
        loadout.Add("E", page0KeyE);
        loadout.Add("R", page0KeyR);
        loadout.Add("F", page0KeyF);

        loadout.Add("1-1", page1Key1);
        loadout.Add("2-1", page1Key2);
        loadout.Add("3-1", page1Key3);
        loadout.Add("4-1", page1Key4);
        loadout.Add("Q-1", page1KeyQ);
        loadout.Add("E-1", page1KeyE);
        loadout.Add("R-1", page1KeyR);
        loadout.Add("F-1", page1KeyF);

        loadout.Add("1-2", page2Key1);
        loadout.Add("2-2", page2Key2);
        loadout.Add("3-2", page2Key3);
        loadout.Add("4-2", page2Key4);
        loadout.Add("Q-2", page2KeyQ);
        loadout.Add("E-2", page2KeyE);
        loadout.Add("R-2", page2KeyR);
        loadout.Add("F-2", page2KeyF);
        return loadout;
    }

    public string GetLoadoutKey(string key) {
        Dictionary<string, string> loadout = GetLoadout();
        return loadout[key.ToUpper()];
    }

    public void FetchCloudLoadout() {
        fetching = true;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        if (result.Data == null) return;
        if (result.Data.ContainsKey("LoadoutKey1")) page0Key1 = result.Data["LoadoutKey1"].Value;
        if (result.Data.ContainsKey("LoadoutKey2")) page0Key2 = result.Data["LoadoutKey2"].Value;
        if (result.Data.ContainsKey("LoadoutKey3")) page0Key3 = result.Data["LoadoutKey3"].Value;
        if (result.Data.ContainsKey("LoadoutKey4")) page0Key4 = result.Data["LoadoutKey4"].Value;
        if (result.Data.ContainsKey("LoadoutKeyQ")) page0KeyQ = result.Data["LoadoutKeyQ"].Value;
        if (result.Data.ContainsKey("LoadoutKeyE")) page0KeyE = result.Data["LoadoutKeyE"].Value;
        if (result.Data.ContainsKey("LoadoutKeyR")) page0KeyR = result.Data["LoadoutKeyR"].Value;
        if (result.Data.ContainsKey("LoadoutKeyF")) page0KeyF = result.Data["LoadoutKeyF"].Value;

        if (result.Data.ContainsKey("LoadoutKey1-1")) page1Key1 = result.Data["LoadoutKey1-1"].Value;
        if (result.Data.ContainsKey("LoadoutKey2-1")) page1Key2 = result.Data["LoadoutKey2-1"].Value;
        if (result.Data.ContainsKey("LoadoutKey3-1")) page1Key3 = result.Data["LoadoutKey3-1"].Value;
        if (result.Data.ContainsKey("LoadoutKey4-1")) page1Key4 = result.Data["LoadoutKey4-1"].Value;
        if (result.Data.ContainsKey("LoadoutKeyQ-1")) page1KeyQ = result.Data["LoadoutKeyQ-1"].Value;
        if (result.Data.ContainsKey("LoadoutKeyE-1")) page1KeyE = result.Data["LoadoutKeyE-1"].Value;
        if (result.Data.ContainsKey("LoadoutKeyR-1")) page1KeyR = result.Data["LoadoutKeyR-1"].Value;
        if (result.Data.ContainsKey("LoadoutKeyF-1")) page1KeyF = result.Data["LoadoutKeyF-1"].Value;

        if (result.Data.ContainsKey("LoadoutKey1-2")) page2Key1 = result.Data["LoadoutKey1-2"].Value;
        if (result.Data.ContainsKey("LoadoutKey2-2")) page2Key2 = result.Data["LoadoutKey2-2"].Value;
        if (result.Data.ContainsKey("LoadoutKey3-2")) page2Key3 = result.Data["LoadoutKey3-2"].Value;
        if (result.Data.ContainsKey("LoadoutKey4-2")) page2Key4 = result.Data["LoadoutKey4-2"].Value;
        if (result.Data.ContainsKey("LoadoutKeyQ-2")) page2KeyQ = result.Data["LoadoutKeyQ-2"].Value;
        if (result.Data.ContainsKey("LoadoutKeyE-2")) page2KeyE = result.Data["LoadoutKeyE-2"].Value;
        if (result.Data.ContainsKey("LoadoutKeyR-2")) page2KeyR = result.Data["LoadoutKeyR-2"].Value;
        if (result.Data.ContainsKey("LoadoutKeyF-2")) page2KeyF = result.Data["LoadoutKeyF-2"].Value;

        fetched = true;
        fetching = false;
        if (CloudLoadoutUIPanel.Instance != null) CloudLoadoutUIPanel.Instance.Refresh();
    }

    public void Bind(string key, string spell) {
        if (key == null || spell == null || key == "" || spell == "") return;
        Debug.Log("Binding spell: "+spell+"    to key: "+key.ToUpper());

        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {("LoadoutKey"+key.ToUpper()), spell}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDiscoveriesDataSend, OnError);
    }

    void OnDiscoveriesDataSend(UpdateUserDataResult result) {
        Debug.Log("Binding Sent to Cloud");
        fetched = false;
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }

}
