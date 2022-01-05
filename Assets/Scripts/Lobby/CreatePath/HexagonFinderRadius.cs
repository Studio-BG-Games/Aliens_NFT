using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MapSpase;
using MapSpase.Hexagon;

namespace CreatePath
{
    public class HexagonFinderRadius
    {
        private Vector3Int[] _directions = new Vector3Int[]
        {
        new Vector3Int(0,1,1), new Vector3Int(1,1,0), new Vector3Int(1,0,-1),
        new Vector3Int(0,-1,-1), new Vector3Int(-1,-1,0), new Vector3Int(-1, 0, 1)
        };

        private int _range;
        private ThreeDimensionalArray _threeDimensionalArray;
        private HexagonModel[][] _cells;
        private List<HexagonModel> _closedCells;

        public HexagonFinderRadius(ThreeDimensionalArray threeDimensionalArray)
        {
            _threeDimensionalArray = threeDimensionalArray;
        }

        public List<HexagonModel> FindToList(HexagonModel firstCell, int range = 1)
        {
            Find(firstCell, range);
            return ToList();
        }

        public HexagonModel[][] Find(HexagonModel firstCell, int range)
        {
            SettingValues(firstCell, range);
            for (int x = 0; x < _range; x++)
            {
                int currentChapel = x + 1;
                List<HexagonModel> listCells = new List<HexagonModel>();

                for (int y = 0; y < _cells[x].Length; y++)
                {
                    if (_cells[x][y] != null && currentChapel < _range)
                    {
                        FindSurroundingCells(_cells[x][y], x, listCells);

                        if (CheckDuplication(_cells[x][y], _closedCells.ToArray()))
                            _closedCells.Add(_cells[x][y]);
                    }
                }

                if (currentChapel < _range)
                    _cells[currentChapel] = listCells.ToArray();
            }

            return _cells;
        }

        private void SettingValues(HexagonModel firstCell, int range)
        {
            _range = range + 1;
            _closedCells = new List<HexagonModel>();
            _cells = new HexagonModel[_range][];
            _cells[0] = new HexagonModel[] { firstCell };
        }

        private List<HexagonModel> FindSurroundingCells(HexagonModel targetCell, int range, List<HexagonModel> listCells)
        {
            foreach (Vector3Int direction in _directions)
            {
                HexagonModel cell = _threeDimensionalArray.GetModel(targetCell.ThreeDimensionalPosition + direction);

                if (cell != null && CheckDuplication(cell, listCells, range))
                    listCells.Add(cell);
            }

            return listCells;
        }

        private bool CheckDuplication(HexagonModel targetCell, HexagonModel[] cells) => Array.Exists(cells, e => e == targetCell);

        private bool CheckDuplication(HexagonModel targetCell, List<HexagonModel> listCells, int range)
        {
            if (CheckDuplication(targetCell, _closedCells.ToArray()))
                return false;
            if (CheckDuplication(targetCell, listCells.ToArray()))
                return false;
            if (CheckDuplication(targetCell, _cells[range]))
                return false;

            return true;
        }

        private List<HexagonModel> ToList()
        {
            List<HexagonModel> hexagons = new List<HexagonModel>();

            List<HexagonModel[]> cells = _cells.ToList();

            foreach (var cellsChild in cells)
                foreach (var cell in cellsChild)
                    hexagons.Add(cell);

            return hexagons;
        }
    }
}
