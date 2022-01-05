using MapCraete;
using MapSpase;
using MapSpase.Environment;
using MapSpase.Hexagon;
using System;
using UnityEngine;

namespace HexagonEditor
{
    [RequireComponent(typeof(HexagonFactory), typeof(HexagonFactory), typeof(Map))]
    public class HexagonEditor : MonoBehaviour
    {
        private HexagonFactory _hexagonFactory;
        private FactoryEnvironmentObject _environmenFactory;
        private Map _map;

        private CellFactory _cellFactory = new CellFactory();

        private void Awake()
        {
            _hexagonFactory = GetComponent<HexagonFactory>();
            _environmenFactory = GetComponent<FactoryEnvironmentObject>();
            _map = GetComponent<Map>();
        }

        public void TryChange(HexagonModel targetHexagon, IChangeCase changeCase)
        {
            var newCellStruct = changeCase.GetCellStruct(targetHexagon);
            Cange(targetHexagon, newCellStruct);
        }

        private void Cange(HexagonModel targetHexagon, CellStruct newCellStruct)
        {
            HexagonModel newHexagonModel = CreateNewHexagonModel(newCellStruct);
            Replace(targetHexagon, newHexagonModel);
        }

        private HexagonModel CreateNewHexagonModel(CellStruct cellStruct)
        {
            Cell cell = _cellFactory.CreateCell(cellStruct);
            HexagonModel newHexagonModel = _hexagonFactory.CreateHexagonModel(cell);
            _environmenFactory.CreateEnvironmentObject(newHexagonModel);
            return newHexagonModel;
        }

        private void Replace(HexagonModel targetHexagon, HexagonModel newHexagon)
        {
            Transform targetTransfor = targetHexagon.transform;

            newHexagon.transform.SetParent(targetTransfor.parent);
            newHexagon.transform.localPosition = targetTransfor.localPosition;
            newHexagon.transform.localRotation = targetTransfor.localRotation;

            _map.ChangeHexagonModel(targetHexagon, newHexagon);
        }
    }
}
