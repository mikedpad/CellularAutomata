using UnityEditor;
using UnityEngine;

public class PM_TextureImporter : AssetPostprocessor {
	private void OnPostprocessTexture(Texture2D texture) {
		// Procedural Map textures must have '_pm' somewhere in the filename
		// in order to be processed. This is automatically added if prefab
		// is created through the "GameObject > Create Other" menu.
		var lowerCaseAssetPath = assetPath.ToLower();
		if (lowerCaseAssetPath.IndexOf("_pm") == -1)
			return;

		TextureImporter textureImporter = (TextureImporter) assetImporter;
		textureImporter.textureType = TextureImporterType.Default;
		textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.wrapMode = TextureWrapMode.Clamp;
	}
}