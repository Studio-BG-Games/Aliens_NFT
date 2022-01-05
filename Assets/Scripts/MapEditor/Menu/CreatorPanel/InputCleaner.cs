using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class InputCleaner : MonoBehaviour
{
    private TMP_InputField _field;
    private MapCreatorPanel _panel;

    private void Awake()
    {
        _field = GetComponent<TMP_InputField>();
        _panel = GetComponentInParent<MapCreatorPanel>();
    }

    private void OnEnable()
    {
        _panel.CreateCompleted += OnCreateCompleted;
    }

    private void OnCreateCompleted()
    {
        _field.text = "";
    }

    private void OnDisable()
    {
        _panel.CreateCompleted -= OnCreateCompleted;
    }
}
