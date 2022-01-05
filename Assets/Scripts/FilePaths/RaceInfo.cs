using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceInfo : MonoBehaviour
{
    private const string Folder = "RaceInfo/";

    public static string GetIconPath(string race)
    {
        switch (race)
        {
            default: 
                return Volcanoids.IconPath;
                /*
            case "Volcanoids":
                return Volcanoids.IconPath;
            default:
                throw new System.InvalidOperationException("No suitable land type.");*/
        }
    }

    public static class Volcanoids
    {
        public const string IconPath = Path + "VolcanoidsIcon";

        private const string Path = Folder + "Volcanoids/";
    }
}
