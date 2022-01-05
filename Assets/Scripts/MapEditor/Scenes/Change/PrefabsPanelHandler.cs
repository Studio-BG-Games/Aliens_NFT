using MapSpase;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexagonEditor
{
    public class PrefabsPanelHandler : MonoBehaviour
    {
        private List<PrefabsPanel> _panels = new List<PrefabsPanel>();

        public void AddPanel(PrefabsPanel panel)
        {
            _panels.Add(panel);
        }

        public void EnabelPanel(GroundType targetGroundType)
        {
            DisabelAllPanel();
            _panels.First(p => p.GroundType == targetGroundType).gameObject.SetActive(true);
        }

        public void DisabelAllPanel()
        {
            foreach (var panel in _panels)
                panel.gameObject.SetActive(false);
        }

        public void DisabelPanel(GroundType targetGroundType)
        {
            _panels.First(p => p.GroundType == targetGroundType).gameObject.SetActive(false);
        }
    }
}
