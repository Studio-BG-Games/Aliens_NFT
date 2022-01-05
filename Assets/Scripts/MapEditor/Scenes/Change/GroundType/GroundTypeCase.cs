using MapCraete;
using MapSpase;
using MapSpase.Hexagon;
using System;

namespace HexagonEditor
{
    public struct GroundTypeCase : IChangeCase
    {
        private GroundType _targetGroundType;
        private int _targetGroundSubtype;

        public GroundTypeCase(GroundType targetGroundType, int targetGroundSubtype)
        {
            _targetGroundType = targetGroundType;
            _targetGroundSubtype = targetGroundSubtype;
        }

        public CellStruct GetCellStruct(HexagonModel targetHexagon)
        {
            CellStruct newCellStruct = new CellStruct(targetHexagon.Cell);
            newCellStruct.ground = Convert.ToInt32(_targetGroundType);
            newCellStruct.ground_subtype = _targetGroundSubtype;

            return newCellStruct;
        }
    }
}
