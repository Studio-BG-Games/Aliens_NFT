using MapCraete;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapDataLoaderFromServer : MonoBehaviour
{  
    public void Load(string url, Action<MapData> data)
    {
        StartCoroutine(CreateData(url, data));
    }

    public void Load(Action<Action<List<MapLoadPrefab>>, List<MapData>> datasAction, Action<List<MapLoadPrefab>> action)
    {
        StartCoroutine(CreateData(HttpAddresses.Lands, datasAction, action));
    }

    private IEnumerator CreateData(string url, Action<Action<List<MapLoadPrefab>>, List<MapData>> datasAction, Action<List<MapLoadPrefab>> action)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
                HttpAddresses.ErrorHandling(webRequest, url);

            MapData mapData = JsonUtility.FromJson<MapData>(webRequest.downloadHandler.text);
            List<MapData> mapDats = new List<MapData>();
            mapDats.Add(mapData);

            datasAction?.Invoke(action, mapDats);
        }
    }

    private IEnumerator CreateData(string url, Action<MapData> action)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
                HttpAddresses.ErrorHandling(webRequest, url);

            MapData mapData = JsonUtility.FromJson<MapData>(webRequest.downloadHandler.text);
            action?.Invoke(mapData);
        }
    }
}
