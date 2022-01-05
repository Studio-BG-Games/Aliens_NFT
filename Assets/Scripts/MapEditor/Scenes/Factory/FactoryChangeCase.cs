using UnityEngine;

namespace HexagonEditor
{
    public class FactoryChangeCase : MonoBehaviour
    {
        [SerializeField] private ChangeCase _changeCase;
        [SerializeField] private Transform _container;
        public ChangeCase Create(IChangeCase iChangeCase, ChangeControler changeControler,string name)
        {
            ChangeCase changeCase = Instantiate(_changeCase, _container);
            changeCase.Initialization(iChangeCase, changeControler, name);
            return changeCase;
        }
    }
}
