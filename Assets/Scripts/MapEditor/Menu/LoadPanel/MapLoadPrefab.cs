using IJunior.TypedScenes;
using MapCraete;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapLoadPrefab : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;

    private Button _button;

    private MapData _mapData;

    public void Initialization(MapData mapData)
    {
        _mapData = mapData;
        _name.text = _mapData.planet_name;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        MapEditor.Load(_mapData);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnButtonClick);
    }
}
