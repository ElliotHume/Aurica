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

    /*
    * GOOD LOD SETTINGS
    * Level 1 - transition height: 0.4
    * Level 2 - transition height: 0.2 - regard curvature
    * Level 3 - transition height: 0.05 - regard curvature
    */
    
    public static GameManager Instance;
    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    public Transform StartingPosition;
    public Transform SceneSpawnPoint;
    public float RespawnTimer = 5.0f;

    public GameObject spellCraftingPanel, glyphCastingPanel, auraPanel, infoPanel, spellListPanel;

    public static float GLOBAL_SPELL_SPEED_MULTIPLIER = 2f;
    public static float GLOBAL_SPELL_DAMAGE_MULTIPLIER = 1f;
    public static float GLOBAL_SPELL_DURATION_MULTIPLIER = 1f;

    public static float GLOBAL_SPELL_HEALING_MULTIPLIER = 1f;

    public static float GLOBAL_SHIELD_HEALTH_MULTIPLIER = 1f;

    public static float GLOBAL_ANIMATION_SPEED_MULTIPLIER = 1.85f;
    public static float GLOBAL_PLAYER_MOVEMENT_SPEED_MULTIPLIER = 1.1f;

    public static float GLOBAL_MANA_COST_MULTIPLIER = 1f;
    public static float GLOBAL_PLAYER_MAX_MANA_MULTIPLIER = 1f;
    public static float GLOBAL_PLAYER_MANA_REGEN_MULTIPLIER = 1f;
    public static float GLOBAL_PLAYER_MANA_GROWTH_MULTIPLIER = 1f;
    
    public static bool GLOBAL_ENABLE_PROJECTILE_AIM_ASSIST = true;
    

    void Start() {
        Instance = this;
        if (playerPrefab == null){
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
        } else {
            if (PlayerManager.LocalPlayerGameObject == null) {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                if (PhotonNetwork.IsConnected) {
                    Vector3 position = StartingPosition != null ? StartingPosition.position : (SceneSpawnPoint != null ? SceneSpawnPoint.position : transform.position);
                    Quaternion rotation = StartingPosition != null ? StartingPosition.rotation : (SceneSpawnPoint != null ? SceneSpawnPoint.rotation : transform.rotation);
                    PhotonNetwork.Instantiate(this.playerPrefab.name, position + (UnityEngine.Random.insideUnitSphere), rotation, 0);
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

    #region Photon Callbacks


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        PhotonNetwork.Destroy(PlayerManager.LocalPlayerGameObject);
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
    }


    #endregion

    #region Static Methods
    
    public static PlayerManager GetPlayerFromID(string PlayerID) {
        PlayerManager[] players = FindObjectsOfType<PlayerManager>();
        return Array.Find(players, player => player.GetUniqueName() == PlayerID);
    }

    #endregion

    #region Public Methods

    public void StartRoomLeave() {
        if (!MasteryManager.Instance.synced) MasteryManager.Instance.SyncMasteries();
        StartCoroutine(WaitLeaveRoom());
    }

    IEnumerator WaitLeaveRoom() {
        while (!MasteryManager.Instance.synced) {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1f);
        LeaveRoom();
    }


    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        Cursor.lockState = CursorLockMode.None;
    }


    #endregion
    #region Private Methods

    public void playerDeath( PlayerManager player ) {
        // Debug.Log("A player has died...");
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