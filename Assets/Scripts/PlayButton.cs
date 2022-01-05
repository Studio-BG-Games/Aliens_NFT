using IJunior.TypedScenes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private Players _player;

    private LobbyLoader _lobbyLoader;

    private void Awake()
    {
        _lobbyLoader = GetComponentInParent<LobbyLoader>() ?? throw new System.NullReferenceException($"{nameof(LobbyLoader)} is null");
    }


    public void OnButtonClick()
    {
        switch (_player)
        {
            case Players.Player1:
                _lobbyLoader.TryLoade(HttpAddresses.Player1);
                break;
            case Players.Player2:
                _lobbyLoader.TryLoade(HttpAddresses.Player2);
                break;
            case Players.Player3:
                _lobbyLoader.TryLoade(HttpAddresses.Player3);
                break;
        }
    }
}

public enum Players
{
    Player1,
    Player2, 
    Player3
}