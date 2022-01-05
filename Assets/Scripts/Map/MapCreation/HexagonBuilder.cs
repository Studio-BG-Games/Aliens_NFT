using MapSpase;
using MapSpase.Hexagon;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapCraete
{
    [RequireComponent(typeof(HexagonFactory))]
    public class HexagonBuilder : MonoBehaviour
    {
        private const float MagnitudeError = 0.018f;
        private HexagonFactory _factory;

        private void Awake()
        {
            _factory = GetComponent<HexagonFactory>();
        }

        public HexagonModel[,] Build(List<Cell> cells)
        {
            HexagonModel[,] gameModelCells = CreateHexagonModels(cells);
            Place(gameModelCells);
            return gameModelCells;
        }
             
        private HexagonModel[,] CreateHexagonModels(List<Cell> cells)
        {
            List<HexagonModel> cellsList = new List<HexagonModel>();

            foreach (var cell in cells)
            {
                HexagonModel hexagonModel = _factory.CreateHexagonModel(cell);
                cellsList.Add(hexagonModel);
            }

            return PopulateArrayCells(cellsList, cells);
        }

        private HexagonModel[,] PopulateArrayCells(List<HexagonModel> cellsList, List<Cell> cells)
        {
            HexagonModel[,] gameModelCells = DefineGridSize(cells);
            Vector2Int arrayDifference = -cellsList[0].Cell.Position;
            int number = 0;

            for (int x = 0; x < gameModelCells.GetLength(0); x++)
            {
                for (int y = 0; y < gameModelCells.GetLength(1); y++)
                {
                    gameModelCells[x, y] = cellsList[number];

                    cellsList[number].TrySetArrayNumberDifference(arrayDifference);
                    number++;
                }
            }

            return gameModelCells;
        }

        private void Place(HexagonModel[,] hexagons)
        {
            Vector2 placementSteps = DefinePlacementSteps();

            for (int x = 0; x < hexagons.GetLength(0); x++)
            {
                for (int y = 0; y < hexagons.GetLength(1); y++)
                {
                    float yPosition = hexagons[x, y].Cell.LocationHeight;
                    float xPosition = (placementSteps.x * 2 * x) + (placementSteps.x * (y % 2));
                    float zPosition = placementSteps.y * 1.5f * y;

                    hexagons[x, y].transform.localPosition = new Vector3(xPosition, yPosition, zPosition);
                }
            }
        }

        private Vector2 DefinePlacementSteps()
        {
            GameObject standardHex = Instantiate(Resources.Load<GameObject>(MapResourcePaths.Ground.StandardHex));
            standardHex.transform.position = Vector3.zero;
            Collider hexColleder = standardHex.GetComponent<Collider>();
            Vector2 placementSteps = Vector2.zero;
            Vector3 rr = hexColleder.ClosestPointOnBounds(new Vector3(int.MaxValue, 0, 0));
            placementSteps.x = (float)Math.Round(hexColleder.ClosestPointOnBounds(new Vector3(int.MaxValue, 0, 0)).x, 2);
            placementSteps.y = (float)Math.Round(hexColleder.ClosestPointOnBounds(new Vector3(0, 0, int.MaxValue)).z, 2);

            placementSteps.x -= MagnitudeError;

            Destroy(standardHex);

            return placementSteps;
        }

        private HexagonModel[,] DefineGridSize(List<Cell> cells)
        {
            int minSizeX = 0, maxSizeX = 0, minSizeY = 0, maxSizeY = 0;

            foreach (var cell in cells)
            {
                if (cell.Position.x > maxSizeX)
                    maxSizeX = cell.Position.x;
                if (cell.Position.x < minSizeX)
                    minSizeX = cell.Position.x;

                if (cell.Position.y > maxSizeY)
                    maxSizeY = cell.Position.y;
                if (cell.Position.y < minSizeY)
                    minSizeY = cell.Position.y;
            }

            if (minSizeX <= 0)
                minSizeX -= 1;
            if (minSizeY <= 0)
                minSizeY -= 1;

            int x = Mathf.Abs(minSizeX) + maxSizeX;
            int y = Mathf.Abs(minSizeY) + maxSizeY;

            return new HexagonModel[x, y];
        }
    }
}
