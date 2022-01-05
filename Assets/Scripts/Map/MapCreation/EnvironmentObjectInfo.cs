using MapSpase.Environment;

namespace MapCraete
{
    [System.Serializable]
    public class EnvironmentObjectInfo
    {
        public string object_id;
        public int rotation;
        public string model_id;
        public int type;
        public int effect;
        public int value;

        public EnvironmentObjectInfo(string id)
        {
            object_id = id;
            model_id = id;
        }

        public EnvironmentObjectInfo(EnvironmentObject environmentObject)
        {
            object_id = environmentObject.Id;
            rotation = environmentObject.Rotation;
            model_id = environmentObject.ModelId;
            type = environmentObject.Type;
            effect = environmentObject.Effect;
            value = environmentObject.Value;
        }
    }
}
