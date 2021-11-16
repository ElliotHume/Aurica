using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuraCreator : MonoBehaviour
{
    public float POWER_THRESHOLD = 3.25f, FLUX = 0.1f, WEAKNESS_MULTIPLIER = 0.66f;

    public DistributionUIDisplayValues questionnaireSectionAuraValues, auraGenerationSectionValues;
    public DistributionUIDisplay auraGenerationSectionDisplay;
    public Dropdown question1, question2, question3, question4, question5, question6, question7, question8, question9, question10, question11, question12, question13;

    public TMP_InputField Structure, Essence, Fire, Water, Earth, Air, Nature, AuraText, AuraJson;
    public TMP_Text aggregatePowerText;
    private Dictionary<string, ManaDistribution> answerOffsets;
    private List<string> answers;
    private List<Dropdown> questions;

    private ManaDistribution offsets, finalOffsets, finalAura;

    // Start is called before the first frame update
    void Start() {
        answers = new List<string>();
        answerOffsets = new Dictionary<string, ManaDistribution>();
        questions = new List<Dropdown>();
        offsets = new ManaDistribution();

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
        answerOffsets.Add("1: societal status", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
        answerOffsets.Add("1: power", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.05f));
        answerOffsets.Add("1: invulnerability", new ManaDistribution(0, 0, 0, 0, 0.05f, 0, 0));
        answerOffsets.Add("1: structured progress", new ManaDistribution(0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: freedom of movement", new ManaDistribution(0, 0, 0, 0, 0, 0.05f, 0));
        answerOffsets.Add("1: unbounded choice", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("1: uncompromising willpower", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.05f));
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

        answerOffsets.Add("3: lasting perfect health", new ManaDistribution(0, 0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: complete inner peace", new ManaDistribution(0, 0, 0, 0.1f, 0, 0, 0));
        answerOffsets.Add("3: right unfairness within a local scale", new ManaDistribution(0, -0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: fully comprehend anything you can see", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: break any and all obligations for yourself and those around you, with greatly lessened consequences", new ManaDistribution(-0.1f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("3: defend anything with a physical form from all harm, within a local scale", new ManaDistribution(0, 0, 0, 0, 0.1f, 0, 0));
        answerOffsets.Add("3: to always be able to provide relevant and good guidance", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.1f));
        answerOffsets.Add("3: gain massive wealth, at the cost of having your memories fade away at the age of 80", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.1f));
        answerOffsets.Add("3: a power that grows stronger as your passion increases", new ManaDistribution(0, 0, 0.1f, 0, 0, 0, 0));
        answerOffsets.Add("3: move freely, no physical constraints can hold you", new ManaDistribution(0, 0, 0, 0, 0, 0.1f, 0));
        answerOffsets.Add("3: unlimited imagination", new ManaDistribution(-0.05f, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("4: yes", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.1f));
        answerOffsets.Add("4: no", new ManaDistribution(0, 0.15f, 0, 0, 0, 0, 0));
        answerOffsets.Add("4: maybe", new ManaDistribution(0.1f, 0, 0, 0, 0, 0, 0));

        answerOffsets.Add("5: 1", new ManaDistribution(0, 0.4f, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 2", new ManaDistribution(0, 0.25f, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 3", new ManaDistribution(0, 0.1f, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 4", new ManaDistribution(0, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 5", new ManaDistribution(0, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 6", new ManaDistribution(0, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("5: 7", new ManaDistribution(0, 0, 0, 0, 0, 0, 0));
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
        answerOffsets.Add("7: anger", new ManaDistribution(0, 0, 0.05f, 0, 0, 0, 0));
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

        answerOffsets.Add("13: elementalist - student of all natural elements (fire, water, earth, air)", new ManaDistribution(0, 0, 0.075f, 0.075f, 0.075f, 0.075f, 0));
        answerOffsets.Add("13: architect - structure mana and spells into great works of progression", new ManaDistribution(0.2f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: witch - deal in curses and blessings, with a taste for the bizarre", new ManaDistribution(-0.2f, 0, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: white mage - imbued with life mana, their healing and physical abilities are unmatched", new ManaDistribution(0, 0.2f, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: black mage - a death mage, not a necromancer, black mages deal in fairness and balance", new ManaDistribution(0, -0.2f, 0, 0, 0, 0, 0));
        answerOffsets.Add("13: infernomancer - incredible combat power with the use of expert fire and light spells", new ManaDistribution(0, 0, 0.2f, 0, 0, 0, 0));
        answerOffsets.Add("13: aquamancer - learning the way of water grants inner strength and flawless manual control", new ManaDistribution(0, 0, 0, 0.2f, 0, 0, 0));
        answerOffsets.Add("13: terramancer - no other mage could best the defensive power of an expert earth mage", new ManaDistribution(0, 0, 0, 0, 0.2f, 0, 0));
        answerOffsets.Add("13: aeromancer - masters of aeromancy can move as freely and quickly as the harshest winds", new ManaDistribution(0, 0, 0, 0, 0, 0.2f, 0));
        answerOffsets.Add("13: nephilim - divine blood allows the nephilim to grant highly powerful blessings", new ManaDistribution(0, 0, 0, 0, 0, 0, 0.2f));
        answerOffsets.Add("13: warlock - make pacts with other magical beings for massive power, at ever lowering cost", new ManaDistribution(0, 0, 0, 0, 0, 0, -0.2f));
    }

    public void RefreshAnswers() {
        answers.Clear();
        offsets = new ManaDistribution();

        string answerText;
        int index = 1;
        foreach (Dropdown question in questions) {
            if (question == null && question.gameObject.activeInHierarchy) continue;
            answerText = (""+index+": "+question.options[question.value].text).ToLower();
            try {
                // Debug.Log("ANSWER: "+answerText+"     CURRENT OFFSETS: "+offsets.ToString()+"    ADD DIST: "+answerOffsets[answerText].ToString()+"     POST-ADD DIST: "+(offsets + answerOffsets[answerText]).ToString());
                offsets = offsets + answerOffsets[answerText];
            } catch {
                Debug.LogError("ANSWER HAS NO VALID OPTION -> "+answerText);
            }
            

            answers.Add(answerText);
            index++;
        }

        Debug.Log("Final Offsets: "+offsets.ToString());
        questionnaireSectionAuraValues.SetDistribution(offsets);
    }

    public void PopulateOffsetsFromQuestionnaire() {
        finalOffsets = offsets;
        RefreshFinalOffsetTexts();
    }

    public void RefreshFinalOffsetTexts() {
        if (Structure == null) return;
        Structure.text = finalOffsets.structure.ToString();
        Essence.text = finalOffsets.essence.ToString();
        Fire.text = finalOffsets.fire.ToString();
        Water.text = finalOffsets.water.ToString();
        Earth.text = finalOffsets.earth.ToString();
        Air.text = finalOffsets.air.ToString();
        Nature.text = finalOffsets.nature.ToString();
    }

    public void SetStructure(string value) {
        finalOffsets.structure = float.Parse(value);
    }
    public void SetEssence(string value) {
        finalOffsets.essence = float.Parse(value);
    }
    public void SetFire(string value) {
        finalOffsets.fire = float.Parse(value);
    }
    public void SetWater(string value) {
        finalOffsets.water = float.Parse(value);
    }
    public void SetEarth(string value) {
        finalOffsets.earth = float.Parse(value);
    }
    public void SetAir(string value) {
        finalOffsets.air = float.Parse(value);
    }
    public void SetNature(string value) {
        finalOffsets.nature = float.Parse(value);
    }

    public void GenerateAura() {
        float structure=0f, essence=0f, fire=0f, water=0f, earth=0f, air=0f, nature=0f;
        float aggregatePower = 0f;
        bool offsetAccurate = false;

        while (aggregatePower < POWER_THRESHOLD || !offsetAccurate) {
            // Roll random values and add offsets
            structure = Mathf.Clamp( Mathf.Round( (RandomFromDistribution.RandomNormalDistribution(0f, FLUX) + finalOffsets.structure) * 100f) / 100f, -1.1f, 1.1f);
            essence = Mathf.Clamp( Mathf.Round( (RandomFromDistribution.RandomNormalDistribution(0f, FLUX) + finalOffsets.essence) * 100f) / 100f, -1.1f, 1.1f);
            fire = Mathf.Min( Mathf.Round( (Random.Range(0f, 1f) + finalOffsets.fire) * 100f) / 100f, 1.1f);
            water = Mathf.Min( Mathf.Round( (Random.Range(0f, 1f) + finalOffsets.water) * 100f) / 100f, 1.1f);
            earth = Mathf.Min( Mathf.Round( (Random.Range(0f, 1f) + finalOffsets.earth) * 100f) / 100f, 1.1f);
            air = Mathf.Min( Mathf.Round( (Random.Range(0f, 1f) + finalOffsets.air) * 100f) / 100f, 1.1f);
            nature = Mathf.Clamp( Mathf.Round( (RandomFromDistribution.RandomNormalDistribution(0f, FLUX) + finalOffsets.nature) * 100f) / 100f, -1.1f, 1.1f);

            // Create an elemental weakness by multiplying the lowest rolled element by WEAKNESS_MULTIPLIER
            // Unless this roll passes a 2.5% chance, in which case, do not create a weakness
            if (Random.Range(0f, 1f) >= 0.025f) {
                float[] manaList = new float[] {fire, water, earth, air};
                int smallestValueIndex = System.Array.IndexOf(manaList, Mathf.Min(manaList));
                float[] offsetList = new float[] {finalOffsets.fire, finalOffsets.water, finalOffsets.earth, finalOffsets.air};
                int greatestOffsetIndex = System.Array.IndexOf(offsetList, Mathf.Max(offsetList));
            
                if (smallestValueIndex != greatestOffsetIndex) {
                    if (smallestValueIndex == 0) { fire = Mathf.Max(0.1f, fire * WEAKNESS_MULTIPLIER);
                    } else if (smallestValueIndex == 1) { water = Mathf.Max(0.1f, water * WEAKNESS_MULTIPLIER);
                    } else if (smallestValueIndex == 2) { earth = Mathf.Max(0.1f, earth * WEAKNESS_MULTIPLIER);
                    } else if (smallestValueIndex == 3) { air = Mathf.Max(0.1f, air * WEAKNESS_MULTIPLIER);
                    }
                } else {
                    if (smallestValueIndex == 0) { water = Mathf.Max(0.1f, water * WEAKNESS_MULTIPLIER);
                    } else if (smallestValueIndex == 1) { fire = Mathf.Max(0.1f, fire * WEAKNESS_MULTIPLIER);
                    } else if (smallestValueIndex == 2) { air = Mathf.Max(0.1f, air * WEAKNESS_MULTIPLIER);
                    } else if (smallestValueIndex == 3) { earth = Mathf.Max(0.1f, earth * WEAKNESS_MULTIPLIER);
                    }
                }
            }

            structure = Mathf.Round(structure * 100f) / 100f;
            essence = Mathf.Round(essence * 100f) / 100f;
            fire = Mathf.Round(fire * 100f) / 100f;
            water = Mathf.Round(water * 100f) / 100f;
            earth = Mathf.Round(earth * 100f) / 100f;
            air = Mathf.Round(air * 100f) / 100f;
            nature = Mathf.Round(nature * 100f) / 100f;

            // Calculate aggregate power
            aggregatePower = Mathf.Abs(structure) + Mathf.Abs(essence) + fire + water + earth + air + Mathf.Abs(nature);

            // Ensure that the predispositions are generally followed.
            // If the predisposition for an aligned mana is for positive make sure the roll is not negative (and vice versa)
            // and that the absolute value is atleast half of the absolute predisposition offset.
            offsetAccurate = ( finalOffsets.structure * structure >= 0.0 && Mathf.Abs(structure) >= Mathf.Abs(finalOffsets.structure)/2f ) && ( finalOffsets.essence * essence >= 0.0 && Mathf.Abs(essence) >= Mathf.Abs(finalOffsets.essence)/2f) && (finalOffsets.nature * nature >= 0.0 && Mathf.Abs(nature) >= Mathf.Abs(finalOffsets.nature)/2f);
        }

        finalAura = new ManaDistribution(structure, essence, fire, water, earth, air, nature);
        Debug.Log("GENERATED AURA: ["+finalAura.ToString()+"]");

        auraGenerationSectionValues.SetDistribution(finalAura);
        if (auraGenerationSectionDisplay != null) auraGenerationSectionDisplay.SetDistribution(finalAura);

        aggregatePowerText.text = "Max Mana Pool: "+(aggregatePower * 100f).ToString();
        if (AuraText != null) AuraText.text = "["+finalAura.ToString()+"]";
        if (AuraJson != null) AuraJson.text = finalAura.GetJson();
    }

    public void GenerateAuraFromQuestionnaire() {
        RefreshAnswers();
        finalOffsets = offsets;
        GenerateAura();
    }

    public void GenerateAuraRandomly() {
        finalOffsets = new ManaDistribution();
        GenerateAura();
    }

    public ManaDistribution GetFinalAura() {
        return finalAura;
    }

    public ManaDistribution GetRandomAura() {
        finalOffsets = new ManaDistribution();
        GenerateAura();
        return finalAura;
    }
}
