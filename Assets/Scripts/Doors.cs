using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    public GameObject leftDoor, rightDoor;
    public float leftDoorClosedRotation = 0f, rightDoorClosedRotation = 0f, leftDoorOpenRotation = -90f, rightDoorOpenRotation = 90f;
    public float openDuration = 1f;
    public bool opened = false;
    public AudioSource openSound, closeSound;

    bool moving = false;
    Quaternion lClosedRotation, rClosedRotation, lOpenRotation, rOpenRotation;

    void Start() {
        lClosedRotation = Quaternion.Euler(0, leftDoorClosedRotation, 0);
        rClosedRotation = Quaternion.Euler(0, rightDoorClosedRotation, 0);
        lOpenRotation = Quaternion.Euler(0, leftDoorOpenRotation, 0);
        rOpenRotation = Quaternion.Euler(0, rightDoorOpenRotation, 0);
    }

    public void Open() {
        if (!opened && !moving) StartCoroutine(OpenDoors());
    }

    public void Close() {
        if (opened && !moving) StartCoroutine(CloseDoors());
    }

    public void Toggle() {
        // Calling both will toggle the state, as only one function will run
        Open();
        Close();
    }

    IEnumerator OpenDoors() {
        moving = true;
        var t = 0f;
        if (openSound != null) openSound.Play();
        while(t < 1f){
            t += Time.deltaTime / openDuration;
            leftDoor.transform.localRotation = Quaternion.Lerp(leftDoor.transform.localRotation, lOpenRotation, t);
            rightDoor.transform.localRotation = Quaternion.Lerp(rightDoor.transform.localRotation, rOpenRotation, t);
            yield return new WaitForFixedUpdate();
        }
        opened = true;
        moving = false;
    }

    IEnumerator CloseDoors(){
        Debug.Log("Closing");
        moving = true;
        var t = 0f;
        if (closeSound != null) closeSound.Play();
        while(t < 1f){
            t += Time.deltaTime / openDuration;
            leftDoor.transform.localRotation = Quaternion.Lerp(leftDoor.transform.localRotation, lClosedRotation, t);
            rightDoor.transform.localRotation = Quaternion.Lerp(rightDoor.transform.localRotation, rClosedRotation, t);
            yield return new WaitForFixedUpdate();
        }
        opened = false;
        moving = false;
    }

}