using MonsterSpace;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardBuilder : MonoBehaviour
{
    private const string PathPrefab = "Card/Card"; 

    public IReadOnlyList<Card> Build(Transform container, IReadOnlyList<Monster> monsterInfos, Action<Card> action)
    {
        return CreateCards(container, monsterInfos, action);
    }

    private IReadOnlyList<Card> CreateCards(Transform container, IReadOnlyList<Monster> monsterInfos, Action<Card> action)
    {
        List<Card> cards = new List<Card>();

        foreach (var monster in monsterInfos)
        {
            if (monster.ActiveOnMap == 0)
            {
                Card card = Instantiate(Resources.Load(PathPrefab) as GameObject, container).GetComponent<Card>();
                card.name = monster.Name;
                Sprite raceId = Resources.Load<Sprite>(RaceInfo.GetIconPath(monster.RaceId));
                card.Initialization(monster, raceId);
                card.CardSelected += action;
                cards.Add(card);
            }
        }

        return cards;
    }
}
