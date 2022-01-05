using MapSpase;
using System;

namespace MapCraete
{
    [Serializable]
    public struct MapData
    {
        public CellStruct[] data;
        public string planet_id;
        public string planet_name;
        public int region_id;
        public MapData(int count, string planetId, string planetName)
        {
            data = new CellStruct[count];
            planet_id = planetId;
            planet_name = planetName;
            region_id = 1;
        }
    }

    [Serializable]
    public struct CellStruct
    {
        public int cell_id;

        public string owner_id;
        public string owner_name;

        public int x;
        public int y;
        public int ground;
        public int ground_subtype;

        public float location_height;

        public string object_id;

        public string monster_id;
        public string player_id;
        public MonsterInfo monster_attributes;

        public EnvironmentObjectInfo object_attributes;

        public CellStruct (ICell cell)
        {
            cell_id = cell.Id;
            x = cell.Position.x;
            y = cell.Position.y;

            owner_id = cell.OwnerId;
            owner_name = cell.OwnerName;

            location_height = cell.LocationHeight;

            ground = (int)cell.CurrentGroundType;
            ground_subtype = cell.GroundSubtype;

            if (cell.EnvironmentObject != null)
            {
                object_id = cell.EnvironmentObject.Id;
                object_attributes = new EnvironmentObjectInfo(cell.EnvironmentObject);
            }
            else
            {
                object_id = null; 
                object_attributes = null;
            }

            if (cell.Monster != null)
            {
                monster_id = cell.Monster.Id;
                monster_attributes = new MonsterInfo(cell.Monster);
            }
            else
            {
                monster_id = null;
                monster_attributes = null;
            }

            player_id = cell.PlayerId;
        }
    }
}