using IJunior.TypedScenes;
using MapCraete;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorLoader : MonoBehaviour, ISceneLoadHandler<MapData>
{
    [SerializeField] private MapBuilder _mapBuilder;

    private MapData _mapData;

    public void OnSceneLoaded(MapData argument)
    {
        _mapData = argument;
    }

    private void Start()
    {
        _mapBuilder.Build(_mapData);
    }
}
