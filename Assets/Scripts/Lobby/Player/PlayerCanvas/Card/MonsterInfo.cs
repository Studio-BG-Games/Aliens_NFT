using MonsterSpace;
using System;

[Serializable]
public class MonsterInfo
{
    public string player_id;
    public int activeOnMap;
    public string monster_id;
    public string monster_name;
    public string model_id;
    public string race_id;
    public int soul_total;
    public int soul_balance;
    public int power;
    public int defense;
    public float speed;
    public int stamina_total;
    public int stamina_balance;
    public string abilities;
    public string affected;

    public MonsterInfo(IMonster monster)
    {        
        player_id = monster.PlayerId;
        activeOnMap = monster.ActiveOnMap;
        monster_id = monster.Id;
        monster_name = monster.Name;
        model_id = monster.ModelId;
        race_id = monster.RaceId;
        soul_total = monster.SoulTotal;
        soul_balance = monster.SoulBalance;
        power = monster.Power;
        defense = monster.Defense;
        speed = monster.Speed;
        stamina_total = monster.StaminaTotal;
        stamina_balance = monster.StaminaBalance;
        abilities = monster.Abilities;
        affected = monster.Affected;
    }
}
