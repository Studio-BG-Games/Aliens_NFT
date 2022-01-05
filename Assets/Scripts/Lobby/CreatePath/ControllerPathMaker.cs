using MapSpase;
using MapSpase.Hexagon;
using UnityEngine;

namespace CreatePath
{
    public class ControllerPathMaker : MonoBehaviour
    {
        private CrossedHexInformation _crossedHexInformation;
        private ViewPathMaker _viewPathMaker;
        private PathMaker _pathGenerator;
        private InputStateSystem _inputStateSystem;

        private Player _player;

        private bool _canSwitch = false;

        public void Initialization(PathMaker pathGenerator, CrossedHexInformation crossedHexInformation, InputStateSystem inputStateSystem) 
        {
            _inputStateSystem = inputStateSystem;

            _pathGenerator = pathGenerator;
            _viewPathMaker = new ViewPathMaker(_pathGenerator);

            _crossedHexInformation = crossedHexInformation;

            _crossedHexInformation.Freeded += OnFreeded;
            _crossedHexInformation.Selected += OnSelected;
            _crossedHexInformation.SetedHex += OnSetedHex;
        }

        public void SetPlayer(Player player)
        {
            _player = player;
        }

        private void OnSelected(HexagonModel hexagon) 
        {
            if (hexagon == null)
                return;

            if (_inputStateSystem.State != InputState.Move)
            {
                if (hexagon.MonsterGameModel != null)
                {
                    if (hexagon.MonsterGameModel.Monster.PlayerId == _player.Id)
                        _pathGenerator.TrySetTarget(hexagon);
                }
            }
            else
            {
                if (!_pathGenerator.TryCreatePath(hexagon))
                    TryResetHexagon(hexagon);
            }

        }

        private void OnFreeded() => _pathGenerator.Reset();

        private void OnSetedHex(ICell obj) => _canSwitch = false;

        private bool TryResetHexagon(HexagonModel hexagon)
        {
            if (!_canSwitch)
            {
                _canSwitch = true;
                return false;
            }

            OnFreeded();
            OnSelected(hexagon);

            return true;
        }

        private void OnDisable()
        {
            _crossedHexInformation.Freeded -= OnFreeded;
            _crossedHexInformation.Selected -= OnSelected;
            _crossedHexInformation.SetedHex -= OnSetedHex;

            _viewPathMaker.Dispose();
        }
    }
}
