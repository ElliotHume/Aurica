using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIPanelManager : MonoBehaviour {
    public GameObject spellCraftingPanel, glyphCastingPanel, auraPanel, infoPanel, spellListPanel, cultivationPanel;

    public bool IsEditingInputField => 
        EventSystem.current.currentSelectedGameObject?.TryGetComponent(out InputField _) ?? false;

    void Update() {
        if (IsEditingInputField) return;
        
        // Bring up the Aura display menu
        if (Input.GetKeyDown("`")) {
            auraPanel.SetActive(!auraPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the spell crafting menu
        if (Input.GetKeyDown(KeyCode.Escape)) {
            spellCraftingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
            if (!spellCraftingPanel.activeInHierarchy) {
                auraPanel.SetActive(false);
                if (infoPanel != null) infoPanel.SetActive(false);
                if (spellListPanel != null) spellListPanel.SetActive(false);
                if (cultivationPanel.activeInHierarchy) {
                    cultivationPanel.GetComponent<RewardsUIPanel>().ClosePanel();
                }
            }
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
    }
}
