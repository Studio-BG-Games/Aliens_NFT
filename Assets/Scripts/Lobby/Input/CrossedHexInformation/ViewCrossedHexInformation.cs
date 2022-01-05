using MapSpase;
using TMPro;
using UnityEngine;

public class ViewCrossedHexInformation : MonoBehaviour
{
    private const string EmptyMessage = "...";
    private const string CellId = "Cell ID: ";
    private const string GroundType = "Ground type: ";
    private const string CellMaster = "Cell master: ";
    private const string MonsterName = "Monster: ";
    private const string EnvironmentObject = "Environment Object: ";
    private const string Heights = "Heights: ";
    
    private CrossedHexInformation _crossedHexInformation;
    
    [SerializeField] private RectTransform _informationWindow;

    [SerializeField] private TMP_Text _id;
    [SerializeField] private TMP_Text _groundType;
    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private TMP_Text _monsterName;
    [SerializeField] private TMP_Text _environmentObjectName;
    [SerializeField] private TMP_Text _heights;

    private Vector3 _offset;
    private Vector3 _currentOffset;

    private ICell _cureentTargetCell;

    public void Initialization(CrossedHexInformation crossedHexInformation)
    {
        _crossedHexInformation = crossedHexInformation;
        _crossedHexInformation.SetedHex += OnSetedHex;
        _crossedHexInformation.ResetedHex += OnResetedHex;
        _offset = new Vector3(_informationWindow.sizeDelta.x / 2, -_informationWindow.sizeDelta.y / 2);
    }

    private void Update()
    {
        if (_informationWindow.gameObject.activeSelf)
        {
            SetOffset();
            _informationWindow.transform.position = Input.mousePosition + _currentOffset;

            if(_cureentTargetCell != null && _heights != null)
                _heights.text = $"{Heights} {_cureentTargetCell.LocationHeight}";
        }
    }

    private void OnResetedHex()
    {
        _cureentTargetCell = null;
        _informationWindow.gameObject.SetActive(false);
    }

    private void OnSetedHex(ICell cell)
    {
        _cureentTargetCell = cell;

        if (!_informationWindow.gameObject.activeSelf)
            _informationWindow.gameObject.SetActive(true);

        _id.text = CellId + $"{cell.Id}";
        _groundType.text = $"{GroundType} {cell.CurrentGroundType}";

        _playerName.text = $"{CellMaster} {(cell.PlayerId != default(string) ? cell.PlayerId : EmptyMessage)}";
        _monsterName.text = $"{MonsterName} {(cell.Monster != null ? cell.Monster.Name : EmptyMessage)}";
        _environmentObjectName.text = $"{EnvironmentObject} {(cell.EnvironmentObject != null ? cell.EnvironmentObject.Id : EmptyMessage)}";
    }

    private void SetOffset()
    {
        _currentOffset = _offset;

        if ((Input.mousePosition + _offset * 2).x >= Screen.width)
            _currentOffset.x = -_informationWindow.sizeDelta.x / 2;
        if ((Input.mousePosition + _offset * 2).y < 0)
            _currentOffset.y = _informationWindow.sizeDelta.y / 2;
    }

    private void OnDisable()
    {
        _crossedHexInformation.SetedHex -= OnSetedHex;
        _crossedHexInformation.ResetedHex -= OnResetedHex;
    }
}
