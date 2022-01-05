using System.Collections.Generic;
using MapSpase.Hexagon;
using MapSpase.Hexagon.Backlight;

namespace CreatePath
{
    public class ViewPathMaker : IBacklightUser
    {
        private PathMaker _pathMaker;

        public ViewPathMaker(PathMaker pathMaker)
        {
            _pathMaker = pathMaker;

            _pathMaker.Enabel += OnEnabel;
            _pathMaker.Disabel += OnDisabel;
        }

        public void OnEnabel(List<HexagonModel> hexagons)
        {
            if (hexagons != null)
                foreach (var hexagon in hexagons)
                    hexagon.HandlerHexagonBacklight.StandartBacklight.On(this);
        }

        public void OnDisabel(List<HexagonModel> hexagons)
        {
            if(hexagons != null)
                foreach (var hexagon in hexagons)
                    hexagon.HandlerHexagonBacklight.StandartBacklight.Off(this);
        }

        public void Dispose()
        {
            _pathMaker.Enabel -= OnEnabel;
            _pathMaker.Disabel -= OnDisabel;

            _pathMaker = null;
        }
    }
}
