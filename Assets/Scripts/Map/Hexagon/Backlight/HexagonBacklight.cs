using MapSpase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapSpase.Hexagon.Backlight
{
    public class HexagonBacklight
    {
        private HexagonModel _gameModel;
        private FactoryHexagonBacklight _factory;

        private GameObject _incorrectBacklight;
        private GameObject _correctBacklight;

        private BacklightType _incorrectType;
        private BacklightType _correctType;

        private GameObject _lastBacklight;

        private List<IBacklightUser> _users = new List<IBacklightUser>();

        public HexagonBacklight(HexagonModel gameModel, FactoryHexagonBacklight factory, BacklightType correctType, BacklightType incorrectType)
        {
            _gameModel = gameModel;
            _factory = factory;
            _incorrectType = incorrectType;
            _correctType = correctType;
        }

        public void On(IBacklightUser user)
        {
            _users.Add(user);
            _users = _users.Distinct().ToList();

            OnBacklight();
        }

        public void Off(IBacklightUser user)
        {
            _users.Remove(user);

            if (_users.Count == 0)
                _lastBacklight.SetActive(false);

        }

        public void OnBacklight()
        {
            if (_gameModel.CanPlace)
            {
                if (_correctBacklight == null)
                    _factory.CreateBacklight(out _correctBacklight, _correctType);

                SetLastBacklight(_correctBacklight);
            }
            else
            {
                if (_incorrectBacklight == null)
                    _factory.CreateBacklight(out _incorrectBacklight, _incorrectType);

                SetLastBacklight(_incorrectBacklight);
            }

            _lastBacklight.SetActive(true);
        }

        private void SetLastBacklight(GameObject backlight)
        {
            if (_lastBacklight != backlight)
            {
                if(_lastBacklight != null)
                    _lastBacklight.SetActive(false);

                _lastBacklight = backlight;
            }
        }
    }

    public interface IBacklightUser
    { }
}
