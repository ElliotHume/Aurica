using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class DamagePopup : MonoBehaviourPun, IPunObservable
{
    public TextMeshProUGUI text;

    public float AntiGravity = 0.05f;
    public float FadeDelay = 2f, FadeMultiplier = 1f;

    [HideInInspector]
    public bool isSceneObject = false;

    Transform cam;
    Vector3 velocity = Vector3.zero, startScale = Vector3.one;
    bool drift = false, damageSet = false, resized = false, shrunk = false;
    float damage;
    string displayText;


    bool startAccumulatingDamage=false, endAccumulatingDamage=false;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(damage);
            stream.SendNext(displayText);
            stream.SendNext(startAccumulatingDamage);
            stream.SendNext(endAccumulatingDamage);
        } else {
            this.damage = (float)stream.ReceiveNext();
            this.displayText = (string)stream.ReceiveNext();
            this.startAccumulatingDamage = (bool)stream.ReceiveNext();
            this.endAccumulatingDamage = (bool)stream.ReceiveNext();
        }
    }

    void Start() {
        cam = Camera.main.transform;
    }

    void Update() {
        if (!photonView.IsMine) {
            if (displayText != null) {
                text.text = displayText;
                damageSet = true;
            }
            if (startAccumulatingDamage) {
                text.text = damage.ToString("F2");
                damageSet = true;
            }
            if (!damageSet && damage != 0f) {
                text.text = damage.ToString("F2");
                damageSet = true;
                StartCoroutine(FadeOut());
            }
            if (endAccumulatingDamage) {
                StartCoroutine(FadeOut());
            }
        } else {
            if (!shrunk && !isSceneObject) {
                startScale = startScale * 0.5f;
                transform.localScale = startScale;
                shrunk = true;
            } else if (shrunk && isSceneObject) {
                startScale = startScale * 2f;
                transform.localScale = startScale;
                shrunk = false;
            }
        }

        // Scale up if the camera is far away
        float distToCamera = Vector3.Distance(cam.position, transform.position);
        if (distToCamera >= 10f) {
            float scale = 1f + distToCamera / 50f;
            transform.localScale = startScale*scale;
            resized = true;
        } else if (resized) {
            transform.localScale = startScale;
            resized = false;
        }


        if (!drift || !photonView.IsMine) return;
        velocity += Physics.gravity * Time.deltaTime;
        transform.position -= velocity * AntiGravity * Time.deltaTime;
    }

    public void AttachTo(GameObject parentGO) {
        transform.SetParent(parentGO.transform);
        startScale = transform.localScale;
    }

    void LateUpdate() {
        // Always face the camera
        transform.LookAt(transform.position + cam.forward);
    }

    IEnumerator FadeOut() {
        yield return new WaitForSeconds(FadeDelay);
        while (text.color.a > 0.0f) {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime * FadeMultiplier));
            yield return new WaitForFixedUpdate();
        }

        if (photonView.IsMine) {
            yield return new WaitForSeconds(2f);
            PhotonNetwork.Destroy(gameObject);
        }
    }


    public void ShowDamage(float dmg) {
        damage = dmg;

        // Update text
        text.text = damage.ToString("F2");
        damageSet = true;

        // Unparent this object, we want it to appear in world space.
        gameObject.transform.SetParent(null);

        // Start drifting upwards
        drift = true;


        // Start the fade out after delay
        StartCoroutine(FadeOut());
    }

    public void ShowText(string newText) {
        damage = 0f;
        displayText = newText;

        // Update text
        text.text = newText;
        damageSet = true;

        // Unparent this object, we want it to appear in world space.
        gameObject.transform.SetParent(null);

        // Start drifting upwards
        drift = true;

        // Start the fade out after delay
        StartCoroutine(FadeOut());
    }

    public void AccumulatingDamagePopup(float dmg) {
        damageSet = true;
        damage = dmg;

        // Update text
        text.text = damage.ToString("F2");
        startAccumulatingDamage = true;
    }

    public void EndAccumulatingDamagePopup() {
        endAccumulatingDamage = true;

        // Start drifting upwards
        drift = true;

        // Unparent the object, we want it to appear in world space when it stops
        Vector3 temp = transform.position;
        transform.SetParent(null);
        transform.localScale = photonView.IsMine ? Vector3.one * ( isSceneObject ? 2f : 0.5f) : Vector3.one;
        startScale = photonView.IsMine ? Vector3.one * ( isSceneObject ? 2f : 0.5f) : Vector3.one;
        transform.position = temp;

        // Start the fade out after delay
        StartCoroutine(FadeOut());
    }
}
