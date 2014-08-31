using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class CharacterData : ISerializable
    {
        public long AccountID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IpAddress { get; set; }

        public CharacterData() { }

        protected CharacterData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            AccountID = (long)info.GetValue("AccountID", typeof(long));
            Username = (string)info.GetValue("Username", typeof(string));
            Password = (string)info.GetValue("Password", typeof(string));
            IpAddress = (string)info.GetValue("IpAddress", typeof(string));
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("AccountID", AccountID);
            info.AddValue("Username", Username);
            info.AddValue("Password", Password);
            info.AddValue("IpAddress", IpAddress);
        }
    }
}
