using UnityEngine;
using System.Linq;

public class CursorLocking : MonoBehaviour {
    public GameObject[] windowsThatUnlockCursor;

    bool AnyUnlockWindowActive()
    {
        // check manually. Linq.Any() is HEAVY(!) on GC and performance
        foreach (GameObject go in windowsThatUnlockCursor)
            if (go.activeSelf)
                return true;
        return false;
    }

    bool HoldingMButton() {
        return Input.GetButton("Fire2");
    }

    void FixedUpdate() {
        Debug.Log("GLYPH TOGGLE: "+(GameUIPanelManager.Instance != null && GameUIPanelManager.Instance.glyphDrawingToggledOn)+"   OTHER: "+(AnyUnlockWindowActive() || HoldingMButton()));
        Cursor.lockState = AnyUnlockWindowActive() || HoldingMButton() || (GameUIPanelManager.Instance != null && GameUIPanelManager.Instance.glyphDrawingToggledOn)
                           ? CursorLockMode.None
                           : CursorLockMode.Locked;

        // OSX auto hides cursor while locked, Windows doesn't so do it manually
        Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
    }
}
