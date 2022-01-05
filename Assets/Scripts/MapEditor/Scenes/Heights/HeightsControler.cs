using MapSpase.Hexagon.Backlight;
using UnityEngine;

namespace HexagonEditor
{
    [RequireComponent(typeof(HeightsModel))]
    public class HeightsControler : HexagonRaycaster, IBacklightUser
    {
        private const string MouseScrollWheel = "Mouse ScrollWheel";

        private HeightsModel _heightsModel;

        private void Awake()
        {
            _heightsModel = GetComponent<HeightsModel>();
        }

        private void Update()
        {
            TryRaycast();

            if (TargetHexagon != null)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _heightsModel.SetCurrentHexagon(TargetHexagon);
                }

                if (Input.GetMouseButtonUp(1))
                {
                    _heightsModel.ResetCurrentHexagon();
                }

                if (Input.GetKey(KeyCode.W))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        _heightsModel.TryUpHexagonWithMultiplier();
                    else
                        _heightsModel.TryUpHexagon();
                }

                if (Input.GetKey(KeyCode.S))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        _heightsModel.TryDownHexagonWithMultiplier();
                    else
                        _heightsModel.TryDownHexagon();
                }

            }
        }
           
        public override void OnRaycast()
        {
            Backlight.SelectedBacklight.On(this);
        }

        public override void OnResetCurrentCell()
        {
            Backlight.SelectedBacklight.Off(this);
        }

        private void OnDisable()
        {
            _heightsModel.ResetCurrentHexagon();
        }
    }
}
