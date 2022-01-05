using System;
using UnityEngine;
using MapSpase;
using MapSpase.Hexagon;
using MonsterSpace.Build;
using System.Collections.Generic;
using MonsterSpace;
using System.Linq;

namespace MapCraete
{
    [RequireComponent(typeof(Map), typeof(HexagonBuilder), typeof(FactoryEnvironmentObject))]
    [RequireComponent(typeof(MonsterBuilder))]
    public class MapBuilder : MonoBehaviour
    {
        private CellFactory _cellFactory = new CellFactory();
        private HexagonBuilder _hexagonModelBuilder;
        private FactoryEnvironmentObject _factoryEnvironmentObject;
        private MonsterBuilder _monsterBuilder;
        private CellCreationThreeDimensionalCoordinates _creationThreeDimensionalCoordinates = new CellCreationThreeDimensionalCoordinates();
        
        public event Action<Map> MapBuilt;

        private string _nameMap;
        private string _idMap;

        private void Awake()
        {
            _hexagonModelBuilder = GetComponent<HexagonBuilder>();
            _factoryEnvironmentObject = GetComponent<FactoryEnvironmentObject>();
            _monsterBuilder = GetComponent<MonsterBuilder>();
        }

        public void Build(MapData mapData)
        {
            //TODO Переделать имя и ID
            _nameMap = mapData.planet_name;
            _idMap = mapData.planet_id;
            _cellFactory.CreateCells(mapData, BuildMap);
        }

        private void BuildMap(List<Cell> cells, List<Monster> monsters)
        {
            HexagonModel[,] hexagons = _hexagonModelBuilder.Build(cells);
            _factoryEnvironmentObject.CreateEnvironmentObjects(hexagons);

            CreateMonster(hexagons, monsters);

            _creationThreeDimensionalCoordinates.Creat(hexagons, out ThreeDimensionalArray threeDimensionalArray);

            Map map = GetComponent<Map>();

            map.Initialization(_nameMap, _idMap, hexagons, threeDimensionalArray);
            MapBuilt?.Invoke(map);
        }

        private void CreateMonster(HexagonModel[,] hexagonList, List<Monster> monsters)
        {
            foreach (var hexagon in hexagonList)
            {
                if (hexagon.Cell.Monster != null)
                {
                    Monster monster = monsters.FirstOrDefault(m => m.Id == hexagon.Cell.Monster.Id)?? throw new ArgumentNullException("No monster with matching ID found");                    
                    _monsterBuilder.TryBuild(monster, hexagon);
                }
            }
        }
    }
}
