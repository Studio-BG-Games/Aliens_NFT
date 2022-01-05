using CreatePath;
using MapSpase;
using MapSpase.Hexagon;
using UnityEngine;

[RequireComponent(typeof(InputStateSystem))]
public class HexagonEditorUIInitializer : MonoBehaviour
{
    [SerializeField] private Map _map;

    private ViewCrossedHexInformation _viewCrossedHexInformation;
    private ControllerCrossedHexInformation _controllerCrossedHexInformation;

    private InputStateSystem _inputStateSystem;

    private void Awake()
    {
        _map.InitializedToArray += OnMapInitializedToArray;

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

        _inputStateSystem.Initialization(null, movePathGenerator);
    }
}
