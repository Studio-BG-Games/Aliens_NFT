using System;
using System.Collections.Generic;

[Serializable]
public struct PlayerInfoData
{
    public Data data;

    [Serializable]
    public struct Data
    {
        public PlayerInfo player;
        public List<MonsterInfo> monsters;
    }
}

[Serializable]
public struct PlayerInfo
{
    public string player_id;
    public int soul_energy;
    public string race;
}