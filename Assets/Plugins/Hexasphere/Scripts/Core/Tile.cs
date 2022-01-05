using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HexasphereGrid {

	public partial class Tile {

		#region Public properties

		/// <summary>
		/// The index of this tile in the tiles list.
		/// </summary>
		public int index;
		
		/// <summary>
		/// The original points used to create the tile. Used internally. Use Vertices property to get the vertices in Vector3 format instead.
		/// </summary>
		public Point[] vertexPoints;

		/// <summary>
		/// Gets the center Point of this tile in local space coordinates
		/// </summary>
		public Point centerPoint;

		/// <summary>
		/// Gets the center of this tile in local space coordinates.
		/// </summary>
		public Vector3 center;

		/// <summary>
		/// Bitwise indicators of which vertices define a border
		/// </summary>
		public int borders;

		/// <summary>
		/// Gets the vertices in local space coordinates. Note that the grid contains a few pentagons.
		/// </summary>
		public Vector3[] vertices {
			get {
				if (!_verticesComputed) {
					ComputeVertices();
				}
				return _vertices;
			}
		}

        public Vector3 polygonCenter {
            get {
                if (!_verticesComputed) {
                    ComputeVertices();
                }
                Vector3 center = Vector3.zero;
                int vertexCount = _vertices.Length;
                for (int k = 0; k < vertexCount; k++) {
                    center.x += _vertices[k].x;
                    center.y += _vertices[k].y;
                    center.z += _vertices[k].z;
                }
                center.x /= vertexCount;
                center.y /= vertexCount;
                center.z /= vertexCount;
                return center;
            }
        }


        /// <summary>
        /// Gets the neighbours tiles.
        /// </summary>
        public Tile[] neighbours {
            get {
                if (!_neighboursComputed) {
                    ComputeNeighbours();
                }
                return _neighbours;
            }
        }

		
		/// <summary>
		/// Gets the neighbours tiles indices.
		/// </summary>
		public int[] neighboursIndices {
			get {
				if (!_neighboursComputed) {
					ComputeNeighbours();
				}
				return _neighboursIndices;
			}
		}

		/// <summary>
		/// Set by ApplyHeightMap() function if this tile contains water.
		/// </summary>
		public bool isWater;

		/// <summary>
		/// Sets if this tile can be crossed when using PathFinding functions.
		/// </summary>
		public bool canCross = true;

		/// <summary>
		/// The group of tiles to which this tile belongs to.
		/// </summary>
		public int group = 1;

		/// <summary>
		/// Tile texture rotation in radians
		/// </summary>
		public float rotation = 0;

		/// <summary>
		/// The tile mesh's renderer. Created when SetTileColor or SetTileTexture is used. Get the tile gameobject using renderer.gameObject
		/// </summary>
		public Renderer renderer;

		/// <summary>
		/// The base material assigned to this tile. 
		/// </summary>
		public Material customMat;

		/// <summary>
		/// The temporary material assigned to this tile. 
		/// </summary>
		public Material tempMat;

		/// <summary>
		/// Extrude amount for this tile. 0 = no extrusion, will render a flat tile which is faster.
		/// </summary>
		public float extrudeAmount = 0f;

		public int uvShadedChunkIndex;
		public int uvShadedChunkStart;
		public int uvShadedChunkLength;
		public int uvWireChunkIndex;
		public int uvWireChunkStart;
		public int uvWireChunkLength;

		/// <summary>
		/// Original value loaded from the heightmap
		/// </summary>
		public float heightMapValue;

		/// <summary>
		/// User-defined misc value (not used by Hexasphere)
		/// </summary>
		public string tag;

		/// <summary>
		/// User-defined misc value (not used by Hexasphere)
		/// </summary>
		public int tagInt;

		/// <summary>
		/// If the tile is visible.
		/// </summary>
		public bool visible;

		/// Used by pathfinding. Cost for crossing a cell for each side. Defaults to 1.
		public float crossCost = 1;

		/// <summary>
		/// Temporary total crossing cost for this tile updated when FindPath method is called.
		/// </summary>
		public float computedCrossCost;

		#endregion

		#region Internal logic

		Vector3[] _vertices;
		bool _verticesComputed;
		Tile[] _neighbours;
		int[] _neighboursIndices;
		bool _neighboursComputed;
		static Triangle[] tempTriangles = new Triangle[20];

		public Tile(Point centerPoint, int index) {
			this.index = index;
			this.centerPoint = centerPoint;
			this.centerPoint.tile = this;
			this.center = centerPoint.projectedVector3;
			this.visible = true;
			int facesCount = centerPoint.GetOrderedTriangles(tempTriangles);
			vertexPoints = new Point[facesCount];

			for (int f = 0; f < facesCount; f++) {
				vertexPoints[f] = tempTriangles[f].GetCentroid();
			}

			// resort if wrong order
			if (facesCount == 6) {
				Vector3 p0 = (Vector3)vertexPoints[0];
				Vector3 p1 = (Vector3)vertexPoints[1];
				Vector3 p5 = (Vector3)vertexPoints[5];
				Vector3 v0 = p1 - p0;
				Vector3 v1 = p5 - p0;
				Vector3 cp = Vector3.Cross(v0, v1);
				float dp = Vector3.Dot(cp, p1);
				if (dp < 0) {
					Point aux;
					aux = vertexPoints[0];
					vertexPoints[0] = vertexPoints[5];
					vertexPoints[5] = aux;
					aux = vertexPoints[1];
					vertexPoints[1] = vertexPoints[4];
					vertexPoints[4] = aux;
					aux = vertexPoints[2];
					vertexPoints[2] = vertexPoints[3];
					vertexPoints[3] = aux;
				}
			} else if (facesCount == 5) {
				Vector3 p0 = (Vector3)vertexPoints[0];
				Vector3 p1 = (Vector3)vertexPoints[1];
				Vector3 p4 = (Vector3)vertexPoints[4];
				Vector3 v0 = p1 - p0;
				Vector3 v1 = p4 - p0;
				Vector3 cp = Vector3.Cross(v0, v1);
				float dp = Vector3.Dot(cp, p1);
				if (dp < 0) {
					Point aux;
					aux = vertexPoints[0];
					vertexPoints[0] = vertexPoints[4];
					vertexPoints[4] = aux;
					aux = vertexPoints[1];
					vertexPoints[1] = vertexPoints[3];
					vertexPoints[3] = aux;
				}
			}
		}

		static List<int> tempInt = new List<int>(6);
		static List<Tile> temp = new List<Tile>(6);

		void ComputeNeighbours() {
			tempInt.Clear();
			temp.Clear();
			for (int k = 0; k < centerPoint.triangleCount; k++) {
				Triangle other = centerPoint.triangles[k];
				for (int j = 0; j < 3; j++) {
					Tile tile = other.points[j].tile;
					if (tile != null && other.points[j] != centerPoint && !tempInt.Contains(tile.index)) {
						temp.Add(tile);
						tempInt.Add(tile.index);
					}
				}
			}
			_neighbours = temp.ToArray();
			_neighboursIndices = tempInt.ToArray();
			_neighboursComputed = true;
		}

		public void ComputeVertices() {
			int l = vertexPoints.Length;
			_vertices = new Vector3[l];
			for (int k = 0; k < l; k++) {
				_vertices[k] = vertexPoints[k].projectedVector3;
			}
			_verticesComputed = true;
		}

		#endregion
	}
}