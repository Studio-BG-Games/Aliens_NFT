using System.Collections.Generic;
using MapSpase.Hexagon;
using MapSpase.Hexagon.Backlight;
using MonsterSpace;

namespace CreatePath
{
    public class Path: IBacklightUser
    {
        private List<HexagonModel> _hexagons;

        public IReadOnlyList<HexagonModel> Hexagons => _hexagons;
        public IMonster Monster { get; }

        public Path(List<HexagonModel> cells, IMonster monster)
        {
            _hexagons = cells;
            Monster = monster;
        }

        public void OnPathHighlighting()
        {
            foreach (var hexagon in Hexagons)
                hexagon.HandlerHexagonBacklight.SelectedBacklight.On(this);
        }

        public void OffPathHighlighting()
        {
            foreach (var hexagon in Hexagons)
                hexagon.HandlerHexagonBacklight.SelectedBacklight.Off(this);
        }

        public void OnStep(HexagonModel passedHexagon)
        {
            passedHexagon.HandlerHexagonBacklight.SelectedBacklight.Off(this);
        }
    }
}
