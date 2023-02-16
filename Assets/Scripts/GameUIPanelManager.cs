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

    List<GameObject> menus;

    [HideInInspector]
    public bool glyphDrawingToggledOn = false;

    void Start() {
        GameUIPanelManager.Instance = this;
        inputManager = InputManager.Instance;

        // Add menus for easy comparison when closing, do not include cultivation
        menus = new List<GameObject>();
        menus.Add(spellCraftingPanel);
        menus.Add(infoPanel);
        menus.Add(spellListPanel);
        menus.Add(auraPanel);
        menus.Add(cloudLoadoutPanel);
        menus.Add(masteryPanel);
        menus.Add(settingsPanel);
        menus.Add(spellDiscoveryPopupPanel);
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

    public bool IsAMenuOpen() {
        return (spellCraftingPanel.activeInHierarchy ||
        infoPanel.activeInHierarchy ||
        spellListPanel.activeInHierarchy ||
        cultivationPanel.activeInHierarchy ||
        auraPanel.activeInHierarchy ||
        cloudLoadoutPanel.activeInHierarchy ||
        masteryPanel.activeInHierarchy ||
        settingsPanel.activeInHierarchy ||
        chatManager.IsChatActive ||
        spellDiscoveryPopupPanel.activeInHierarchy);
    }

    public void CloseAllMenus(GameObject exception=null) {
        foreach(GameObject menu in menus) {
            if (exception != menu) menu.SetActive(false);
        }
        if (cultivationPanel.activeInHierarchy && exception != cultivationPanel) {
            cultivationPanel.GetComponent<RewardsUIPanel>().ClosePanel();
        }
        if (exception != chatManager) {
            chatManager.UnFocus();
        }
    }

    void Update() {
        if (IsEditingInputField()) return;
        if (inputManager == null) inputManager = InputManager.Instance;
        if (chatManager == null) chatManager = PhotonChatManager.Instance;

        // Bring up the Menu
        if (inputManager.GetKeyDown(KeybindingActions.Menu)) {
            if (IsAMenuOpen()) {
                CloseAllMenus();
                return;
            }
            menuPanel.SetActive(!menuPanel.activeInHierarchy);
            glyphDrawingToggledOn = false;
        }
        
        // Bring up the Aura display menu
        if (inputManager.GetKeyDown(KeybindingActions.AuraMenu)) {
            auraPanel.SetActive(!auraPanel.activeInHierarchy);
        }

        // Bring up the spell crafting menu
        if (inputManager.GetKeyDown(KeybindingActions.CraftingMenu)) {
            if (IsAMenuOpen()) CloseAllMenus(spellCraftingPanel);
            spellCraftingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
            glyphDrawingToggledOn = false;
        }

        // Bring up spell list menu
        if (inputManager.GetKeyDown(KeybindingActions.GrimoireMenu)) {
            if (IsAMenuOpen()) CloseAllMenus(spellListPanel);
            spellListPanel.SetActive(!spellListPanel.activeInHierarchy);
        }

        // Bring up cultivation menu
        if (inputManager.GetKeyDown(KeybindingActions.CultivationMenu)) {
            if (cultivationPanel.activeInHierarchy) {
                cultivationPanel.GetComponent<RewardsUIPanel>().ClosePanel();
            } else {
                cultivationPanel.SetActive(true);
            }
        }

        // Bring up the info menus
        if (inputManager.GetKeyDown(KeybindingActions.InfoMenu)) {
            infoPanel.SetActive(!infoPanel.activeInHierarchy);
        }

        // Bring up the personal class loadout menu
        if (inputManager.GetKeyDown(KeybindingActions.LoadoutMenu)) {
            if (IsAMenuOpen()) CloseAllMenus(cloudLoadoutPanel);
            cloudLoadoutPanel.SetActive(!cloudLoadoutPanel.activeInHierarchy);
        }

        // Bring up the spell mastery loadout menu
        if (inputManager.GetKeyDown(KeybindingActions.MasteryMenu)) {
            if (IsAMenuOpen()) CloseAllMenus(masteryPanel);
            masteryPanel.SetActive(!masteryPanel.activeInHierarchy);
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

        glyphCastingPanel.SetActive(!IsAMenuOpen());
        
        // If no menus are open, and the player presses the LeftAlt button, toggle the glyphdrawing menu
        if (inputManager.GetKeyDown(KeybindingActions.GlyphCastingMenu)) {
            if (!IsAMenuOpen()) {
                if (!glyphDrawingFrame.activeInHierarchy) {
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
