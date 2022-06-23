using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TipWindow : MonoBehaviour {
    public static TipWindow Instance;

    public GameObject TipPanel;
    public Text title, text;
    public float closeDelay = 8f;
    public UnityEvent OnClose;

    Coroutine hideRoutine;
    private float setCloseDelay = 0f;

    void Start() {
        TipWindow.Instance = this;
        TipPanel.SetActive(false);
    }

    public void ShowTip(string newTitle, string newText, float close = 0f) {
        TipPanel.SetActive(true);
        title.text = newTitle;
        text.text = newText;

        // set close time to -1 to not close
        if (close >= 0f) {
            setCloseDelay = close;
            if (hideRoutine == null) {
                hideRoutine = StartCoroutine(HideTip());
            } else {
                StopCoroutine(hideRoutine);
                hideRoutine = StartCoroutine(HideTip());
            }
        }
    }

    IEnumerator HideTip() {
        float timer = setCloseDelay > 0f ? setCloseDelay : closeDelay;
        yield return new WaitForSeconds(timer);
        OnClose.Invoke();
        hideRoutine = null;
    }

    public void ShowTutorial(string text) {
        ShowTip("Tutorial", text, -1f);
    }

    public void ShowTipText(string text) {
        ShowTip("Tip", text, closeDelay);
    }

    public void ShowShortTipText(string text) {
        ShowTip("Tip", text, 3f);
    }
}
