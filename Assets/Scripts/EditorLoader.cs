using IJunior.TypedScenes;
using MapCraete;
using UnityEngine;

[RequireComponent(typeof(MapDataLoaderFromServer))]
public class EditorLoader : MonoBehaviour
{
    private MapDataLoaderFromServer _mapDataLoaderFromServer;

    private bool _isLoading = false;

    private void Awake()
    {
        _mapDataLoaderFromServer = GetComponent<MapDataLoaderFromServer>();
    }

    public void TryLoade()
    {
        if (!_isLoading)
        {
            _isLoading = true;
            _mapDataLoaderFromServer.Load(HttpAddresses.Lands, Loade);
        }
    }

    private void Loade(MapData mapData)
    {
        _isLoading = false;
        MapEditor.Load(mapData);
    }
}

