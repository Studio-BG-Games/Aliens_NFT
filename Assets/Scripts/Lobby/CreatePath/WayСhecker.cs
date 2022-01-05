namespace CreatePath
{
    public static class WayСhecker
    {
        public static bool DuplicatePathCheck(Path currentPath, Path newPath)
        {
            if (currentPath.Hexagons.Count != newPath.Hexagons.Count)
                return true;

            for (int i = 0; i < currentPath.Hexagons.Count; i++)
            {
                if (currentPath.Hexagons[i] != newPath.Hexagons[i])
                    return true;
            }

            return false;
        }
    }
}
