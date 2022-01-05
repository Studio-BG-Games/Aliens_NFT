using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public abstract class CheckingEnteredValue : MonoBehaviour
{
    private Color _errorColor = Color.red;
    private Color _standartColor = Color.white;

    private TMP_InputField _field;

    protected virtual string StandartValue { get; set; } = "";

    public event Action<string> ChangeValue;

    private void Awake()
    {
        _field = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        _field.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(string value)
    {        
        if (Validate(value))
        {
            _field.textComponent.color = _standartColor;
        }
        else
        {
            _field.textComponent.color = _errorColor;
            _field.text = StandartValue;
        }

        ChangeValue?.Invoke(_field.text);
    }

    protected abstract bool Validate(string valueString);

    private void OnDisable()
    {
        _field.onValueChanged.RemoveListener(OnValueChanged);
    }
}
