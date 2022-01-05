using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace HexasphereGrid {
	
	public class Triangle {

		public Point[] points;
		public int getOrderedFlag;

		Point centroid;
		bool centroIdComputed;

		public Triangle (Point point1, Point point2, Point point3, bool register = true) {
			this.points = new Point[] { point1, point2, point3 };
			if (register) {
				point1.RegisterTriangle (this);
				point2.RegisterTriangle (this);
				point3.RegisterTriangle (this);
			}
		}

		public bool isAdjacentTo (Triangle tri2) {
			// returns true if 2 of the vertices are the same
			bool match = false;
			for (int i = 0; i < 3; i++) {
				Point p1 = points [i];
				for (var j = 0; j < 3; j++) {
					Point p2 = tri2.points [j];
					if (p1.x == p2.x && p1.y == p2.y && p1.z == p2.z) {
						if (match)
							return true;
						match = true;
					}
				}
			}
			return false;
		}

		public Point GetCentroid () {
			if (centroIdComputed) {
				return centroid;
			}
			centroIdComputed = true;
			float x = (points [0].x + points [1].x + points [2].x) / 3;
			float y = (points [0].y + points [1].y + points [2].y) / 3;
			float z = (points [0].z + points [1].z + points [2].z) / 3;
			centroid = new Point (x, y, z);
			return centroid;
			
		}

	}



}
