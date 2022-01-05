using MapSpase;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonEditor
{
    [RequireComponent(typeof(FactoryButton), typeof(PrefabsPanelHandler), typeof(FactoryPrefabsPanel))]
    public class PrefabsPanelBuilder : MonoBehaviour
    {
        private void Awake()
        {
            ButtonsPanel buttonsPanel = GetComponentInChildren<ButtonsPanel>() ?? throw new NullReferenceException($"{nameof(ButtonsPanel)} is null");
            PanelsContainer panelsContainer = GetComponentInChildren<PanelsContainer>() ?? throw new NullReferenceException($"{nameof(PanelsContainer)} is null");
            ChangeControler changeControler = GetComponentInParent<ChangeControler>() ?? throw new NullReferenceException($"{nameof(ChangeControler)} is null");
            FactoryButton factoryButton = GetComponent<FactoryButton>();
            PrefabsPanelHandler prefabsPanelHandler = GetComponent<PrefabsPanelHandler>();
            FactoryPrefabsPanel factoryPrefabsPanel = GetComponent<FactoryPrefabsPanel>();

            foreach (GroundType groundType in Enum.GetValues(typeof(GroundType)))
            {
                factoryButton.Create(groundType, prefabsPanelHandler, buttonsPanel);
                factoryPrefabsPanel.Create(groundType, prefabsPanelHandler, panelsContainer, changeControler);
            }
        }
    }
}
