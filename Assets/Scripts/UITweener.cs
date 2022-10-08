using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIAnimationTypes {
    Move,
    Scale,
    Fade
}

public class UITweener : MonoBehaviour {

    public GameObject objectToAnimate;

    public UIAnimationTypes animationType;
    public LeanTweenType easeType;
    public float duration, delay;
    public bool loop, pingpong;

    public bool startPositionOffset;
    public Vector3 from, to;

    public bool showOnEnable, disableOnComplete;

    private LTDescr _tweenObject;

    public void OnEnable() {
        if (showOnEnable) HandleTween();
    }

    public void HandleTween() {
        if (objectToAnimate == null) objectToAnimate = gameObject;

        switch (animationType) {
            case UIAnimationTypes.Fade:
                Fade();
                break;
            case UIAnimationTypes.Move:
                Move();
                break;
            case UIAnimationTypes.Scale:
                Scale();
                break;
        }

        _tweenObject.setDelay(delay);
        _tweenObject.setEase(easeType);

        if (loop) {
            _tweenObject.loopCount = int.MaxValue;
        }
        if (pingpong) {
            _tweenObject.setLoopPingPong();
        }
        if (disableOnComplete) {
            _tweenObject.setOnComplete(() => {
                objectToAnimate.SetActive(false);
            });
        }
    }

    public void Fade() {
        if (gameObject.GetComponent<CanvasGroup>() == null) {
            gameObject.AddComponent<CanvasGroup>();
        }
        if (startPositionOffset) {
            objectToAnimate.GetComponent<CanvasGroup>().alpha = from.x;
        }
        _tweenObject = LeanTween.alphaCanvas(objectToAnimate.GetComponent<CanvasGroup>(), to.x, duration);
    }

    public void Move() {
        objectToAnimate.GetComponent<RectTransform>().anchoredPosition = from;
        _tweenObject = LeanTween.move(objectToAnimate.GetComponent<RectTransform>(), to, duration);
    }

    public void Scale() {
        if (startPositionOffset) {
            objectToAnimate.GetComponent<RectTransform>().localScale = from;
        }
        _tweenObject = LeanTween.scale(objectToAnimate, to, duration);
    }
}
