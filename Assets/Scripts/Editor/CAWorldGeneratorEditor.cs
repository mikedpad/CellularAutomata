// Cellular Automata World Generator
// by Michael Daniel Padilla - @michaeldpadilla
//
// The editor is not really complete to where I want it but take it and
// use it as a jumping off point.
//
// Based on code by:
// Michael Cook - @mtrc
// Christer Kaitila - @McFunkypants
//
// You are free to do with this code whatever you wish!
// Use it for your game jam or commericial product!
// Though a credit for myself and the aforementioned persons would be most kind :)
//
// To add a world generator texture, go to your Hierarchy (or GameObject drop-down) and
// navigate to 'Create Other' -> 'Cellular Automata Generator'
//
// Notes:
// - If you experience what appears to be anti-aliasing, check your
//   texture's Import Settings. This is something I'll probably add in the future.
// - The CAWorldGeneratorEditor is a Unity Editor script and as just as all Editor scripts,
//   this file MUST be placed within a subfolder called "Editor" to function
//

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CAWorldGenerator))]
public class CAWorldGeneratorEditor : Editor {
	private CAWorldGenerator _generator;
	private string _texturePath;

	private SerializedProperty _worldWidth;
	private SerializedProperty _worldHeight;
	private SerializedProperty _aliveColor;
	private SerializedProperty _deadColor;

	private SerializedProperty _chanceToStartAlive;
	private SerializedProperty _birthLimit;
	private SerializedProperty _deathLimit;
	private SerializedProperty _numberOfSteps;

	private void OnEnable() {
		_generator = (CAWorldGenerator) target;
		if (_texturePath == null)
			_texturePath = AssetDatabase.GetAssetPath(_generator.renderer.sharedMaterial.mainTexture);
		_worldWidth = serializedObject.FindProperty("worldWidth");
		_worldHeight = serializedObject.FindProperty("worldHeight");
		_aliveColor = serializedObject.FindProperty("aliveColor");
		_deadColor = serializedObject.FindProperty("deadColor");
		_chanceToStartAlive = serializedObject.FindProperty("chanceToStartAlive");
		_birthLimit = serializedObject.FindProperty("birthLimit");
		_deathLimit = serializedObject.FindProperty("deathLimit");
		_numberOfSteps = serializedObject.FindProperty("numberOfSteps");
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		// World Properties
		EditorGUILayout.PropertyField(_worldWidth);
		EditorGUILayout.PropertyField(_worldHeight);
		EditorGUILayout.PropertyField(_aliveColor);
		EditorGUILayout.PropertyField(_deadColor);
		EditorGUILayout.Separator();
		EditorGUILayout.Slider(_chanceToStartAlive, 0, 1);
		EditorGUILayout.IntSlider(_birthLimit, 1, 10);
		EditorGUILayout.IntSlider(_deathLimit, 1, 10);
		EditorGUILayout.IntSlider(_numberOfSteps, 0, 10);
		EditorGUILayout.Separator();

		serializedObject.ApplyModifiedProperties();

		// Line Separator
		EditorGUILayout.Separator();
		GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });
		EditorGUILayout.Separator();

		// Buttons
		if (GUILayout.Button("Create New World")) {
			_generator.Create();
			UpdateTexture();
		}
		EditorGUILayout.Separator();

		if (GUILayout.Button("Iterate")) {
			_generator.Iterate();
			UpdateTexture();
		}
		EditorGUILayout.Separator();
	}

	// The following code is just for convenience. It will prompt you to save as a prefab and create
	// a material and texture for you automagically. While you CAN easily piece together a GameObject
	// yourself with your own Material and Texture... but... Be advised!!!
	// Warning! The world generator will replace any texture assigned to the material!!!
	// Use the Create menu in the Hierarchy to make a new GameObject instead. It's easier.
	#region Save To Prefab
	/// <summary>
	/// Creates a new prefab world generator GameObject which should be saved to Assets folder.
	/// </summary>
	[MenuItem("GameObject/Create Other/Cellular Automata Generator")]
	public static void CreateCAGenerator() {
		// Prompt for filename
		string prefabPath = EditorUtility.SaveFilePanelInProject("Save Prefab As", "myWorld", "prefab", "Enter a filename to save asset to");
		string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
		string prefabFolder = System.IO.Path.GetDirectoryName(prefabPath) + "/";
		string materialPath = prefabFolder + prefabName + ".mat";
		string texturePath = prefabFolder + prefabName + "_ca.png";

		// Create GameObject
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);		// GameObject with Quad Mesh Primitive
		go.name = prefabName;												// Set name of GameObject from Prefab
		DestroyImmediate(go.collider);										// Remove the default Mesh Collider
		go.AddComponent<CAWorldGenerator>();								// Add CA World Generator Component

		// Create Prefab
		UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPath);
		AssetDatabase.Refresh();

		// Create Texture
		Texture2D texture = CreateTexture(1, 1, new Color32[] { Color.blue });	// Solid blue texture
		SaveTextureToDisk(texture, texturePath);								// Save texture to disk

		// Create Material
		Material material = new Material(Shader.Find("Sprites/Default"));
		material.mainTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
		go.renderer.sharedMaterial = material;
		AssetDatabase.CreateAsset(material, materialPath);						// Create Material Asset on disk
		AssetDatabase.Refresh();

		// Add GameObject to Prefab
		PrefabUtility.ReplacePrefab(go, prefab);

		// Destroy Temporary GameObject
		DestroyImmediate(go);

		// Instantiate Prefab GameObject to add to scene
		PrefabUtility.InstantiatePrefab((GameObject) AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
	}
	#endregion Save To Prefab

	#region Texture Update
	/// <summary>
	/// Creates and returns a Texture2D.
	/// </summary>
	/// <param name="width">Specified width of the texture</param>
	/// <param name="height">Specified height of the texture</param>
	/// <param name="pixels">A Color32 array of the pixels</param>
	/// <returns>A shiny, new Texture2D</returns>
	private static Texture2D CreateTexture(int width, int height, Color32[] pixels) {
		Texture2D texture = new Texture2D(width, height);
		texture.SetPixels32(pixels);
		texture.Apply();
		return texture;
	}

	/// <summary>
	/// Writes a texture to disk. Warning! This will overwrite any existing texture used in the material.
	/// </summary>
	/// <param name="texture">The texture that will be saved to disk.</param>
	/// <param name="texturePath">The location where the texture will be saved.</param>
	private static void SaveTextureToDisk(Texture2D texture, string texturePath) {
		// PNG Encoding
		byte[] textureBytes = texture.EncodeToPNG();

		// Write Texture to File
		if (textureBytes != null)
			System.IO.File.WriteAllBytes(texturePath, textureBytes);

		// Destroy the temporary texture
		DestroyImmediate(texture);

		AssetDatabase.Refresh();
	}

	/// <summary>
	/// Uses the supplied texture and saves it to disk in place of existing texture.
	/// The supplied texture is then destroyed to prevent leaks and the material
	/// is then assigned the new texture loaded from asset folder.
	/// </summary>
	private void UpdateTexture() {
		Material material = _generator.renderer.sharedMaterial;
		Texture2D texture = (Texture2D) material.mainTexture;

		SaveTextureToDisk(texture, _texturePath);					// Save Texture To Disk


		// Load texture from disk and assign to material
		material.mainTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(_texturePath, typeof(Texture2D));
		material.mainTexture.filterMode = FilterMode.Point;
		material.mainTexture.wrapMode = TextureWrapMode.Clamp;
	}
	#endregion Texture Update
}