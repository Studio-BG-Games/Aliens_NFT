using MapCraete;
using MapSpase;
using MapSpase.Hexagon;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class MapSaver : MonoBehaviour
{
    private void Awake()
    {
        if (!Directory.Exists(MapResourcePaths.Map.PathSaveAndLoad))
            Directory.CreateDirectory(MapResourcePaths.Map.PathSaveAndLoad);
    }

    public void Save(Map map)
    {
        IReadOnlyList<HexagonModel> hexagons = map.Hexagons;

        MapData mapData = new MapData(hexagons.Count, map.Id, map.Name);
        foreach (var hexagon in hexagons)
        {
            CellStruct cellStruct = new CellStruct(hexagon.Cell);
            mapData.data[hexagon.Cell.Id - 1] = cellStruct;
        }

        Save(mapData);
    }

    public void Save(MapData mapData)
    {
        StartCoroutine(TrySaveToServer(mapData));
    }

    private IEnumerator TrySaveToServer(MapData mapData)
    {
        string url = HttpAddresses.Lands;
        WWWForm formData = new WWWForm();

        using (UnityWebRequest webRequest = UnityWebRequest.Post(url, formData))
        {
            string json = JsonUtility.ToJson(mapData);

            byte[] psotBytes = Encoding.UTF8.GetBytes(json);

            UploadHandler uploadHandler = new UploadHandlerRaw(psotBytes);

            webRequest.uploadHandler = uploadHandler;
            webRequest.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
                HttpAddresses.ErrorHandling(webRequest, url);

            MapData postAnswer = JsonUtility.FromJson<MapData>(webRequest.downloadHandler.text);

        }
    }
}
