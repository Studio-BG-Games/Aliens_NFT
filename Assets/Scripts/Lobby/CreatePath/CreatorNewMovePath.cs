using MapSpase;
using MapSpase.Hexagon;
using MonsterSpace;
using System.Collections.Generic;

namespace CreatePath
{
    public class CreatorNewMovePath
    {
        PathFinder _pathFinder;

        public CreatorNewMovePath(ThreeDimensionalArray threeDimensionalArray)
        {
            _pathFinder = new PathFinder(threeDimensionalArray);
        }
    
        public Path Create(List<HexagonModel> area, IMonster monster, HexagonModel start, HexagonModel target)
        {
            List<HexagonModel> hexagonModels = _pathFinder.Find(area, start, target);
            return new Path(hexagonModels, monster);
        }
    }
}
