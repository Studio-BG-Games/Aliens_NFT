using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapLoadPool : MonoBehaviour
{
    private MapLoadPrefab _prefab;    
    private List<MapLoadPrefab> _pool;
    private Transform _container;

    public void Initialization(MapLoadPrefab prefab, Transform container)
    {
        _prefab = prefab;
        _pool = new List<MapLoadPrefab>();
        _container = container;
    }

    public MapLoadPrefab Get()
    {
        MapLoadPrefab result = _pool.FirstOrDefault(p => p.gameObject.activeSelf == false);

        if (result == null)
            result = CreateFiller();

        return result;
    }

    private MapLoadPrefab CreateFiller()
    {
        MapLoadPrefab newFiller = Instantiate(_prefab, _container);
        _pool.Add(newFiller);

        newFiller.gameObject.SetActive(false);

        return newFiller;
    }
}