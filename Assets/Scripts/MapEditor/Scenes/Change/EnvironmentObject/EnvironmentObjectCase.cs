using MapCraete;
using MapSpase.Environment;
using MapSpase.Hexagon;

namespace HexagonEditor
{
    public struct EnvironmentObjectCase : IChangeCase
    {
        private EnvironmentObject _targetObject;

        public EnvironmentObjectCase(EnvironmentObject targetObject)
        {
            _targetObject = targetObject;
        }

        public CellStruct GetCellStruct(HexagonModel targetHexagon)
        {
            CellStruct newCellStruct = new CellStruct(targetHexagon.Cell);

            EnvironmentObjectInfo info = null;
            string id = null;

            if (_targetObject != null)
            {
                info = new EnvironmentObjectInfo(_targetObject);
                id = _targetObject.Id;
            }

            newCellStruct.object_id = id;
            newCellStruct.object_attributes = info;

            return newCellStruct;
        }
    }
}
