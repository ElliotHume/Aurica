using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class DiscoveryManager : MonoBehaviour {

    public static DiscoveryManager Instance;
    public List<AuricaSpell> StarterSpells;
    public AudioSource DiscoverySound;

    private string rawDiscoveries = "[]";
    private List<AuricaSpell> discoveredSpells;
    private bool fetched = false, fetching = false;

    private AuricaSpell[] allSpells;
    private List<AuricaSpell> allSpellsList;

    void Start() {
        DiscoveryManager.Instance = this;
        discoveredSpells = new List<AuricaSpell>();

        allSpells = Resources.LoadAll<AuricaSpell>("AuricaSpells");
        allSpellsList = new List<AuricaSpell>(allSpells);
    }

    void FixedUpdate() {
        if (!fetched && !fetching) GetDiscoveries();
    }

    public List<AuricaSpell> GetDiscoveredSpells() {
        return discoveredSpells;
    }

    public void GetDiscoveries() {
        fetching = true;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        if (result.Data != null && result.Data.ContainsKey("Discoveries")) {
            rawDiscoveries = result.Data["Discoveries"].Value;
            discoveredSpells = GetListFromString(rawDiscoveries);
            fetched = true;
            fetching = false;
            Debug.Log("Fetched discoveries from cloud: "+rawDiscoveries+"");
            AuricaCaster.LocalCaster.RetrieveDiscoveredSpells();
        } else {
            Discover(StarterSpells);
            rawDiscoveries = GetStringFromList(StarterSpells);
            Debug.Log("No discoveries found, discover starter spells: "+rawDiscoveries);
        }
    }

    public void Discover(AuricaSpell spell) {
        if (discoveredSpells.Contains(spell)) return;
        DiscoverySound.Play();
        discoveredSpells.Add(spell);

        string rawDiscoveries = GetStringFromList(discoveredSpells.Distinct().ToList());
        Debug.Log("Discovering spell: "+spell.c_name+"    raw: "+rawDiscoveries);

        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"Discoveries", rawDiscoveries}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDiscoveriesDataSend, OnError);
        NotificationText.Instance.ShowDiscovery(spell.c_name);
    }

    public void Discover(List<AuricaSpell> spells) {
        DiscoverySound.Play();
        discoveredSpells.AddRange(spells);

        string rawDiscoveries = GetStringFromList(discoveredSpells.Distinct().ToList());
        Debug.Log("Discovering spells: "+GetStringFromList(spells)+"    raw: "+rawDiscoveries);

        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"Discoveries", rawDiscoveries}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDiscoveriesDataSend, OnError);
        NotificationText.Instance.ShowDiscovery(spells.Count+"x new spells");
    }

    void OnDiscoveriesDataSend(UpdateUserDataResult result) {
        Debug.Log("Discoveries Sent to Cloud : "+rawDiscoveries);
        fetched = false;
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }

    List<AuricaSpell> GetListFromString(string rawText) {
        if (rawText == null || rawText == "") return new List<AuricaSpell>();
        string[] splitText = rawText.Split(',');
        return new List<AuricaSpell>(allSpells.Where((s) => splitText.Contains(s.c_name)).ToArray());
    }

    string GetStringFromList(List<AuricaSpell> spellList) {
        string rawText = "";
        foreach(AuricaSpell spell in spellList) {
            rawText += spell.c_name+","; 
        }
        return rawText;
    }
}
