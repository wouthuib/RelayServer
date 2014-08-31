using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class AccountData : ISerializable
    {
        public long AccountID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IpAddress { get; set; }
        public string Connected { get; set; }

        public AccountData() { }

        protected AccountData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            AccountID = (long)info.GetValue("AccountID", typeof(long));
            Username = (string)info.GetValue("Username", typeof(string));
            Password = (string)info.GetValue("Password", typeof(string));
            IpAddress = (string)info.GetValue("IpAddress", typeof(string));
            Connected = (string)info.GetValue("Connected", typeof(string));
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
            info.AddValue("Connected", Connected);
        }
    }
}
