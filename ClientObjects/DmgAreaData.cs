using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class DmgAreaData : ISerializable
    {
        public string Owner { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int AreaWidth { get; set; }
        public int AreaHeigth { get; set; }
        public string Permanent { get; set; }
        public int MobHitCount { get; set; }
        public float Timer { get; set; }
        public int DmgPercent { get; set; }

        public DmgAreaData() { }

        protected DmgAreaData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            Owner = (string)info.GetValue("Owner", typeof(string));
            PositionX = (int)info.GetValue("PositionX", typeof(int));
            PositionX = (int)info.GetValue("PositionY", typeof(int));
            AreaWidth = (int)info.GetValue("AreaWidth", typeof(int));
            AreaHeigth = (int)info.GetValue("AreaHeigth", typeof(int));
            Permanent = (string)info.GetValue("Permanent", typeof(string));
            MobHitCount = (int)info.GetValue("MobHitCount", typeof(int));
            Timer = (float)info.GetValue("Timer", typeof(float));
            DmgPercent = (int)info.GetValue("DmgPercent", typeof(int));
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("Owner", Owner);
            info.AddValue("PositionX", PositionX);
            info.AddValue("PositionY", PositionY);
            info.AddValue("AreaWidth", AreaWidth);
            info.AddValue("AreaHeigth", AreaHeigth);
            info.AddValue("Permanent", Permanent);
            info.AddValue("MobHitCount", MobHitCount);
            info.AddValue("Timer", Timer);
            info.AddValue("DmgPercent", DmgPercent);
        }
    }
}
