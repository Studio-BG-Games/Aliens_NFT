using MapSpase.Hexagon;
using UnityEngine;

namespace MapCraete
{
    public class FactoryEnvironmentObject : MonoBehaviour
    {
        public void CreateEnvironmentObject(HexagonModel hexagonModel)
        {
            if (hexagonModel.Cell.EnvironmentObject != null)
            {
                string path = MapResourcePaths.EnvironmentObject.GetPaths(hexagonModel.Cell.EnvironmentObject.Id);

                GameObject environmentObject = Instantiate(Resources.Load<GameObject>(path), hexagonModel.transform);

                environmentObject.transform.localPosition = hexagonModel.PlacementPosition;
                environmentObject.transform.eulerAngles = new Vector3(0, RotationAngleInformation.GetAngle(hexagonModel.Cell.EnvironmentObject.Rotation), 0);

            }
        }

        public void CreateEnvironmentObjects(HexagonModel[,] hexagonModels)
        {
            foreach (var hexagonModel in hexagonModels)
            {
                CreateEnvironmentObject(hexagonModel);
            }
        }
    }
}
