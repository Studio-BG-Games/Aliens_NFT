using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace HexasphereGrid {
	
	/* Event definitions */
	public delegate float PathFindingEvent (int tileIndex);


	public enum HeuristicFormula {
		SphericalDistance = 1,
		Euclidean = 2,
		EuclideanNoSQR = 3
	}


	public partial class Hexasphere : MonoBehaviour {

		public const int ALL_TILES = ~0;

		/// <summary>
		/// Fired when path finding algorithmn evaluates a tile. Return the increased cost for tile.
		/// </summary>
		public event PathFindingEvent OnPathFindingCrossTile;

		[SerializeField]
		HeuristicFormula
			_pathFindingHeuristicFormula = HeuristicFormula.SphericalDistance;

		/// <summary>
		/// The path finding heuristic formula to estimate distance from current position to destination
		/// </summary>
		public HeuristicFormula pathFindingHeuristicFormula {
			get { return _pathFindingHeuristicFormula; }
			set {
				if (value != _pathFindingHeuristicFormula) {
					_pathFindingHeuristicFormula = value;
				}
			}
		}

		[SerializeField]
		int
			_pathFindingSearchLimit = 2000;

		/// <summary>
		/// The maximum path length.
		/// </summary>
		public int pathFindingSearchLimit {
			get { return _pathFindingSearchLimit; }
			set {
				if (value != _pathFindingSearchLimit) {
					_pathFindingSearchLimit = value;
				}
			}
		}


		[SerializeField]
		bool
			_pathFindingUseExtrusion = false;

		/// <summary>
		/// If extrusion (altitude) should be accounted for pathfinding computation.
		/// </summary>
		public bool pathFindingUseExtrusion {
			get { return _pathFindingUseExtrusion; }
			set {
				if (value != _pathFindingUseExtrusion) {
					_pathFindingUseExtrusion = value;
					needRefreshRouteMatrix = true;
				}
			}
		}


		[SerializeField]
		int
			_pathFindingExtrusionWeight = 10000;

		/// <summary>
		/// The extrusion contribution to tile crossing costs (extrusion ranges 0..1 and is multiplied by this value then added to the tile crossing cost)
		/// </summary>
		public int pathFindingExtrusionWeight {
			get { return _pathFindingExtrusionWeight; }
			set {
				if (value != _pathFindingExtrusionWeight) {
					_pathFindingExtrusionWeight = value;
					needRefreshRouteMatrix = true;
				}
			}
		}


		#region Public Path Finding functions

		/// <summary>
		/// Returns an optimal path from startPosition to endPosition with options
		/// </summary>
		/// <returns>The route consisting of a list of tile indexes.</returns>
		/// <param name="tileIndexStart">Start position in map coordinates (-0.5...0.5)</param>
		/// <param name="tileIndexEnd">End position in map coordinates (-0.5...0.5)</param>
		/// <param name="indices">List where results are written. Must be an initialized list.</param>
		/// <param name="searchLimit">Maximum number of steps (0=unlimited)</param>
		/// <param name="groupMask">Optional bitwise mask for choosing valid tiles. By default, a tile belongs to group 1. Use SetTileGroup to change any tile group.</param>
		/// <param name="ignoreTileCanCross">If set to true, the tile canCross property is ignored.</param>
		public int FindPath (int tileIndexStart, int tileIndexEnd, List<int>indices, int searchLimit = 0, int groupMask = ALL_TILES, bool ignoreTileCanCross = false) {

			int startingPoint = tileIndexStart;
			int endingPoint = tileIndexEnd;
			if (indices == null) {
				Debug.LogError ("FindPathNonAlloc: supplied indices list must be initialized.");
				return 0;
			}

			// Minimum distance for routing?
			indices.Clear();
			if (startingPoint != endingPoint) {
				ComputeRouteMatrix (groupMask);
				mSearchLimit = searchLimit == 0 ? _pathFindingSearchLimit : searchLimit;
				mIgnoreTileCanCross = ignoreTileCanCross;
				List<PFClosedNode> route = FindPathFast (startingPoint, endingPoint);
				if (route != null) {
					int routeCount = route.Count;
					for (int r = routeCount - 2; r > 0; r--) {
						int t = route [r].index;
						indices.Add (t);
					}
					indices.Add (tileIndexEnd);
				} else {
					return 0;	// no route available
				}
			}
			return indices.Count;
		}


		/// <summary>
		/// Returns an optimal path from startPosition to endPosition with options or null if no path found
		/// </summary>
		/// <returns>The route consisting of a list of tile indexes.</returns>
		/// <param name="tileIndexStart">Start position in map coordinates (-0.5...0.5)</param>
		/// <param name="tileIndexEnd">End position in map coordinates (-0.5...0.5)</param>
		/// <param name="searchLimit">Maximum number of steps (0=unlimited)</param>
		/// <param name="groupMask">Optional bitwise mask for choosing valid tiles. By default, a tile belongs to group 1. Use SetTileGroup to change any tile group.</param>
		/// <param name="ignoreTileCanCross">If set to true, the tile canCross property is ignored.</param>
		public List<int> FindPath (int tileIndexStart, int tileIndexEnd, int searchLimit = 0, int groupMask = ALL_TILES, bool ignoreTileCanCross = false) {
			List<int> results = new List<int> ();
			int count = FindPath (tileIndexStart, tileIndexEnd, results, searchLimit, groupMask, ignoreTileCanCross);
			return count == 0 ? null : results;
		}

		#endregion


	
	}
}

