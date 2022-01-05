using MonsterSpace;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class Card : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;
    [SerializeField] private TMP_Text _soulTotal;
    [SerializeField] private TMP_Text _power;
    [SerializeField] private TMP_Text _speed;
    [SerializeField] private Image _raceId;

    private bool _isInit = false;
    
    public Monster Monster { get; private set; }

    public event Action<Card> CardSelected;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public void Initialization(Monster monster, Sprite raceId)
    {
        if (_isInit)
            throw new InvalidOperationException("Reinitialization");

        Monster = monster;

        _name.text = Monster.Name;
        _soulTotal.text = Monster.SoulTotal.ToString();
        _power.text = Monster.Power.ToString();
        _speed.text = Monster.Speed.ToString();
        _raceId.sprite = raceId;

        _isInit = true;
    }


    private void OnButtonClick()
    {
        CardSelected?.Invoke(this);
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(OnButtonClick);
    }
}
