using MapSpase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapSpase.Hexagon.Backlight
{
    [RequireComponent(typeof(HexagonModel))]
    [RequireComponent(typeof(FactoryHexagonBacklight))]
    public class HandlerHexagonBacklight : MonoBehaviour
    {
        public HexagonBacklight StandartBacklight { get; private set; }
        public HexagonBacklight SelectedBacklight { get; private set; }

        private HexagonModel _gameModel;
        private FactoryHexagonBacklight _factory;

        private void Awake()
        {
            _factory = GetComponent<FactoryHexagonBacklight>();
            _gameModel = GetComponent<HexagonModel>();

            StandartBacklight = new HexagonBacklight(_gameModel, _factory, BacklightType.CorrectBacklight, BacklightType.Incorrect);
            SelectedBacklight = new HexagonBacklight(_gameModel, _factory, BacklightType.SelectedCorrectBacklight, BacklightType.SelectedIncorrect);
        }
    }
}
