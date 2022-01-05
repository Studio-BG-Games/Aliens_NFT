using MapCraete;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataCreator : MonoBehaviour
{
    public MapData Create(Vector2Int size, string name)
    {
        int allSize = size.x * size.y;
        int number = 1;
        MapData mapData = new MapData(allSize, name, name);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                CellStruct cellStruct = new CellStruct();
                cellStruct.x = x;
                cellStruct.y = y;
                cellStruct.ground = 2;
                cellStruct.ground_subtype = 0;
                cellStruct.cell_id = number;
                mapData.data[number-1] = cellStruct;
                number++;
            }
        }

        return mapData;
    }
}
