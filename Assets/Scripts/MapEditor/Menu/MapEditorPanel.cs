using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditorPanel : MonoBehaviour
{
    [SerializeField] private MapCreatorPanel _mapCreatorPanel;
    [SerializeField] private MapLoadPanel _mapLoadPanel;
    [SerializeField] private GameObject _buttonPanel;

    private List<GameObject> _panels = new List<GameObject>();

    private void Awake()
    {
        _mapCreatorPanel.CreateCompleted += OpenMapLoadPanel;

        _panels.Add(_mapCreatorPanel.gameObject);
        _panels.Add(_mapLoadPanel.gameObject);
        _panels.Add(_buttonPanel);
        Open(_buttonPanel);
    }

    public void OpenMapLoadPanel() => Open(_mapLoadPanel.gameObject);

    public void CloseMapLoadPanel() => Close(_mapLoadPanel.gameObject);

    public void OpenMapCreatorPanel() => Open(_mapCreatorPanel.gameObject);

    public void CloseMapCreatorPanel() => Close(_mapCreatorPanel.gameObject);

    private void Open(GameObject target)
    {
        foreach (var panel in _panels)
        {
            if (panel != target)
                panel.SetActive(false);
        }

        target.SetActive(true);
    }

    private void Close(GameObject target)
    {
        target.SetActive(false);
        _buttonPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        _mapCreatorPanel.CreateCompleted -= OpenMapLoadPanel;
    }
}
