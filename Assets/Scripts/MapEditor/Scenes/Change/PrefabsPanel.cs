using MapSpase;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexagonEditor
{
    [RequireComponent(typeof(FactoryChangeCase))]
    public abstract class PrefabsPanel : MonoBehaviour
    {
        public GroundType GroundType { get; private set; }

        protected FactoryChangeCase FactoryChangeCase { get; private set; }
        protected ChangeControler ChangeControler { get; private set; }

        private void Awake()
        {
            FactoryChangeCase = GetComponent<FactoryChangeCase>();
        }

        public void Initialization(GroundType targetGroundType, ChangeControler changeControler)
        {
            GroundType = targetGroundType;
            ChangeControler = changeControler;
            Filling();
        }

        protected abstract void Filling();
    }
}
