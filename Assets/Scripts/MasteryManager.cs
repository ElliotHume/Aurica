using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using PlayFab;
using PlayFab.ClientModels;

public class MasteryManager : MonoBehaviour {
    public enum MasteryLevel {
        Novice, Adept, Master, Legend
    };

    public enum MasteryCategories {
        Auric, Order, Chaos, Life, Death, Fire, Water, Earth, Air, Divine, Demonic,
        Movement, Battlemage, Support, Defender
    };

    public static MasteryManager Instance;
    [HideInInspector]
    public int AuricMastery, OrderMastery, ChaosMastery, LifeMastery, DeathMastery, FireMastery, WaterMastery, EarthMastery, AirMastery, DivineMastery, DemonicMastery;
    [HideInInspector]
    public int MovementMastery, BattlemageMastery, SupportMastery, DefenderMastery;
    [HideInInspector]
    public Dictionary<MasteryCategories, int> Masteries = new Dictionary<MasteryCategories, int>();
    [HideInInspector]
    public bool synced = true;

    private bool fetched = false, fetching = false;
    private Dictionary<string, MasteryCategories> cloudKeys;
    

    void Start() {
        MasteryManager.Instance = this;
        cloudKeys = new Dictionary<string, MasteryCategories>();
        cloudKeys.Add("AuricMastery", MasteryCategories.Auric);
        cloudKeys.Add("OrderMastery", MasteryCategories.Order);
        cloudKeys.Add("ChaosMastery", MasteryCategories.Chaos);
        cloudKeys.Add("LifeMastery", MasteryCategories.Life);
        cloudKeys.Add("DeathMastery", MasteryCategories.Death);
        cloudKeys.Add("FireMastery", MasteryCategories.Fire);
        cloudKeys.Add("WaterMastery", MasteryCategories.Water);
        cloudKeys.Add("EarthMastery", MasteryCategories.Earth);
        cloudKeys.Add("AirMastery", MasteryCategories.Air);
        cloudKeys.Add("DivineMastery", MasteryCategories.Divine);
        cloudKeys.Add("DemonicMastery", MasteryCategories.Demonic);
        cloudKeys.Add("MovementMastery", MasteryCategories.Movement);
        cloudKeys.Add("BattlemageMastery", MasteryCategories.Battlemage);
        cloudKeys.Add("SupportMastery", MasteryCategories.Support);
        cloudKeys.Add("DefenderMastery", MasteryCategories.Defender);

        Masteries = new Dictionary<MasteryCategories, int>();
        Masteries.Add(MasteryCategories.Auric, AuricMastery);
        Masteries.Add(MasteryCategories.Order, OrderMastery);
        Masteries.Add(MasteryCategories.Chaos, ChaosMastery);
        Masteries.Add(MasteryCategories.Life, LifeMastery);
        Masteries.Add(MasteryCategories.Death, DeathMastery);
        Masteries.Add(MasteryCategories.Fire, FireMastery);
        Masteries.Add(MasteryCategories.Water, WaterMastery);
        Masteries.Add(MasteryCategories.Earth, EarthMastery);
        Masteries.Add(MasteryCategories.Air, AirMastery);
        Masteries.Add(MasteryCategories.Divine, DivineMastery);
        Masteries.Add(MasteryCategories.Demonic, DemonicMastery);
        Masteries.Add(MasteryCategories.Movement, MovementMastery);
        Masteries.Add(MasteryCategories.Battlemage, BattlemageMastery);
        Masteries.Add(MasteryCategories.Support, SupportMastery);
        Masteries.Add(MasteryCategories.Defender, DefenderMastery);
    }

    void FixedUpdate() {
        if (!fetched && !fetching) FetchMasteries();
    }

    public Dictionary<MasteryCategories, int> GetMasteries() {
        return Masteries;
    }

    public int GetMastery(MasteryCategories category) {
        return Masteries[category];
    }

    public void FetchMasteries() {
        fetching = true;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        if (result.Data == null) return;

        if (result.Data.ContainsKey("Masteries")) {
            string rawMasteries = result.Data["Masteries"].Value;
            SetMasteriesFromString(rawMasteries);
            Debug.Log("Fetched Masteries from cloud: "+rawMasteries);
        } else {
            Debug.Log("No Masteries found in cloud");
        }

        fetched = true;
        fetching = false;
    }

    public void AddMasteries(List<MasteryCategories> categories) {
        if (categories.Count == 0) return;
        foreach(MasteryCategories category in categories){
            Masteries[category] += 1;
            Debug.Log("Added 1 mastery to "+category.ToString());
        }
        synced = false;
    }

    public void AddMastery(MasteryCategories category, int amount) {
        if (amount == 0) return;
        Masteries[category] += amount;
        synced = false;
        Debug.Log("Added "+amount+" mastery to "+category.ToString());
    }

    public void UpdateMasteries(Dictionary<MasteryCategories, int> addedMasteries) {
        foreach(KeyValuePair<MasteryCategories,int> mastery in addedMasteries) {
            if (mastery.Value > 0) {
                Masteries[mastery.Key] = mastery.Value;
            }
        }
        synced = false;
    }

    public void SyncMasteries() {
        Debug.Log("Syncing Masteries");

        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"Masteries", MasteriesToString()}
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDiscoveriesDataSend, OnError);
    }

    void OnDiscoveriesDataSend(UpdateUserDataResult result) {
        Debug.Log("Masteries Synced with Cloud");
        synced = true;
        fetched = false;
    }

    void OnError(PlayFabError error) {
        Debug.LogError(error.GenerateErrorReport());
    }

    string MasteriesToString() {
        string text = "";
        foreach(KeyValuePair<MasteryCategories,int> mastery in Masteries) {
            text += ""+mastery.Key.ToString()+":"+mastery.Value+"|";
        }
        return text;
    }

    void SetMasteriesFromString(string rawText) {
        List<string> splitText = rawText.Split('|').ToList();
        foreach(string mastery in splitText){
            if (mastery == "") continue;
            List<string> splitMastery = mastery.Split(':').ToList();
            MasteryCategories category = (MasteryCategories)System.Enum.Parse( typeof(MasteryCategories), splitMastery[0] );
            Masteries[category] = int.Parse(splitMastery[1]);
        }
    }

}
