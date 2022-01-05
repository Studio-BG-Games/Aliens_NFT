using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexagonDistanceSearcher
{
    public static int Search(Vector3Int start, Vector3Int target)
    {
        if (start.x == target.x || start.z == target.z)
        {
            return Mathf.Abs(target.y - start.y);
        }
        else
        {
            int y = Mathf.Abs(target.y - start.y);
            int x = Mathf.Abs(target.x - start.x) + y;
            int z = Mathf.Abs(target.z - start.z) + y;

            return x > z ? z : x;
        }
    }
}
