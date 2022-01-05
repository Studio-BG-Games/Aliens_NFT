using MapSpase;
using MapSpase.Hexagon;
using System.Collections.Generic;
using UnityEngine;

namespace MapCraete
{
    public class CellCreationThreeDimensionalCoordinates
    {
        private HexagonModel[,] _mapCells;
        private List<PositionAssignmentCell> _positions;

        public void Creat(HexagonModel[,] cellsArray, out ThreeDimensionalArray threeDimensionalArray)
        {
            _mapCells = cellsArray;
            CreatePositionsList();
            FindSize(out int size);

            SetPosition(new HexagonModel[size][], new LogicX(_mapCells.GetLength(0)));
            SetPosition(new HexagonModel[size][], new LogicZ(_mapCells), _mapCells.GetLength(1) - 1);
            SetPositionY();

            DetermineDimensions(out Vector3Int sizeArray);

            foreach (var item in _mapCells)
            {
                Vector3Int position = _positions[item.Cell.Id - 1].Position;
                item.name = position.ToString();
                item.TrySetPosition(position);
            }

            threeDimensionalArray = new ThreeDimensionalArray(cellsArray, sizeArray);
        }

        private void DetermineDimensions(out Vector3Int size)
        {
            size = Vector3Int.zero;

            foreach (var position in _positions)
            {
                if (position.Position.x > size.x)
                    size.x = position.Position.x;
                if (position.Position.y > size.y)
                    size.y = position.Position.y;
                if (position.Position.z > size.z)
                    size.z = position.Position.z;
            }

            size += Vector3Int.one;
        }

        private void CreatePositionsList()
        {
            _positions = new List<PositionAssignmentCell>();

            for (int i = 0; i < _mapCells.Length; i++)
                _positions.Add(new PositionAssignmentCell(i));
        }

        private void FindSize(out int size)
        {
            size = _mapCells.GetLength(0) + _mapCells.GetLength(1) / 2;

            if (_mapCells[0, 0].Cell.Position.x <= 0)
                size += 1;
        }

        private void SetPositionY()
        {
            for (int x = 0; x < _mapCells.GetLength(0); x++)
            {
                for (int y = 0; y < _mapCells.GetLength(1); y++)
                {
                    _positions[_mapCells[x, y].Cell.Id - 1].Position.y = y;
                }
            }
        }

        private void SetPosition(HexagonModel[][] cells, ILogic logicX, int x = 0)
        {
            int y = 0, number = 0, YWithParity = 0;

            while (logicX.CheckOpportunityContinue(x))
            {
                List<HexagonModel> cellsList = new List<HexagonModel>();

                if (YWithParity < _mapCells.GetLength(1))
                {
                    Step(_mapCells[x, YWithParity], cellsList, logicX);
                    y++;
                }
                else if (logicX.CheckOpportunityContinue(x))
                {
                    x = logicX.ChangeValueAfterStep(x);
                    Step(_mapCells[x, _mapCells.GetLength(1) - 1], cellsList, logicX);
                    x = logicX.ChangeValueBeforeStep(x);
                }

                foreach (var item in cellsList)
                {
                    logicX.SetPositionValue(_positions[item.Cell.Id - 1], number);
                }

                YWithParity = logicX.CheckYWithParity(y);
                cells[number] = cellsList.ToArray();
                number++;
            }
        }

        private void Step(HexagonModel targetCell, List<HexagonModel> cells, ILogic logicX)
        {
            HexagonModel nextCell = null;
            Vector2Int position = logicX.TakePosition(targetCell.Cell);
            nextCell = FindCells(position, targetCell);

            cells.Add(targetCell);

            if (nextCell != null)
                Step(nextCell, cells, logicX);
        }

        private HexagonModel FindCells(Vector2Int direction, HexagonModel targetCell)
        {
            int x = Mathf.Clamp(direction.x + targetCell.ArrayNumberDifference.x, -1, int.MaxValue);
            int y = Mathf.Clamp(direction.y + targetCell.ArrayNumberDifference.y, -1, int.MaxValue);

            if (x >= 0 && y >= 0)
                if (_mapCells.GetLength(0) > x && _mapCells.GetLength(1) > y)
                    if (_mapCells[x, y] != targetCell)
                        return _mapCells[x, y];

            return null;
        }

        public class PositionAssignmentCell
        {
            public int ID;
            public Vector3Int Position;

            public PositionAssignmentCell(int iD)
            {
                ID = iD;
            }
        }

        private interface ILogic
        {
            public int CheckYWithParity(int y);
            public bool CheckOpportunityContinue(int x);
            public int ChangeValueBeforeStep(int x);
            public int ChangeValueAfterStep(int x);
            public Vector2Int TakePosition(ICell targetCell);
            public void SetPositionValue(PositionAssignmentCell assignmentCell, int value);
        }

        private struct LogicZ : ILogic
        {
            private HexagonModel[,] _mapCells;

            public LogicZ(HexagonModel[,] mapCells) => _mapCells = mapCells;

            public int CheckYWithParity(int y) => Mathf.Clamp(2 * y + 1, 0, int.MaxValue);

            public bool CheckOpportunityContinue(int x) => x >= 0;

            public int ChangeValueBeforeStep(int x) => x - 1;

            public int ChangeValueAfterStep(int x)
            {
                if (x == _mapCells.GetLength(1) - 1 && _mapCells[0, 0].Cell.Position.x < 0)
                    return x - 1;
                else
                    return x;
            }

            public Vector2Int TakePosition(ICell targetCell)
            {
                if (Mathf.Abs(targetCell.Position.y) % 2 <= 0)
                    return targetCell.Position + new Vector2Int(-1, -1);
                else
                    return targetCell.Position + new Vector2Int(0, -1);
            }

            public void SetPositionValue(PositionAssignmentCell assignmentCell, int value)
            {
                assignmentCell.Position.z = value;
            }
        }

        private struct LogicX : ILogic
        {
            private int _maxValue;

            public LogicX(int maxValue) => _maxValue = maxValue;

            public int CheckYWithParity(int y) => Mathf.Clamp(2 * y, 0, int.MaxValue);

            public bool CheckOpportunityContinue(int x) => x < _maxValue;

            public int ChangeValueAfterStep(int x) => x;

            public int ChangeValueBeforeStep(int x) => x + 1;

            public Vector2Int TakePosition(ICell targetCell)
            {
                if (Mathf.Abs(targetCell.Position.y) % 2 <= 0)
                    return targetCell.Position + Vector2Int.down;
                else
                    return targetCell.Position + new Vector2Int(1, -1);
            }

            public void SetPositionValue(PositionAssignmentCell assignmentCell, int value)
            {
                assignmentCell.Position.x = value;
            }
        }
    }
}
