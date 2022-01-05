using MapSpase;
using UnityEngine;

namespace HexagonEditor
{
    public class FactoryPrefabsPanel : MonoBehaviour
    {
        [SerializeField] private PrefabsPanel _prefab;

        public PrefabsPanel Create(GroundType targetGroundType, PrefabsPanelHandler prefabsPanel, PanelsContainer container, ChangeControler changeControler)
        {
            PrefabsPanel panel = Instantiate(_prefab, container.transform);
            panel.Initialization(targetGroundType, changeControler);
            prefabsPanel.AddPanel(panel);
            panel.gameObject.SetActive(false);
            panel.name = targetGroundType.ToString();
            return panel;
        }
    }
}
