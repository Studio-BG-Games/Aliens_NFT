using IJunior.TypedScenes;
using MapCraete;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapCreatorButton : MonoBehaviour
{
    private const float TimeWaite = 2f;
    private const string Create = nameof(Create);
    private const string Waite = nameof(Waite);

    [SerializeField] private TMP_Text _text;
    [SerializeField] private Color _errorColor;

    private Button _button;
    private MapCreatorPanel _mapCreatorPanel;

    private void Awake()
    {
        _text.text = Create;
        _button = GetComponent<Button>();
        _mapCreatorPanel = GetComponentInParent<MapCreatorPanel>() ?? throw new ArgumentNullException($"{nameof(MapCreatorPanel)} is null");
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnButtonClick);
        _mapCreatorPanel.CreateError += OnCreateError;
        _mapCreatorPanel.CreateStarted += OnCreateStarted;
        _mapCreatorPanel.CreateCompleted += OnCreateCompleted;
    }

    private void OnCreateCompleted()
    {
        _text.text = Create;
    }

    private void OnCreateStarted()
    {
        _text.text = Waite;
    }

    private void OnCreateError()
    {
        _text.text = Create;
        StartCoroutine(WaiteError());
    }

    private void OnButtonClick()
    {
        _mapCreatorPanel.Create();
    }

    private IEnumerator WaiteError()
    {
       _button.interactable = false;

        Color mainColor = _text.color;
        _text.color = _errorColor;
        yield return new WaitForSeconds(TimeWaite);
        _text.color = mainColor;

        _button.interactable = true;
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnButtonClick);
        _mapCreatorPanel.CreateError -= OnCreateError;
        _mapCreatorPanel.CreateStarted -= OnCreateStarted;
        _mapCreatorPanel.CreateCompleted -= OnCreateCompleted;
    }
}
