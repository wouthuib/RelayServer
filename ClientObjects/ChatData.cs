using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class ChatData : ISerializable
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }

        public ChatData() { }

        protected ChatData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            Name = (string)info.GetValue("Name", typeof(string));
            Text = (string)info.GetValue("Text", typeof(string));
            PositionX = (int)info.GetValue("PositionX", typeof(int));
            PositionX = (int)info.GetValue("PositionY", typeof(int));
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("Name", Name);
            info.AddValue("Text", Text);
            info.AddValue("PositionX", PositionX);
            info.AddValue("PositionY", PositionY);
        }
    }
}
