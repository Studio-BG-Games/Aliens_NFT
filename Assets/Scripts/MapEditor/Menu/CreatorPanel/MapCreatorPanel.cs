using MapCraete;
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MapDataCreator), typeof(MapSaver))]
public class MapCreatorPanel : MonoBehaviour
{
    private bool _isCanCreate = true;
    private Vector2Int _size;
    private string _name = null;

    public event Action CreateStarted;
    public event Action CreateCompleted;
    public event Action CreateError;

    public void ChageX(string xString)
    {
        int x = ToInt(xString);
        _size.x = x;
    }

    public void ChageY(string yString)
    {
        int y = ToInt(yString);
        _size.y = y;
    }

    public void ChageName(string name)
    {
        _name = name;
    }

    private int ToInt(string valueString)
    {
        int value = Convert.ToInt32(valueString);
        if (value < 0)
            value *= -1;
        return value;
    }

    public void Create()
    {
        try
        {
            if (_isCanCreate)
            {
                if (CheckCreate())
                {
                    CreateStarted?.Invoke();
                    _isCanCreate = false;
                    MapData mapData = GetComponent<MapDataCreator>().Create(_size, _name);
                    GetComponent<MapSaver>().Save(mapData);
                    _isCanCreate = true;
                    CreateCompleted?.Invoke();
                }
                else
                {
                    CreateError?.Invoke();
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            CreateError?.Invoke();
        }
    }

    private bool CheckCreate()
    {
        if (_size.x <= 0 || _size.y <= 0)
            return false;
        if (_name == null || _name == "")
            return false;

        return true;
    }
}
