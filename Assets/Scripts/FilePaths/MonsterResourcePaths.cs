using UnityEngine;

public class MonsterResourcePaths : MonoBehaviour
{    
    public static class Model
    {
        private const string Folder = "Monster/Model/";

        public static string GetPaths(string race, string monsterId)
        {
            switch (race)
            {
                case "Volcanoids":
                    return Volcanoids.GetPaths(monsterId);
                default:
                    return Folder + "Volcanoids/Vine-crab";
                    //throw new System.InvalidOperationException("No suitable land type.");
            }
        }

        public static class Volcanoids
        {
            public const string Icon = Path + "VolcanoidsIcon";

            private const string Path = Folder + "Volcanoids/";

            public const string GreenAlien1 = Path + "Purple-alien-warrior/Prefab/purple-alien-warrior";
            public const string GreenAlien2 = Path + "Vine-crab"; 
            public const string GreenAlien3 = Path + "Purple-alien-warrior/Prefab/purple-alien-warrior";

            public static string GetPaths(string monsterId)
            {
                switch (monsterId)
                {
                    case "GREEN_ALIEN_1":
                        return GreenAlien1;
                    case "GREEN_ALIEN_2":
                        return GreenAlien2;
                    case "GREEN_ALIEN_3":
                        return GreenAlien3;
                    default:
                        throw new System.InvalidOperationException("No suitable land type.");
                }
            }
        }        
    }
}
