using MapSpase.Hexagon;
using MapSpase.Hexagon.Backlight;
using System;
using UnityEngine;

namespace HexagonEditor
{
    public class HeightsModel : MonoBehaviour, IBacklightUser
    {
        private const float StandartValue = 0.1f;
        private const float Multiplier = 0.01f;

        private HexagonModel _currentHexagon;

        public event Action<float> SetHexagonModel;
        public event Action<float> ChangeHeights;
        public event Action ResetHexagonModel;

        public void SetCurrentHexagon(HexagonModel currentHexagon)
        {
            if (currentHexagon != _currentHexagon)
            {
                if (_currentHexagon != null)
                    ResetCurrentHexagon();

                _currentHexagon = currentHexagon;
                _currentHexagon.HandlerHexagonBacklight.SelectedBacklight.On(this);
                SetHexagonModel?.Invoke(_currentHexagon.transform.position.y);
            }
        }

        public void ResetCurrentHexagon()
        {
            if (_currentHexagon != null)
            {
                _currentHexagon.HandlerHexagonBacklight.SelectedBacklight.Off(this);
                _currentHexagon = null;
                ResetHexagonModel?.Invoke();
            }
        }

        public void TryUpHexagon() => ChangeHeight(StandartValue);

        public void TryUpHexagonWithMultiplier() => ChangeHeight(StandartValue * Multiplier);

        public void TryDownHexagon() => ChangeHeight(-StandartValue);

        public void TryDownHexagonWithMultiplier() => ChangeHeight(-StandartValue * Multiplier);

        public bool TryChangeHeight(float value)
        {
            try
            {
                if (_currentHexagon != null)
                {
                    _currentHexagon.ChangeLocationHeight(value);
                    ChangeHeights?.Invoke(_currentHexagon.transform.position.y);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool ChangeHeight(float value)
        {
            value += _currentHexagon.transform.localPosition.y;
            return TryChangeHeight(value);
        }

        private void OnDisable()
        {
            ResetCurrentHexagon();
        }
    }
}
