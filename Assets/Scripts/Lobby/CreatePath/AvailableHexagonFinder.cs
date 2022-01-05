using MapSpase;
using MapSpase.Hexagon;
using System.Collections.Generic;
using System.Linq;

namespace CreatePath
{
    public class AvailableHexagonFinder
    {
        private HexagonFinderRadius _finderRadius;
        private PathFinder _pathFinder;

        public AvailableHexagonFinder(ThreeDimensionalArray threeDimensionalArray)
        {
            _pathFinder = new PathFinder(threeDimensionalArray);
            _finderRadius = new HexagonFinderRadius(threeDimensionalArray);
        }

        public List<HexagonModel> Finde(HexagonModel[,] mapCells, HexagonModel firstCell, int range)
        {
            List<HexagonModel> allHexagons = _finderRadius.FindToList(firstCell, range);
            List<HexagonModel> oneRange = _finderRadius.FindToList(firstCell, 1);
            List<HexagonModel> tempHexagonModels = CreateTempList(allHexagons, oneRange);

            List<HexagonModel> listArea = WorkingWithArrays.ConvertToList(mapCells);
            List<HexagonModel> path = new List<HexagonModel>();

            foreach (var target in tempHexagonModels.ToList())
            {
                if (target.CanPlace && _pathFinder.TryFind(out List<HexagonModel> tempPath, listArea, firstCell, target, range))
                {
                    path.Add(target);
                    tempHexagonModels = tempHexagonModels.Except(tempPath).ToList();
                }

                tempHexagonModels.Remove(target);
            }

            allHexagons = allHexagons.Where(h => !h.CanPlace).ToList();
            path = path.Concat(allHexagons).Concat(oneRange).Distinct().ToList();
            return path;
        }

        private List<HexagonModel> CreateTempList(List<HexagonModel> allHexagons, List<HexagonModel> oneRange)
        {
            List<HexagonModel> tempHexagonModels = new List<HexagonModel>();
            for (int i = allHexagons.Count - 1; i >= 0; i--)
                tempHexagonModels.Add(allHexagons[i]);
            
            tempHexagonModels = tempHexagonModels.Except(oneRange).ToList();

            return tempHexagonModels;
        }
    }
}
