using MapSpase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapResourcePaths
{
    public static class Map
    {
        public static readonly string PathSaveAndLoad = Application.streamingAssetsPath + "/Maps";
    }

    public static class Ground
    {
        private const string Folder = "Tiles/Hex/";
        public const string StandardHex = Folder + "StandardHex";

        public static string GetPaths(GroundType groundType, int groundSubtype)
        {
            string path = Folder + $"{groundType}/{groundSubtype}/{groundType} {groundSubtype}";
            return path;            
        }
    }

    public static class EnvironmentObject
    {
        private const string Folder = "EnvironmentObject/";

        public static string GetPaths(string objectId)
        {
            int firstChar = int.Parse(objectId.Substring(0, 1));
            string id = objectId.Substring(1);

            string groundType = $"{(GroundType)firstChar}";
            string path = Folder + $"{groundType}/{id}";

            return path;            
        }
    }
}

