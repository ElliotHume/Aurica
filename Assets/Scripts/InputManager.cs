using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    public static InputManager Instance;
    public Keybindings keybindings;

    private Dictionary<KeyCode, string> keyTranslations = new Dictionary<KeyCode, string>();

    void Awake() {
        InputManager.Instance = this;

        PopulateKeyTranslations();
    }

    public KeyCode GetPrimaryActionKeyCode(KeybindingActions keybindingAction) {
        foreach(Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks) {
            if (keybindingCheck.keybindingAction == keybindingAction) {
                if (HasCustomPrimaryKey(keybindingAction)){
                    return GetCustomPrimaryKey(keybindingAction);
                }
                return keybindingCheck.keyCode;
            }
        }
        return KeyCode.None;
    }

    public KeyCode GetAlternateActionKeyCode(KeybindingActions keybindingAction) {
        foreach(Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks) {
            if (keybindingCheck.keybindingAction == keybindingAction) {
                if (HasCustomAlternateKey(keybindingAction)){
                    return GetCustomAlternateKey(keybindingAction);
                }
                return keybindingCheck.altKeyCode;
            }
        }
        return KeyCode.None;
    }

    public bool GetKeyDown(KeybindingActions keybindingAction) {
        foreach(Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks) {
            if (keybindingCheck.keybindingAction == keybindingAction) {
                if (HasCustomPrimaryKey(keybindingAction) && Input.GetKeyDown(GetCustomPrimaryKey(keybindingAction))){
                    return true;
                } else if (HasCustomAlternateKey(keybindingAction) && Input.GetKeyDown(GetCustomAlternateKey(keybindingAction))){
                    return true;
                }
                return (!HasCustomPrimaryKey(keybindingAction) && Input.GetKeyDown(keybindingCheck.keyCode)) || (!HasCustomAlternateKey(keybindingAction) && keybindingCheck.altKeyCode != KeyCode.None && Input.GetKeyDown(keybindingCheck.altKeyCode));
                
            }
        }
        return false;
    }

    public bool GetKey(KeybindingActions keybindingAction) {
        foreach(Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks) {
            if (keybindingCheck.keybindingAction == keybindingAction) {
                if (HasCustomPrimaryKey(keybindingAction) && Input.GetKey(GetCustomPrimaryKey(keybindingAction))){
                    return true;
                } else if (HasCustomAlternateKey(keybindingAction) && Input.GetKey(GetCustomAlternateKey(keybindingAction))){
                    return true;
                }
                return (!HasCustomPrimaryKey(keybindingAction) && Input.GetKey(keybindingCheck.keyCode)) || (!HasCustomAlternateKey(keybindingAction) && keybindingCheck.altKeyCode != KeyCode.None && Input.GetKey(keybindingCheck.altKeyCode));
            }
        }
        return false;
    }

    public bool GetKeyUp(KeybindingActions keybindingAction) {
        foreach(Keybindings.KeybindingCheck keybindingCheck in keybindings.keybindingChecks) {
            if (keybindingCheck.keybindingAction == keybindingAction) {
                if (HasCustomPrimaryKey(keybindingAction) && Input.GetKeyUp(GetCustomPrimaryKey(keybindingAction))){
                    return true;
                } else if (HasCustomAlternateKey(keybindingAction) && Input.GetKeyUp(GetCustomAlternateKey(keybindingAction))){
                    return true;
                }
                return (!HasCustomPrimaryKey(keybindingAction) && Input.GetKeyUp(keybindingCheck.keyCode)) || (!HasCustomAlternateKey(keybindingAction) && keybindingCheck.altKeyCode != KeyCode.None && Input.GetKeyUp(keybindingCheck.altKeyCode));
            }
        }
        return false;
    }

    public void RebindActionPrimaryKey(KeybindingActions keybindingAction, KeyCode key) {
        PlayerPrefs.SetInt(keybindingAction.ToString(), (int)key);
        BindingUIPanel.LocalInstance.Startup();
    }

    public void RebindActionAlternateKey(KeybindingActions keybindingAction, KeyCode key) {
        PlayerPrefs.SetInt(keybindingAction.ToString()+"Alt", (int)key);
    }

    public bool HasCustomPrimaryKey(KeybindingActions keybindingAction) {
        return PlayerPrefs.HasKey(keybindingAction.ToString());
    }

    public bool HasCustomAlternateKey(KeybindingActions keybindingAction) {
        return PlayerPrefs.HasKey(keybindingAction.ToString()+"Alt");
    }

    public KeyCode GetCustomPrimaryKey(KeybindingActions keybindingAction) {
        if (!HasCustomPrimaryKey(keybindingAction)) return KeyCode.None;
        return (KeyCode)PlayerPrefs.GetInt(keybindingAction.ToString());
    }

    public KeyCode GetCustomAlternateKey(KeybindingActions keybindingAction) {
        if (!HasCustomAlternateKey(keybindingAction)) return KeyCode.None;
        return (KeyCode)PlayerPrefs.GetInt(keybindingAction.ToString()+"Alt");
    }

    public string GetKeyTranslation(KeyCode code) {
        if (!keyTranslations.ContainsKey(code)) return code.ToString();
        return keyTranslations[code];
    }

    public string GetKeyTranslationOfAction(KeybindingActions keybindingAction, bool primary = true) {
        return GetKeyTranslation(primary ? GetPrimaryActionKeyCode(keybindingAction) : GetAlternateActionKeyCode(keybindingAction));
    }

    private void PopulateKeyTranslations() {
        keyTranslations.Add(KeyCode.Alpha0, "0");
        keyTranslations.Add(KeyCode.Alpha1, "1");
        keyTranslations.Add(KeyCode.Alpha2, "2");
        keyTranslations.Add(KeyCode.Alpha3, "3");
        keyTranslations.Add(KeyCode.Alpha4, "4");
        keyTranslations.Add(KeyCode.Alpha5, "5");
        keyTranslations.Add(KeyCode.Alpha6, "6");
        keyTranslations.Add(KeyCode.Alpha7, "7");
        keyTranslations.Add(KeyCode.Alpha8, "8");
        keyTranslations.Add(KeyCode.Alpha9, "9");

        keyTranslations.Add(KeyCode.RightShift, "R Shift");
        keyTranslations.Add(KeyCode.LeftShift, "L Shift");
        keyTranslations.Add(KeyCode.RightControl, "R Ctrl");
        keyTranslations.Add(KeyCode.LeftControl, "L Ctrl");
        keyTranslations.Add(KeyCode.RightAlt, "R Alt");
        keyTranslations.Add(KeyCode.LeftAlt, "L Alt");

        keyTranslations.Add(KeyCode.BackQuote, "~");
        keyTranslations.Add(KeyCode.Comma, ",");
        keyTranslations.Add(KeyCode.Period, ".");
    }
}
