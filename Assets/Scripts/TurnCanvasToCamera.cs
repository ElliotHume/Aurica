using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCanvasToCamera : MonoBehaviour {
    public Canvas canvas;

    void Start() {
        Canvas canvas = GetComponent<Canvas>();
    }

    void Update() {
        if (canvas != null && Camera.main != null) {
            gameObject.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
