using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace HexagonEditor
{
    [RequireComponent(typeof(TMP_InputField))]
    public class HeightsInput : MonoBehaviour
    {
        private const char TragetChar = '.';

        private TMP_InputField _field;

        public event Action<float> ChangedValue;

        public void Initialization()
        {
            _field = GetComponent<TMP_InputField>();
        }

        private void OnEnable()
        {
            _field.onValueChanged.AddListener(OnValueChanged);
        }

        public void ChangeValue(float value)
        {
            _field.text = value.ToString();
        }

        private void OnValueChanged(string valueString)
        {
            if (valueString.Length != 0)
            {
                valueString = valueString.Replace(',', TragetChar);
                valueString = Convert.ToString(valueString, new CultureInfo("en-US"));
                Debug.Log(valueString);
                if (!Char.IsDigit(valueString[valueString.Length - 1]) && valueString[valueString.Length - 1] != TragetChar)
                {
                    valueString = valueString.Remove(valueString.Length - 1);
                }

                if (valueString.Length != 0 && valueString[valueString.Length - 1] != TragetChar)
                {
                    if (float.TryParse(valueString, NumberStyles.Any, new CultureInfo("en-US"), out float value))
                    {
                        _field.text = value.ToString();
                        ChangedValue?.Invoke(value);
                    }

                }
                else if (valueString[0] == TragetChar)
                {
                    _field.text = $"0{TragetChar }";
                }
                else if (valueString.Length == 0)
                {
                    _field.text = "0";
                }
            }
            else
            {
                _field.text = "0";
            }
        }

        private void OnDisable()
        {
            _field.onValueChanged.RemoveListener(OnValueChanged);
        }
    }
}
