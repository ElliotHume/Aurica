using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance;
    // private Vector3 startPos;
    void Start() {
        Instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButton("Fire2")) {
            transform.position = Input.mousePosition;
        }
        // else {
        //     GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        // }
    }

    public Vector3 GetWorldPoint() {
        Ray ray = Camera.main.ScreenPointToRay( transform.position );
        RaycastHit hit;
        if( Physics.Raycast( ray, out hit, 1000f ) ) {
            // Debug.Log("Point hit: "+hit.point);
            return hit.point;
        }

        return transform.forward * 100f;
    }
}
