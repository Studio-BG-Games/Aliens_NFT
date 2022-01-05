using System;
using MapSpase.Hexagon.Backlight;
using MapSpase.Hexagon;
using MapSpase;

public class CrossedHexInformation : IBacklightUser
{    
    private InputStateSystem _inputStateSystem;

    public event Action<ICell> SetedHex;
    public event Action ResetedHex;

    public event Action<HexagonModel> Selected;
    public event Action Freeded;

    private HexagonModel _target;

    public CrossedHexInformation(InputStateSystem inputStateSystem)
    {
        _inputStateSystem = inputStateSystem;
    }   

    public void TrySetCrossedHex(HexagonModel hexagonModel)
    {
        if (_target == hexagonModel)
            return;

        if (_target != null)
            ResetCrossedHex();

        if (hexagonModel != null)
        {
            _target = hexagonModel;
            SetedHex?.Invoke(_target.Cell);
        }
    }

    public void ResetCrossedHex()
    {
        _target = null;
        ResetedHex?.Invoke();
    }

    public void TrySelect()
    {
        if (_inputStateSystem.State != InputState.Spawn)
        {
            if (_target == null)
                return;
            Selected?.Invoke(_target);
        }
    }

    public void Freed()
    {
        Freeded?.Invoke();
    }
}