using System.Collections.Generic;
using System.Linq;
using System;
using MapSpase;
using MapSpase.Hexagon;
using MonsterSpace;

namespace CreatePath
{
    public class PathMaker
    {
        private AvailableHexagonFinder _availableFinder;
        private CreatorNewMovePath _pathCreator;

        private HexagonModel[,] _mapCells;
        private HexagonModel _start;

        private MonsterGameModel _monster;
        private List<HexagonModel> _area;

        public event Action StartGenerate;
        public event Action StopGenerate;

        public event Action<List<HexagonModel>> Enabel;
        public event Action<List<HexagonModel>> Disabel;

        public PathMaker(HexagonModel[,] cells, ThreeDimensionalArray threeDimensionalArray)
        {
            _availableFinder = new AvailableHexagonFinder(threeDimensionalArray);
            _pathCreator = new CreatorNewMovePath(threeDimensionalArray);
            _mapCells = cells;
        }

        public bool TrySetTarget(HexagonModel currentCell)
        {
            if(currentCell.Cell.Monster == null) 
                throw new Exception($"{nameof(currentCell)} is null");
            if (currentCell.Cell.Monster == null)
                throw new Exception("New monster is null");

            if (_monster != null)
            {
                TryCreatePath(currentCell);
                return false;
            }
            else
            {
                SetMonster(currentCell.MonsterGameModel);

                _start = currentCell;                
                _area = _availableFinder.Finde(_mapCells, currentCell, _monster.Monster.StaminaBalance);

                StartGenerate?.Invoke();

                if (_area != null)
                    Enabel?.Invoke(_area);

                return true;
            }
        }

        private void SetMonster(MonsterGameModel monster)
        {
            _monster = monster;

            _monster.Mover.StartedMove += OnStartedMove;
            _monster.Mover.StopedMove += OnStopedMove;
            _monster.Mover.OnSelected();
        }

        private void OnStartedMove()
        {
            if (_area != null)
                Disabel?.Invoke(_area);
        }

        private void OnStopedMove()
        {
            HexagonModel newTarget = null;

            foreach (var hexagon in _mapCells)
            {
                if (_monster == hexagon.MonsterGameModel)
                {
                    newTarget = hexagon;
                    break;
                }
            }

            Reset();
            TrySetTarget(newTarget);
        }

        public bool TryCreatePath(HexagonModel targetCell)
        {
            if (_area == null)
                return false;
            if (targetCell.Cell.EnvironmentObject != null)
                return false;
            if (_area.FirstOrDefault(h => h == targetCell) == null)
                return false;
            if (!targetCell.CanPlace)// ToDo Убрать прои добавлении логики на сервере 
                return false; 
            if (!targetCell.CheckCanMove(targetCell))
                return false;

            Path path = _pathCreator.Create(_area, _monster.Monster, _start, targetCell);

            if (path.Hexagons.Count <= 1)
                return false;

            _monster.Mover.TrySetPath(path);
            return true;
        }

        public void Reset()
        {
            Disabel?.Invoke(_area);
            StopGenerate?.Invoke();

            if (_monster != null)
            {
                _monster.Mover.StartedMove -= OnStartedMove;
                _monster.Mover.StopedMove -= OnStopedMove;

                _monster.Mover.OnSelectionCanceled();

                _monster = null;
                _start = null;
                _area = null;
            }
        }
    }
}
