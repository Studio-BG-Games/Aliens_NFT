using MonsterSpace;
using System.Collections.Generic;

public class Player
{
    public string Id { get; private set; }
    public int SoulEnergy { get; private set; }
    public string Race { get; private set; }

    public IReadOnlyList<Monster> Monsters { get; }

    public Player(PlayerInfo info, IReadOnlyList<Monster> monsters)
    {
        Id = info.player_id;
        SoulEnergy = info.soul_energy;
        Race = info.race;

        Monsters = monsters;
    }
}
