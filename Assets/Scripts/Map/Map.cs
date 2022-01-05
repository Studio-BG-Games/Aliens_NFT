using MapSpase.Hexagon;
using MapSpase.Hexagon.Backlight;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSpase
{
    //TODO —делать инкапсул€цию массивов.
    public class Map : MonoBehaviour, IBacklightUser
    {
        private ThreeDimensionalArray _threeDimensionalArray;
        private List<HexagonModel> _hexagons;
        private HexagonModel[,] _hexagonsArray;

        public string Id { get; private set; } = "Test";
        public string Name { get; private set; } = "Test";

        public IReadOnlyList<HexagonModel> Hexagons => _hexagons;

        public event Action<IReadOnlyList<HexagonModel>> InitializedToList;
        public event Action<HexagonModel[,], ThreeDimensionalArray> InitializedToArray;

        public void Initialization(string name, string id, HexagonModel[,] cells, ThreeDimensionalArray threeDimensionalArray)
        {
            Id = id;
            Name = name;

            _threeDimensionalArray = threeDimensionalArray;
            _hexagonsArray = cells;
            _hexagons = new List<HexagonModel>();
            foreach (var cell in cells)
                _hexagons.Add(cell);

            InitializedToArray?.Invoke(cells, _threeDimensionalArray);
            InitializedToList?.Invoke(_hexagons);
        }

        public void ChangeHexagonModel(HexagonModel target, HexagonModel newHexagon)
        {
            if (target == null)
                throw new ArgumentNullException($"{nameof(target)} is null");
            if (newHexagon == null)
                throw new ArgumentNullException($"{nameof(newHexagon)} is null");
            try
            {
                int index = _hexagons.IndexOf(target);
                _hexagons[index] = newHexagon;

                int x = target.ArrayNumberDifference.x;
                int y = target.ArrayNumberDifference.y;

                _hexagonsArray[x, y] = newHexagon;

                Destroy(target.gameObject);
            }
            catch
            {
                Destroy(newHexagon.gameObject);
            }
        }
    }
}
