using MapCraete;
using MapSpase.Hexagon;

namespace HexagonEditor
{
    public interface IChangeCase
    {
        public CellStruct GetCellStruct(HexagonModel targetHexagon);
    }
}
