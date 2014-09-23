using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class MonsterData : ISerializable
    {
        public int MonsterID { get; set; }
        public string InstanceID { get; set; }
        public string MapName { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public int BorderMin { get; set; }
        public int BorderMax { get; set; }
        public string spritestate { get; set; }
        public string direction { get; set; }
        public string spriteEffect { get; set; }

        public MonsterData() { }

        protected MonsterData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            MonsterID = (int)info.GetValue("MonsterID", typeof(int));
            InstanceID = (string)info.GetValue("InstanceID", typeof(string));
            MapName = (string)info.GetValue("MapName", typeof(string));
            PositionX = (float)info.GetValue("PositionX", typeof(float));
            PositionY = (float)info.GetValue("PositionY", typeof(float));
            BorderMin = (int)info.GetValue("BorderMin", typeof(int));
            BorderMax = (int)info.GetValue("BorderMax", typeof(int));
            spritestate = (string)info.GetValue("spritestate", typeof(string));
            direction = (string)info.GetValue("direction", typeof(string));
            spriteEffect = (string)info.GetValue("spriteEffect", typeof(string));
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("MonsterID", MonsterID);
            info.AddValue("InstanceID", InstanceID);
            info.AddValue("MapName", MapName);
            info.AddValue("PositionX", PositionX);
            info.AddValue("PositionY", PositionY);
            info.AddValue("BorderMin", BorderMin);
            info.AddValue("BorderMax", BorderMax);
            info.AddValue("spritestate", spritestate);
            info.AddValue("direction", direction);
            info.AddValue("spriteEffect", spriteEffect);
        }
    }
}
