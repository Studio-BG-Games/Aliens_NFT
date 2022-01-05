using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreatePath;

public class InputStateSystem : MonoBehaviour
{
    private PreparingCreateMonster _preparingCreateMonster;
    private PathMaker _movePathGenerator;

    public InputState State { get; private set; } = InputState.Idel;

    public void Initialization(PreparingCreateMonster preparingCreateMonster, PathMaker movePathGenerator)
    {
        if (preparingCreateMonster != null)
        {
            _preparingCreateMonster = preparingCreateMonster;
            _movePathGenerator = movePathGenerator;

            Subscribe();
        }
    }

    private void Subscribe()
    {
        _preparingCreateMonster.StartPlaced += OnStartPlaced;
        _preparingCreateMonster.CardPlaced += OnStopPlaced;
        _preparingCreateMonster.CardReset += OnStopPlaced;

        _movePathGenerator.StartGenerate += OnStartGenerate;
        _movePathGenerator.StopGenerate += OnStopGenerate;
    }

    private void OnStartPlaced()
    {
        if(State == InputState.Move)
            _movePathGenerator.Reset();

        State = InputState.Spawn;
    }

    private void OnStartGenerate() => State = InputState.Move;

    private void OnStopGenerate() => State = InputState.Idel;

    private void OnStopPlaced() => State = InputState.Idel;

    private void OnDisable() => Unsubscribe();

    private void Unsubscribe()
    {
        _preparingCreateMonster.StartPlaced -= OnStartPlaced;
        _preparingCreateMonster.CardPlaced -= OnStopPlaced;
        _preparingCreateMonster.CardReset -= OnStopPlaced;

        _movePathGenerator.StartGenerate -= OnStartGenerate;
        _movePathGenerator.StopGenerate -= OnStopGenerate;
    }
}

public enum InputState
{
    Idel,
    Move,
    Spawn
}
