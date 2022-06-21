using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingObject : MonoBehaviour {

    public float radius=1f, speed=1f;

    private float timer;
    private Vector3 centerPosition;

    // Start is called before the first frame update
    void Start() {
        centerPosition = transform.position;
    }

    // Update is called once per frame
    void Update() {
        timer += Time.deltaTime * speed;
        float x = -Mathf.Cos(timer) * radius;
        float z = Mathf.Sin(timer) * radius;
        Vector3 pos = new Vector3(x, 0, z);
        transform.position = pos + centerPosition;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
