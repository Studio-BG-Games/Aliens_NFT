using MapSpase.Hexagon;
using UnityEngine;

public static class RotationAngleInformation
{
    private const float _rotationAngleStep = 60;
    
    private static Vector3Int[] _directions = new Vector3Int[]
    {
        new Vector3Int(1,1,0), new Vector3Int(1,0,-1), new Vector3Int(0,-1,-1),
        new Vector3Int(-1,-1,0), new Vector3Int(-1, 0, 1), new Vector3Int(0,1,1)
    };

    private static float _firstAngel => _rotationAngleStep / 2;

    public static float GetAngle(int number) => _firstAngel + _rotationAngleStep * number;

    public static int CurentAngleNumber(float angleY) => Mathf.RoundToInt((angleY - _firstAngel) / _rotationAngleStep);

    public static int TargetAngleNumber(HexagonModel current, HexagonModel targeet)
    {
        Vector3Int direction = targeet.ThreeDimensionalPosition - current.ThreeDimensionalPosition;

        for (int i = 0; i < _directions.Length; i++)
        {
            if (_directions[i] == direction)
                return i;
        }

        throw new System.InvalidOperationException("DeterminantDirectioRotation");
    }

    public static int FindDistancesBetweenAngleNumber(int first, int second)
    {
        int value = first;

        int clockwiseResult = 0;
        int counterClockwiseResult = 0;

        while (value != second)
        {
            clockwiseResult++;
            value++;
            if (value >= _directions.Length)
                value = 0;
        }

        value = first;

        while (value != second)
        {
            counterClockwiseResult++;
            value--;
            if (value < 0)
                value = _directions.Length - 1;
        }

        return clockwiseResult < counterClockwiseResult ? clockwiseResult : counterClockwiseResult;
    }
}
