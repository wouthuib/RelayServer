using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class ScreenData : ISerializable
    {
        public string MainScreenName { get; set; }
        public string MainScreenPhase { get; set; }
        public string MainScreenMenu { get; set; }
        public string SubScreenName { get; set; }
        public string SubScreenPhase { get; set; }
        public string SubScreenMenu { get; set; }

        public ScreenData() { }

        protected ScreenData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            MainScreenName = (string)info.GetValue("MainScreenName", typeof(string));
            MainScreenPhase = (string)info.GetValue("MainScreenPhase", typeof(string));
            MainScreenMenu = (string)info.GetValue("MainScreenMenu", typeof(string));
            SubScreenName = (string)info.GetValue("SubScreenName", typeof(string));
            SubScreenPhase = (string)info.GetValue("SubScreenPhase", typeof(string));
            SubScreenMenu = (string)info.GetValue("SubScreenMenu", typeof(string));
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("MainScreenName", MainScreenName);
            info.AddValue("MainScreenPhase", MainScreenPhase);
            info.AddValue("MainScreenMenu", MainScreenMenu);
            info.AddValue("SubScreenName", SubScreenName);
            info.AddValue("SubScreenPhase", SubScreenPhase);
            info.AddValue("SubScreenMenu", SubScreenMenu);
        }
    }
}
