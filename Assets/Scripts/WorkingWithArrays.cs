using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WorkingWithArrays
{
    public static List<T> ConvertToList<T>(T[,] aray)
    {
        List<T> list = new List<T>();
        foreach (T hexagon in aray)
            list.Add(hexagon);

        return list;
    }

    public static List<T> ConvertToList<T>(T[] aray)
    {
        List<T> list = new List<T>();
        foreach (T hexagon in aray)
            list.Add(hexagon);

        return list;
    }
}
