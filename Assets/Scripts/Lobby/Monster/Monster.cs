namespace MonsterSpace
{
    public class Monster : IMonster
    {    
        public string PlayerId { get; }
        public int ActiveOnMap { get; private set; }

        public string Id { get; }
        public string Name { get; }
        public string ModelId { get; }
        public string RaceId { get; }

        public int SoulTotal { get; }
        public int SoulBalance { get; private set; }

        public int Power { get; private set; }
        public int Defense { get; private set; }
        public float Speed { get; private set; }
        public int StaminaTotal { get; }
        public int StaminaBalance { get; private set; }

        public string Abilities { get; }
        public string Affected { get; }

        public Monster(MonsterInfo info)
        {
            Name = info.monster_name;
            PlayerId = info.player_id;
            Id = info.monster_id;
            ModelId = info.model_id;
            RaceId = info.race_id;

            ActiveOnMap = info.activeOnMap;

            SoulTotal = info.soul_total;
            SoulBalance = info.soul_balance;
            Power = info.power;
            Defense = info.defense;
            Speed = info.speed;
            StaminaBalance = info.stamina_balance;

            StaminaTotal = info.stamina_total;
            Abilities = info.abilities;
        }

        public void PlaceOnMap()
        {
            ActiveOnMap = 1;
        }
    }

    public interface IMonster
    {
        public string Name { get; }
        public string PlayerId { get; }
        public string Id { get; }
        public string ModelId { get; }
        public string RaceId { get; }

        public abstract int ActiveOnMap { get; }

        public int SoulTotal { get; }
        public abstract int SoulBalance { get; }
        public abstract int Power { get; }
        public abstract int Defense { get; }
        public abstract float Speed { get; }
        public abstract int StaminaBalance { get; }
        public abstract int StaminaTotal { get; }
        public string Abilities { get; }
        public string Affected { get; }
    }
}
