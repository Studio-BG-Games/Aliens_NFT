using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;

namespace HexasphereGrid_Demos {
	public class Demo3 : MonoBehaviour {

		[Tooltip ("Texture with elevation data. The red channel is used as the height value.")]
		public Texture2D heightMap;

		[Tooltip ("Texture with water mask data. The alpha channel determines the location of water (values < constant threshold)")]
		public Texture2D waterMask;

		Vector3 epicenter;
		Hexasphere hexa;
		float lastQuakeTime;

		void Start () {
			// Gets the script for the "Hexasphere" gameobject
			hexa = Hexasphere.GetInstance ("Hexasphere");

			// Load world heightmap and watermask
			hexa.ApplyHeightMap (heightMap, waterMask);

			NewQuake ();
		}

		void Update () {
			// Updates extrusion amount for each tile; doing it this way for all tiles and every frame can be consuming, but this is an example
			float time = Time.time * 5f;
			for (int tileIndex = 0; tileIndex < hexa.tiles.Length; tileIndex++) {
				Tile tile = hexa.tiles [tileIndex];

				// only affect water tiles
				if (!tile.isWater) continue;

				// directly get the center from the field value instead of calling GetTileCenter
				Vector3 pos = tile.center;	

				// distance from tile to epicenter
				float distance = 1.0f + (pos - epicenter).sqrMagnitude * 50f;

				// calculate wave
				float extrusionAmount = 0.5f + Mathf.Sin (-time + distance * 10f) * 0.5f / (distance * distance);

				// make average with previous value to smooth changes
				extrusionAmount = (extrusionAmount + tile.extrudeAmount) * 0.5f;

				// Set new extrusion amount for this tile
				hexa.SetTileExtrudeAmount (tileIndex, extrusionAmount);
			}

			if (Time.time - lastQuakeTime > 10f) {
				NewQuake ();
			}
		}


		void NewQuake () {
												
			lastQuakeTime = Time.time;

			// Random epicenter
			epicenter = Random.onUnitSphere * 0.5f;

			// Focus on epicenter
			hexa.FlyTo (epicenter, 1f);
		}
			
	}
}