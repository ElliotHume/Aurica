using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIPanelManager : MonoBehaviour {
    public static GameUIPanelManager Instance;
    public GameObject menuPanel, spellCraftingPanel, glyphCastingPanel, glyphDrawingFrame, auraPanel, infoPanel, spellListPanel, cultivationPanel, cloudLoadoutPanel, masteryPanel, settingsPanel, spellDiscoveryPopupPanel;

    InputManager inputManager;
    PhotonChatManager chatManager;

    [HideInInspector]
    public bool glyphDrawingToggledOn = false;

    void Start() {
        GameUIPanelManager.Instance = this;
        inputManager = InputManager.Instance;
    }

    public bool IsEditingInputField() {
        bool selectedInput = EventSystem.current.currentSelectedGameObject?.TryGetComponent(out InputField _) ?? false;
        if (selectedInput) {
            InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (inputField == null) return false;
            return inputField.isFocused;
        }
        return false;
    }

    public bool HasSelectedInputField() {
        bool selectedInput = EventSystem.current.currentSelectedGameObject?.TryGetComponent(out InputField _) ?? false;
        return selectedInput;
    }
        

    public bool ShouldProcessInputs() {
        return !spellCraftingPanel.activeInHierarchy && !cloudLoadoutPanel.activeInHierarchy && !settingsPanel.activeInHierarchy && !IsEditingInputField();
    }

    void Update() {
        if (IsEditingInputField()) return;
        if (inputManager == null) inputManager = InputManager.Instance;
        if (chatManager == null) chatManager = PhotonChatManager.Instance;

        // Bring up the Menu
        if (inputManager.GetKeyDown(KeybindingActions.Menu)) {
            if (spellCraftingPanel.activeInHierarchy || infoPanel.activeInHierarchy || spellListPanel.activeInHierarchy || cultivationPanel.activeInHierarchy || auraPanel.activeInHierarchy || cloudLoadoutPanel.activeInHierarchy || masteryPanel.activeInHierarchy || settingsPanel.activeInHierarchy || chatManager.IsChatActive || spellDiscoveryPopupPanel.activeInHierarchy) {
                spellCraftingPanel.SetActive(false);
                infoPanel.SetActive(false);
                spellListPanel.SetActive(false);
                auraPanel.SetActive(false);
                spellDiscoveryPopupPanel.SetActive(false);
                if (cultivationPanel.activeInHierarchy) {
                    cultivationPanel.GetComponent<RewardsUIPanel>().ClosePanel();
                }
                cloudLoadoutPanel.SetActive(false);
                masteryPanel.SetActive(false);
                settingsPanel.SetActive(false);
                chatManager.UnFocus();
                return;
            }
            menuPanel.SetActive(!menuPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!menuPanel.activeInHierarchy);
            glyphDrawingToggledOn = false;
        }
        
        // Bring up the Aura display menu
        if (inputManager.GetKeyDown(KeybindingActions.AuraMenu)) {
            auraPanel.SetActive(!auraPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the spell crafting menu
        if (inputManager.GetKeyDown(KeybindingActions.CraftingMenu)) {
            spellCraftingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
            glyphDrawingToggledOn = false;
        }

        // Bring up spell list menu
        if (inputManager.GetKeyDown(KeybindingActions.GrimoireMenu)) {
            spellListPanel.SetActive(!spellListPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up cultivation menu
        if (inputManager.GetKeyDown(KeybindingActions.CultivationMenu)) {
            if (cultivationPanel.activeInHierarchy) {
                cultivationPanel.GetComponent<RewardsUIPanel>().ClosePanel();
            } else {
                cultivationPanel.SetActive(true);
            }
            glyphCastingPanel.SetActive(!cultivationPanel.activeInHierarchy);
        }

        // Bring up the info menus
        if (inputManager.GetKeyDown(KeybindingActions.InfoMenu)) {
            infoPanel.SetActive(!infoPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the personal class loadout menu
        if (inputManager.GetKeyDown(KeybindingActions.LoadoutMenu)) {
            cloudLoadoutPanel.SetActive(!cloudLoadoutPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the spell mastery loadout menu
        if (inputManager.GetKeyDown(KeybindingActions.MasteryMenu)) {
            masteryPanel.SetActive(!masteryPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the chat menu
        if (!HasSelectedInputField() && inputManager.GetKeyDown(KeybindingActions.ChatMenu)) {
            chatManager.Focus();
        }

        // If no menus are open, and the player is holding right mouse, open the glyphdrawingmenu
        if (Input.GetButton("Fire2")) {
            if (!spellCraftingPanel.activeInHierarchy && !infoPanel.activeInHierarchy && !cultivationPanel.activeInHierarchy && !spellListPanel.activeInHierarchy && !auraPanel.activeInHierarchy) {
                glyphDrawingFrame.SetActive(true);
            }
        } else {
            if (!glyphDrawingToggledOn) glyphDrawingFrame.SetActive(false);
        }
        
        // If no menus are open, and the player presses the LeftAlt button, toggle the glyphdrawing menu
        if (inputManager.GetKeyDown(KeybindingActions.GlyphCastingMenu)) {
            if (!(spellCraftingPanel.activeInHierarchy || infoPanel.activeInHierarchy || spellListPanel.activeInHierarchy || cultivationPanel.activeInHierarchy || auraPanel.activeInHierarchy || cloudLoadoutPanel.activeInHierarchy || masteryPanel.activeInHierarchy)) {
                if (!glyphDrawingFrame.activeInHierarchy && !spellCraftingPanel.activeInHierarchy && !infoPanel.activeInHierarchy && !cultivationPanel.activeInHierarchy && !spellListPanel.activeInHierarchy && !auraPanel.activeInHierarchy) {
                    glyphDrawingFrame.SetActive(true);
                    glyphDrawingToggledOn = true;
                } else {
                    glyphDrawingFrame.SetActive(false);
                    glyphDrawingToggledOn = false;
                }
            } else {
                glyphDrawingFrame.SetActive(false);
                glyphDrawingToggledOn = false;
            }
        } 
    }
}
