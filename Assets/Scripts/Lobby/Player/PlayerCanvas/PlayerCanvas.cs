using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCanvas : MonoBehaviour
{
    private CardHandler _cardHandler;

    private void Awake()
    {
        _cardHandler = GetComponentInChildren<CardHandler>() ?? throw new System.NullReferenceException();
    }

    public void SetPlayer(Player player)
    {
        _cardHandler.CreateCards(player.Monsters);
    }
}
