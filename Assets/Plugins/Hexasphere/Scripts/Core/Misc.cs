using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HexasphereGrid {
				
	public static class Misc {

		public static Vector3 Vector4zero = Vector4.zero;
		public static Vector3 Vector3one = Vector3.one;
		public static Vector3 Vector3zero = Vector3.zero;
		public static Vector3 Vector3up = Vector3.up;
		public static Vector2 Vector2one = Vector2.one;
		public static Vector2 Vector2zero = Vector2.zero;
		public static Color32 Color32White = Color.white;

		public static float Vector3SqrDistance (Vector3 a, Vector3 b) {
			float dx = a.x - b.x;
			float dy = a.y - b.y;
			float dz = a.z - b.z;
			return dx * dx + dy * dy + dz * dz;
		}

	}

}