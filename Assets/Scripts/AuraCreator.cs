using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuraCreator : MonoBehaviour
{
    public DistributionUIDisplayValues auraValues;
    public Dropdown question1, question2, question3, question4, question5, question6, question7, question8, question9, question10, question11, question12, question13;
    private Dictionary<string, ManaDistribution> answerOffsets;
    private List<string> answers;
    private List<Dropdown> questions;

    private ManaDistribution finalOffsets;

    // Start is called before the first frame update
    void Start() {
        answers = new List<string>();
        answerOffsets = new Dictionary<string, ManaDistribution>();
        questions = new List<Dropdown>();
        finalOffsets = new ManaDistribution();

        questions.Add(question1);
        questions.Add(question2);
        questions.Add(question3);
        questions.Add(question4);
        questions.Add(question5);
        questions.Add(question6);
        questions.Add(question7);
        questions.Add(question8);
        questions.Add(question9);
        questions.Add(question10);
        questions.Add(question11);
        questions.Add(question12);
        questions.Add(question13);

        answerOffsets.Add("1: Inner peace", new ManaDistribution(0, 0, 0, 0.05f, 0, 0, 0));
        answerOffsets.Add("1: Status among peers", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("1: Power above all others", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.05f));
        answerOffsets.Add("1: Invulnerability", new ManaDistribution(0, 0, 0, 0, 0.05f, 0, 0));
        answerOffsets.Add("1: Structured progress", new ManaDistribution(0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: Freedom of movement", new ManaDistribution(0, 0, 0, 0, 0, 0.05f, 0));
        answerOffsets.Add("1: Unbounded choice", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: Passion manifested", new ManaDistribution(0, 0, 0.05f, 0, 0, 0, 0));
        answerOffsets.Add("1: Ingenuity", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: Fairness and balance", new ManaDistribution(0, -0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: Growth and understanding", new ManaDistribution(0, 0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: Nothing", new ManaDistribution(-0.15f, 0, 0, 0, 0, 0, 0));
    }

    public void RefreshAnswers() {
        answers.Clear();
        finalOffsets = new ManaDistribution();

        string answerText;
        int index = 1;
        foreach (Dropdown question in questions) {
            if (question == null) continue;
            answerText = ""+index+": "+question.options[question.value].text;
            finalOffsets += answerOffsets[answerText];
            answers.Add(answerText);
            index++;
        }

        Debug.Log("Final Offsets: "+finalOffsets.ToString());
        auraValues.SetDistribution(finalOffsets);
    }
}
