using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

using System.Collections;

/// <summary>
/// Player manager.
/// Handles fire Input and Beams.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [Tooltip("The current Health of our player")]
    public float Health = 100f;
    public HealthBar healthBar;

    [Tooltip("Where spells witll spawn from when being cast forwards")]
    public Transform frontCastingAnchor;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;


    private Animator animator;
    private string currentSpellCast = "";
    private Transform currentCastingTransform;
    private PlayerMovementManager movementManager;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // We own this player: send the others our data
            stream.SendNext(Health);
        } else {
            // Network player, receive data
            this.Health = (float)stream.ReceiveNext();
        }
    }

    void Start() {
        // Follow the player character with the camera
        CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
        if (_cameraWork != null) {
            if (photonView.IsMine){
                _cameraWork.OnStartFollowing();
            }
        } else {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

        // Get animator for casting
        animator = GetComponent<Animator>();
        if (!animator) {
            Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
        }

        // Get movement manager
        movementManager = GetComponent<PlayerMovementManager>();

        if (healthBar == null) {
            healthBar = Object.FindObjectOfType(typeof(HealthBar)) as HealthBar;
        }
    }

    void Awake() {
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine) {
            PlayerManager.LocalPlayerInstance = this.gameObject;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    void Update() {
        if (photonView.IsMine) {
            this.ProcessInputs();
            if (Health <= 0f) {
                GameManager.Instance.LeaveRoom();
            }
        }
    }

    [PunRPC]
    void OnSpellCollide(float Damage, string ManaDamageType, string SpellEffectType) {
        if (!photonView.IsMine) return;
        Debug.Log("Collision with spell of type: "+ManaDamageType);
        // Spell spell = spellGO.GetComponent<Spell>();
        if (Damage != null) {
            switch (SpellEffectType) {
                case "projectile":
                    Health -= Damage;
                    break;
                default:
                    Debug.Log("Default Spell effect --> Take direct damage");
                    Health -= Damage;
                    break;
            }
        }

        healthBar.SetHealth(Health);
        Debug.Log("Current Health: "+Health);
    }

    void ProcessInputs() {
        if (Input.GetKeyDown("v")) {
            StartCastFireball();
        }
    }


    //  --------------------  SPELLS ------------------------

    void StartCastFireball() {
        currentSpellCast = "Spell_Fireball";
        currentCastingTransform = frontCastingAnchor;
        animator.SetTrigger("Cast");
        animator.SetInteger("CastType", 1);
    }

    void CastSpell() {
        if (photonView.IsMine) PhotonNetwork.Instantiate(currentSpellCast, currentCastingTransform.position, transform.rotation);
    }
}