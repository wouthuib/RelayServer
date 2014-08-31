using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class EffectData : ISerializable
    {
        public string Path { get; set; }
        public int FrameCount { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string PlayerLockName { get; set; }
        public string InstanceLockName { get; set; }

        public EffectData() { }

        protected EffectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            Path = (string)info.GetValue("Path", typeof(string));
            FrameCount = (int)info.GetValue("FrameCount", typeof(int));
            OffsetX = (int)info.GetValue("OffsetX", typeof(int));
            OffsetY = (int)info.GetValue("OffsetY", typeof(int));
            PositionX = (int)info.GetValue("PositionX", typeof(int));
            PositionX = (int)info.GetValue("PositionY", typeof(int));
            PlayerLockName = (string)info.GetValue("PlayerLockName", typeof(string));
            InstanceLockName = (string)info.GetValue("InstanceLockName", typeof(string));
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("Path", Path);
            info.AddValue("FrameCount", FrameCount);
            info.AddValue("OffsetX", OffsetX);
            info.AddValue("OffsetY", OffsetY);
            info.AddValue("PositionX", PositionX);
            info.AddValue("PositionY", PositionY);
            info.AddValue("PlayerLockName", PlayerLockName);
            info.AddValue("InstanceLockName", InstanceLockName);
        }
    }
}
