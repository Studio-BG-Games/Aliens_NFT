using MapSpase.Hexagon.Backlight;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerCrossedHexInformation : HexagonRaycaster, IBacklightUser
{
    private CrossedHexInformation _crossedHexInformation;

    private bool _isInit => _crossedHexInformation != null;

    public void Initialization(CrossedHexInformation crossedHexInformation) => _crossedHexInformation = crossedHexInformation;

    private void Update()
    {
        if (!_isInit)
            return;

        if (TryRaycast() && !EventSystem.current.IsPointerOverGameObject())
            _crossedHexInformation.TrySetCrossedHex(TargetHexagon);
        else
            _crossedHexInformation.ResetCrossedHex();

        if (Input.GetMouseButtonDown(0))
            _crossedHexInformation.TrySelect();

        if (Input.GetMouseButtonDown(1))
            _crossedHexInformation.Freed();        
    }

    public override void OnRaycast() => Backlight.StandartBacklight.On(this);

    public override void OnResetCurrentCell() => Backlight.StandartBacklight.Off(this);
}
