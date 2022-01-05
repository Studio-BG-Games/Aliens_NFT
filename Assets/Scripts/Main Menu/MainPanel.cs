using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanel : MonoBehaviour
{
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _editorPanel;

    public void Switch()
    {
        _gamePanel.SetActive(!_gamePanel.activeInHierarchy);
        _editorPanel.SetActive(!_editorPanel.activeInHierarchy);
    }
}
