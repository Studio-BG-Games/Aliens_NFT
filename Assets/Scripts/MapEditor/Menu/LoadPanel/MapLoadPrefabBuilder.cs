using MapCraete;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapLoadPool), typeof(MapDataLoaderFromServer))]
public class MapLoadPrefabBuilder : MonoBehaviour
{
    private MapDataLoaderFromServer _mapDataLoader;
    private MapLoadPool _poll;

    [SerializeField] private MapLoadPrefab _prefab;
    [SerializeField] private Transform _container;

    public void Initialization()
    {
        _poll = GetComponent<MapLoadPool>();
        _poll.Initialization(_prefab, _container);
        _mapDataLoader = GetComponent<MapDataLoaderFromServer>();
    }

    public void StartBuild(Action<List<MapLoadPrefab>> action)
    {
        _mapDataLoader.Load(Build, action);
    }

    private void Build(Action<List<MapLoadPrefab>> action, List<MapData> mapDatas)
    {
        List<MapLoadPrefab> mapLoads = new List<MapLoadPrefab>();

        foreach (var mapData in mapDatas)
        {
            MapLoadPrefab mapLoadPrefab = _poll.Get();
            mapLoadPrefab.Initialization(mapData);
            mapLoadPrefab.gameObject.SetActive(true);
            mapLoads.Add(mapLoadPrefab);
        }

        action?.Invoke(mapLoads);
    }
}
