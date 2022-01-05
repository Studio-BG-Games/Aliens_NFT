using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HexagonEditor
{
    [RequireComponent(typeof(ToggleGroup))]
    public class ButtonsPanel : MonoBehaviour
    {
        public ToggleGroup ToggleGroup { get; private set; }

        private void Awake()
        {
            ToggleGroup = GetComponent<ToggleGroup>();
        }
    }
}
