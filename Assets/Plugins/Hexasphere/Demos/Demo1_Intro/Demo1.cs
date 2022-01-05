using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;

namespace HexasphereGrid_Demos {
	public class Demo1 : MonoBehaviour {
								
		enum DEMO_MODE {
			Idle,
			Paint,
			PathFind_StartTile,
			PathFind_EndTile
		}

		[Tooltip ("Sample texture used to paint tiles with mouse click.")]
		public Texture2D paintTexture;

		[Tooltip ("Texture with elevation data. The red channel is used as the height value.")]
		public Texture2D heightMap;

		[Tooltip ("Texture with water mask data. The alpha channel determines the location of water (values < constant threshold)")]
		public Texture2D waterMask;

		[Tooltip ("Object to spawn in this demo.")]
		public GameObject spawnObject;

		[Tooltip("Capsule to spawn in this demo.")]
		public GameObject prefabCapsule;

		DEMO_MODE selectMode;
		Hexasphere hexa;
		int divisions = 8;
		bool paintInColor = true;
		string divisionsText = "Divisions";
		int selectedTile;
		GUIStyle labelStyle;
		int animateTileIndex;
		public Sprite sprite;
		GameObject capsule;

		void Start () {
			// Gets the script for the "Hexasphere" gameobject
			hexa = Hexasphere.GetInstance ("Hexasphere");

			hexa.OnTileClick += TileClick;
			hexa.OnTileMouseOver += TileMouseOver;

			// Color North in red
			int ti = hexa.GetTileAtPos (new Vector3 (0, 50, 55));
			hexa.SetTileColor (ti, Color.red, true);

			// Color opposite tile (South) in blue
			ti = hexa.GetTileAtPolarOpposite (ti);
			hexa.SetTileColor (ti, Color.blue);

			// Color opposite tile (Horizon) in green
			ti = hexa.GetTileAtPos (new Vector3 (0, 0, 5));
			hexa.SetTileColor (ti, Color.green);

		}

		bool avoidGUI;


		void OnGUI () {
			if (avoidGUI)
				return;
			GUI.Label (new Rect (10, 10, 220, 30), divisionsText);
			divisions = (int)GUI.HorizontalSlider (new Rect (10, 30, 220, 30), divisions, 1, 100);
			if (divisions != hexa.numDivisions) {
				hexa.numDivisions = divisions;
				divisionsText = "Divisions (" + hexa.tiles.Length + " tiles)";
			}

			GUI.Label (new Rect (10, 60, 220, 30), "Divisions");
			bool wireframe = GUI.Toggle (new Rect (10, 80, 220, 20), hexa.style == STYLE.Wireframe, "Wireframe");
			if (wireframe)
				hexa.style = STYLE.Wireframe;
			bool shaded = GUI.Toggle (new Rect (10, 100, 220, 20), hexa.style == STYLE.Shaded, "Shaded");
			if (shaded)
				hexa.style = STYLE.Shaded;
			bool shadedWireframe = GUI.Toggle (new Rect (10, 120, 220, 20), hexa.style == STYLE.ShadedWireframe, "Shaded + Wireframe");
			if (shadedWireframe)
				hexa.style = STYLE.ShadedWireframe;

			if (GUI.Button (new Rect (10, 155, 220, 30), "Paint Mode")) {
				if (selectMode != DEMO_MODE.Paint) {
					selectMode = DEMO_MODE.Paint;
				} else {
					selectMode = DEMO_MODE.Idle;
				}
			}

			bool prevPaintColor = paintInColor;
			paintInColor = GUI.Toggle (new Rect (10, 190, 200, 20), paintInColor, "Color");
			paintInColor = !GUI.Toggle (new Rect (10, 210, 200, 20), !paintInColor, "Texture");
			if (prevPaintColor != paintInColor) {
				selectMode = DEMO_MODE.Paint;
			}

			if (GUI.Button (new Rect (10, 240, 100, 30), "Colorize")) {
				RandomColors ();
			}

			if (GUI.Button (new Rect (120, 240, 110, 30), "Extrusion")) {
				if (hexa.extruded) {
					hexa.extruded = false;
					RandomColors ();

				} else {
					RandomExtrusion ();
				}
			}

			if (GUI.Button (new Rect (10, 280, 220, 30), "Random Obstacles")) {
				RandomObstacles ();
			}

			if (GUI.Button (new Rect (10, 320, 220, 30), "Clear Tiles")) {
				ClearTiles ();
			}

			if (GUI.Button (new Rect (10, 360, 100, 30), "Random Path")) {
				RandomPath ();
			}

			if (GUI.Button (new Rect (120, 360, 110, 30), "Custom Path")) {
				selectMode = DEMO_MODE.PathFind_StartTile;
			}

			if (GUI.Button (new Rect (10, 400, 220, 30), "Load Heightmap")) {
				if (divisions < 50) {
					Debug.Log ("Hint: increase hexasphere divisions for heightmap sample.");
				}
				hexa.extrudeMultiplier = 0.1f;
				hexa.ApplyHeightMap (heightMap, waterMask);
			}

			if (labelStyle == null) {
				labelStyle = new GUIStyle (GUI.skin.label);
				labelStyle.normal.textColor = Color.yellow;
			}

			switch (selectMode) {
			case DEMO_MODE.Paint:
				GUI.Label (new Rect (10, 440, 220, 30), "Click to paint a tile.", labelStyle);
				break;
			case DEMO_MODE.PathFind_StartTile:
				GUI.Label (new Rect (10, 440, 220, 30), "Click to select starting tile.", labelStyle);
				break;
			case DEMO_MODE.PathFind_EndTile:
				GUI.Label (new Rect (10, 440, 220, 30), "Click to finish path.", labelStyle);
				break;
			}

			if (hexa.lastHighlightedTileIndex >= 0) {
				GUI.Label (new Rect (10, 460, 220, 30), "Highlighted Tile # = " + hexa.lastHighlightedTileIndex.ToString (), labelStyle);
			}

			if (Input.GetKeyDown (KeyCode.S)) {
				hexa.SetTileColor (hexa.lastHighlightedTileIndex, Color.white);
			}

			if (GUI.Button (new Rect (Screen.width - 230, 10, 220, 30), "Spawn Objects")) {
				SpawnObjects ();
			}

			if (GUI.Button (new Rect (Screen.width - 230, 50, 220, 30), "Spawn Texts")) {
				SpawnTexts ();
			}

			if (GUI.Button (new Rect (Screen.width - 230, 90, 220, 30), "Parent Sprite")) {
				ParentSprite ();
			}

			if (GUI.Button(new Rect(Screen.width - 230, 130, 220, 30), "Spawn Capsule")) {
				SpawnCapsule();
			}

		}

		/// <summary>
		/// Colors all tiles with random colors
		/// </summary>
		void RandomColors () {
			Color[] colors = new Color[16];
			for (int k = 0; k < colors.Length; k++) {
				colors [k] = new Color (Random.value, Random.value, Random.value);
			}
			for (int k = 0; k < hexa.tiles.Length; k++) {
				Color co = colors [Random.Range (0, 7)];
				hexa.SetTileColor (k, co);
			}
		}

		/// <summary>
		/// Applies a random extrusion to all tiles
		/// </summary>
		void RandomExtrusion () {
			hexa.extruded = true;
			hexa.extrudeMultiplier = 0.05f;
			for (int k = 0; k < hexa.tiles.Length; k++) {
				float extrusionAmount = UnityEngine.Random.value;
				if (extrusionAmount > 0.015f) {
					hexa.SetTileExtrudeAmount (k, extrusionAmount);
					Color landColor = new Color (0, extrusionAmount, 0);
					hexa.SetTileColor (k, landColor);
				} else {
					hexa.SetTileExtrudeAmount (k, 0f);
					hexa.SetTileColor (k, new Color (0f, 0.411f, 0.58f));
				}
			}
			animateTileIndex = Random.Range (0, hexa.tiles.Length - 1);
			hexa.FlyTo (animateTileIndex, 2f);
		}

		void Update () {
			if (hexa.extruded && animateTileIndex >= 0 && hexa.tiles != null && animateTileIndex < hexa.tiles.Length) {
				// Animate tile height
				hexa.SetTileExtrudeAmount (animateTileIndex, Mathf.PingPong (Time.time, 1f));
				// Also animate neighbours
				Tile tile = hexa.tiles [animateTileIndex];
				hexa.SetTileExtrudeAmount (tile.neighbours, Mathf.PingPong (Time.time - 0.1f, 1f));
			}

			if (Input.GetKeyDown (KeyCode.G)) { // hides GUI buttons
				avoidGUI = !avoidGUI;
			}
		}

		/// <summary>
		/// Sets random tiles as obstacles (can't be crossed by pathfinding)
		/// </summary>
		void RandomObstacles () {
			int numObstacles = (int)(hexa.tiles.Length * 0.1);
			for (int k = 0; k < numObstacles; k++) {
				int tileIndex = Random.Range (0, hexa.tiles.Length - 1);
				hexa.SetTileColor (tileIndex, Color.black);
				hexa.SetTileCanCross (tileIndex, false);
			}
		}

		/// <summary>
		/// Clears all tile colors
		/// </summary>

		void ClearTiles () {
			// Hides and remove any assigned color/texture to the tiles
			hexa.ClearTiles ();
		}

		/// <summary>
		/// Draws a Random Path
		/// </summary>
		void RandomPath () {
			// Sets random start and end tiles
			int tileStart = Random.Range (0, hexa.tiles.Length - 1);
			int tileEnd = Random.Range (0, hexa.tiles.Length - 1);

			// Compute path
			List<int> steps = hexa.FindPath (tileStart, tileEnd);
			if (steps == null)
				return; // no path found between tileStart and tileEnd

			// Show the path
			hexa.SetTileColor (steps, Color.white, true);
			hexa.SetTileColor (tileStart, Color.red, true);
			hexa.SetTileColor (tileEnd, Color.green, true);

			hexa.FlyTo (tileEnd, 2f);
		}


		/// <summary>
		/// Manages user clicks
		/// </summary>
		void TileClick (int tileIndex) {
			switch (selectMode) {
			case DEMO_MODE.PathFind_StartTile:
				hexa.SetTileColor (tileIndex, Color.grey);
				selectedTile = tileIndex;
				selectMode = DEMO_MODE.PathFind_EndTile;
				break;
			case DEMO_MODE.Paint:
				if (paintInColor) {
					hexa.SetTileColor (tileIndex, Color.red);
				} else {
					hexa.SetTileTexture (tileIndex, paintTexture);
				}
				break;
			case DEMO_MODE.PathFind_EndTile:
				selectMode = DEMO_MODE.Idle;
				break;
			default:
				Debug.Log ("Clicked on tile #" + tileIndex);
				break;
			}
		}

		/// <summary>
		/// Draws a path as the user moves the mouse over the hexasphere
		/// </summary>
		void TileMouseOver (int tileIndex) {
			if (selectMode == DEMO_MODE.PathFind_EndTile && tileIndex != selectedTile) {
				// Clear tiles
				hexa.ClearTiles (true, false, false);
				// Color starting tile
				hexa.SetTileColor (selectedTile, Color.blue, true);
				// Color path
				List<int> steps = hexa.FindPath (selectedTile, tileIndex);
				if (steps != null) {
					hexa.SetTileColor (steps, Color.yellow, true);
					// Color end tile
					hexa.SetTileColor (tileIndex, Color.red, true);
				}
			}
		}

		void SpawnObjects () {

			// To apply a proper scale, get as a reference the length of a diagonal in tile 0 (note the "false" argument which specifies the position is in local coordinates)
			float size = Vector3.Distance (hexa.GetTileVertexPosition (0, 0, false), hexa.GetTileVertexPosition (0, 3, false));
			Vector3 scale = new Vector3 (size, size, size);

			// Make it 50% smaller so it does not occupy entire tile
			scale *= 0.5f;

			// Spawn 50 objects
			for (int k = 0; k < 50; k++) {
				GameObject obj = Instantiate(spawnObject);

				// Move object to center of tile (GetTileCenter also takes into account extrusion)
				int tileIndex = Random.Range (0, hexa.tiles.Length);
				obj.transform.position = hexa.GetTileCenter (tileIndex);

				// Parent it to hexasphere, so it rotates along it
				obj.transform.SetParent (hexa.transform);

				// Align with surface
				obj.transform.LookAt (hexa.transform.position);

				// Set scale
				obj.transform.localScale = scale;

				// Set a random color (notice the use of material and not sharedMaterial so every cube can have a different color)
				obj.GetComponent<Renderer> ().material.color = new Color (Random.value, Random.value, Random.value);
			}
		}

		void SpawnTexts () {
			Material textMaterial = null;

			// Spawn 50 objects
			for (int k = 0; k < 50; k++) {
				int tileIndex = Random.Range (0, hexa.tiles.Length);

				GameObject obj = new GameObject (tileIndex.ToString ());

				// Text
				TextMesh tm = obj.AddComponent<TextMesh> ();
				tm.text = obj.name;
				tm.alignment = TextAlignment.Center;
				tm.anchor = TextAnchor.MiddleCenter;
				tm.fontSize = 60;	// make font big so we reduce it with a small scale. It looks better.

				// Changes the material and shader of the text mesh
				Renderer renderer = obj.GetComponent<Renderer> ();
				if (textMaterial == null) {
					textMaterial = renderer.material; // retrieves an instanced copy of the current material so we can modify it freely
					textMaterial.shader = Shader.Find ("Hexasphere/Text");
					textMaterial.color = Color.red;
				}
				renderer.sharedMaterial = textMaterial;

				float scaleFactor = 0.6f;
				hexa.ParentAndAlignToTile (obj, tileIndex, 0, false, true, scaleFactor);
			}
		}


		void ParentSprite () {
			
			int tileIndex = Random.Range (0, hexa.tiles.Length);
			if (hexa.tiles [tileIndex].vertices.Length != 6)
				return;

			// Create the tile sprite
			GameObject obj = new GameObject ("Tile");
			SpriteRenderer renderer = obj.AddComponent<SpriteRenderer> ();
			renderer.sprite = sprite;

			// Move object to center of tile (GetTileCenter also takes into account extrusion)
			hexa.ParentAndAlignToTile (obj, tileIndex);

			// Visualize sprite
			hexa.FlyTo (tileIndex, 0.5f);

		}





		void SpawnCapsule() {

			int tileIndex = Random.Range(0, hexa.tiles.Length);
			if (hexa.tiles[tileIndex].vertices.Length != 6)
				return;

			if (capsule != null) {
				DestroyImmediate(capsule);
            }

			// Create the tile sprite
			capsule = Instantiate(prefabCapsule);

			// Parent it to hexasphere, so it rotates along it
			capsule.transform.SetParent(hexa.transform);

			// Position capsule on top of tile
			capsule.transform.position = hexa.GetTileCenter(tileIndex);

			// Visualize sprite
			hexa.FlyTo(tileIndex, 0.5f);

		}

	}
}