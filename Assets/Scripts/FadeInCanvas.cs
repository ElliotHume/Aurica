using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeInCanvas : MonoBehaviour
{
    public bool fadeWithRadius = true;
    public float radius = 5f, fadeInSpeed = 1f, fadeOutSpeed = 0.75f;
    public LayerMask layerMask;

    CanvasGroup canvasGroup;
    bool fadeIn = false;

    void Start() {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    // Update is called once per frame
    void FixedUpdate() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        fadeIn = false;
        foreach(var player in players) {
            if (Vector3.Distance(player.transform.position, transform.position) < radius) {
                fadeIn = true;
                break;
            }
        }
        // Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layerMask);
        // fadeIn = hitColliders.Length > 0;
    }

    void Update() {
        if (fadeIn) {
            if (canvasGroup.alpha < 0.95f) {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, fadeInSpeed * Time.deltaTime);
            } else {
                canvasGroup.alpha = 1f;
            } 
        } else {
            if (canvasGroup.alpha > 0.05f) {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, fadeOutSpeed * Time.deltaTime);
            } else {
                canvasGroup.alpha = 0f;
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
