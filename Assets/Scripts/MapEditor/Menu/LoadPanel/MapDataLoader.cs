using MapCraete;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapDataLoader 
{ 
    private const string Json = ".json";

    public List<MapData> Load()
    {
        List<MapData> mapDatas = new List<MapData>();
        List<string> paths = FindPaths();

        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                MapData mapData = JsonUtility.FromJson<MapData>(File.ReadAllText(path));
                mapDatas.Add(mapData);
            }
            else
            {
                throw new System.InvalidOperationException("Path not found");
            }
        }

        return mapDatas;
    }

    private List<string>  FindPaths()
    {
        List<string> paths = new List<string>();

        if (Directory.Exists(MapResourcePaths.Map.PathSaveAndLoad))
        {
            string[] fileEntries = Directory.GetFiles(MapResourcePaths.Map.PathSaveAndLoad);

            foreach (string fileName in fileEntries)
                if (Path.GetExtension(fileName) == Json)
                    paths.Add(fileName);
        }

        return paths;
    }
}
