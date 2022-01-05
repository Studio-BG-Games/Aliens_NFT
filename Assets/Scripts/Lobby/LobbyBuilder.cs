using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IJunior.TypedScenes;
using MapCraete;

[RequireComponent(typeof(AskingLobbyMembers))]
public class LobbyBuilder : MonoBehaviour, ISceneLoadHandler<LobbyInformationCase>
{
    private string _playerId;
    private MapData _mapData;

    private AskingLobbyMembers _askingLobbyMembers;
    private PlayerCanvas _playerCanvas;
    private MapBuilder _mapBuilder;
    private UIHandler _uIHandler;

    private void Awake()
    {
        _mapBuilder = GetComponentInChildren<MapBuilder>() ?? throw new System.NullReferenceException();
        _playerCanvas = GetComponentInChildren<PlayerCanvas>() ?? throw new System.NullReferenceException();
        _uIHandler = GetComponentInChildren<UIHandler>() ?? throw new System.NullReferenceException();

        _askingLobbyMembers = GetComponent<AskingLobbyMembers>();
    }

    public void OnSceneLoaded(LobbyInformationCase Case)
    {
        _mapData = Case.MapData;
        _playerId = Case.PlayerUrl;
    }

    public void Start()
    {
        if (_playerId == null)
            _playerId = HttpAddresses.Player3;

        Builde();
    }

    public void Builde()
    {
        _askingLobbyMembers.PlayerReceived += OnPlayerReceived;
        _askingLobbyMembers.PlayerReceived += _uIHandler.OnPlayerReceived;
        _mapBuilder.Build(_mapData);
        _askingLobbyMembers.Asking(_playerId);
    }

    private void OnPlayerReceived(Player player)
    {
        _playerCanvas.SetPlayer(player);
    }

    private void OnDestroy()
    {
        _askingLobbyMembers.PlayerReceived -= _uIHandler.OnPlayerReceived;
        _askingLobbyMembers.PlayerReceived -= OnPlayerReceived;
    }
}
