using MapCraete;

namespace MapSpase.Environment
{
    public class EnvironmentObject
    {
        public string Id { get; }
        public string ModelId { get; }
        public int Type { get; }
        public int Effect { get; }
        public int Value { get; }
        public int Rotation { get; }

        public EnvironmentObject(EnvironmentObjectInfo info)
        {
            Id = info.object_id;
            ModelId = info.model_id;
            Type = info.type;
            Effect = info.effect;
            Value = info.value;
            Rotation = info.rotation;
        }
    }
}
