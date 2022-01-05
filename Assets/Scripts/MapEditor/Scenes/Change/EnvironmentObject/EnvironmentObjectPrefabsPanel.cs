using MapCraete;
using MapSpase.Environment;
using UnityEngine;

namespace HexagonEditor
{
    public class EnvironmentObjectPrefabsPanel : PrefabsPanel
    {
        protected override void Filling()
        {
            int groundId = (int)GroundType;
            int number = 0;

            EnvironmentObjectCase objectCase = new EnvironmentObjectCase();
            FactoryChangeCase.Create(objectCase, ChangeControler, "Delete");

            while (true && number < 100)
            {
                try
                {
                    string id;

                    if (number < 10)
                        id = $"{groundId}0{number}";
                    else
                        id = $"{groundId}{number}";

                    if (Resources.Load(MapResourcePaths.EnvironmentObject.GetPaths(id)))
                    {
                        EnvironmentObjectInfo info = new EnvironmentObjectInfo(id);
                        EnvironmentObject environmentObject = new EnvironmentObject(info);
                        objectCase = new EnvironmentObjectCase(environmentObject);
                        FactoryChangeCase.Create(objectCase, ChangeControler, id);
                    }

                    number++;
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
