using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    public static InputManager Instance;
    public Keybindings keybindings;

    void Awake() {
        InputManager.Instance = this;
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
                return Input.GetKeyDown(keybindingCheck.keyCode) || (keybindingCheck.altKeyCode != KeyCode.None && Input.GetKeyDown(keybindingCheck.altKeyCode));
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
                return Input.GetKey(keybindingCheck.keyCode) || (keybindingCheck.altKeyCode != KeyCode.None && Input.GetKey(keybindingCheck.altKeyCode));
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
                return Input.GetKeyUp(keybindingCheck.keyCode) || (keybindingCheck.altKeyCode != KeyCode.None && Input.GetKeyUp(keybindingCheck.altKeyCode));
            }
        }
        return false;
    }

    public void RebindActionPrimaryKey(KeybindingActions keybindingAction, KeyCode key) {
        PlayerPrefs.SetInt(keybindingAction.ToString(), (int)key);
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
}
