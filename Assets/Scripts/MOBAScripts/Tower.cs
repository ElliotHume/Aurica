using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class Tower : Structure, IPunObservable {

    [Tooltip("Radius in which the turret will fire at players")]
    [SerializeField]
    private float FiringRadius = 6f;

    [Tooltip("Offset for where the firing radius' center is")]
    [SerializeField]
    private Vector3 RadiusOffset = Vector3.up;

    [Tooltip("Transform anchor for where to shoot attacks from")]
    [SerializeField]
    private Transform FiringAnchor;

    [Tooltip("Objects to disable when the tower explodes")]
    [SerializeField]
    private List<GameObject> DisableObjectsOnExplode;

    [Tooltip("Objects to enable when the tower explodes")]
    [SerializeField]
    private List<GameObject> EnableObjectsOnExplode;

    [Tooltip("Effects to play when the tower is restored")]
    [SerializeField]
    private List<ParticleSystem> RestorationParticles;


    private List<GameObject> playersInRadius, firingAtPlayers;
    private Dictionary<GameObject, TowerAttack> activeAttacks;

    // Start is called before the first frame update
    void Start() {
        Health = StartingHealth;

        if (StructureUIPrefab != null) {
            UIDisplayGO = Instantiate(StructureUIPrefab, UIDisplayAnchor.position, UIDisplayAnchor.rotation, transform);
            UIDisplay = UIDisplayGO.GetComponent<StructureUIDisplay>();
            UIDisplay.SetStructure(this);
        }

        playersInRadius = new List<GameObject>();
        firingAtPlayers = new List<GameObject>();
        activeAttacks = new Dictionary<GameObject, TowerAttack>();
    }

    void FixedUpdate() {
        // If the structure has been broken, nothing more needs to be done to it.
        if (broken) return;

        // Check if null sphere is in range, disable attacks if it is
        disabled = IsNullSphereInRadius(transform, FiringRadius, RadiusOffset);

        // Play the null sphere disabled particles if we are not already doing so
        if (disabled && !playingDisableParticles) {
            foreach(GameObject obj in ToggleObjectsOnDisable){ obj.SetActive(!obj.activeInHierarchy); }
            foreach(ParticleSystem effect in DisabledParticles){ effect.Play(); }
            playingDisableParticles = true;
        } else if (!disabled && playingDisableParticles) {
            foreach(GameObject obj in ToggleObjectsOnDisable){ obj.SetActive(!obj.activeInHierarchy); }
            foreach(ParticleSystem effect in DisabledParticles){ effect.Stop(); }
            playingDisableParticles = false;
        }

        if (photonView.IsMine) {
            // Regen health if the structure is not broken or disabled
            if (!broken && !disabled && HealthRegenPerSecond > 0f){
                healthRegenTimer += Time.deltaTime;
                if (healthRegenTimer >= DelayBeforeHealhRegen && Health < StartingHealth) {
                    Health += HealthRegenPerSecond * Time.deltaTime;
                }
                if (Health > StartingHealth) {
                    Health = StartingHealth;
                }
            }

            // Get players inside the firing radius
            playersInRadius = GetPlayersInRadius(transform, FiringRadius, RadiusOffset);

            

            if (!disabled) {
                // For each player in the radius, if we dont already have an attack being fired at them, create the attack and set them as the target.
                foreach(GameObject playerGO in playersInRadius) {
                    MOBAPlayer mp = playerGO.GetComponent<MOBAPlayer>();
                    PlayerManager pm = playerGO.GetComponent<PlayerManager>();
                    // Do not attack players on the allied team
                    if (mp != null && mp.Side == Team) continue;
                    // Do not attack dead players
                    if (pm != null && pm.dead) continue;

                    if (!firingAtPlayers.Contains(playerGO)) {
                        firingAtPlayers.Add(playerGO);
                        GameObject newAttack = PhotonNetwork.Instantiate("ZZZTowerAttack", FiringAnchor.position, FiringAnchor.rotation);
                        TowerAttack towerAttack = newAttack.GetComponent<TowerAttack>();
                        if (towerAttack != null) {
                            towerAttack.SetStructure(this);
                            towerAttack.SetTarget(playerGO);
                        }
                        activeAttacks.Add(playerGO, towerAttack);
                    }
                }
            }
            

            // For each player we are firing an attack at, check if they are still eligible to be attacked
            // Players are no longer eligible if they are outside of the radius, if they are dead, or if the structure is disabled
            foreach(GameObject playerGO in firingAtPlayers) {
                PlayerManager pm = playerGO.GetComponent<PlayerManager>();
                if (!playersInRadius.Contains(playerGO) || pm.dead || disabled) {
                    TowerAttack attackToDestroy = activeAttacks[playerGO];
                    activeAttacks.Remove(playerGO);
                    PhotonNetwork.Destroy(attackToDestroy.gameObject);
                    firingAtPlayers.Remove(playerGO);
                    break;
                }
            }
        }
    }

    protected override void NetworkExplode() {
        if (!photonView.IsMine) return;
        Debug.Log("Tower NetworkExplode");
        if (photonView.IsMine) {
            // Destroy any attacks left when the tower breaks
            foreach(GameObject playerGO in firingAtPlayers) {
                PhotonNetwork.Destroy(activeAttacks[playerGO].gameObject);
            }
        }
        //TODO: Call MOBAMatchManager to do something when the tower explodes
        MOBAMatchManager.Instance.NetworkMasterTowerBroken(Team);
    }

    protected override void LocalEffectExplode() {
        broken = true;
        Debug.Log("Tower LocalEffectExplode");
        foreach(GameObject obj in EnableObjectsOnExplode) obj.SetActive(true);
        foreach(GameObject obj in DisableObjectsOnExplode) obj.SetActive(false);
        
        if (UIDisplay != null) UIDisplay.Hide();

        Collider[] colliders = GetComponents<Collider>();
        foreach(Collider collider in colliders) collider.enabled = false;
    }

    public override void Restore() {
        if (photonView.IsMine) Health = StartingHealth;
        broken = false;
        Debug.Log("Tower Restore");

        firingAtPlayers.Clear();
        playersInRadius.Clear();
        activeAttacks.Clear();

        foreach(GameObject obj in DisableObjectsOnExplode) obj.SetActive(true);
        foreach(GameObject obj in EnableObjectsOnExplode) obj.SetActive(false);
        foreach(ParticleSystem ps in RestorationParticles) ps.Play();

        if (UIDisplay != null) UIDisplay.Show();

        Collider[] colliders = GetComponents<Collider>();
        foreach(Collider collider in colliders) collider.enabled = true;
    }

    public override string GetName() {
        return Team.ToString()+" Tower";
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position+RadiusOffset, FiringRadius);
    }
}
