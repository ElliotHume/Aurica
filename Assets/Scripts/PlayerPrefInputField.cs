using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(InputField))]
public class PlayerPrefInputField : MonoBehaviour
{

    // Store the PlayerPref Key to avoid typos
    public string playerPrefKey = "";

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start () {
        string defaultName = string.Empty;
        InputField _inputField = this.GetComponent<InputField>();
        if (_inputField!=null) {
            if (PlayerPrefs.HasKey(playerPrefKey) && PlayerPrefs.GetString(playerPrefKey) != "") {
                defaultName = PlayerPrefs.GetString(playerPrefKey);
                _inputField.text = defaultName;
            }
        }
    }


    public void SetPlayerPref(string value) {
        if (value == "") return;
        PlayerPrefs.SetString(playerPrefKey,value);
    }
}