using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootItem : MonoBehaviour {
    public float rewardPoints = 0f;
    public GameObject pickupEffect;

    private bool goToPlayer = false;

    // Start is called before the first frame update
    void Start() {
        // After a timeout, the loot will travel towards the player
        Invoke("GoToPlayer", 15f);
    }

    // Update is called once per frame
    void Update() {
        if (goToPlayer) {
            transform.position = Vector3.Lerp(transform.position, PlayerManager.LocalPlayerGameObject.transform.position+ new Vector3(0f, 1f, 0f), Time.deltaTime);
        }
    }

    void GoToPlayer() {
        goToPlayer = true;
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log("Collided with: "+other+"    "+(other.gameObject == PlayerManager.LocalPlayerGameObject));
        if (other.gameObject == PlayerManager.LocalPlayerGameObject && rewardPoints > 0f) {
            RewardsManager.Instance.AddRewards(rewardPoints);
            Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
