using System;

[Serializable]
public struct PostMoveData
{
    public PostMoveStruct data;
}

[Serializable]
public struct PostMoveStruct
{/*
    public string _id;
    public int cell_id;
    public int x;
    public int y;
    public int ground;
    public string object_id;
    public string __v;*/
    public string monster_id;
    public string player_id;
}
