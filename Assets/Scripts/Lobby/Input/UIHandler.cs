using CreatePath;
using MapSpase;
using MapSpase.Hexagon;
using UnityEngine;

[RequireComponent(typeof(InputStateSystem))]
public class UIHandler : MonoBehaviour
{
    [SerializeField] private Map _map;

    private PreparingCreateMonster _preparingCreateMonster;
    private ControllerPathMaker _controllerPathMaker;

    private ViewCrossedHexInformation _viewCrossedHexInformation;
    private ControllerCrossedHexInformation _controllerCrossedHexInformation;

    private InputStateSystem _inputStateSystem;

    private void Awake()
    {
        _map.InitializedToArray += OnMapInitializedToArray;
               
        _preparingCreateMonster = GetComponentInChildren<PreparingCreateMonster>() ?? throw new System.NullReferenceException(nameof(PreparingCreateMonster));
        _controllerPathMaker = GetComponentInChildren<ControllerPathMaker>() ?? throw new System.NullReferenceException(nameof(ControllerPathMaker));

        _viewCrossedHexInformation = GetComponentInChildren<ViewCrossedHexInformation>() ?? throw new System.NullReferenceException(nameof(ViewCrossedHexInformation));
        _controllerCrossedHexInformation = GetComponentInChildren<ControllerCrossedHexInformation>() ?? throw new System.NullReferenceException(nameof(ControllerCrossedHexInformation));

        _inputStateSystem = GetComponent<InputStateSystem>();        
    }

    private void OnMapInitializedToArray(HexagonModel[,] array, ThreeDimensionalArray threeDimensionalArray)
    {
        CrossedHexInformation crossedHexInformation = new CrossedHexInformation(_inputStateSystem);

        _viewCrossedHexInformation.Initialization(crossedHexInformation);
        _controllerCrossedHexInformation.Initialization(crossedHexInformation);

        PathMaker movePathGenerator = new PathMaker(array, threeDimensionalArray);

        _controllerPathMaker.Initialization(movePathGenerator, crossedHexInformation, _inputStateSystem);
        _inputStateSystem.Initialization(_preparingCreateMonster, movePathGenerator);

    }

    public void OnPlayerReceived(Player player)
    {
        _controllerPathMaker.SetPlayer(player);
    }
}
