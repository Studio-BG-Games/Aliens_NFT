using UnityEngine;
using System;


namespace HexasphereGrid {

	[Serializable]
	public struct TileSaveData {
		public int tileIndex;
		public Color color;
		public int textureIndex;
		public string tag;
		public int tagInt;
	}

	[Serializable]
	public class HexasphereSaveData {
		public TileSaveData[] tiles;
	}

}
