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
// Simulate Step Rules:
// 1. If a living cell has less than two living neighbours, it dies.
// 2. If a living cell has two or three living neighbours, it stays alive.
// 3. If a living cell has more than three living neighbours, it dies.
// 4. If a dead cell has exactly three living neighbours, it becomes alive.
// - Michael Cook, http://bit.ly/1szImUT
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

using System.Collections;
using UnityEngine;

public class CAWorldGenerator : MonoBehaviour {
	private bool[,] world;

	[Header("Cellular Automata World Generator")]
	public int worldWidth = 32;
	public int worldHeight = 32;
	[Tooltip("Color for a cell that is alive (Solid)")]
	public Color aliveColor = new Color(1, 1, 1, 1);
	[Tooltip("Color for a cell that is dead (Empty)")]
	public Color deadColor = new Color(0, 0, 0, 1);
	[Tooltip("The chance that each cell will be alive (Solid)")]
	public float chanceToStartAlive = 0.4f;
	[Tooltip("Read rules in source for more info on rules.")]
	public int birthLimit = 4;
	[Tooltip("Read rules in source for more info on rules.")]
	public int deathLimit = 3;
	[Tooltip("Number of simulated steps to be performed on new world creation. Default is 2. Set to 0 and advance manually for more control.")]
	public int numberOfSteps = 2;

	// Called from Editor script
	public void Create() {
		world = GenerateMap();
		Redraw();
	}

	// Called from Editor script
	public void Iterate() {
		world = SimulateStep(world);
		Redraw();
	}

	private bool[,] GenerateMap() {
		bool[,] map = new bool[worldWidth, worldHeight];

		// Initialize Map
		for (int x = 0; x < worldWidth; x++) {
			for (int y = 0; y < worldHeight; y++) {
				// Make all cells empty to start
				map[x, y] = false;
			}
		}

		// "Seed"
		for (int x = 0; x < worldWidth; x++) {
			for (int y = 0; y < worldHeight; y++) {
				// Compare random number to chance of starting alive
				if (Random.value < chanceToStartAlive)
					map[x, y] = true;
			}
		}

		// Simulation Steps To Perform On Creating New World
		for (int i = 0; i < numberOfSteps; i++)
			map = SimulateStep(map);

		return map;
	}

	private void Redraw() {
		Color32[] pixels = new Color32[worldWidth * worldHeight];

		// Write colors
		for (int y = 0; y < worldHeight; y++) {
			for (int x = 0; x < worldWidth; x++) {
				pixels[x + y * worldWidth] = world[x, y] ? aliveColor : deadColor;
			}
		}

		// Create the texture
		Texture2D texture = new Texture2D(worldWidth, worldHeight);
		texture.SetPixels32(pixels);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply();
		renderer.sharedMaterial.mainTexture = texture;
	}

	private bool[,] SimulateStep(bool[,] map) {
		bool[,] newMap = new bool[worldWidth, worldHeight];

		for (int x = 0; x < worldWidth; x++) {
			for (int y = 0; y < worldHeight; y++) {
				// Get the number of active cells neighboring current
				int livingNeighbors = CountLivingNeighbors(map, x, y);

				// If this cell is a solid
				if (map[x, y]) {
					if (livingNeighbors < deathLimit)
						newMap[x, y] = false;
					else
						newMap[x, y] = true;
				}
					// If this cell is empty
				else {
					if (livingNeighbors > birthLimit)
						newMap[x, y] = true;
					else
						newMap[x, y] = false;
				}
			}
		}

		return newMap;
	}

	private int CountLivingNeighbors(bool[,] map, int x, int y) {
		int count = 0;
		for (int i = -1; i < 2; i++) {
			for (int j = -1; j < 2; j++) {
				int nb_x = i + x;
				int nb_y = j + y;

				if (i == 0 && j == 0) {
				} else if (nb_x < 0 || nb_y < 0 || nb_x >= worldWidth || nb_y >= worldHeight) {
					count++;
				} else if (map[nb_x, nb_y] == true) {
					count++;
				}
			}
		}

		return count;
	}
}