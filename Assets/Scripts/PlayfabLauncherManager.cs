using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayfabLauncherManager : MonoBehaviourPun
{

    public TMP_Text MessageText;
    public TMP_InputField EmailField, PasswordField;

    private string AuraText;

    void Start() {
        
    }

    public void RegisterButton() {
        if (PasswordField.text.Length < 6) {
            MessageText.text = "Password must be 6 or more characters.";
            return;
        }

        var request = new RegisterPlayFabUserRequest {
            Email = EmailField.text,
            Password = PasswordField.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result) {
        MessageText.text = "Successfully registered new user!";
    }

    public void LoginButton() {
        var request = new LoginWithEmailAddressRequest {
            Email = EmailField.text,
            Password = PasswordField.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result) {
        MessageText.text = "Successful login!";
        GetAura();
    }

    public void ResetPasswordButton () {

    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result) {

    }

    

    public void GetAura() {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        Debug.Log("Recieved player data");

        if (result.Data != null && result.Data.ContainsKey("Aura")) {

        } else {
            string playerName = PhotonNetwork.NickName;
            Debug.Log("Trying to load aura file: " + playerName + "-aura");
            TextAsset auraFile = Resources.Load<TextAsset>("Auras/" + playerName + "-aura");

            // No personal aura was found, use default
            if (auraFile == null) auraFile = Resources.Load<TextAsset>("Auras/default-aura");

            if (auraFile != null) {
                ManaDistribution AuraDistribution = JsonUtility.FromJson<ManaDistribution>(auraFile.text);
                AuraText = AuraDistribution.ToString();
            }
        }
    }

    public void SaveAura() {
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"Aura", ""}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }

    void OnDataSend(UpdateUserDataResult result) {

    }

    void OnError(PlayFabError error) {
        MessageText.text = error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}
