using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;


using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{

    /*
    * WHEN BUILDING ON MAC
    * - Build normally
    * - Then run command on the app from terminal when in the corrrect folder:
    *       codesign --deep -s - -f Aurica.app
    */
    public static GameManager Instance;
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    public Transform SceneSpawnPoint;
    public float RespawnTimer = 5.0f;

    public GameObject spellCraftingPanel, glyphCastingPanel, auraPanel, infoPanel, spellListPanel;

    public static float GLOBAL_SPELL_SPEED_MULTIPLIER = 2f;
    public static float GLOBAL_SPELL_DAMAGE_MULTIPLIER = 1f;
    public static float GLOBAL_SPELL_DURATION_MULTIPLIER = 1f;
    public static float GLOBAL_SHIELD_HEALTH_MULTIPLIER = 0.5f;
    public static float GLOBAL_ANIMATION_SPEED_MULTIPLIER = 1.5f;
    public static float GLOBAL_PLAYER_MOVEMENT_SPEED_MULTIPLIER = 1.25f;
    

    void Start() {
        Instance = this;
        if (playerPrefab == null){
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
        } else {
            if (PlayerManager.LocalPlayerInstance == null) {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                if (PhotonNetwork.IsConnected) {
                    PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position, transform.rotation, 0);
                } else {
                    // Go back to the launcher, as the connection has failed at some point (or you are loading the game from the wrong scene)
                    Cursor.lockState = CursorLockMode.None;
                    SceneManager.LoadScene(0);
                }
            } else {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }

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
            }
        }

        // Bring up spell list menu
        if (Input.GetKeyDown("c")) {
            spellListPanel.SetActive(!spellListPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }

        // Bring up the info menus
        if (Input.GetKeyDown("i")) {
            infoPanel.SetActive(!infoPanel.activeInHierarchy);
            glyphCastingPanel.SetActive(!spellCraftingPanel.activeInHierarchy);
        }
    }

    #region Photon Callbacks


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        PhotonNetwork.Destroy(PlayerManager.LocalPlayerInstance);
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
    }


    #endregion


    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        Cursor.lockState = CursorLockMode.None;
    }


    #endregion
    #region Private Methods

    public void playerDeath( PlayerManager player ) {
        Debug.Log("A player has died...");
        // Only handle respawning if the spawn point is set, otherwise other game type managers will handle respawns
        if (SceneSpawnPoint != null) StartCoroutine(RespawnPlayer(player));
    }

    IEnumerator RespawnPlayer(PlayerManager player) {
        yield return new WaitForSeconds(RespawnTimer);
        player.Respawn();
        if (SceneSpawnPoint != null) {
            player.Teleport(SceneSpawnPoint.position);
        }
    }
    
    public GameObject GetSpellCraftingPanel() {
        return spellCraftingPanel;
    }

    public GameObject GetGlyphCastingPanel() {
        return glyphCastingPanel;
    }
    #endregion
}