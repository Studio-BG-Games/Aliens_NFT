using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapSpase.Hexagon;

namespace MapSpase
{
    public class ThreeDimensionalArray
    {
        private List<HexagonModel>[,] _hexagons;

        public ThreeDimensionalArray(HexagonModel[,] cellsArray, Vector3Int sizeArray)
        {
            _hexagons = new List<HexagonModel>[sizeArray.y, sizeArray.x];

            for (int y = 0; y < _hexagons.GetLength(0); y++)
                for (int x = 0; x < _hexagons.GetLength(1); x++)
                    _hexagons[y, x] = new List<HexagonModel>();

            foreach (var item in cellsArray)
                _hexagons[item.ThreeDimensionalPosition.y, item.ThreeDimensionalPosition.x].Add(item);
        }

        public HexagonModel GetModel(Vector3Int position)
        {
            if (position.y < 0 || position.y >= _hexagons.GetLength(0))
                return null;
            if (position.x < 0 || position.x >= _hexagons.GetLength(1))
                return null;
            if (position.z < 0)
                return null;

            List<HexagonModel> hexagonsZ = _hexagons[position.y, position.x];
            HexagonModel hexagonModel = hexagonsZ.FirstOrDefault(o => o.ThreeDimensionalPosition.z == position.z);

            return hexagonModel;
        }

        public void ChangeHexagonModel(HexagonModel target, HexagonModel newHexagon)
        {
            List<HexagonModel> hexagonsZ = _hexagons[target.ThreeDimensionalPosition.y, target.ThreeDimensionalPosition.x];
            int index = hexagonsZ.IndexOf(target);
            hexagonsZ[index] = newHexagon;
        }
    }
}
