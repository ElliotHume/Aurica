using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public Text titleText, contentText;
    public bool TypeSentencesSlowly = false, TurnCanvasToPlayer = true;
    private Queue<string> sentences;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    void Update() {
        if (TurnCanvasToPlayer) {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null && Camera.main != null) {
                gameObject.transform.rotation = Camera.main.transform.rotation;
            }
        }
    }

    public void StartDialogue(Dialogue dialogue) {
        titleText.text = dialogue.title;

        sentences.Clear();

        foreach(string sentence in dialogue.sentences) {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence() {
        if (sentences.Count == 0) {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        if (TypeSentencesSlowly) {
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence));
        } else {
            contentText.text = sentence;
        }
    }

    IEnumerator TypeSentence( string sentence ) {
        contentText.text = "";
        foreach( char letter in sentence.ToCharArray()) {
            contentText.text += letter;
            yield return new WaitForFixedUpdate();
        }
    }

    public void EndDialogue() {
        Debug.Log("end of conversation");
    }
}
