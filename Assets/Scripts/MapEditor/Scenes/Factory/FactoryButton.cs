using MapSpase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonEditor
{
    public class FactoryButton : MonoBehaviour
    {
        [SerializeField] private GroundTypeButton _prefab;

        public GroundTypeButton Create(GroundType targetGroundType, PrefabsPanelHandler prefabsPanel, ButtonsPanel buttonsPanel)
        {
            GroundTypeButton button = Instantiate(_prefab, buttonsPanel.transform);
            button.Initialization(targetGroundType, prefabsPanel, buttonsPanel.ToggleGroup);
            return button;
        }
    }
}
