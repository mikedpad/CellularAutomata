using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public abstract class ProceduralMapEditor : Editor {
	// Cached
	protected ProceduralMap _map;
	protected Material _material;
	protected string _texturePath;

	// Properties
	protected SerializedProperty _mapWidth;
	protected SerializedProperty _mapHeight;

	// Implemented in Child-Class
	protected abstract void OnEnableEditor();
	protected abstract void DisplayProperties();
	protected abstract void DisplayButtons();

	private void OnEnable() {
		_map = (ProceduralMap) target;

		if (_material == null)
			_material = ((ProceduralMap) target).GetComponent<Renderer>().sharedMaterial;
		if (_texturePath == null)
			_texturePath = AssetDatabase.GetAssetPath(_material.mainTexture);

		// Procedural Map Serialized Objects
		_mapWidth = serializedObject.FindProperty("_width");
		_mapHeight = serializedObject.FindProperty("_height");

		// OnEnable for Child-Classes
		OnEnableEditor();
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		// Procedural Map Properties
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Map Dimensions", EditorStyles.boldLabel);
		EditorGUI.indentLevel++;

		EditorGUIUtility.labelWidth = 60;
		GUILayoutOption mapGuiOption = GUILayout.Width(EditorGUIUtility.labelWidth + 40);

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(_mapWidth, new GUIContent("Width", "The width of the map."), mapGuiOption);
		EditorGUILayout.PropertyField(_mapHeight, new GUIContent("Height", "The height of the map."), mapGuiOption);
		EditorGUILayout.EndHorizontal();

		EditorGUIUtility.labelWidth = 0;

		EditorGUI.indentLevel--;
		EditorGUILayout.Separator();

		// Procedural Map Child-Class Properties
		DisplayProperties();

		serializedObject.ApplyModifiedProperties();

		// Line Separator
		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });

		// Draw Child-Class Buttons
		EditorGUILayout.Separator();
		DisplayButtons();
		EditorGUILayout.Separator();
	}

	#region Update Map Visuals
	/// <summary>
	/// Updates the visual representation of the map using colors (for now)
	/// </summary>
	protected void Redraw() {
		// Map Dimensions
		int width = _mapWidth.intValue;
		int height = _mapHeight.intValue;

		// Map Tiles
		int[,] mapData = _map.Data;
		Color32[] colorData = _map.ColorData;
		Color32[] pixels = new Color32[width * height];

		// Write colors
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				pixels[x + y * width] = colorData[(int) mapData[x, y]];
			}
		}

		// Load texture from disk and assign to material
		_material.mainTexture = CreateAndSaveTexture(width, height, pixels, _texturePath);
		_material.mainTexture.filterMode = FilterMode.Point;
		_material.mainTexture.wrapMode = TextureWrapMode.Clamp;
	}

	/// <summary>
	/// Creates a temporary texture, saves that texture to disk and destroys temporary texture
	/// </summary>
	/// <param name="width">Width of the texture</param>
	/// <param name="height">Height of the texture</param>
	/// <param name="pixels">Color array of the pixels</param>
	/// <param name="texturePath">File where the texture will be written</param>
	/// <returns>The Texture2D asset loaded from disk</returns>
	protected static Texture2D CreateAndSaveTexture(int width, int height, Color32[] pixels, string texturePath) {
		// Create temporary Texture2D
		Texture2D texture = new Texture2D(width, height);
		texture.SetPixels32(pixels);
		texture.Apply();

		// PNG Encoding
		byte[] textureBytes = texture.EncodeToPNG();

		// Write Texture to File
		if (textureBytes != null)
			File.WriteAllBytes(texturePath, textureBytes);

		// Destroy the temporary Texture2D
		DestroyImmediate(texture);

		// Update the asset database to detect new texture asset
		AssetDatabase.Refresh();

		// Return the new texture asset from disk
		return (Texture2D) AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
	}
	#endregion Update Map Visuals

	#region Create Prefab Asset
	/// <summary>
	/// Creates a new prefab GameObject which should be saved in the Assets folder.
	/// </summary>
	protected static void CreateProceduralMapPrefab<T>() where T : ProceduralMap {
		// Prompt for filename
		string prefabPath = EditorUtility.SaveFilePanelInProject("Save Map Prefab As", "myMap", "prefab", "Enter a filename to save prefab to");
		string objName = Path.GetFileNameWithoutExtension(prefabPath);
		string objFolder = Path.GetDirectoryName(prefabPath) + "/";
		string materialPath = objFolder + objName + ".mat";
		string texturePath = objFolder + objName + "_pm.png";

		// Create GameObject
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);		// GameObject with Quad Mesh Primitive
		go.name = objName;													// Set name of GameObject from Prefab
		DestroyImmediate(go.GetComponent<Collider>());										// Remove the default Mesh Collider
		go.AddComponent<T>();												// Add script component

		// Create Prefab
		UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
		AssetDatabase.Refresh();

		// Create Material
		Material material = new Material(Shader.Find("Sprites/Default"));
		material.mainTexture = CreateAndSaveTexture(1, 1, new Color32[] { Color.blue }, texturePath);
		go.GetComponent<Renderer>().sharedMaterial = material;

		// Create Material Asset
		AssetDatabase.CreateAsset(material, materialPath);
		AssetDatabase.Refresh();

		// Add GameObject to Prefab
		PrefabUtility.ReplacePrefab(go, prefab);

		// Destroy Temporary GameObject
		DestroyImmediate(go);

		// Instantiate Prefab GameObject to add to scene
		PrefabUtility.InstantiatePrefab((GameObject) AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
	}
	#endregion Create Prefab Asset
}