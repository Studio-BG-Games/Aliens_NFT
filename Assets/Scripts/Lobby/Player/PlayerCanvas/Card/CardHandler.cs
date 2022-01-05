using MonsterSpace;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardBuilder))]
public class CardHandler : MonoBehaviour
{
    [SerializeField] private Transform _cardContent;

    private IReadOnlyList<Card> _cards;

    private CardBuilder _cardBuilder;
    private PreparingCreateMonster _preparingCreateMonster;

    private Card _selectedCard;

    private void Awake()
    {
        _cardBuilder = GetComponent<CardBuilder>();
        _preparingCreateMonster = GetComponentInParent<LobbyBuilder>().GetComponentInChildren<PreparingCreateMonster>();

        _preparingCreateMonster.CardPlaced += OnCardPlaced;
        _preparingCreateMonster.CardReset += OnCardReset;
    }

    public void CreateCards(IReadOnlyList<Monster> monsters)
    {
       _cards = _cardBuilder.Build(_cardContent, monsters, OnCardSelected);
    }

    private void OnCardSelected(Card card)
    {
        if (card == _selectedCard)
            return;
        if(_selectedCard != null)
            _selectedCard.gameObject.SetActive(true);

        _selectedCard = card;
        _selectedCard.gameObject.SetActive(false);
        _preparingCreateMonster.SetMonster(_selectedCard.Monster);
    }

    private void OnCardPlaced()
    {
        _selectedCard = null;
    }

    private void OnCardReset()
    {
        _selectedCard.gameObject.SetActive(true);
        _selectedCard = null;
    }

    private void OnDisable()
    {
        foreach (var card in _cards)
            card.CardSelected -= OnCardSelected;

        _preparingCreateMonster.CardPlaced -= OnCardPlaced;
        _preparingCreateMonster.CardReset -= OnCardReset;
    }

}
