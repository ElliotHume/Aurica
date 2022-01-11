using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootCreator : MonoBehaviour {

    public float minRewardPoints = 0.001f, maxRewardPoints = 0.001f;
    public float chanceToDrop = 1f;

    public GameObject LootPrefab;

    public void DropLoot() {
        if (Random.value > chanceToDrop) return;
        float rewardPoints = maxRewardPoints == 0f ? minRewardPoints : Mathf.Round(Random.Range(minRewardPoints, maxRewardPoints) * 1000f) / 1000f;

        for (int i=0; i < Mathf.Round(rewardPoints/0.001f); i++) {
            Vector2 randomCirclePos = Random.insideUnitCircle;
            GameObject lootGO = Instantiate(LootPrefab, transform.position + (transform.forward * randomCirclePos.x) + (transform.right * randomCirclePos.y), transform.rotation);
        }
    }
}
