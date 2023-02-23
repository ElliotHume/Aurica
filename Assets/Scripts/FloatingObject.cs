using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    Vector3 resetPosition, rotationAngles;

    public bool bobs = true, rotates = true;
    public Vector3 spinVector = new Vector3(0.1f, 0.1f, 0.1f);
    public float bobbingSpeed, bobbingHeight;
    public bool delay = false;
    public float delayMinimum=1f, delayMaximum=5f;

    private bool shouldBob, shouldRotate, coinflip;

    void Start()
    {
        resetPosition = transform.position;
        rotationAngles = new Vector3(Random.Range(0f, spinVector.x), Random.Range(0f, spinVector.y), Random.Range(0f, spinVector.z));
        shouldBob = bobs;
        shouldRotate = rotates;
        if (delay) {
            StopMoving();
            Invoke("StartMoving", Random.Range(delayMinimum, delayMaximum));
        }
        coinflip = Random.Range(0f, 1f) >= 0.5f;
    }

    void FixedUpdate() {
        if (shouldRotate) transform.Rotate(rotationAngles);
        if (shouldBob){
            //calculate what the new Y position will be
            float newY = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            if (coinflip) newY = -newY;
            //set the object's Y to the new calculated Y
            transform.position += new Vector3(0, newY, 0) * Time.deltaTime * 0.5f;
        }
    }

    public void StartMoving() {
        if (bobs) shouldBob = true;
        if (rotates) shouldRotate = true;
    }

    public void StopMoving() {
        if (bobs) shouldBob = false;
        if (rotates) shouldRotate = false;
    }
}