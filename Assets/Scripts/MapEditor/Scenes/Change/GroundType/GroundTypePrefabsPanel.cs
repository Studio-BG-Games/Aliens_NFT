using UnityEngine;

namespace HexagonEditor
{
    public class GroundTypePrefabsPanel : PrefabsPanel
    {
        protected override void Filling()
        {
            int number = 0;

            while (true && number < 100)
            {
                try
                {
                    if (Resources.Load(MapResourcePaths.Ground.GetPaths(GroundType, number)))
                    {
                        GroundTypeCase objectCase = new GroundTypeCase(GroundType, number);
                        string name = $"{GroundType} {number}";
                        ChangeCase newCase = FactoryChangeCase.Create(objectCase, ChangeControler, name);
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
