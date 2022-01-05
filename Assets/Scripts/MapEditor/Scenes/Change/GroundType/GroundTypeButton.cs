using MapSpase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexagonEditor
{
    [RequireComponent(typeof(Toggle))]
    public class GroundTypeButton : MonoBehaviour
    {
        private GroundType _targetGroundType;
        private PrefabsPanelHandler _prefabsPanel;

        private Toggle _button;

        [SerializeField] private TMP_Text _name;

        public void Initialization(GroundType targetGroundType, PrefabsPanelHandler prefabsPanel, ToggleGroup toggleGroup)
        {
            _targetGroundType = targetGroundType;
            _prefabsPanel = prefabsPanel;
            _button.group = toggleGroup;
            gameObject.name = targetGroundType.ToString();
            _name.text = targetGroundType.ToString();
        }

        private void Awake()
        {
            _button = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            _button.onValueChanged.AddListener(OnButtonClick);
        }

        private void OnButtonClick(bool arg)
        {
            _prefabsPanel.EnabelPanel(_targetGroundType);
        }

        private void OnDisable()
        {
            _button.onValueChanged.RemoveListener(OnButtonClick);
        }

    }
}
