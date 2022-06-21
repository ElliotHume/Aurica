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
    public string key1, key2, key3, key4, keyQ, keyE, keyR, keyF;
    

    void Start() {
        CloudLoadoutManager.Instance = this;
    }

    void FixedUpdate() {
        if (!fetched && !fetching) FetchCloudLoadout();
    }

    public Dictionary<string, string> GetLoadout() {
        Dictionary<string, string> loadout = new Dictionary<string, string>();
        loadout.Add("1", key1);
        loadout.Add("2", key2);
        loadout.Add("3", key3);
        loadout.Add("4", key4);
        loadout.Add("Q", keyQ);
        loadout.Add("E", keyE);
        loadout.Add("R", keyR);
        loadout.Add("F", keyF);
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

        if (result.Data.ContainsKey("LoadoutKey1")) {
            key1 = result.Data["LoadoutKey1"].Value;
            Debug.Log("Fetched key \"1\" from cloud: "+key1);
        } else {
            Debug.Log("No spell for key: \"1\" found");
        }

        if (result.Data.ContainsKey("LoadoutKey2")) {
            key2 = result.Data["LoadoutKey2"].Value;
            Debug.Log("Fetched key \"2\" from cloud: "+key2);
        } else {
            Debug.Log("No spell for key: \"2\" found");
        }

        if (result.Data.ContainsKey("LoadoutKey3")) {
            key3 = result.Data["LoadoutKey3"].Value;
            Debug.Log("Fetched key \"3\" from cloud: "+key3);
        } else {
            Debug.Log("No spell for key: \"3\" found");
        }

        if (result.Data.ContainsKey("LoadoutKey4")) {
            key4 = result.Data["LoadoutKey4"].Value;
            Debug.Log("Fetched key \"4\" from cloud: "+key4);
        } else {
            Debug.Log("No spell for key: \"4\" found");
        }

        if (result.Data.ContainsKey("LoadoutKeyQ")) {
            keyQ = result.Data["LoadoutKeyQ"].Value;
            Debug.Log("Fetched key \"Q\" from cloud: "+keyQ);
        } else {
            Debug.Log("No spell for key: \"Q\" found");
        }

        if (result.Data.ContainsKey("LoadoutKeyE")) {
            keyE = result.Data["LoadoutKeyE"].Value;
            Debug.Log("Fetched key \"E\" from cloud: "+keyE);
        } else {
            Debug.Log("No spell for key: \"E\" found");
        }

        if (result.Data.ContainsKey("LoadoutKeyR")) {
            keyR = result.Data["LoadoutKeyR"].Value;
            Debug.Log("Fetched key \"R\" from cloud: "+keyR);
        } else {
            Debug.Log("No spell for key: \"R\" found");
        }

        if (result.Data.ContainsKey("LoadoutKeyF")) {
            keyF = result.Data["LoadoutKeyF"].Value;
            Debug.Log("Fetched key \"F\" from cloud: "+keyF);
        } else {
            Debug.Log("No spell for key: \"F\" found");
        }

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
