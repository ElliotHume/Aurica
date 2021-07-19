using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AuricaSpellComponent))]
[CanEditMultipleObjects]
public class AuricaSpellComponentEditor : Editor {
    AuricaSpellComponent component;
    bool importstr;
    string importString;

    void OnEnable() {
        component = (AuricaSpellComponent)target;
        // if (component.basicDistribution.Count == 0) component.basicDistribution = new List<float>(new float[]{0f, 0f, 0f, 0f, 0f, 0f, 0f});
    }

    public override void OnInspectorGUI() {
        component.c_name = EditorGUILayout.TextField("Name", component.c_name);

        component.category = (AuricaSpellComponent.Category)EditorGUILayout.EnumPopup("Category:", component.category);

        component.classification = (AuricaSpellComponent.Classification)EditorGUILayout.EnumPopup("Classification:", component.classification);

        if (component.category == AuricaSpellComponent.Category.ManaType) {
            component.classification = AuricaSpellComponent.Classification.ManaType;
        } else if (component.category == AuricaSpellComponent.Category.WildGlyph) {
            component.classification = AuricaSpellComponent.Classification.Wild;
        } else if (component.category == AuricaSpellComponent.Category.ManaSiphon) {
            component.classification = AuricaSpellComponent.Classification.Siphon;
        }

        if (component.classification == AuricaSpellComponent.Classification.Attack
            || component.classification == AuricaSpellComponent.Classification.Defend
            || component.classification == AuricaSpellComponent.Classification.Support
            || component.classification == AuricaSpellComponent.Classification.Summon
        ) {
            component.category = AuricaSpellComponent.Category.SpellBasis;

        } else if (component.classification == AuricaSpellComponent.Classification.Dart
            || component.classification == AuricaSpellComponent.Classification.Sphere
            || component.classification == AuricaSpellComponent.Classification.Wall
            || component.classification == AuricaSpellComponent.Classification.Aura
            || component.classification == AuricaSpellComponent.Classification.Area
            || component.classification == AuricaSpellComponent.Classification.Infusion
            || component.classification == AuricaSpellComponent.Classification.Blade
            || component.classification == AuricaSpellComponent.Classification.Shield
        ) {
            component.category = AuricaSpellComponent.Category.SpellForm;

        } else if (component.classification == AuricaSpellComponent.Classification.Self
            || component.classification == AuricaSpellComponent.Classification.Other
            || component.classification == AuricaSpellComponent.Classification.Surface
            || component.classification == AuricaSpellComponent.Classification.Mana
            || component.classification == AuricaSpellComponent.Classification.Form
        ) {
            component.category = AuricaSpellComponent.Category.SpellFocus;

        } else if (component.classification == AuricaSpellComponent.Classification.Propel
            || component.classification == AuricaSpellComponent.Classification.Throw
            || component.classification == AuricaSpellComponent.Classification.Target
            || component.classification == AuricaSpellComponent.Classification.Sustain
            || component.classification == AuricaSpellComponent.Classification.Control
            || component.classification == AuricaSpellComponent.Classification.Collect
            || component.classification == AuricaSpellComponent.Classification.Contain
            || component.classification == AuricaSpellComponent.Classification.Expel
            || component.classification == AuricaSpellComponent.Classification.Pull
            || component.classification == AuricaSpellComponent.Classification.Bless
            || component.classification == AuricaSpellComponent.Classification.Curse
        ) {
            component.category = AuricaSpellComponent.Category.SpellAction;

        }

        EditorGUILayout.LabelField("Description");
        GUIStyle myCustomStyle = new GUIStyle(GUI.skin.GetStyle("textArea")) { wordWrap = true };
        component.description = EditorGUILayout.TextArea(component.description, myCustomStyle);

        component.manaCostMultiplier = EditorGUILayout.FloatField("Mana Cost Multiplier", component.manaCostMultiplier);
        EditorGUILayout.Space();

        component.hasBasicDistribution = EditorGUILayout.Toggle("Basic distribution", component.hasBasicDistribution);
        if (component.hasBasicDistribution) {
            Rect r = EditorGUILayout.BeginVertical();
            component.basicDistribution.structure = EditorGUILayout.DelayedFloatField("Structure", component.basicDistribution.structure);
            component.basicDistribution.essence = EditorGUILayout.DelayedFloatField("Essence", component.basicDistribution.essence);
            component.basicDistribution.fire = EditorGUILayout.DelayedFloatField("Fire", component.basicDistribution.fire);
            component.basicDistribution.water = EditorGUILayout.DelayedFloatField("Water", component.basicDistribution.water);
            component.basicDistribution.earth = EditorGUILayout.DelayedFloatField("Earth", component.basicDistribution.earth);
            component.basicDistribution.air = EditorGUILayout.DelayedFloatField("Air", component.basicDistribution.air);
            component.basicDistribution.nature = EditorGUILayout.DelayedFloatField("Nature", component.basicDistribution.nature);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        component.hasAuricDistribution = EditorGUILayout.Toggle("Auric distribution", component.hasAuricDistribution);
        if (component.hasAuricDistribution) {
            Rect r = EditorGUILayout.BeginVertical();
            component.auricDistribution.structure = EditorGUILayout.DelayedFloatField("Structure", component.auricDistribution.structure);
            component.auricDistribution.essence = EditorGUILayout.DelayedFloatField("Essence", component.auricDistribution.essence);
            component.auricDistribution.fire = EditorGUILayout.DelayedFloatField("Fire", component.auricDistribution.fire);
            component.auricDistribution.water = EditorGUILayout.DelayedFloatField("Water", component.auricDistribution.water);
            component.auricDistribution.earth = EditorGUILayout.DelayedFloatField("Earth", component.auricDistribution.earth);
            component.auricDistribution.air = EditorGUILayout.DelayedFloatField("Air", component.auricDistribution.air);
            component.auricDistribution.nature = EditorGUILayout.DelayedFloatField("Nature", component.auricDistribution.nature);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        component.hasFluxDistribution = EditorGUILayout.Toggle("Flux distribution", component.hasFluxDistribution);
        if (component.hasFluxDistribution) {
            Rect r = EditorGUILayout.BeginVertical();
            component.fluxDistribution.structure = EditorGUILayout.DelayedFloatField("Structure", component.fluxDistribution.structure);
            component.fluxDistribution.essence = EditorGUILayout.DelayedFloatField("Essence", component.fluxDistribution.essence);
            component.fluxDistribution.fire = EditorGUILayout.DelayedFloatField("Fire", component.fluxDistribution.fire);
            component.fluxDistribution.water = EditorGUILayout.DelayedFloatField("Water", component.fluxDistribution.water);
            component.fluxDistribution.earth = EditorGUILayout.DelayedFloatField("Earth", component.fluxDistribution.earth);
            component.fluxDistribution.air = EditorGUILayout.DelayedFloatField("Air", component.fluxDistribution.air);
            component.fluxDistribution.nature = EditorGUILayout.DelayedFloatField("Nature", component.fluxDistribution.nature);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        component.hasSiphonDistribution = EditorGUILayout.Toggle("Siphon distribution", component.hasSiphonDistribution);
        if (component.hasSiphonDistribution) {
            Rect r = EditorGUILayout.BeginVertical();
            component.siphonDistribution.structure = EditorGUILayout.DelayedFloatField("Structure", component.siphonDistribution.structure);
            component.siphonDistribution.essence = EditorGUILayout.DelayedFloatField("Essence", component.siphonDistribution.essence);
            component.siphonDistribution.fire = EditorGUILayout.DelayedFloatField("Fire", component.siphonDistribution.fire);
            component.siphonDistribution.water = EditorGUILayout.DelayedFloatField("Water", component.siphonDistribution.water);
            component.siphonDistribution.earth = EditorGUILayout.DelayedFloatField("Earth", component.siphonDistribution.earth);
            component.siphonDistribution.air = EditorGUILayout.DelayedFloatField("Air", component.siphonDistribution.air);
            component.siphonDistribution.nature = EditorGUILayout.DelayedFloatField("Nature", component.siphonDistribution.nature);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        importstr = EditorGUILayout.Toggle("String Import", importstr);
        if (importstr) {
            importString = EditorGUILayout.TextField("String: ", importString);
            if (GUILayout.Button("Import")) {
                string[] stringSeperator = new string[] { "\", " };
                string[] splitStr = importString.Split(stringSeperator, System.StringSplitOptions.None);
                splitStr[0] = splitStr[0].Replace("\"", "").Replace("(", "");
                splitStr[1] = splitStr[1].Replace("\"", "");
                splitStr[2] = splitStr[2].Replace("\"", "").Replace(")", "");

                component.c_name = splitStr[0];
                Debug.Log("Name: " + component.c_name);
                component.description = splitStr[1];
                Debug.Log("Description: " + component.description);

                splitStr[2] = splitStr[2].Substring(1, splitStr[2].Length - 2);
                string[] distributionSeperator = new string[] { "], [" };
                string[] splitDistributions = splitStr[2].Split(distributionSeperator, System.StringSplitOptions.None);
                int iter = 0;
                component.hasBasicDistribution = false;
                component.hasAuricDistribution = false;
                component.hasFluxDistribution = false;
                foreach (var item in splitDistributions) {
                    if (iter == 0) {
                        component.hasBasicDistribution = true;
                        component.basicDistribution = new ManaDistribution(item);
                        Debug.Log("Basic dist: " + component.basicDistribution.ToString());
                        if (component.basicDistribution.IsEmpty()) component.hasBasicDistribution = false;
                    } else if (iter == 1) {
                        component.hasAuricDistribution = true;
                        component.auricDistribution = new ManaDistribution(item);
                        Debug.Log("Auric dist: " + component.auricDistribution.ToString());
                        if (component.auricDistribution.IsEmpty()) component.hasAuricDistribution = false;
                    } else if (iter == 2) {
                        component.hasFluxDistribution = true;
                        component.fluxDistribution = new ManaDistribution(item);
                        Debug.Log("Flux dist: " + component.fluxDistribution.ToString());
                        if (component.fluxDistribution.IsEmpty()) component.hasFluxDistribution = false;
                    }
                    iter += 1;
                }
            }
            Undo.RecordObject(target, "Import values");
        }
        EditorUtility.SetDirty(target);
    }
}