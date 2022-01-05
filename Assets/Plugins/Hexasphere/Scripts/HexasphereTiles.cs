/// <summary>
/// Hexasphere Grid System
/// Created by Ramiro Oliva (Kronnect)
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace HexasphereGrid {

	public partial class Hexasphere: MonoBehaviour {

		#region Public API

		/// <summary>
		/// Array of generated tiles.
		/// </summary>
		public Tile[] tiles;

		/// <summary>
		/// Returns the index of the tile in the tiles list
		/// </summary>
		public int GetTileIndex (Tile tile) {
			if (tiles == null)
				return -1;
			return tile.index;
		}


		/// <summary>
		/// Sets the tile material.
		/// </summary>
		/// <returns><c>true</c>, if tile material was set, <c>false</c> otherwise.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="mat">Material to be used.</param>
		/// <param name="temporary">If set to <c>true</c> the material is not saved anywhere and will be restored to default tile material when tile gets unselected.</param>
		public bool SetTileMaterial (int tileIndex, Material mat, bool temporary = false) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			Tile tile = tiles [tileIndex];
			if (temporary) {
				if (tile.tempMat == mat)
					return false; // nochanges
				if (tile.renderer == null) {
					GenerateTileMesh (tileIndex, mat);
				} else {
					tile.renderer.sharedMaterial = mat;
					tile.renderer.enabled = true;
				}
			} else {
				if (tile.customMat == mat)
					return false; // nochanges
				Color32 matColor = Misc.Color32White;
				if (mat.HasProperty(ShaderParams.Color)) {
					matColor = mat.color;
				} else if (mat.HasProperty(ShaderParams.BaseColor)) {
					matColor = mat.GetColor(ShaderParams.BaseColor);
                }
				pendingColorsUpdate = true;
				Texture matTexture = null;
				if (mat.HasProperty(ShaderParams.MainTex)) {
					matTexture = mat.mainTexture;
                } else if (mat.HasProperty(ShaderParams.BaseMap)) {
					matTexture = mat.GetTexture(ShaderParams.BaseMap);
                }
				if (matTexture != null) {
					pendingTextureArrayUpdate = true;
				} else {
					List<Color32> colorChunk = colorShaded [tile.uvShadedChunkIndex];
					for (int k = 0; k < tile.uvShadedChunkLength; k++) {
						colorChunk [tile.uvShadedChunkStart + k] = matColor;
					}
					colorShadedDirty [tile.uvShadedChunkIndex] = true;
				}
				// Only if wire color is set to use the tile color
				List<Color32> colorWireChunk = colorWire [tile.uvWireChunkIndex];
				if (!_wireframeColorFromTile) {
					matColor = Misc.Color32White;
				}
																				
				for (int k = 0; k < tile.uvWireChunkLength; k++) {
					colorWireChunk [tile.uvWireChunkStart + k] = matColor;
				}
				colorWireDirty [tile.uvWireChunkIndex] = true;

				if (_smartEdges) {
					needRegenerateWireframe = true;
					shouldUpdateMaterialProperties = true;
				}
			}

			if (mat != highlightMaterial) {
				if (temporary) {
					tile.tempMat = mat;
				} else {
					tile.customMat = mat;
				}
			}

			if (highlightMaterial != null && tile == lastHighlightedTile) {
				if (tile.renderer != null) {
					tile.renderer.sharedMaterial = highlightMaterial;
				}
				Material srcMat = null;
				if (tile.tempMat != null) {
					srcMat = tile.tempMat;
				} else if (tile.customMat != null) {
					srcMat = tile.customMat;
                }
				if (srcMat != null) {
					Color32 color = Misc.Color32White;
					if (srcMat.HasProperty(ShaderParams.Color)) {
						color = srcMat.color;
                    } else if (srcMat.HasProperty(ShaderParams.BaseColor)) {
						color = srcMat.GetColor(ShaderParams.BaseColor);
                    }
					highlightMaterial.SetColor (ShaderParams.Color2, color);
					Texture tempMatTexture = null;
					if (srcMat.HasProperty(ShaderParams.MainTex)) {
						tempMatTexture = srcMat.mainTexture;
                    } else if (srcMat.HasProperty(ShaderParams.BaseMap)) {
						tempMatTexture = srcMat.GetTexture(ShaderParams.BaseMap);
					}
					if (tempMatTexture != null && highlightMaterial.HasProperty (ShaderParams.MainTex)) {
						highlightMaterial.mainTexture = tempMatTexture;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Sets the color of the tile.
		/// </summary>
		/// <returns><c>true</c>, if tile color was set, <c>false</c> otherwise.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="color">Color.</param>
		/// <param name="temporary">If set to <c>true</c> the tile is colored temporarily and returns to default color when it gets unselected.</param>
		public bool SetTileColor (int tileIndex, Color color, bool temporary = false) {
			Material mat = GetCachedMaterial (color);
			return SetTileMaterial (tileIndex, mat, temporary);
		}

		
		/// <summary>
		/// Sets the color of a list of tiles.
		/// </summary>
		/// <returns><c>true</c>, if tile color was set, <c>false</c> otherwise.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="color">Color.</param>
		/// <param name="temporary">If set to <c>true</c> the tile is colored temporarily and returns to default color when it gets unselected.</param>
		public void SetTileColor (List<int> tileIndices, Color color, bool temporary = false) {
			if (tileIndices == null)
				return;
			Material mat = GetCachedMaterial (color);
			int tc = tileIndices.Count;
			for (int k = 0; k < tc; k++) {
				int tileIndex = tileIndices [k];
				SetTileMaterial (tileIndex, mat, temporary);
				if (!temporary) {
					Tile tile = tiles [tileIndex];
					colorShadedDirty [tile.uvShadedChunkIndex] = true;
				}
			}
			if (!temporary) {
				pendingColorsUpdate = true;
			}
		}

		/// <summary>
		/// Sets the texture of the tile.
		/// </summary>
		/// <returns><c>true</c>, if tile color was set, <c>false</c> otherwise.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="texture">Color.</param>
		/// <param name="temporary">If set to <c>true</c> the tile is colored temporarily and returns to default color when it gets unselected.</param>
		public bool SetTileTexture (int tileIndex, Texture2D texture, bool temporary = false) {
			if (!temporary)
				pendingTextureArrayUpdate = true;
			return SetTileTexture (tileIndex, texture, Color.white, temporary);
		}

		/// <summary>
		/// Sets the texture and tint color of the tile.
		/// </summary>
		/// <returns><c>true</c>, if tile color was set, <c>false</c> otherwise.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="texture">Color.</param>
		/// <param name="tint">Optional tint color.</param>
		/// <param name="temporary">If set to <c>true</c> the tile is colored temporarily and returns to default color when it gets unselected.</param>
		public bool SetTileTexture (int tileIndex, Texture2D texture, Color tint, bool temporary = false) {
			Material mat = GetCachedMaterial (tint, texture);
			return SetTileMaterial (tileIndex, mat, temporary);
		}

		/// <summary>
		/// Sets the texture (by texture index in the global texture array) and tint color of the tile
		/// </summary>
		/// <returns><c>true</c>, if tile texture was set, <c>false</c> otherwise.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="textureIndex">Texture index.</param>
		/// <param name="tint">Tint.</param>
		/// <param name="temporary">If set to <c>true</c> temporary.</param>
		public bool SetTileTexture (int tileIndex, int textureIndex, Color tint, bool temporary = false) {
			Texture2D texture = null;
			if (textureIndex >= 0 && textureIndex < textures.Length) {
				texture = textures [textureIndex];
			}
			return SetTileTexture (tileIndex, texture, tint, temporary);
		}


		/// <summary>
		/// Sets texture rotation in radians of tile
		/// </summary>
		public bool SetTileTextureRotation (int tileIndex, float rotation) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			if (rotation != tiles [tileIndex].rotation) {
				if (tileIndex == lastHighlightedTileIndex) {
					HideHighlightedTile ();
				}
				if (tiles [tileIndex].renderer != null) {
					DestroyImmediate (tiles [tileIndex].renderer);
					tiles [tileIndex].renderer = null;
				}
				tiles [tileIndex].rotation = rotation;
				pendingTextureArrayUpdate = true;
			}
			return true;
		}

		/// <summary>
		/// Returns tile texture rotation
		/// </summary>
		/// <param name="tileIndex">Tile index.</param>
		public float GetTileTextureRotation (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return 0;
			return tiles [tileIndex].rotation;
		}


		/// <summary>
		/// Returns the angle between vertex0 and North (specifically it computes the signed angle between two vectors: v0= vertex 0 - tile center and v1 = North - tile center)
		/// </summary>
		/// <returns>The tile rotation.</returns>
		/// <param name="tileIndex">Tile index.</param>
		public float GetTileVertex0Angle (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return 0;

			Vector3 tileCenter = (tiles [tileIndex].vertices [0] + tiles [tileIndex].vertices [3]) * 0.5f;
			Vector3 v0 = tiles [tileIndex].vertices [0] - tileCenter;

			float angle;
			if (tileCenter.y > 0) {
				Vector3 v1 = new Vector3 (0, 0.5f, 0) - tileCenter;
				angle = -SignedAngle (v0, v1, tileCenter);
			} else {
				Vector3 v1 = new Vector3 (0, -0.5f, 0) - tileCenter;
				angle = -SignedAngle (v0, v1, tileCenter) + 180f;
			}
			return angle * Mathf.Deg2Rad;
		}



		/// <summary>
		/// Sets texture rotation of tile so its top edge points to North
		/// </summary>
		public bool SetTileTextureRotationToNorth (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			float angle = GetTileVertex0Angle (tileIndex);
			float rotation = -angle - Mathf.PI * 0.5f;
			return SetTileTextureRotation (tileIndex, rotation);
		}


		/// <summary>
		/// Returns current tile's fill texture index (if texture exists in textures list).
		/// Texture index is from 1..32. It will return 0 if texture does not exist or it does not match any texture in the list of textures.
		/// </summary>
		public int GetTileTextureIndex (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length) {
				return 0;
			}
			Material mat = tiles [tileIndex].customMat;
			if (mat == null || !mat.HasProperty (ShaderParams.MainTex)) {
				return 0;
			}
			Texture2D tex = (Texture2D)mat.mainTexture;
			for (int k = 1; k < textures.Length; k++) {
				if (tex == textures [k])
					return k;
			}
			return 0;
		}


		/// <summary>
		/// Sets if path finding can cross this tile.
		/// </summary>
		public bool SetTileCanCross (int tileIndex, bool canCross) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			tiles [tileIndex].canCross = canCross;
			needRefreshRouteMatrix = true;
			return true;
		}


		/// <summary>
		/// Sets the crossing cost for this tile.
		/// </summary>
		public bool SetTileCrossCost (int tileIndex, float crossCost) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			tiles [tileIndex].crossCost = crossCost;
			needRefreshRouteMatrix = true;
			return true;
		}


		/// <summary>
		/// Gets the crossing cost for a given tile.
		/// </summary>
		public float GetTileCrossCost (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return 0;
			return tiles [tileIndex].crossCost;
		}


		/// <summary>
		/// Specifies the tile group (by default 1) used by FindPath tileGroupMask optional argument
		/// </summary>
		public bool SetTileGroup (int tileIndex, int group) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			tiles [tileIndex].group = group;
			needRefreshRouteMatrix = true;
			return true;
		}

		/// <summary>
		/// Returns tile group (default 1)
		/// </summary>
		public int GetTileGroup (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return -1;
			return tiles [tileIndex].group;
		}


		/// <summary>
		/// Sets the tile extrude amount. Returns true if tile has been set.
		/// </summary>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="extrudeAmount">Extrude amount (0-1).</param>
		public bool SetTileExtrudeAmount (int tileIndex, float extrudeAmount) {
			if (tileIndex < 0 || tiles == null || tileIndex >= tiles.Length)
				return false;
			Tile tile = tiles [tileIndex];
            if (extrudeAmount < 0)
                extrudeAmount = 0;
            else if (extrudeAmount > 1f)
                extrudeAmount = 1f;
            if (extrudeAmount == tile.extrudeAmount)
				return true;
			tile.extrudeAmount = extrudeAmount;
			if (tile.renderer != null) {
                UpdateTileMeshVertexPositions(tileIndex);
			}
			if (_highlightEnabled && tileIndex == lastHighlightedTileIndex) {
				RefreshHighlightedTile ();
			}
			// Fast update uv info
			if (_style != STYLE.Wireframe) {
				List<Vector4> uvShadedChunk = uvShaded [tile.uvShadedChunkIndex];
				for (int k = 0; k < tile.uvShadedChunkLength; k++) {
					Vector4 uv4 = uvShadedChunk [tile.uvShadedChunkStart + k];
					uv4.w = tile.extrudeAmount;
					uvShadedChunk [tile.uvShadedChunkStart + k] = uv4;
				}
				uvShadedDirty [tile.uvShadedChunkIndex] = true;
			}
			if (_style != STYLE.Shaded) {
				List<Vector2> uvWireChunk = uvWire [tile.uvWireChunkIndex];
				for (int k = 0; k < tile.uvWireChunkLength; k++) {
					Vector2 uv2 = uvWireChunk [tile.uvWireChunkStart + k];
					uv2.y = tile.extrudeAmount;
					uvWireChunk [tile.uvWireChunkStart + k] = uv2;
				}
				uvWireDirty [tile.uvWireChunkIndex] = true;
			}
			pendingUVUpdateFast = true;
			return true;
		}

		/// <summary>
		/// Changes individual vertex elevation
		/// </summary>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="vertexIndex">Vertex index.</param>
		/// <param name="elevation">Elevation.</param>
		public bool SetTileVertexElevation (int tileIndex, int vertexIndex, float elevation) {
			if (tileIndex < 0 || tiles == null || tileIndex >= tiles.Length)
				return false;
			Tile tile = tiles [tileIndex];
			if (vertexIndex < 0 || vertexIndex >= tile.vertexPoints.Length)
				return false;
			Point p = tile.vertexPoints [vertexIndex];
			p.elevation = elevation;
			needRegenerate = true;
			shouldUpdateMaterialProperties = true;
			return true;
		}


		/// <summary>
		/// Returns the individual vertex elevation
		/// </summary>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="vertexIndex">Vertex index.</param>
		public float GetTileVertexElevation (int tileIndex, int vertexIndex) {
			if (tileIndex < 0 || tiles == null || tileIndex >= tiles.Length)
				return 0;
			Tile tile = tiles [tileIndex];
			if (vertexIndex < 0 || vertexIndex >= tile.vertexPoints.Length)
				return 0;
			Point p = tile.vertexPoints [vertexIndex];
			return p.elevation;
		}



		/// <summary>
		/// Sets the tile extrude amount for a group of tiles.
		/// </summary>
		/// <param name="tiles">Array of tiles.</param>
		/// <param name="extrudeAmount">Extrude amount (0-1).</param>
		public void SetTileExtrudeAmount (Tile[] tiles, float extrudeAmount) {
			if (tiles == null)
				return;
			extrudeAmount = Mathf.Clamp01 (extrudeAmount);
			for (int k = 0; k < tiles.Length; k++) {
				Tile tile = tiles [k];
				if (extrudeAmount != tile.extrudeAmount) {
					tile.extrudeAmount = extrudeAmount;
					if (tile.renderer != null) {
						DestroyImmediate (tile.renderer.gameObject);
						tile.renderer = null;
						if (_highlightEnabled && tile.index == lastHighlightedTileIndex) {
							RefreshHighlightedTile ();
						}
					}
				}
				// Fast update uv info
				if (_style != STYLE.Wireframe) {
					List<Vector4> uvShadedChunk = uvShaded [tile.uvShadedChunkIndex];
					for (int j = 0; j < tile.uvShadedChunkLength; j++) {
						Vector4 uv4 = uvShadedChunk [tile.uvShadedChunkStart + j];
						uv4.w = tile.extrudeAmount;
						uvShadedChunk [tile.uvShadedChunkStart + j] = uv4;
					}
					uvShadedDirty [tile.uvShadedChunkIndex] = true;
				}
				if (_style != STYLE.Shaded) {
					List<Vector2> uvWireChunk = uvWire [tile.uvWireChunkIndex];
					for (int j = 0; j < tile.uvWireChunkLength; j++) {
						Vector4 uv2 = uvWireChunk [tile.uvWireChunkStart + j];
						uv2.y = tile.extrudeAmount;
						uvWireChunk [tile.uvWireChunkStart + j] = uv2;
					}
					uvWireDirty [tile.uvWireChunkIndex] = true;
				}
			}
			pendingUVUpdateFast = true;
		}

		/// <summary>
		/// Sets the tile extrude amount for a group of tiles.
		/// </summary>
		/// <param name="tiles">Array of tiles.</param>
		/// <param name="extrudeAmount">Extrude amount (0-1).</param>
		public void SetTileExtrudeAmount (List<int> tileIndices, float extrudeAmount) {
			if (tiles == null)
				return;
			extrudeAmount = Mathf.Clamp01 (extrudeAmount);
			int indicesCount = tileIndices.Count;
			for (int k = 0; k < indicesCount; k++) {
				int tileIndex = tileIndices [k];
				Tile tile = tiles [tileIndex];
				if (extrudeAmount != tile.extrudeAmount) {
					tile.extrudeAmount = extrudeAmount;
					if (tile.renderer != null) {
						DestroyImmediate (tile.renderer.gameObject);
						tile.renderer = null;
						if (_highlightEnabled && tile.index == lastHighlightedTileIndex) {
							RefreshHighlightedTile ();
						}
					}
				}
				// Fast update uv info
				if (_style != STYLE.Wireframe) {
					List<Vector4> uvShadedChunk = uvShaded [tile.uvShadedChunkIndex];
					for (int j = 0; j < tile.uvShadedChunkLength; j++) {
						Vector4 uv4 = uvShadedChunk [tile.uvShadedChunkStart + j];
						uv4.w = tile.extrudeAmount;
						uvShadedChunk [tile.uvShadedChunkStart + j] = uv4;
					}
					uvShadedDirty [tile.uvShadedChunkIndex] = true;
				}
				if (_style != STYLE.Shaded) {
					List<Vector2> uvWireChunk = uvWire [tile.uvWireChunkIndex];
					for (int j = 0; j < tile.uvWireChunkLength; j++) {
						Vector4 uv2 = uvWireChunk [tile.uvWireChunkStart + j];
						uv2.y = tile.extrudeAmount;
						uvWireChunk [tile.uvWireChunkStart + j] = uv2;
					}
					uvWireDirty [tile.uvWireChunkIndex] = true;
				}
			}
			pendingUVUpdateFast = true;
		}

		/// <summary>
		/// Sets the tile extrude amount for a group of tiles.
		/// </summary>
		/// <param name="tiles">List of tiles.</param>
		/// <param name="extrudeAmount">Extrude amount (0-1).</param>
		public void SetTileExtrudeAmount (List<Tile> tiles, float extrudeAmount) {
			Tile[] tempArray = tiles.ToArray ();
			SetTileExtrudeAmount (tempArray, extrudeAmount);
		}

		/// <summary>
		/// Sets the user-defined string tag of a given tile
		/// </summary>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="tag">String data.</param>
		public bool SetTileTag (int tileIndex, string tag) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			tiles [tileIndex].tag = tag;
			return true;
		}

		/// <summary>
		/// Sets the user-defined integer tag of a given tile
		/// </summary>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="tag">Integer data.</param>
		public bool SetTileTag (int tileIndex, int tag) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			tiles [tileIndex].tagInt = tag;
			return true;
		}

		/// <summary>
		/// Gets the tile string tag.
		/// </summary>
		/// <returns>The tile string tag.</returns>
		/// <param name="tileIndex">Tile index.</param>
		public string GetTileTag (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return null;
			return tiles [tileIndex].tag;
		}

		/// <summary>
		/// Gets the tile int tag.
		/// </summary>
		/// <returns>The tile string tag.</returns>
		/// <param name="tileIndex">Tile index.</param>
		public int GetTileTagInt (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return 0;
			return tiles [tileIndex].tagInt;
		}


		/// <summary>
		/// Removes any extrusion amount from all tiles
		/// </summary>
		public void ClearTilesExtrusion () {
			SetTileExtrudeAmount (tiles, 0);
		}

		/// <summary>
		/// Returns whether path finding can cross this tile.
		/// </summary>
		public bool GetTileCanCross (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			return tiles [tileIndex].canCross;
		}

		/// <summary>
		/// Returns current tile color.
		/// </summary>
		public Color GetTileColor (int tileIndex, bool ignoreTemporaryColor = false) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return _defaultShadedColor;
			Tile tile = tiles [tileIndex];
			if (tile.tempMat != null && !ignoreTemporaryColor)
				return tile.tempMat.color;
			if (tile.customMat != null)
				return tile.customMat.color;
			return _defaultShadedColor;
		}


		/// <summary>
		/// Returns current tile height or extrude amount.
		/// </summary>
		public float GetTileExtrudeAmount (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return 0;
			return tiles [tileIndex].extrudeAmount;
		}

		/// <summary>
		/// Gets the neighbours indices of a given tile
		/// </summary>
		public int[] GetTileNeighbours (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return null;
			return tiles [tileIndex].neighboursIndices;
		}

		/// <summary>
		/// Gets the neighbours objects of a given tile
		/// </summary>
		public Tile[] GetTileNeighboursTiles (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return null;
			return tiles [tileIndex].neighbours;
		}

		/// <summary>
		/// Gets the index of the tile on the exact opposite pole
		/// </summary>
		public int GetTileAtPolarOpposite (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return -1;
			return GetTileAtLocalPosition (-tiles [tileIndex].center);
		}

		/// <summary>
		/// Gets an array of tile indices found within a distance to a given tile
		/// </summary>
		/// <returns>The tiles within distance.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="worldSpace">By default, distance is used in local space in the range of 0..1 (which is faster). Using world space = true will compute distances in world space which will apply the current transform to the examined tile centers.</param>
		public List<int> GetTilesWithinDistance (int tileIndex, float distance, bool worldSpace = false) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return null;

			Vector3 refPos;
			if (worldSpace) {
				refPos = GetTileCenter (tileIndex);
			} else {
				refPos = tiles [tileIndex].center;
			}
			float d2 = distance * distance;
			List <int> candidates = new List<int> (GetTileNeighbours (tileIndex));
			Dictionary<int,bool> processed = new Dictionary<int,bool> (); // dictionary is faster for value types than HashSet
			processed [tileIndex] = true;
			List<int> results = new List<int> ();
			int candidateLast = candidates.Count - 1;
			while (candidateLast >= 0) {
				// Pop candidate
				int t = candidates [candidateLast];
				candidates.RemoveAt (candidateLast);
				candidateLast--;
				float dist;
				if (worldSpace) {
					dist = Misc.Vector3SqrDistance (GetTileCenter (t), refPos);
				} else {
					dist = Misc.Vector3SqrDistance (tiles [t].center, refPos);
				}
				if (dist < d2) {
					results.Add (t);
					processed [t] = true;
					int[] nn = GetTileNeighbours (t);
					for (int k = 0; k < nn.Length; k++) {
						if (!processed.ContainsKey (nn [k])) {
							candidates.Add (nn [k]);
							candidateLast++;
						}
					}
				}
			}
			return results;
		}

		/// <summary>
		/// Gets an array of tile indices found within a distance to the given tile that satisfies a custom criteria
		/// </summary>
		/// <returns>The tiles within distance.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="worldSpace">By default, distance is used in local space in the range of 0..1 (which is faster). Using world space = true will compute distances in world space which will apply the current transform to the examined tile centers.</param>
		/// <param name="criteria">A user defined function that accepts a single argument, the tile index, and must return a boolean value specifying if the tile is valid and should be included in the results or not.</param>
		public List<int> GetTilesWithinDistance (int tileIndex, float distance, bool worldSpace, Func<int, bool> criteria) {
			List<int> tiles = GetTilesWithinDistance (tileIndex, distance, worldSpace);
			if (tiles != null) {
				int count = tiles.Count;
				List<int> results = new List<int> (count);
				for (int k = 0; k < count; k++) {
					if (criteria (tiles [k])) {
						results.Add (tiles [k]);
					}
				}
				return results;
			}
			return null;
		}



		/// <summary>
		/// Gets an array of tile indices found within a number of tile steps
		/// </summary>
		/// <returns>The tiles within distance.</returns>
		/// <param name="maxSteps">Max number of steps.</param>
		public List<int> GetTilesWithinSteps (int tileIndex, int maxSteps) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return null;

			List <int> candidates = new List<int> (GetTileNeighbours (tileIndex));
			Dictionary<int,bool> processed = new Dictionary<int,bool> (tileIndex); // dictionary is faster for value types than HashSet
			processed [tileIndex] = true;
			List<int> results = new List<int> ();
			int candidateLast = candidates.Count - 1;
			while (candidateLast >= 0) {
				// Pop candidate
				int t = candidates [candidateLast];
				candidates.RemoveAt (candidateLast);
				candidateLast--;
				List<int> tt = FindPath (tileIndex, t, maxSteps);
				if (tt != null && !processed.ContainsKey (t)) {
					results.Add (t);
					processed [t] = true;
					int[] nn = GetTileNeighbours (t);
					for (int k = 0; k < nn.Length; k++) {
						if (!processed.ContainsKey (nn [k])) {
							candidates.Add (nn [k]);
							candidateLast++;
						}
					}
				}
			}
			return results;
		}


		/// <summary>
		/// Gets an array of tile indices found within a number of tile steps that satisfies a custom criteria
		/// </summary>
		/// <returns>The tiles within distance.</returns>
		/// <param name="maxSteps">Max number of steps.</param>
		/// <param name="criteria">A user defined function that accepts a single argument, the tile index, and must return a boolean value specifying if the tile is valid and should be included in the results or not.</param>
		public List<int> GetTilesWithinSteps (int tileIndex, int maxSteps, Func<int, bool> criteria) {
			List<int> tiles = GetTilesWithinSteps (tileIndex, maxSteps);
			if (tiles != null) {
				int count = tiles.Count;
				List<int> results = new List<int> (count);
				for (int k = 0; k < count; k++) {
					if (criteria (tiles [k])) {
						results.Add (tiles [k]);
					}
				}
				return results;
			}
			return null;
		}


		/// <summary>
		/// Gets an array of tile indices found within a number of tile steps
		/// </summary>
		/// <returns>The tiles within distance.</returns>
		/// <param name="minSteps">Min number of steps.</param>
		/// <param name="maxSteps">Max number of steps.</param>
		/// <param name="results">List of tile indices. Must be initialized.</param>
		public int GetTilesWithinSteps (int tileIndex, int minSteps, int maxSteps, List<int> results) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return 0;

			if (results == null) {
				Debug.LogError ("GetTilesWithinStepsNonAlloc: results parameter must be initialized.");
				return 0;
			}

			List<int> candidates = GetTempList<int> (ref tmpCandidates);
			candidates.AddRange (GetTileNeighbours (tileIndex));
			Dictionary<int,bool> processed = GetTempDictionary<int,bool> (ref tmpDict); // dictionary is faster for value types than HashSet
			processed [tileIndex] = true;
			for (int k = 0; k < candidates.Count; k++) {
				processed [candidates [k]] = true;
			}
			results.Clear ();
			int candidateLast = candidates.Count - 1;
			List<int> tt = GetTempList<int> (ref tmpList);
			while (candidateLast >= 0) {
				// Pop candidate
				int t = candidates [candidateLast];
				candidates.RemoveAt (candidateLast);
				candidateLast--;
				int pathLength = FindPath (tileIndex, t, tt, maxSteps);
				if (pathLength > 0) {
					if (pathLength >= minSteps && pathLength <= maxSteps) {
						results.Add (t);
					}
					int[] nn = GetTileNeighbours (t);
					for (int k = 0; k < nn.Length; k++) {
						int nindex = nn [k];
						if (!processed.ContainsKey (nindex)) {
							processed [nindex] = true;
							candidates.Add (nindex);
							candidateLast++;
						}
					}
				}
			}
			return results.Count;
		}

		/// <summary>
		/// Gets an array of tile indices found within a number of tile steps or null if no tile found
		/// </summary>
		/// <returns>The tiles within distance.</returns>
		/// <param name="minSteps">Min number of steps.</param>
		/// <param name="maxSteps">Max number of steps.</param>
		public List<int> GetTilesWithinSteps (int tileIndex, int minSteps, int maxSteps) {
			List<int> results = new List<int> ();
			int count = GetTilesWithinSteps (tileIndex, minSteps, maxSteps, results);
			return count == 0 ? null : results;
		}


		/// <summary>
		/// Gets an array of tile indices found within a number of tile steps that satisfies a custom criteria
		/// </summary>
		/// <returns>The tiles within distance.</returns>
		/// <param name="minSteps">Min number of steps.</param>
		/// <param name="maxSteps">Max number of steps.</param>
		/// <param name="criteria">A user defined function that accepts a single argument, the tile index, and must return a boolean value specifying if the tile is valid and should be included in the results or not.</param>
		public List<int> GetTilesWithinSteps (int tileIndex, int minSteps, int maxSteps, Func<int, bool> criteria) {
			List<int> tiles = GetTilesWithinSteps (tileIndex, minSteps, maxSteps);
			if (tiles != null) {
				int count = tiles.Count;
				List<int> results = new List<int> (count);
				for (int k = 0; k < count; k++) {
					if (criteria (tiles [k])) {
						results.Add (tiles [k]);
					}
				}
				return results;
			}
			return null;
		}


		/// <summary>
		/// Gets the tiles within two tiles.
		/// </summary>
		/// <returns>The tile indices within two tiles.</returns>
		/// <param name="tileIndex1">Tile index1.</param>
		/// <param name="tileIndex2">Tile index2.</param>
		public List<int>GetTilesWithinTwoTiles (int tileIndex1, int tileIndex2) {
			return FindPath (tileIndex1, tileIndex2, 0, -1, true);
		}


		/// <summary>
		/// Hide a given tile
		/// </summary>
		public void ClearTile (int tileIndex, bool clearTemporaryColor = false, bool clearAllColors = true, bool clearObstacles = true) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return;
			Tile tile = tiles [tileIndex];
			Renderer tileRenderer = tile.renderer;
			tile.tempMat = null;
			if (tileRenderer != null) {
				tileRenderer.enabled = false;
			}
			if (clearAllColors) {
				if (tile.customMat != null) {
					if (tile.customMat.HasProperty (ShaderParams.MainTex) && tile.customMat.mainTexture != null) {
						pendingTextureArrayUpdate = true;
					}
					tile.customMat = null;
				}
				pendingColorsUpdate = true;
				Color32 matColor = _defaultShadedColor;
				List<Color32> colorChunk = colorShaded [tile.uvShadedChunkIndex];
				for (int k = 0; k < tile.uvShadedChunkLength; k++) {
					colorChunk [tile.uvShadedChunkStart + k] = matColor;
				}
				colorShadedDirty [tile.uvShadedChunkIndex] = true;
			}
			if (clearObstacles) {
				tile.canCross = true;
			}
		}


		/// <summary>
		/// Hide all tiles
		/// </summary>
		public void ClearTiles (bool clearTemporaryColors = false, bool clearAllColors = true, bool clearObstacles = true) {
			for (int k = 0; k < tiles.Length; k++) {
				ClearTile (k, clearTemporaryColors, clearAllColors, clearObstacles);
			}
			ResetHighlightMaterial ();
		}

		/// <summary>
		/// Destroys a colored tile
		/// </summary>
		public void DestroyTile (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return;
			if (lastHighlightedTileIndex >= 0 && tileIndex == lastHighlightedTileIndex) {
				HideHighlightedTile ();
			}
			if (tiles [tileIndex].customMat != null) {
				tiles [tileIndex].customMat = null;
				pendingColorsUpdate = true;
				pendingTextureArrayUpdate = true;
				colorShadedDirty [tiles [tileIndex].uvShadedChunkIndex] = true;
			}
			if (tiles [tileIndex].renderer != null) {
				DestroyImmediate (tiles [tileIndex].renderer.gameObject);
				tiles [tileIndex].renderer = null;
			}
		}

		/// <summary>
		/// Toggles tile visibility
		/// </summary>
		public bool ToggleTile (int tileIndex, bool visible) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			if (tiles [tileIndex].visible != visible) {
				tiles [tileIndex].visible = visible;
				needRegenerate = true;
				shouldUpdateMaterialProperties = true;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Hides a colored tile
		/// </summary>
		public bool HideTile (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			if (tiles [tileIndex].visible) {
				tiles [tileIndex].visible = false;
				needRegenerate = true;
				shouldUpdateMaterialProperties = true;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Shows a colored tile
		/// </summary>
		public bool ShowTile (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return false;
			if (!tiles [tileIndex].visible) {
				tiles [tileIndex].visible = true;
				needRegenerate = true;
				shouldUpdateMaterialProperties = true;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the tile center in local or world space coordinates.
		/// </summary>
		/// <returns>The tile center.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="worldSpace">If set to <c>true</c> it returns the world space coordinates.</param>
        /// <param name="fitToSphere">The returned position is fitted to the sphere. A tile is a flat polygon so the center doesn't match exactly the position on the sphere surface. By default, this method returns the surface position. Passing false to this argument will return the real center of the tile.</param>
		public Vector3 GetTileCenter (int tileIndex, bool worldSpace = true, bool includeExtrusion = true, bool fitToSphere = true) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return Misc.Vector3zero;
			Tile tile = tiles [tileIndex];
			Vector3 v = fitToSphere ? tile.center : tile.polygonCenter;
			if (includeExtrusion && _extruded) {
				v *= 1.0f + tile.extrudeAmount * _extrudeMultiplier;
			}
			if (worldSpace) {
				v = transform.TransformPoint (v);
			}
			return v;
		}

		/// <summary>
		/// Returns the center of the tile in world space coordinates.
		/// </summary>
		public Vector3 GetTileVertexPosition (int tileIndex, int vertexIndex, bool worldSpace = true, bool includeExtrusion = true) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return Misc.Vector3zero;
			Tile tile = tiles [tileIndex];
			if (vertexIndex < 0 || vertexIndex >= tile.vertices.Length)
				return Misc.Vector3zero;
			Vector3 v = tile.vertices [vertexIndex];
			if (includeExtrusion && _extruded) {
				v *= 1.0f + tile.extrudeAmount * _extrudeMultiplier;
			}
			if (worldSpace) {
				v = transform.TransformPoint (v);
			}
			return v;
		}

		/// <summary>
		/// Returns the tile latitude and longitude in a Vector2 field.
		/// </summary>
		/// <returns>The tile lat lon.</returns>
		/// <param name="tileIndex">Tile index.</param>
		public Vector2 GetTileLatLon (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return Misc.Vector2zero;
			Tile tile = tiles [tileIndex];

			float latDec = Mathf.Asin (tile.center.y * 2.0f);
			float lonDec = -Mathf.Atan2 (tile.center.x, tile.center.z);
			return new Vector2 (latDec * Mathf.Rad2Deg, lonDec * Mathf.Rad2Deg);
		}


		/// <summary>
		/// Returns a vertex latitude and longitude in a Vector2 field.
		/// </summary>
		/// <returns>The vertex lat lon.</returns>
		/// <param name="tileIndex">Tile index.</param>
		/// <param name="vertexIndex">Vertex index (0-5 in an hexagon, 0-4 in a pentagon).</param>
		public Vector2 GetTileVertexLatLon(int tileIndex, int vertexIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return Misc.Vector2zero;
			Tile tile = tiles[tileIndex];
			if (vertexIndex < 0 || vertexIndex >= tile.vertices.Length) return Misc.Vector2zero;
			Vector3 vertex = tile.vertices[vertexIndex];
			float latDec = Mathf.Asin(vertex.y * 2.0f);
			float lonDec = -Mathf.Atan2(vertex.x, vertex.z);
			return new Vector2(latDec * Mathf.Rad2Deg, lonDec * Mathf.Rad2Deg);
		}

		/// <summary>
		/// Returns the UV texture coordinate of the tile center (mapped to a 2D texture) in a Vector2 field.
		/// </summary>
		/// <returns>The tile lat lon.</returns>
		/// <param name="tileIndex">Tile index.</param>
		public Vector2 GetTileUV (int tileIndex) {
			if (tileIndex < 0 || tileIndex >= tiles.Length)
				return Misc.Vector2zero;
			Tile tile = tiles [tileIndex];
			float latDec = Mathf.Asin (tile.center.y * 2.0f);
			float lonDec = -Mathf.Atan2 (tile.center.x, tile.center.z);
			float u = (lonDec + Mathf.PI) / (2f * Mathf.PI);
			float v = latDec / Mathf.PI + 0.5f;
			return new Vector2 (u, v);
		}


        /// <summary>
        /// Returns the tile index corresponding to a given UV coordinate mapped to the hexasphere. This method is the inverse of GetTileUV.
        /// </summary>
        /// <param name="uv">UV coordinates.</param>
        /// <returns>The index of the tile</returns>
        public int GetTileAtUV(Vector2 uv) {
            float lon = uv.x * 360f - 180f;
            float lat = (uv.y - 0.5f) * 2f * 90f;
            Vector3 localPos = GetSpherePointFromLatLon(lat, lon);
            return GetTileAtLocalPos(localPos);
        }

		/// <summary>
		/// Gets the tile under a given position in world space coordinates.
		/// </summary>
		/// <returns>The tile at position.</returns>
		/// <param name="worldPosition">World position.</param>
		public int GetTileAtPos (Vector3 worldPosition) {
			if (tiles == null)
				return -1;

			Vector3 localPosition = transform.InverseTransformPoint (worldPosition);
			return GetTileAtLocalPosition (localPosition);
		}

		/// <summary>
		/// Gets the tile under a given position in local space coordinates.
		/// </summary>
		/// <returns>The tile at local position.</returns>
		/// <param name="localPosition">Local position.</param>
		public int GetTileAtLocalPos (Vector3 localPosition) {
			return GetTileAtLocalPosition (localPosition);
		}


		/// <summary>
		/// Returns a jSON formatted representation of current tiles settings.
		/// </summary>
		public string GetTilesConfigurationData () {
			List<TileSaveData> tsd = new List<TileSaveData> ();
			for (int k = 0; k < tiles.Length; k++) {
				Tile tile = tiles [k];
				if (tile.tagInt != 0 || tile.customMat != null || !string.IsNullOrEmpty (tile.tag)) {
					TileSaveData sd = new TileSaveData ();
					sd.tileIndex = k;
					sd.color = tile.customMat.color;
					sd.textureIndex = GetTileTextureIndex (k);
					sd.tag = tile.tag;
					sd.tagInt = tile.tagInt;
					tsd.Add (sd);
				}
			}
			HexasphereSaveData hsd = new HexasphereSaveData ();
			hsd.tiles = tsd.ToArray ();
			return JsonUtility.ToJson (hsd);
		}

		public void SetTilesConfigurationData (string json) {
			if (tiles == null)
				return;
												
			HexasphereSaveData hsd = JsonUtility.FromJson<HexasphereSaveData> (json);
			for (int k = 0; k < hsd.tiles.Length; k++) {
				int tileIndex = hsd.tiles [k].tileIndex;
				if (tileIndex < 0 || tileIndex >= tiles.Length)
					continue;
				tiles [tileIndex].tag = hsd.tiles [k].tag;
				tiles [tileIndex].tagInt = hsd.tiles [k].tagInt;
				SetTileTexture (tileIndex, hsd.tiles [k].textureIndex, hsd.tiles [k].color);
			}
		}

		#endregion

		
	}

}