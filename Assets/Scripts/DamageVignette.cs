using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance;

    public Image image;

    // Start is called before the first frame update
    void Start() {
        DamageVignette.Instance = this;
        if (image == null) image = GetComponent<Image>();
    }

    void FixedUpdate() {
        if (image.color.a > 0) {
            image.color = new Color(1f, 0, 0, image.color.a < 0.0005f ? 0f : image.color.a/1.1f);
        }
    }

    public void FlashDamage(float damage) {
        image.color = new Color(1f, 0, 0, damage/5f);
    }
}
