using UnityEngine;
using UnityEditor;

public class CA_TextureImporter : AssetPostprocessor {
	private void OnPostprocessTexture(Texture2D texture) {
		// Cellular Automata textures must have '_ca' somewhere in the filename
		// in order to be processed. This is automatically added if prefab
		// is created through the "GameObject > Create Other" menu.
		var lowerCaseAssetPath = assetPath.ToLower();
		if (lowerCaseAssetPath.IndexOf ("_ca") == -1)
			return;

		TextureImporter textureImporter = (TextureImporter) assetImporter;
		textureImporter.textureType = TextureImporterType.Image;
		textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.wrapMode = TextureWrapMode.Clamp;
	}
}
