using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BlinkingText : MonoBehaviour
{
    public int numberOfFlashes = 3;
    public float timeBetweenFlashes = 1f;

    TextMeshProUGUI text;


    public void Fire() {
        text = GetComponent<TextMeshProUGUI>();
        StopAllCoroutines();
        StartCoroutine(Blink());
    }

    IEnumerator Blink() {
        for(int j = 0; j < numberOfFlashes; j++) {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            while (text.color.a < 1.0f)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime * timeBetweenFlashes));
                yield return null;
            }
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
            while (text.color.a > 0.0f)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime * timeBetweenFlashes));
                yield return null;
            }
        }
    }
}
