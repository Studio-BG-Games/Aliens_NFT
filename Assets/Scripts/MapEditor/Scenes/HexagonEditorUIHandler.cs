using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonEditor
{
    public class HexagonEditorUIHandler : MonoBehaviour
    {
        [SerializeField] private ChangeControler _prefabsPanesControler;

        [SerializeField] private GameObject _groundPanel;
        [SerializeField] private GameObject _environmentPanel;
        [SerializeField] private GameObject _heightPanel;

        public void OpenGroundPanel()
        {
            ChangePanel(_groundPanel, _environmentPanel);
        }

        public void OpenEnvironmentPanel()
        {
            ChangePanel(_environmentPanel, _groundPanel);
        }

        private void ChangePanel(GameObject first, GameObject second)
        {
            if (_heightPanel.activeInHierarchy)
                _heightPanel.SetActive(false);

            CloseChangeControler();
            OpenChangeControler();

            if (second.gameObject.activeInHierarchy)
                second.SetActive(false);

            first.SetActive(true);
        }

        public void OpenHeightPanel()
        {
            if (_prefabsPanesControler.gameObject.activeInHierarchy)
                CloseChangeControler();

            _heightPanel.SetActive(true);
        }

        private void OpenChangeControler()
        {
            _prefabsPanesControler.gameObject.SetActive(true);
        }

        private void CloseChangeControler()
        {
            _prefabsPanesControler.ResetChangeCase();
            _prefabsPanesControler.gameObject.SetActive(false);
        }
    }
}
