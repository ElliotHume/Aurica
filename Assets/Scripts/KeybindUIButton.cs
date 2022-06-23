using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybindUIButton : MonoBehaviour {

    public KeybindingActions keyBindAction;
    public Text KeyCodeText;
    public bool isPrimaryButton;

    private bool updated = false, waitingForInput = false;
    private InputManager inputManager;

    void Update() {
        if (waitingForInput) {
            // This sucks but we dont do it much.
            foreach(KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode))){
                if (Input.GetKeyDown(keyCode)) {
                    waitingForInput = false;
                    if (keyCode.ToString() == "Mouse0" || keyCode.ToString() == "Mouse1") {
                        SetCustomKey(KeyCode.None);
                        return;
                    }
                    SetCustomKey(keyCode);
                    break;
                }
            }
        }
    }


    void FixedUpdate() {
        if (!updated && InputManager.Instance != null) {
            inputManager = InputManager.Instance;
            if (isPrimaryButton) {
                KeyCodeText.text = inputManager.GetPrimaryActionKeyCode(keyBindAction).ToString();
            } else {
                KeyCodeText.text = inputManager.GetAlternateActionKeyCode(keyBindAction).ToString();
            }
            updated = true;
        }
    }

    public void ActivateSetCustomKey() {
        waitingForInput = true;
        KeyCodeText.text = "...";
    }

    public void SetCustomKey(KeyCode key) {
        Debug.Log("SET TO "+key.ToString());
        if (isPrimaryButton) {
            inputManager.RebindActionPrimaryKey(keyBindAction, key);
        } else {
            inputManager.RebindActionAlternateKey(keyBindAction, key);
        }
        updated = false;
    }
}
