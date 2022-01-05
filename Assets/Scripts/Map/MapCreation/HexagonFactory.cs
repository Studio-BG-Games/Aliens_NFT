using MapSpase;
using MapSpase.Hexagon;
using UnityEngine;

namespace MapCraete
{
    public class HexagonFactory : MonoBehaviour
    {
        public HexagonModel CreateHexagonModel(Cell cell)
        {
            string path = MapResourcePaths.Ground.GetPaths(cell.CurrentGroundType, cell.GroundSubtype);

            HexagonModel cellObject = Instantiate(Resources.Load<HexagonModel>(MapResourcePaths.Ground.StandardHex), transform);
            cellObject.GetComponent<MeshRenderer>().material = Resources.Load<Material>(path);

            cellObject.transform.localPosition = Vector3.zero;
            cellObject.name = $"X: {cell.Position.x} | Y: {cell.Position.y}";

            cellObject.Initialization(cell);
            return cellObject;
        }
    }
}
