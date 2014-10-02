// Cellular Automata Map Generator
// https://github.com/michaeldpadilla/CellularAutomata
// by Michael Daniel Padilla - @michaeldpadilla
//
// Based on the following:
// Michael Cook - @mtrc - "Generate Random Cave Levels Using Cellular Automata" -> http://bit.ly/1szImUT
// Christer Kaitila - @McFunkypants - "Random World Generator" -> http://bit.ly/1rndvtp
//
// To add a map generator texture, go to your Hierarchy (or GameObject drop-down) and
// navigate to 'Create Other' -> 'Cellular Automata Generator'

using UnityEngine;

public class CAMap : ProceduralMap {
	[SerializeField]
	private float _wallSpawnChance = 0.4f;
	[SerializeField]
	private int _numberOfSteps = 3;
	[SerializeField]
	private int _birthLimit = 4;
	[SerializeField]
	private int _deathLimit = 3;

	#region Cellular Automata Generation
	public override void GenerateMap() {
		GenerateEmptyMap();

		// Generate random wall cells
		for (int x = 0; x < _width; x++) {
			for (int y = 0; y < _height; y++) {
				if (Random.value < _wallSpawnChance)
					_data[x, y] = (int) TileType.Wall; // 1
			}
		}

		// Simulate Step
		for (int i = 0; i < _numberOfSteps; i++)
			StepSimulation();
	}

	public void StepSimulation() {
		int[,] newMap = new int[_width, _height];

		for (int x = 0; x < _width; x++) {
			for (int y = 0; y < _height; y++) {
				// Get the number of active cells neighboring current
				int livingNeighbors = CountLivingNeighbors(x, y);

				// If Cell is a Wall
				if (_data[x, y] == (int) TileType.Wall) {
					if (livingNeighbors < _deathLimit)
						newMap[x, y] = (int) TileType.None; // 0
					else
						newMap[x, y] = (int) TileType.Wall; // 1
				} else {
					if (livingNeighbors > _birthLimit)
						newMap[x, y] = (int) TileType.Wall; // 1
					else
						newMap[x, y] = (int) TileType.None; // 0
				}
			}
		}

		_data = newMap;
	}

	private int CountLivingNeighbors(int x, int y) {
		int count = 0;
		for (int i = -1; i < 2; i++) {
			for (int j = -1; j < 2; j++) {
				int nb_x = i + x;
				int nb_y = j + y;

				if (i == 0 && j == 0) {
				} else if (nb_x < 0 || nb_y < 0 || nb_x >= _width || nb_y >= _height) {
					count++;
				} else if (_data[nb_x, nb_y] == (int) TileType.Wall) {
					count++;
				}
			}
		}

		return count;
	}
	#endregion Cellular Automata Generation
}