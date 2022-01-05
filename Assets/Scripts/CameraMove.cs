using System.Collections.Generic;
using UnityEngine;
using MapSpase;
using MapSpase.Hexagon;

public class CameraMove : MonoBehaviour
{
    private const string MouseScrollWheel = "Mouse ScrollWheel";

    [SerializeField] private float MoveSensivity = 10f;
    [SerializeField] private float MouseScrollWheelSensivity = 150f;
    [SerializeField] private float RotateSensivity = 10;

    [SerializeField] private Map _map;

    private Vector2 _heightLimit = new Vector2(1f, 4f);
    private Vector2 _moveIndent = new Vector2(20f, 20f);

    private Vector3 _targetPosition = Vector3.zero;
    private float _targetHeight;

    private Transform _cameraAxis;
    private Vector2 _multiplierMaxDistanse = new Vector2(3, 0.25f);
    private Vector2 _maxDistanse = Vector3.zero;

    private void Awake()
    {
        //ScreenCapture
        _cameraAxis = transform.parent;
        _targetHeight = _heightLimit.y;
        _map.InitializedToList += OnInitialized;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A)) 
            _cameraAxis.transform.Rotate(0, -RotateSensivity * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.D))
            _cameraAxis.transform.Rotate(0, RotateSensivity * Time.deltaTime, 0);

        if (Input.GetAxis(MouseScrollWheel) != 0)
            _targetHeight = Mathf.Clamp(_targetHeight + (Time.deltaTime * -Input.GetAxis(MouseScrollWheel) * MouseScrollWheelSensivity), _heightLimit.x, _heightLimit.y);

        if (CheckTheNeedMovement())
            transform.localPosition = _targetPosition;
    }

    private void OnInitialized(IReadOnlyList<HexagonModel> cells)
    {
        if (cells == null || cells.Count == 0)
            return;

        MapEdgePoints mapEdgePoints = new MapEdgePoints(cells, _map.transform);

        float x = (mapEdgePoints.MaxPosition.x - mapEdgePoints.MinPosition.x) /2;
        float z = (mapEdgePoints.MaxPosition.y - mapEdgePoints.MinPosition.y) /2;

        _cameraAxis.position = new Vector3(x, _map.transform.position.y, z);
        _maxDistanse = new Vector2(x, z);
             
        transform.localPosition = new Vector3(0, _targetHeight, -_maxDistanse.y * _multiplierMaxDistanse.x);

        _heightLimit = new Vector2(_heightLimit.x * x, _heightLimit.y * z);
        _targetHeight = _heightLimit.y;

        CheckTheNeedMovement();
    }

    private bool CheckTheNeedMovement()
    {
        _targetPosition = transform.localPosition;
        _targetPosition.y = _targetHeight;

        if (Input.mousePosition.x <= _moveIndent.x)
            _targetPosition.x -= Time.deltaTime * MoveSensivity;
        else if (Input.mousePosition.x >= (Screen.width - _moveIndent.x))
            _targetPosition.x += Time.deltaTime * MoveSensivity;

        if (Input.mousePosition.y <= _moveIndent.y)
            _targetPosition.z -= Time.deltaTime * MoveSensivity;
        else if (Input.mousePosition.y >= (Screen.height - _moveIndent.y))
            _targetPosition.z += Time.deltaTime * MoveSensivity;
        
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, -_maxDistanse.x * _multiplierMaxDistanse.y, _maxDistanse.x * _multiplierMaxDistanse.y);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, -_maxDistanse.y * _multiplierMaxDistanse.x, -_maxDistanse.y * _multiplierMaxDistanse.y);

        if (_targetPosition == transform.localPosition)
            return false;

        return true;
    }
}

public struct MapEdgePoints
{
    public Vector2 MinPosition;
    public Vector2 MaxPosition;

    public MapEdgePoints(IReadOnlyList<HexagonModel> cells, Transform map)
    {
        Vector3 position;

        MinPosition = new Vector2(cells[0].transform.localPosition.x, cells[0].transform.localPosition.z);
        MaxPosition = new Vector2(cells[0].transform.localPosition.x, cells[0].transform.localPosition.z);

        foreach (var item in cells)
        {
            position = item.transform.localPosition;

            if (MaxPosition.x < position.x)
                MaxPosition = new Vector2(position.x, MaxPosition.y);
            if (MaxPosition.y < position.z)
                MaxPosition = new Vector2(MaxPosition.x, position.z);

            if (MinPosition.x > position.x)
                MinPosition = new Vector2(position.x, MinPosition.y);
            if (MinPosition.y > position.z)
                MinPosition = new Vector2(MinPosition.x, position.z);
        }

        MinPosition += new Vector2(map.position.x, map.position.z);
        MaxPosition += new Vector2(map.position.x, map.position.z);
    }
}
