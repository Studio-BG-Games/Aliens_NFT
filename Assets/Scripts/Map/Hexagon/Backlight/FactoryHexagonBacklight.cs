using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapSpase.Hexagon.Backlight
{
    public class FactoryHexagonBacklight : MonoBehaviour
    {
        public void CreateBacklight(out GameObject backlight, BacklightType type)
        {
            backlight = Instantiate(Resources.Load(BacklightPaths.GetPath(type)) as GameObject, transform);
        }
    }
}
