using MapCraete;
using MapSpase.Environment;
using MonsterSpace;
using System;
using UnityEngine;

namespace MapSpase
{
    public class Cell : ICell
    {
        public int Id { get; }
        public string PlayerId { get; }
        public Vector2Int Position { get; }
        public GroundType CurrentGroundType { get; }
        public IMonster Monster { get; private set; }
        public EnvironmentObject EnvironmentObject { get; }
        public int GroundSubtype { get; }

        public string OwnerId { get; }
        public string OwnerName { get; }
        public float LocationHeight { get; private set; }

        public Cell(CellStruct cellStruct, Monster monster, EnvironmentObject environmentObject)
        {
            Id = cellStruct.cell_id;
            Position = new Vector2Int(cellStruct.x, cellStruct.y);
            PlayerId = cellStruct.player_id;
            EnvironmentObject = environmentObject;
            Monster = monster;
            GroundSubtype = cellStruct.ground_subtype;

            LocationHeight = cellStruct.location_height;

            OwnerId = cellStruct.owner_id;
            OwnerName = cellStruct.owner_name;

            if (CheckPresenceValueInEnum(cellStruct.ground))
                CurrentGroundType = (GroundType)cellStruct.ground;
            else
                throw new InvalidOperationException("No suitable land type");
        }

        private bool CheckPresenceValueInEnum(int value)
        {
            foreach (int item in Enum.GetValues(typeof(GroundType)))
                if (item == value)
                    return true;

            return false;
        }

        public void ChangeLocationHeight(float height)
        {
            LocationHeight = height;
        }

        public bool TrySetMonster(IMonster monster)
        {
            if (Monster == null || Monster.Id == monster.Id)
            {
                Monster = monster;
                return true;
            }

            return false;
        }

        public bool TryRemoveMonster()
        {
            if (Monster != null)
            {
                Monster = null;
                return true;
            }

            return false;
        }
    }

    public enum GroundType
    {
        Void = 0,
        Water = 1,
        LightGrass = 2,
        DarkGrass = 3,
        Swamp = 4,
        Snow = 5,
        Volcanic = 6,
        Brown = 7,
        Sand = 8
    }

    public interface ICell
    {
        public int Id { get; }

        public string OwnerId { get; }
        public string OwnerName { get; }
        public string PlayerId { get; }
        public float LocationHeight { get; }
        public Vector2Int Position { get; }
        public GroundType CurrentGroundType { get; }
        public int GroundSubtype { get; }
        public abstract IMonster Monster { get; }
        public EnvironmentObject EnvironmentObject { get; }
    }
}
