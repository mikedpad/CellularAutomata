using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[CustomEditor(typeof(CAMap))]
public class CAMapEditor : ProceduralMapEditor {
	private SerializedProperty _wallSpawnChance;
	private SerializedProperty _birthLimit;
	private SerializedProperty _deathLimit;
	private SerializedProperty _numberOfSteps;

	private AnimBool _advancedOptions;

	protected override void OnEnableEditor() {
		_wallSpawnChance = serializedObject.FindProperty("_wallSpawnChance");
		_birthLimit = serializedObject.FindProperty("_birthLimit");
		_deathLimit = serializedObject.FindProperty("_deathLimit");
		_numberOfSteps = serializedObject.FindProperty("_numberOfSteps");
		_advancedOptions = new AnimBool(false, Repaint);
	}

	protected override void DisplayProperties() {
		// Cellular Automata Map Serialized Objects
		EditorGUILayout.LabelField("Cellular Automata Options", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUIUtility.labelWidth = 135;
		EditorGUIUtility.fieldWidth = 45;
		EditorGUILayout.Slider(_wallSpawnChance, 0, 1, new GUIContent("Wall Spawn Chance", "The chance (in percentage) that the each tile start as a wall. This is compared to a random number."));
		EditorGUILayout.IntSlider(_numberOfSteps, 0, 10, new GUIContent("Simulation Steps", "The number of simulated steps of rules that will be performed after generation completed."));
		EditorGUIUtility.labelWidth = 0;
		EditorGUIUtility.fieldWidth = 0;

		EditorGUILayout.Separator();

		// Advanced Properties
		EditorGUI.indentLevel++;
		_advancedOptions.target = EditorGUILayout.Foldout(_advancedOptions.target, "Advanced Options", EditorStyles.foldout);
		if (EditorGUILayout.BeginFadeGroup(_advancedOptions.faded)) {
			EditorGUI.indentLevel++;
			EditorGUIUtility.labelWidth = 120;
			EditorGUIUtility.fieldWidth = 30;
			EditorGUILayout.IntSlider(_birthLimit, 1, 8);
			EditorGUILayout.IntSlider(_deathLimit, 1, 8);
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndFadeGroup();
		EditorGUI.indentLevel--;

		EditorGUILayout.Separator();
		EditorGUI.indentLevel--;
	}

	protected override void DisplayButtons() {
		EditorGUILayout.BeginHorizontal();

		// Generate Map Button
		if (GUILayout.Button("Generate Map")) {
			_map.GenerateMap();
			Redraw();
		}
		// Step Simulation Button
		if (GUILayout.Button("Step Simulation")) {
			((CAMap) _map).StepSimulation();
			Redraw();
		}
		// Reset Button
		if (GUILayout.Button("Reset", GUILayout.Width(50))) {
			if (EditorUtility.DisplayDialog("Revert To Default Settings?", "You will lose any changes to your current map generation settings.", "Yes", "No")) {
				PrefabUtility.RevertPrefabInstance(Selection.activeGameObject);
				Debug.Log("Map Prefab Preferences Have Been Reset.");
			}
		}

		EditorGUILayout.EndHorizontal();
	}

	/// <summary>
	/// Creates a Cellular Automata Prefab Asset
	/// </summary>
	[MenuItem("GameObject/Create Other/Cellular Automata Generator")]
	public static void CreateCAGenerator() {
		CreateProceduralMapPrefab<CAMap>();
	}
}