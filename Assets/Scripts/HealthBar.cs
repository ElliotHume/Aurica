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
        if (text != null) text.text = health.ToString("F0");
    }

    public void SetMaxHealth(float health) {
        slider.maxValue = health;
    }

    public void BlinkText() {
        if (blinkingText != null) blinkingText.Fire();
    }

}
