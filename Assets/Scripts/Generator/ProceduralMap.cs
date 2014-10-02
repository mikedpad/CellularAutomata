using System.Collections.Generic;
using UnityEngine;

public enum TileType {
	None = 0,
	Wall
};

public abstract class ProceduralMap : MonoBehaviour {
	[SerializeField]
	protected int _width = 128;
	[SerializeField]
	protected int _height = 128;
	protected int[,] _data;
	public int[,] Data { get { return _data; } }

	// Map Tiles (Placeholder Colors - To be replaced with tile textures)
	protected Color32[] _colorData = {
		new Color32(255, 223, 0, 255),	// Empty - Golden Yellow
		new Color32(120, 81, 169, 255),	// Wall - Royal Purple
	};
	public Color32[] ColorData { get { return _colorData; } }

	// Properties

	#region Common Map Methods
	public abstract void GenerateMap();

	/// <summary>
	/// Creates a new, blank map based on map dimensions.
	/// </summary>
	/// <returns>An empty map.</returns>
	protected void GenerateEmptyMap() {
		_data = new int[_width, _height];

		for (int x = 0; x < _width; x++) {
			for (int y = 0; y < _height; y++) {
				_data[x, y] = 0;
			}
		}
	}
	#endregion Common Map Methods
}