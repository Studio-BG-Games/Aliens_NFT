using MapCraete;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapLoadPrefabBuilder))]
public class LoadPanelContentLoader : MonoBehaviour
{
    private MapLoadPrefabBuilder _mapLoadPrefabFactory;
    private List<MapLoadPrefab> _mapLoads;

    private void Awake()
    {
        _mapLoadPrefabFactory = GetComponent<MapLoadPrefabBuilder>();
        _mapLoadPrefabFactory.Initialization();
    }

    private void OnEnable()
    {
        _mapLoadPrefabFactory.StartBuild(SetMapLoads);
    }

    private void SetMapLoads(List<MapLoadPrefab> mapLoads)
    {
        _mapLoads = mapLoads;
    }

    private void OnDisable()
    {
        foreach (var mapLoad in _mapLoads)
        {
            mapLoad.gameObject.SetActive(false);
        }
    }


}
