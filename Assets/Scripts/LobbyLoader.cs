using IJunior.TypedScenes;
using MapCraete;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MapDataLoaderFromServer))]
public class LobbyLoader : MonoBehaviour
{
    private MapDataLoaderFromServer _mapDataLoaderFromServer;

    private bool _isLoading = false;

    private string _currentPlayerUrl;

    private void Awake()
    {
        _mapDataLoaderFromServer = GetComponent<MapDataLoaderFromServer>();
    }

    public void TryLoade(string playerUrl)
    {
        if (!_isLoading)
        {
            _isLoading = true;
            _currentPlayerUrl = playerUrl;
            _mapDataLoaderFromServer.Load(HttpAddresses.Lands, Loade);
        }
    }

    private void Loade(MapData mapData)
    {
        _isLoading = false;
        LobbyInformationCase Case = new LobbyInformationCase(_currentPlayerUrl, mapData);
        Map.Load(Case);
    }
}
