using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MapCreatorPanel))]
public class MapCreatorBinder : MonoBehaviour
{
    [SerializeField] private TMP_InputField _xField;
    [SerializeField] private TMP_InputField _yField;
    [SerializeField] private TMP_InputField _nameField;

    MapCreatorPanel _panel;

    private void Awake()
    {
        _panel = GetComponent<MapCreatorPanel>();
    }

    private void OnEnable()
    {
        _xField.onEndEdit.AddListener(_panel.ChageX);
        _yField.onEndEdit.AddListener(_panel.ChageY);
        _nameField.onEndEdit.AddListener(_panel.ChageName);
    }

    private void OnDisable()
    {
        _xField.onEndEdit.RemoveListener(_panel.ChageX);
        _yField.onEndEdit.RemoveListener(_panel.ChageY);
        _nameField.onEndEdit.RemoveListener(_panel.ChageName);
    }
}
