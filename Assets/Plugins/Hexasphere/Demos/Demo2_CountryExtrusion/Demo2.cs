using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;

namespace HexasphereGrid_Demos {
	public class Demo2 : MonoBehaviour {

		[Tooltip ("Texture with colored territories.")]
		public Texture2D worldTerritories;

		Hexasphere hexa;
		Color32 seaColor;

		bool cycleColorsEnabled;
		Color32 currentColor;
		Color32 blinkColor;
		bool blinkBit;
		List<int> currentTerritoryTiles;

		void Start () {
			// Gets the script for the "Hexasphere" gameobject
			hexa = Hexasphere.GetInstance ("Hexasphere");

			// Set background color to ignore those tiles
			seaColor = new Color32 (0, 27, 82, 1);
			hexa.OnTileMouseOver += TileMouseOver;
			hexa.ApplyColors (worldTerritories);

			// Create a buffer for selecting tiles of the current territory
			currentTerritoryTiles = new List<int> ();

			// Start coroutine that cycles colors of the highlight
			StartCoroutine (CycleColors ());
		}


		void TileMouseOver (int tileIndex) {

			// Get color of current cell
			Color32 cellColor = hexa.GetTileColor (tileIndex, true);

			// If color is same, don't do anything
			if (cellColor.r == currentColor.r && cellColor.g == currentColor.g && cellColor.b == currentColor.b) {
				return;
			}

			// Clears temporary colors
			hexa.ClearTiles (true, false, false);

			// Reset extrusion
			hexa.ClearTilesExtrusion ();

			// Select cell
			currentColor = cellColor;

			// If current cell is "sea", ignore and return
			if (currentColor.r == seaColor.r && currentColor.g == seaColor.g && currentColor.b == seaColor.b) {
				cycleColorsEnabled = false;
				return;
			}

			// Select tiles with same color
			currentTerritoryTiles.Clear ();
			for (int k = 0; k < hexa.tiles.Length; k++) {
				if (hexa.GetTileColor (k, true) == currentColor) {
					currentTerritoryTiles.Add (k);
				}
			}

			// Extrude tiles
			hexa.SetTileExtrudeAmount (currentTerritoryTiles, 0.5f);

			// Enable territory highlight color cycle
			cycleColorsEnabled = true;
			blinkBit = false;
												 
		}

		IEnumerator CycleColors () {
			while (true) {
				if (cycleColorsEnabled) {

					// Alternate between colors
					blinkBit = !blinkBit;

					Color currentColor = Color.yellow;
					Color darkerColor = Color.blue;
					if (hexa.lastHighlightedTileIndex >= 0) {
						// get one color from current tile
						currentColor = hexa.GetTileColor (hexa.lastHighlightedTileIndex, true);
						// make second color darker
						darkerColor = currentColor;
						darkerColor.r *= 0.8f;
						darkerColor.g *= 0.8f;
						darkerColor.b *= 0.8f;
					}
					blinkColor = blinkBit ? darkerColor : currentColor;

					// Change all cells with same color than selected cell to red and start animation
					hexa.SetTileColor (currentTerritoryTiles, blinkColor, true);
				}
				yield return new WaitForSeconds (0.3f);
			}

		}

	}
}