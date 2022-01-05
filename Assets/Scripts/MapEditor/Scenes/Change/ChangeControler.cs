using MapSpase.Hexagon.Backlight;
using MonsterSpace;
using System;
using TMPro;
using UnityEngine;

namespace HexagonEditor
{
    public class ChangeControler : HexagonRaycaster, IBacklightUser
    {
        [SerializeField] private HexagonEditor _hexagonEditor;

        private IChangeCase _changeCase;

        private void Update()
        {         
            if (_changeCase != null)
            {
                TryRaycast();

                if (TargetHexagon != null)
                {
                    if (Input.GetMouseButton(0))
                        _hexagonEditor.TryChange(TargetHexagon, _changeCase);
                    if (Input.GetMouseButtonDown(1))
                        ResetChangeCase();
                }
            }
        }

        public void ResetChangeCase()
        {
            _changeCase = null;
        }

        public void SetChangeCase(IChangeCase changeCase)
        {
            _changeCase = changeCase;
        }

        public override void OnRaycast()
        {
            Backlight.SelectedBacklight.On(this);
        }

        public override void OnResetCurrentCell()
        {
            Backlight.SelectedBacklight.Off(this);
        }
    }
}
