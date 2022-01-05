using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HexasphereGrid;

namespace HexasphereGrid_Demos {
	public class Demo5 : MonoBehaviour {

		public Texture2D heightMap, worldColors;
		public float rotationSpeed = 10f;

		Hexasphere hexa;

		void Start () {
			// Gets the script for the "Hexasphere" gameobject
			hexa = Hexasphere.GetInstance ("Hexasphere");

			// Apply world texture
			hexa.ApplyColors (worldColors);

			// Apply world heights to vertices
			hexa.ApplyHeightMap (heightMap, 0.1f, null, false);
		}


		void Update() {
			hexa.transform.Rotate (-Camera.main.transform.right, rotationSpeed * Time.deltaTime, Space.World);
		}

	}
}