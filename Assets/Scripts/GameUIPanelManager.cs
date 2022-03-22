using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIPanelManager : MonoBehaviour {
    public static GameUIPanelManager Instance;
    public GameObject spellCraftingPanel, glyphCastingPanel, glyphDrawingFrame, auraPanel, infoPanel, spellListPanel, cultivationPanel, cloudLoadoutPanel;

    void Start() {
        GameUIPanelManager.Instance = this;
    }

    public bool IsEditingInputField => 
        EventSystem.current.currentSelectedGameObject?.TryGetComponent(out InputField _) ?? false;

    public bool ShouldProcessInputs() {
        return !spellCraftingPanel.activeInHierarchy && !cloudLoadoutPanel.activeInHierarchy;
    }

    void Update() {
        if (IsEditingInputField) return;
        
        // Bring up the Aura display menu
        if (Input.GetKeyDown("`")) {
            auraPanel.SetActive(!auraPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the spell crafting menu
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (infoPanel.activeInHierarchy || spellListPanel.activeInHierarchy || cultivationPanel.activeInHierarchy || auraPanel.activeInHierarchy || cloudLoadoutPanel.activeInHierarchy) {
                infoPanel.SetActive(false);
                spellListPanel.SetActive(false);
                auraPanel.SetActive(false);
                if (cultivationPanel.activeInHierarchy) {
                    cultivationPanel.GetComponent<RewardsUIPanel>().ClosePanel();
                }
                cloudLoadoutPanel.SetActive(false);
                return;
            }
            spellCraftingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up spell list menu
        if (Input.GetKeyDown("z")) {
            spellListPanel.SetActive(!spellListPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up cultivation menu
        if (Input.GetKeyDown("x")) {
            if (cultivationPanel.activeInHierarchy) {
                cultivationPanel.GetComponent<RewardsUIPanel>().ClosePanel();
            } else {
                cultivationPanel.SetActive(true);
            }
            glyphCastingPanel.SetActive(!cultivationPanel.activeInHierarchy);
        }

        // Bring up the info menus
        if (Input.GetKeyDown("i")) {
            infoPanel.SetActive(!infoPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the personal class loadout menu
        if (Input.GetKeyDown("l")) {
            cloudLoadoutPanel.SetActive(!cloudLoadoutPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // If no menus are open, and the player is holding right mouse, open the glyphdrawingmenu
        if (Input.GetButton("Fire2")) {
            if (!spellCraftingPanel.activeInHierarchy && !infoPanel.activeInHierarchy && !cultivationPanel.activeInHierarchy && !spellListPanel.activeInHierarchy && !auraPanel.activeInHierarchy) {
                glyphDrawingFrame.SetActive(true);
            }
        } else {
            glyphDrawingFrame.SetActive(false);
        }
    }
}
