using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexagonEditor
{
    [RequireComponent(typeof(Button))]
    public class ChangeCase : MonoBehaviour
    {
        [SerializeField] private TMP_Text _name;

        private ChangeControler _changeControler;
        private Button _button;
        private IChangeCase _changeCase;

        public void Initialization(IChangeCase changeCase, ChangeControler changeControler, string name)
        {
            _changeControler = changeControler;
            _name.text = name;
            gameObject.name = name;
            _changeCase = changeCase;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            _changeControler.SetChangeCase(_changeCase);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
