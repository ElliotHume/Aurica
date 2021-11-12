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

        answerOffsets.Add("1: inner peace", new ManaDistribution(0, 0, 0, 0.05f, 0, 0, 0));
        answerOffsets.Add("1: status among peers", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("1: power above all others", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.05f));
        answerOffsets.Add("1: invulnerability", new ManaDistribution(0, 0, 0, 0, 0.05f, 0, 0));
        answerOffsets.Add("1: structured progress", new ManaDistribution(0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: freedom of movement", new ManaDistribution(0, 0, 0, 0, 0, 0.05f, 0));
        answerOffsets.Add("1: unbounded choice", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: passion manifested", new ManaDistribution(0, 0, 0.05f, 0, 0, 0, 0));
        answerOffsets.Add("1: ingenuity", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: fairness and balance", new ManaDistribution(0, -0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: growth and understanding", new ManaDistribution(0, 0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: nothing", new ManaDistribution(-0.15f, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("2: creativity", new ManaDistribution(-0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("2: intelligence", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("2: self-worth", new ManaDistribution(0, 0, 0.05f, 0.05f, 0, 0, 0));
        answerOffsets.Add("2: bravery", new ManaDistribution(0, 0, 0.1f, 0, 0, 0, 0));
        answerOffsets.Add("2: fairness", new ManaDistribution(0, -0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("2: compassion", new ManaDistribution(0, 0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("2: knowledge", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.1f));
        answerOffsets.Add("2: self-control", new ManaDistribution(0, 0, 0, 0.05f, 0.05f, 0, 0));
        answerOffsets.Add("2: open-mindedness", new ManaDistribution(-0.05f, 0, 0, 0.05f, 0, 0, 0));
        answerOffsets.Add("2: dominance", new ManaDistribution(0, 0, 0.05f, 0, 0, 0, 0.05f));
        answerOffsets.Add("2: problem-solving", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("2: art of trade", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.1f));
        answerOffsets.Add("2: physical skill", new ManaDistribution(0, 0, 0, 0, 0.05f, 0.05f, 0));
        answerOffsets.Add("2: skill expertise", new ManaDistribution(0, 0.05f, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("2: mindfulness", new ManaDistribution(0, 0, 0, 0.1f, 0, 0, 0));
        answerOffsets.Add("2: awareness", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.1f));

        answerOffsets.Add("3: lasting health", new ManaDistribution(0, 0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: inner peace", new ManaDistribution(0, 0, 0, 0.1f, 0, 0, 0));
        answerOffsets.Add("3: right unfairness", new ManaDistribution(0, -0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: full comprehension", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: break obligations", new ManaDistribution(-0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: defend things", new ManaDistribution(0, 0, 0, 0, 0.1f, 0, 0));
        answerOffsets.Add("3: provide guidance", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.1f));
        answerOffsets.Add("3: gain wealth at a cost", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.1f));
        answerOffsets.Add("3: passion power", new ManaDistribution(0, 0, 0.1f, 0, 0, 0, 0));
        answerOffsets.Add("3: move freely", new ManaDistribution(0, 0, 0, 0, 0, 0.1f, 0));
        answerOffsets.Add("3: unlimited imagination", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("4: yes", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.1f));
        answerOffsets.Add("4: no", new ManaDistribution(0, 0.15f, 0, 0, 0, 0, 0));
        answerOffsets.Add("4: maybe", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("5: 1", new ManaDistribution(0, 0.4f, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 2", new ManaDistribution(0, 0.25f, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 3", new ManaDistribution(0, 0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 4-7", new ManaDistribution(0, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 8", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.1f));
        answerOffsets.Add("5: 9", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.25f));
        answerOffsets.Add("5: 10", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.4f));

        answerOffsets.Add("6: mind affecting mind", new ManaDistribution(0, 0, 0, 0.2f, 0, 0, 0));
        answerOffsets.Add("6: body affecting mind", new ManaDistribution(0, 0, 0, 0, 0, 0.1f, 0));
        answerOffsets.Add("6: mind affecting body", new ManaDistribution(0, 0, 0.1f, 0, 0, 0, 0));
        answerOffsets.Add("6: body affecting body", new ManaDistribution(0, 0, 0, 0, 0.2f, 0, 0));
        answerOffsets.Add("6: mind affecting mind & body affecting mind", new ManaDistribution(0, 0, 0, 0.2f, 0, 0.1f, 0));
        answerOffsets.Add("6: mind affecting mind & mind affecting body", new ManaDistribution(0, 0, 0.1f, 0.2f, 0, 0, 0));
        answerOffsets.Add("6: mind affecting mind & body affecting body", new ManaDistribution(0, 0, 0, 0.2f, 0.2f, 0, 0));
        answerOffsets.Add("6: body affecting mind & mind affecting body", new ManaDistribution(0, 0, 0.1f, 0, 0, 0.1f, 0));
        answerOffsets.Add("6: body affecting mind & body affecting body", new ManaDistribution(0, 0, 0, 0, 0.2f, 0.1f, 0));
        answerOffsets.Add("6: mind affecting body & body affecting body", new ManaDistribution(0, 0, 0.1f, 0, 0.2f, 0, 0));
        answerOffsets.Add("6: mind affecting mind & body affecting mind & mind affecting body", new ManaDistribution(0, 0, 0.1f, 0.2f, 0, 0.1f, 0));
        answerOffsets.Add("6: mind affecting mind & body affecting mind & body affecting body", new ManaDistribution(0, 0, 0, 0.2f, 0.2f, 0.1f, 0));
        answerOffsets.Add("6: mind affecting mind & mind affecting body & body affecting body", new ManaDistribution(0, 0, 0.1f, 0.2f, 0.2f, 0, 0));
        answerOffsets.Add("6: body affecting mind & mind affecting body & body affecting body", new ManaDistribution(0, 0, 0.1f, 0, 0.2f, 0.1f, 0));
        answerOffsets.Add("6: all of them", new ManaDistribution(0, 0, 0.1f, 0.2f, 0.2f, 0.1f, 0));
        answerOffsets.Add("6: none", new ManaDistribution(0, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("7: sadness", new ManaDistribution(0, 0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("7: fright", new ManaDistribution(0, 0, 0, 0, 0.05f, 0, 0));
        answerOffsets.Add("7: anger", new ManaDistribution(0, 0, 0, 0.05f, 0, 0, 0));
        answerOffsets.Add("7: curiosity", new ManaDistribution(0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("7: confusion", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.05f));
        answerOffsets.Add("7: surprise", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("7: arousal", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.3f));

        answerOffsets.Add("8: dark", new ManaDistribution(0, 0, 0.1f, 0, 0, 0, 0));
        answerOffsets.Add("8: being alone", new ManaDistribution(0, 0.15f, 0, 0, 0, 0, 0));
        answerOffsets.Add("8: pain", new ManaDistribution(0, 0, 0, 0, 0.1f, 0, 0));
        answerOffsets.Add("8: disease", new ManaDistribution(0, 0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("8: heights", new ManaDistribution(0, 0, 0, 0, 0, 0.05f, 0));
        answerOffsets.Add("8: deep water", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.1f));
        answerOffsets.Add("8: confinement", new ManaDistribution(0, 0, 0, 0, 0, 0.1f, 0));
        answerOffsets.Add("8: wild animals", new ManaDistribution(0, -0.2f, 0, 0, 0, 0, 0));
        answerOffsets.Add("8: blood", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("8: natural disasters", new ManaDistribution(0.2f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("8: mankind", new ManaDistribution(-0.2f, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("9: yes", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.05f));
        answerOffsets.Add("9: no", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("10: fear", new ManaDistribution(0, 0, 0.05f, 0, 0, 0, 0));
        answerOffsets.Add("10: naivety", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("10: foolishness", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("10: overconfidence", new ManaDistribution(0, 0, 0, 0.05f, 0, 0, 0));
        answerOffsets.Add("10: cowardice", new ManaDistribution(0, 0, 0.05f, 0, 0, 0, 0));
        answerOffsets.Add("10: selfishness", new ManaDistribution(0, 0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("10: selflessness", new ManaDistribution(0, -0.05f, 0, 0, 0, 0, 0));
        answerOffsets.Add("10: impatience", new ManaDistribution(0, 0, 0, 0.05f, 0, 0, 0));
        answerOffsets.Add("10: laziness", new ManaDistribution(0, 0, 0, 0, 0, 0.05f, 0));
        answerOffsets.Add("10: lust", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("10: greed", new ManaDistribution(0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("10: close-mindedness", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("10: pridefulness", new ManaDistribution(0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("10: jealousy", new ManaDistribution(0, 0.05f, 0, 0, 0, 0, 0));

        answerOffsets.Add("11: 1-3", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("11: 4-6", new ManaDistribution(0, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("11: 7-8", new ManaDistribution(-0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("11: 9-10", new ManaDistribution(0, 0, 0, 0.1f, 0, 0, 0));

        answerOffsets.Add("12: order", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("12: chaos", new ManaDistribution(-0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("12: life", new ManaDistribution(0, 0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("12: death", new ManaDistribution(0, -0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("12: fire", new ManaDistribution(0, 0, 0.1f, 0, 0, 0, 0));
        answerOffsets.Add("12: water", new ManaDistribution(0, 0, 0, 0.1f, 0, 0, 0));
        answerOffsets.Add("12: earth", new ManaDistribution(0, 0, 0, 0, 0.1f, 0, 0));
        answerOffsets.Add("12: air", new ManaDistribution(0, 0, 0, 0, 0, 0.1f, 0));
        answerOffsets.Add("12: divine", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.1f));
        answerOffsets.Add("12: demonic", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.1f));

        answerOffsets.Add("13: wizard", new ManaDistribution(0, 0, 0, 0, 0, 0, 0)); // Special case
        answerOffsets.Add("13: sorcerer", new ManaDistribution(0, 0, 0, 0, 0, 0, 0)); // Special case
        answerOffsets.Add("13: elementalist", new ManaDistribution(0, 0, 0.075f, 0.075f, 0.075f, 0.075f, 0));
        answerOffsets.Add("13: architect", new ManaDistribution(0.2f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: witch", new ManaDistribution(-0.2f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: white mage", new ManaDistribution(0, 0.2f, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: black mage", new ManaDistribution(0, -0.2f, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: infernomancer", new ManaDistribution(0, 0, 0.2f, 0, 0, 0, 0));
        answerOffsets.Add("13: aquamancer", new ManaDistribution(0, 0, 0, 0.2f, 0, 0, 0));
        answerOffsets.Add("13: terramancer", new ManaDistribution(0, 0, 0, 0, 0.2f, 0, 0));
        answerOffsets.Add("13: aeromancer", new ManaDistribution(0, 0, 0, 0, 0, 0.2f, 0));
        answerOffsets.Add("13: nephilim", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.2f));
        answerOffsets.Add("13: warlock", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.2f));
    }

    public void RefreshAnswers() {
        answers.Clear();
        finalOffsets = new ManaDistribution();

        string answerText;
        int index = 1;
        foreach (Dropdown question in questions) {
            if (question == null) continue;
            answerText = (""+index+": "+question.options[question.value].text).ToLower();
            finalOffsets += answerOffsets[answerText];

            // Special cases
            switch (answerText) {
                case "13: wizard":
                    if (finalOffsets.nature > 0) {
                        finalOffsets -= new ManaDistribution(0,0,0,0,0,0, Mathf.Max(0.2f, finalOffsets.nature));
                    } else if (finalOffsets.nature < 0) {
                        finalOffsets += new ManaDistribution(0,0,0,0,0,0, Mathf.Max(0.2f, -finalOffsets.nature));
                    }
                    break;
            }

            answers.Add(answerText);
            index++;
        }

        Debug.Log("Final Offsets: "+finalOffsets.ToString());
        auraValues.SetDistribution(finalOffsets);
    }
}
