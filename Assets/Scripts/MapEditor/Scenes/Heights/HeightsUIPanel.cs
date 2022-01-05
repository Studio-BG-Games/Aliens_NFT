using System;
using UnityEngine;

namespace HexagonEditor
{
    [RequireComponent(typeof(HeightsModel))]
    public class HeightsUIPanel : MonoBehaviour
    {
        private HeightsModel _heightsModel;
        private HeightsInput _inputPanel;

        private void Awake()
        {
            _heightsModel = GetComponent<HeightsModel>();
            _inputPanel = GetComponentInChildren<HeightsInput>() ?? throw new NullReferenceException($"{nameof(HeightsInput)} is null");
            _inputPanel.Initialization();
            OnResetHexagonModel();
        }

        private void OnEnable()
        {
            _heightsModel.SetHexagonModel += OnSetHexagonModel;
            _heightsModel.ChangeHeights += OnChangeHeights;
            _heightsModel.ResetHexagonModel += OnResetHexagonModel;
        }

        private void OnSetHexagonModel(float value)
        {
            _inputPanel.gameObject.SetActive(true);
            _inputPanel.ChangedValue += OnChangedValue;
            _inputPanel.ChangeValue(value);
        }

        private void OnChangeHeights(float value)
        {
            _inputPanel.ChangeValue(value);
        }

        private void OnResetHexagonModel()
        {
            _inputPanel.gameObject.SetActive(false);
            _inputPanel.ChangedValue -= OnChangedValue;
            _inputPanel.ChangeValue(0);
        }
          
        private void OnChangedValue(float value)
        {
            _heightsModel.TryChangeHeight(value);
        }

        private void OnDisable()
        {
            _heightsModel.SetHexagonModel -= OnSetHexagonModel;
            _heightsModel.ChangeHeights -= OnChangeHeights;
            _heightsModel.ResetHexagonModel -= OnResetHexagonModel;
        }
    }
}
