using UnityEngine;
using System;
using MapSpase.Hexagon.Backlight;
using MonsterSpace;

namespace MapSpase.Hexagon
{
    [RequireComponent(typeof(HandlerHexagonBacklight))]
    public class HexagonModel : MonoBehaviour
    {
        [SerializeField] private Vector3 _placementPosition;

        private Cell _cell;
        public ICell Cell => _cell;

        public bool CanPlace => CheckCanPlace();

        public Vector2Int ArrayNumberDifference { get; private set; } = Vector2Int.zero;
        public Vector3Int ThreeDimensionalPosition { get; private set; } = Vector3Int.zero;
        public Vector3 PlacementPosition => _placementPosition;

        public HandlerHexagonBacklight HandlerHexagonBacklight { get; private set; }
        public MonsterGameModel MonsterGameModel { get; private set; }

        public event Action ChangedInfo;

        private void Awake()
        {
            HandlerHexagonBacklight = GetComponent<HandlerHexagonBacklight>();
        }

        public void Initialization(Cell cell)
        {
            _cell = cell;
        }

        public void ChangeLocationHeight(float height)
        {
            _cell.ChangeLocationHeight(height);

            Vector3 position = transform.localPosition;
            position.y = height;
            transform.localPosition = position;
        }

        public bool TrySetMonsterGameModel(MonsterGameModel monsterGameModel)
        {
            if (_cell.TrySetMonster(monsterGameModel.Monster))
            {
                if (MonsterGameModel == null)
                {
                    MonsterGameModel = monsterGameModel;
                    ChangedInfo?.Invoke();
                    return true;
                }
            }

            return false;
        }

        public bool TryRemoveMonsterGameModel()
        {
            if (_cell.TryRemoveMonster())
            {
                if (MonsterGameModel != null)
                {
                    MonsterGameModel = null;
                    ChangedInfo?.Invoke();
                    return true;
                }
            }

            return false;
        }

        public void TrySetArrayNumberDifference(Vector2Int arrayNumberDifference)
        {
            if (ArrayNumberDifference == Vector2Int.zero)
                ArrayNumberDifference = arrayNumberDifference;
        }

        public void TrySetPosition(Vector3Int position)
        {
            if (ThreeDimensionalPosition == Vector3Int.zero)
                ThreeDimensionalPosition = position;
        }

        public bool CheckCanMove(HexagonModel hexagon)
        {
            if (Cell.CurrentGroundType == GroundType.Void)
                return false;
            if (Cell.CurrentGroundType == GroundType.Water)
                return false;
            if (Cell.EnvironmentObject != null)
                return false;
            if (hexagon.Cell.Monster != null && Cell.Monster.PlayerId == hexagon.Cell.Monster.PlayerId)
                return false;

            return true;
        }

        private bool CheckCanPlace()
        {
            if (Cell.CurrentGroundType == GroundType.Water)
                return false;
            if (Cell.CurrentGroundType == GroundType.Void)
                return false;
            if (Cell.EnvironmentObject != null)
                return false;
            if (Cell.Monster != null)
                return false;

            return true;
        }
    }
}
