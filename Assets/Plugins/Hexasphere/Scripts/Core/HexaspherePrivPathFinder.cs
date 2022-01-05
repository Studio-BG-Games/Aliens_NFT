using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HexasphereGrid {

	struct PFClosedNode {
		public float f;
		public float g;
		public int index;
		public int prevIndex;
	}

	struct PFNodeFast {
		public float f;
		public float g;
		public int prevIndex;
		public byte status;
	}

	public partial class Hexasphere : MonoBehaviour {


		bool needRefreshRouteMatrix;

		PQInt open;
		PFNodeFast[] pfCalc;
		byte openTileValue = 1;
		byte closeTileValue = 2;
		List<PFClosedNode> close = new List<PFClosedNode> ();
		int mSearchLimit = 2000;
		bool mIgnoreTileCanCross;
		int lastRouteMatrixGroupMask = -1;

		void ComputeRouteMatrix (int groupMask) {

			if (!needRefreshRouteMatrix && lastRouteMatrixGroupMask == groupMask)
				return;

			needRefreshRouteMatrix = false;
			lastRouteMatrixGroupMask = groupMask;

			// Compute route
			for (int j = 0; j < tiles.Length; j++) {
				if ((mIgnoreTileCanCross || tiles [j].canCross) && (tiles [j].group & groupMask) > 0) {   // set navigation bit
					float cost = tiles [j].crossCost;
					if (_pathFindingUseExtrusion && _extruded) {
						cost += tiles [j].extrudeAmount * _pathFindingExtrusionWeight;
					}
					tiles[j].computedCrossCost = cost;
				} else {       // clear navigation bit
					tiles[j].computedCrossCost = 0;
				}
			}

			if (pfCalc == null || pfCalc.Length != tiles.Length) {
				pfCalc = new PFNodeFast[tiles.Length];
			}
			if (open == null) {
				open = new PQInt (new PFNodesComparer (pfCalc), tiles.Length);
			} else {
				open.Clear ();
				Array.Clear (pfCalc, 0, pfCalc.Length);
				PFNodesComparer comparer = (PFNodesComparer)open.comparer;
				comparer.SetMatrix (pfCalc);
			}
		}

		List<PFClosedNode> FindPathFast (int tileStartIndex, int tileEndIndex) {
			bool found = false;
			int stepsCounter = 0;
			if (openTileValue > 250) {
				openTileValue = 1;
				closeTileValue = 2;
			} else {
				openTileValue += 2;
				closeTileValue += 2;
			}
			open.Clear ();
			close.Clear ();

			int currentTileIndex = tileStartIndex;
			int nextTileIndex;
			Vector3 destinationCenter = tiles [tileEndIndex].center;

			pfCalc [currentTileIndex].g = 0;
			pfCalc [currentTileIndex].f = 2;
			pfCalc [currentTileIndex].prevIndex = tileStartIndex;
			pfCalc [currentTileIndex].status = openTileValue;

			open.Push (currentTileIndex);

			while (open.tilesCount > 0) {
				currentTileIndex = open.Pop ();

				if (pfCalc [currentTileIndex].status == closeTileValue)
					continue;

				if (currentTileIndex == tileEndIndex) {
					pfCalc [currentTileIndex].status = closeTileValue;
					found = true;
					break;
				}

				if (stepsCounter >= mSearchLimit) {
					return null;
				}

				int maxi = tiles [currentTileIndex].neighbours.Length;
				for (int i = 0; i < maxi; i++) {
					nextTileIndex = tiles [currentTileIndex].neighboursIndices [i];

					float gridValue = tiles [nextTileIndex].computedCrossCost;
					if (gridValue == 0)
						continue;
					// Custom tile crossing logic
					if (OnPathFindingCrossTile != null) {
						gridValue += OnPathFindingCrossTile (nextTileIndex);
					}

					float mNewG = pfCalc [currentTileIndex].g + gridValue;

					if (pfCalc [nextTileIndex].status == openTileValue || pfCalc [nextTileIndex].status == closeTileValue) {
						if (pfCalc [nextTileIndex].g <= mNewG)
							continue;
					}

					pfCalc [nextTileIndex].prevIndex = currentTileIndex;
					pfCalc [nextTileIndex].g = mNewG;

					float dist = 1;
					switch (_pathFindingHeuristicFormula) {
					case HeuristicFormula.SphericalDistance:
						dist = Vector3.Angle (destinationCenter, tiles [nextTileIndex].center);
						break;
					case HeuristicFormula.Euclidean:
						dist = Vector3.Distance (destinationCenter, tiles [nextTileIndex].center);
						break;
					case HeuristicFormula.EuclideanNoSQR:
						dist = Vector3.SqrMagnitude (destinationCenter - tiles [nextTileIndex].center);
						break;
					}
					pfCalc [nextTileIndex].f = mNewG + 2f * dist;
					open.Push (nextTileIndex);
					pfCalc [nextTileIndex].status = openTileValue;
				}

				stepsCounter++;
				pfCalc [currentTileIndex].status = closeTileValue;
			}

			if (found) {
				close.Clear ();
				int pos = tileEndIndex;

				PFNodeFast tileTmp = pfCalc [tileEndIndex];
				PFClosedNode stepTile;
				stepTile.f = tileTmp.f;
				stepTile.g = tileTmp.g;
				stepTile.prevIndex = tileTmp.prevIndex;
				stepTile.index = tileEndIndex;
				while (stepTile.index != stepTile.prevIndex) {
					close.Add (stepTile);
					pos = stepTile.prevIndex;
					tileTmp = pfCalc [pos];
					stepTile.f = tileTmp.f;
					stepTile.g = tileTmp.g;
					stepTile.prevIndex = tileTmp.prevIndex;
					stepTile.index = pos;
				}
				close.Add (stepTile);
				return close;
			}
			return null;
		}

	}

	class PFNodesComparer : IComparer<int> {
		PFNodeFast[] m;

		public PFNodesComparer (PFNodeFast[] nodes) {
			m = nodes;
		}

		public int Compare (int a, int b) {
			if (m [a].f > m [b].f)
				return 1;
			else if (m [a].f < m [b].f)
				return -1;
			return 0;
		}

		public void SetMatrix (PFNodeFast[] nodes) {
			m = nodes;
		}
	}

	class PQInt {
		int[] tiles;
		IComparer<int> mComparer;
		public int tilesCount;

		public IComparer<int> comparer { get { return mComparer; } }

		public PQInt (IComparer<int> comparer, int capacity) {
			mComparer = comparer;
			tiles = new int[capacity];
			tilesCount = 0;
		}

		void Swap (int i, int j) {
			int h = tiles [i];
			tiles [i] = tiles [j];
			tiles [j] = h;
		}

		int Compare (int i, int j) {
			return mComparer.Compare (tiles [i], tiles [j]);
		}

		public int Pop () {
			int result = tiles [0];
			int p = 0, p1, p2, pn;
			int count = tilesCount - 1;
			tiles [0] = tiles [count];
			tilesCount--;
			do {
				pn = p;
				p1 = 2 * p + 1;
				p2 = p1 + 1;
				if (count > p1 && Compare (p, p1) > 0)
					p = p1;
				if (count > p2 && Compare (p, p2) > 0)
					p = p2;

				if (p == pn)
					break;
				Swap (p, pn);
			} while (true);

			return result;
		}


		public int Push (int item) {
			int p = tilesCount, p2;
			tiles [tilesCount] = item;
			tilesCount++;
			do {
				if (p == 0)
					break;
				p2 = (p - 1) / 2;
				if (Compare (p, p2) < 0) {
					Swap (p, p2);
					p = p2;
				} else
					break;
			} while (true);
			return p;
		}

		public void Clear () {
			tilesCount = 0;
		}


	}


}