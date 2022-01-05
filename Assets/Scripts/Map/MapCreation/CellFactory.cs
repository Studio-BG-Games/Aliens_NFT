using MapSpase;
using MapSpase.Environment;
using MonsterSpace;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapCraete
{
    public class CellFactory
    {
        public void CreateCells(MapData mapData, Action<List<Cell>> action) => CreateCells(mapData, action);

        public void CreateCells(MapData mapData, Action<List<Cell>, List<Monster>> action) => CreateCells(mapData, actionMonster: action);

        public void CreateCells(MapData mapData, Action<List<Cell>> action = null, Action<List<Cell>, List<Monster>> actionMonster = null)
        {
            List<Cell> cells = CreateCellsList(mapData);

            action?.Invoke(cells);

            if (actionMonster != null)
            {
                List<Monster> monsters = CreateListMonster(mapData);
                actionMonster?.Invoke(cells, monsters);
            }
        }

        public Cell CreateCell(CellStruct cellStruct)
        {
            EnvironmentObject newEnvironmentObject = null;
            Monster monster = null;

            if (cellStruct.object_id != "" && cellStruct.object_id != null)
                newEnvironmentObject = new EnvironmentObject(cellStruct.object_attributes);
            if (cellStruct.monster_id != "" && cellStruct.monster_id != null)
                monster = new Monster(cellStruct.monster_attributes);

            Cell newCell = new Cell(cellStruct, monster, newEnvironmentObject);
            return newCell;
        }

        private List<Monster> CreateListMonster(MapData mapData)
        {
            List<Monster> monsters = new List<Monster>();

            foreach (var cellStruct in mapData.data)
            {
                if (cellStruct.monster_id != "" && cellStruct.monster_id != null)
                {
                    Monster monster = new Monster(cellStruct.monster_attributes);
                    monsters.Add(monster);
                }
            }

            return monsters;
        }

        private List<Cell> CreateCellsList(MapData mapData) 
        {
            List<Cell> cells = new List<Cell>();

            foreach (var cellStruct in mapData.data)
            {
                Cell newCell = CreateCell(cellStruct);
                cells.Add(newCell);
            }

            return cells;
        }
    }
}
