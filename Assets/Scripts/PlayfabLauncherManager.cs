using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class PlayfabLauncherManager : MonoBehaviourPun
{

    public TMP_Text MessageText;
    public InputField UsernameField, EmailField, PasswordField, ResetPasswordEmailField;
    public AuraCreator auraCreator;
    public GameObject questionnairePanel;

    public UnityEvent OnLogin;

    private string AuraText;

    void Start() {
        // If the player is already logged in we want to skip the login process.
        CheckIfLoggedIn();
    }

    void CheckIfLoggedIn() {
        // Call the playfab API, if no error is returned then we are logged in.
        try {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), IsLoggedIn, IsNotLoggedIn);
        } catch {
            // Do nothing, if an error is caught it means we are not logged in.
        }
    }

    void IsLoggedIn(GetUserDataResult result) {
        OnLogin.Invoke();
    }

    void IsNotLoggedIn(PlayFabError error) {
        // Do nothing, player is not logged in
    }

    public void RegisterButton() {
        if (PasswordField.text.Length < 6) {
            MessageText.text = "Password must be 6 or more characters.";
            return;
        }

        var request = new RegisterPlayFabUserRequest {
            Username = UsernameField.text,
            Email = EmailField.text,
            Password = PasswordField.text,
            TitleId = "FAEE6"
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result) {
        MessageText.text = "Successfully registered new user!";
    }

    public void LoginButton() {
        var request = new LoginWithPlayFabRequest {
            Username = UsernameField.text,
            Password = PasswordField.text,
            TitleId = "FAEE6"
        };
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result) {
        MessageText.text = "Successful login!";
        GetAura();
    }

    public void ResetPasswordButton () {
        var request = new SendAccountRecoveryEmailRequest {
            Email = ResetPasswordEmailField.text,
            TitleId = "FAEE6"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result) {
        MessageText.text = "Password reset email sent!";
    }


    public void GetAura() {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataRecieved, OnError);
    }

    void OnDataRecieved (GetUserDataResult result) {
        Debug.Log("Recieved player data");

        if (result.Data != null && result.Data.ContainsKey("Aura")) {
            PlayerPrefs.SetString("Aura", result.Data["Aura"].Value);
            OnLogin.Invoke();
        } else {
            string playerName = UsernameField.text;
            Debug.Log("Trying to load aura file: " + playerName + "-aura");
            TextAsset auraFile = Resources.Load<TextAsset>("Auras/" + playerName + "-aura");

            if (auraFile != null) {
                ManaDistribution AuraDistribution = JsonUtility.FromJson<ManaDistribution>(auraFile.text);
                AuraText = AuraDistribution.ToString();

                SaveAura(AuraText);
                PlayerPrefs.SetString("Aura", AuraText);
                OnLogin.Invoke();
            }

            // No personal aura was found, generate a random aura and save it.
            // This user can then later do the questionnaire and an admin will set their aura in the database.
            if (auraFile == null) {
                questionnairePanel.SetActive(true);
            }
        }
    }

    public void SaveAura(string AuraText) {
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                {"Aura", AuraText}
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSend, OnError);
    }

    void OnDataSend(UpdateUserDataResult result) {
        MessageText.text = "Personal Aura saved to cloud services: ["+AuraText+"]";
        Debug.Log("Player Aura Sent to Cloud : "+AuraText);
    }

    public void SaveQuestionnaireAuraResults() {
        ManaDistribution AuraDistribution = auraCreator.GetFinalAura();
        Debug.Log("Rolled aura: "+AuraText);
        AuraText = AuraDistribution.ToString();
        SaveAura(AuraText);
        PlayerPrefs.SetString("Aura", AuraText);
        OnLogin.Invoke();
    }

    void OnError(PlayFabError error) {
        MessageText.text = error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport());
    }
}
