using UnityEngine;
using UnityEngine.SceneManagement;
using HexasphereGrid;
using IJunior.TypedScenes;

public class PopUpMenu : MonoBehaviour
{
    private Vector3 _offsetServerInfoText = new Vector3(-145, 100);

    [SerializeField] private Hexasphere _hexasphere;
    [SerializeField] private GameObject _serverInfoText;
    
    private void Start()
    {
        _hexasphere.OnTileMouseOver += OnHexasphereTileMouseOver;
        _hexasphere.OnTileClick += OnHexasphereTileClick;

        _serverInfoText.SetActive(false);
    }

    /// <summary>
    /// ��������� �������������� ������� �� ����� tile.
    /// ������� ����� �� �������� �� ���������.
    /// </summary>
    private void Update()
    {
        if (!_hexasphere.isMouseOver)
            _serverInfoText.SetActive(false);
        else
            _serverInfoText.transform.position = Input.mousePosition - _offsetServerInfoText;
    }

    /// <summary>
    /// ������� ��������� �� tile.
    /// �������� ��������� � �����������, ������ ��������.
    /// </summary>
    /// <param name="tileIndex"></param>
    private void OnHexasphereTileMouseOver(int tileIndex)
    {
        _serverInfoText.SetActive(true);
    }

    /// <summary>
    /// ������� ������� �� tile.
    /// ������ �����
    /// </summary>
    /// <param name="tileIndex"></param>
    private void OnHexasphereTileClick(int tileIndex)
    {/*
        IJunior.TypedScenes.Map.Load(HttpAddresses.Player1);*/
    }
}
