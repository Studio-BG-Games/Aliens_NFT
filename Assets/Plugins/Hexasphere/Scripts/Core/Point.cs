using UnityEngine;
using System;
using System.Collections.Generic;

namespace HexasphereGrid {

	public class Point: IEqualityComparer<Point>, IEquatable<Point> {

		public float x, y, z;

		float _elevation;

		public float elevation {
			get {
				return _elevation;
			}
			set {
				if (_elevation != value) {
					_elevation = value;
					_projectedVector3Computed = false;
				}
			}
		}


		Vector3 _projectedVector3;
		bool _projectedVector3Computed;

		public Vector3 projectedVector3 {
			get {
				if (_projectedVector3Computed) {
					return _projectedVector3;
				} else {
					ComputeProjectedVertex ();
					return _projectedVector3;
				}
			}
		}

		public Triangle[] triangles;
		public int triangleCount;
		public Tile tile;

		int hashCode;

		public Point (float x, float y, float z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public List<Point>Subdivide (Point point, int count, GetCachedPointDelegate checkPoint) {
			List<Point> segments = new List<Point> (count + 1);
			segments.Add (this);

			double dx = point.x - this.x;
			double dy = point.y - this.y;
			double dz = point.z - this.z;
			double doublex = (double)this.x;
			double doubley = (double)this.y;
			double doublez = (double)this.z;
			double doubleCount = (double)count;
			for (int i = 1; i < count; i++) {
				Point np = new Point (
					           (float)(doublex + dx * (double)i / doubleCount),
					           (float)(doubley + dy * (double)i / doubleCount),
					           (float)(doublez + dz * (double)i / doubleCount)
				           );
				np = checkPoint (np);
				segments.Add (np);
			}
			
			segments.Add (point);
			
			return segments;
			
		}

		public void ComputeProjectedVertex () {
			double len = 2.0 * System.Math.Sqrt ((double)x * (double)x + (double)y * (double)y + (double)z * (double)z);
			len /= (1.0 + _elevation);
			double xx = x / len;
			double yy = y / len;
			double zz = z / len;
			_projectedVector3 = new Vector3 ((float)xx, (float)yy, (float)zz);
			_projectedVector3Computed = true;
		}

		public void RegisterTriangle (Triangle triangle) {
			if (triangles == null)
				triangles = new Triangle[6];
			triangles [triangleCount++] = triangle;
		}

		public static int flag = 0;

		public int GetOrderedTriangles (Triangle[] tempTriangles) {
			if (triangleCount == 0) {
				return 0;
			}
			tempTriangles [0] = triangles [0];
			int count = 1;
			flag++;
			for (int i = 0; i < triangleCount - 1; i++) {
				for (int j = 1; j < triangleCount; j++) {
					if (triangles [j].getOrderedFlag != flag && tempTriangles [i] != null && triangles [j].isAdjacentTo (tempTriangles [i])) {
						tempTriangles [count++] = triangles [j];
						triangles [j].getOrderedFlag = flag;
						break;
					}
				}
			}

			return count;
		}

		
		public override string ToString () {
			return (int)(this.x * 100f) / 100f + "," + (int)(this.y * 100f) / 100f + "," + (int)(this.z * 100f) / 100f;
			
		}

		public override bool Equals (object obj) {
			if (obj is Point) {
				Point other = (Point)obj;
				return x == other.x && y == other.y && z == other.z;
			}
			return false;
		}

		public bool Equals (Point p2) {
			return x == p2.x && y == p2.y && z == p2.z;
		}

		public bool Equals (Point p1, Point p2) {
			return  p1.x == p2.x && p1.y == p2.y && p1.z == p2.z;
		}

		public override int GetHashCode () {
			if (hashCode == 0) {
				hashCode = x.GetHashCode () ^ y.GetHashCode () << 2 ^ z.GetHashCode () >> 2;
			}
			return hashCode;
		}

		public int GetHashCode (Point p) {
			if (hashCode == 0) {
				hashCode = p.x.GetHashCode () ^ p.y.GetHashCode () << 2 ^ p.z.GetHashCode () >> 2;
			}
			return hashCode;
		}

		public static explicit operator Vector3 (Point point) {
			return new Vector3 (point.x, point.y, point.z);
		}

		public static Point operator * (Point point, float v) {
			return new Point (point.x * v, point.y * v, point.z * v);
		}

		public static float SqrDistance (Point p1, Point p2) {
			float dx = p2.x - p1.x;
			float dy = p2.y - p1.y;
			float dz = p2.z - p1.z;
			return dx * dx + dy * dy + dz * dz;
		}

		public static Point Average (Point p1, Point p2) {
			float mx = (p1.x + p2.x) * 0.5f;
			float my = (p1.y + p2.y) * 0.5f;
			float mz = (p1.z + p2.z) * 0.5f;
			return new Point (mx, my, mz);
		}

		public static float Distance (Point p1, Point p2) {
			float dx = p2.x - p1.x;
			float dy = p2.y - p1.y;
			float dz = p2.z - p1.z;
			return (float)Math.Sqrt (dx * dx + dy * dy + dz * dz);
		}

		public void ClampDistance (Point center, float factor) {
			float dx = x - center.x;
			float dy = y - center.y;
			float dz = z - center.z;
			x = center.x + dx * factor;
			y = center.y + dy * factor;
			z = center.z + dz * factor;
			_projectedVector3Computed = false;
		}

		public void Add (Point p) {
			x += p.x;
			y += p.y;
			z += p.z;
			_projectedVector3Computed = false;
		}

		public void DivideBy (float d) {
			x /= d;
			y /= d;
			z /= d;
			_projectedVector3Computed = false;
		}

	}

}
