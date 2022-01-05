using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MapSpase;
using MapSpase.Hexagon;

namespace CreatePath
{
    public class PathFinder
    {
        private List<HexagonModel> _area;
        private HexagonFinderRadius _radius;
        private List<PathCell> _openList;
        private List<PathCell> _closedList;
        private HexagonModel _target;

        public PathFinder(ThreeDimensionalArray threeDimensionalArray)
        {
            _radius = new HexagonFinderRadius(threeDimensionalArray);
        }

        public bool TryFind(out List<HexagonModel> path, List<HexagonModel> area, HexagonModel start, HexagonModel target, int distance = int.MaxValue)
        {
            path = Find(area, start, target);

            if(path.Count -1 > distance)
                return false;

            return true;
        }

        public List<HexagonModel> Find(List<HexagonModel> area, HexagonModel start, HexagonModel target)
        {
            SettingValues(area, start, target);
            PathCell pathCell = new PathCell(start, null, HexagonDistanceSearcher.Search(start.ThreeDimensionalPosition, _target.ThreeDimensionalPosition));

            while (pathCell.CellGameModel != target)
            {
                FindefindSurroundingCells(pathCell);
                _closedList.Add(pathCell);
                if (_openList.Count > 0)
                    pathCell = SelectNextCell();
                else
                    break;
            }

            return ShapePath(pathCell);
        }

        private List<HexagonModel> ShapePath(PathCell pathCell)
        {
            List<HexagonModel> cells = new List<HexagonModel>();

            while (pathCell.PastCell != null)
            {
                cells.Add(pathCell.CellGameModel);
                pathCell = pathCell.PastCell;
            }

            cells.Add(pathCell.CellGameModel);
            cells.Reverse();
            return cells;
        }

        private void SettingValues(List<HexagonModel> area, HexagonModel start, HexagonModel target)
        {
            _area = area;
            _target = target;
            _openList = new List<PathCell>();
            _closedList = new List<PathCell>();
        }

        private PathCell SelectNextCell()
        {
            int max = _openList.Min(o => o.CurrentTravelDistance);
            List<PathCell> pathCells = _openList.Where(o => o.CurrentTravelDistance == max).ToList();
            PathCell pathCell = pathCells[Random.Range(0, pathCells.Count)];
            _openList.Remove(pathCell);
            return pathCell;
        }

        private void FindefindSurroundingCells(PathCell pathCell)
        {
            List<HexagonModel> cells = _radius.FindToList(pathCell.CellGameModel);
            foreach (var cell in cells)
            {
                if (CanAddToOpenLit(cell) && _area.FirstOrDefault(a => a == cell) != null)
                {
                    _openList.Add(new PathCell(cell, pathCell, HexagonDistanceSearcher.Search(cell.ThreeDimensionalPosition, _target.ThreeDimensionalPosition)));
                }
            }
        }

        private bool CanAddToOpenLit(HexagonModel cell)
        {
            if (_closedList.FirstOrDefault(c => c.CellGameModel == cell) != null)
                return false;
            if (_openList.FirstOrDefault(o => o.CellGameModel == cell) != null)
                return false;
            if (!cell.CanPlace)
                return false;

            return true;
        }

        private class PathCell
        {
            public HexagonModel CellGameModel { get; }
            public PathCell PastCell { get; }
            public int TargetRange { get; }
            public int CostMove { get; }

            public int CurrentTravelDistance => CostMove + TargetRange;

            public PathCell(HexagonModel cellGameModel, PathCell pastCell, int targetRange)
            {
                CellGameModel = cellGameModel;
                PastCell = pastCell;
                TargetRange = targetRange;
                CostMove = 1 + (pastCell == null ? 0 : PastCell.CostMove);
            }
        }
    }
}
