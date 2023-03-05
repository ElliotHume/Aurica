using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public BlinkingText blinkingText;
    public Text text;

    public void SetHealth(float health) {
        slider.value = health;
        if (text != null) text.text = health.ToString(health >= 10 ? "F0" : "F2");
    }

    public void LerpTowards(float health) {
        slider.value = Mathf.Lerp(slider.value, health, Time.deltaTime * 2f);
        if (text != null) text.text = health.ToString(health >= 10 ? "F0" : "F2");
    }

    public void SetMaxHealth(float health) {
        slider.maxValue = health;
    }

    public void BlinkText() {
        if (blinkingText != null) blinkingText.Fire();
    }

    public void SetFillColor(Color c) {
        Transform fill = transform.Find("Fill");
        if (fill != null) {
            Image fillImg = fill.gameObject.GetComponent<Image>();
            if (fillImg != null) fillImg.color = c;
        }
    }
}
