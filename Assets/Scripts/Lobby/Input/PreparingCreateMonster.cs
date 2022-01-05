using MapSpase.Hexagon.Backlight;
using MonsterSpace;
using MonsterSpace.Build;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MonsterBuilder))]
public class PreparingCreateMonster : HexagonRaycaster, IBacklightUser
{
    private Monster _monster;

    private MonsterBuilder _monsterBuilder;

    public event Action StartPlaced;
    public event Action CardPlaced;
    public event Action CardReset;

    private void Awake()
    {
        _monsterBuilder = GetComponent<MonsterBuilder>();
    }
      
    private void Update()
    {
        if (_monster != null)
        {
            TryRaycast();

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                Place();

            if (Input.GetMouseButtonDown(1))
                OnReset();
        }        
    }

    public void SetMonster(Monster monster)
    {
        if (_monster != null)
            ResetMonster();

        _monster = monster;
        StartPlaced?.Invoke();
    }

    public override void OnRaycast()
    {
        Backlight.SelectedBacklight.On(this);
    }

    private void Place()
    {
        if (TargetHexagon != null && TargetHexagon.CanPlace)
            _monsterBuilder.TryBuildCheckedOnServer(_monster, TargetHexagon, OnTryBuildCheckedOnServer);
    }

    private void OnTryBuildCheckedOnServer(bool result)
    {
        if (result)
        {
            CardPlaced?.Invoke();
            ResetMonster();
        }
    }

    private void OnReset()
    {
        ResetMonster();
        CardReset?.Invoke();
    }

    private void ResetMonster()
    {        
        _monster = null;
        ResetCurrentCell();
    }

    public override void OnResetCurrentCell()
    {
        Backlight.SelectedBacklight.Off(this);
    }
}
