using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationText : MonoBehaviour {
    public static NotificationText Instance;

    public Text text;
    public float fadeTime = 3f;

    bool isVisible = false;
    float currentPoints = 0;

    void Start() {
        NotificationText.Instance = this;
    }

    void Update() {
        if (isVisible) {
            float alpha = text.color.a - (Time.deltaTime / fadeTime);
            if (alpha <= 0f) {
                isVisible = false;
                currentPoints = 0f;
                alpha = 0f;
            }
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
        }
    }

    public void ShowText(string newText) {
        isVisible = true;
        text.text = newText;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
    }

    public void ShowCultivationPoints(float points) {
        currentPoints += points;
        isVisible = true;
        text.text = "+"+Mathf.Round(currentPoints * 1000f)+" Cultivation points";
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
    }

    public void ShowDiscovery(string spell) {
        isVisible = true;
        text.text = "Added to Grimoire: "+spell;
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
    }
}
