using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    // private Vector3 startPos;
    // void Start() {
    //     startPos = transform.position;
    // }

    // Update is called once per frame
    void Update() {
        if (Input.GetButton("Fire2")) {
            transform.position = Input.mousePosition;
        } else {
            GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        }
    }
}
