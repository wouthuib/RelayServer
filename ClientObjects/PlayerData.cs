using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace RelayServer.ClientObjects
{
    [Serializable]
    public class playerData : ISerializable
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public long AccountID { get; set; }
        public long CharacterID { get; set; }
        public string Action { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public string spritename { get; set; }
        public string spritestate { get; set; }
        public string direction { get; set; }
        public int prevspriteframe { get; set; }
        public int maxspriteframe { get; set; }
        public string attackSprite { get; set; }
        public string spriteEffect { get; set; }
        public string mapName { get; set; }
        public string skincol { get; set; }
        public string facespr { get; set; }
        public string hairspr { get; set; }
        public string hailcol { get; set; }
        public string armor { get; set; }
        public string headgear { get; set; }
        public string weapon { get; set; }

        public playerData() { }

        protected playerData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            Name = (string)info.GetValue("Name", typeof(string));
            IP = (string)info.GetValue("IP", typeof(string));
            AccountID = (long)info.GetValue("AccountID", typeof(long));
            CharacterID = (long)info.GetValue("CharacterID", typeof(long));
            Action = (string)info.GetValue("Action", typeof(string));
            PositionX = (int)info.GetValue("PositionX", typeof(int));
            PositionX = (int)info.GetValue("PositionY", typeof(int));
            spritename = (string)info.GetValue("spritename", typeof(string));
            spritestate = (string)info.GetValue("spritestate", typeof(string));
            direction = (string)info.GetValue("direction", typeof(string));
            prevspriteframe = (int)info.GetValue("prevspriteframe", typeof(int));
            maxspriteframe = (int)info.GetValue("maxspriteframe", typeof(int));
            attackSprite = (string)info.GetValue("attackSprite", typeof(string));
            spriteEffect = (string)info.GetValue("spriteEffect", typeof(string));
            mapName = (string)info.GetValue("mapName", typeof(string));
            skincol = (string)info.GetValue("skincol", typeof(string));
            facespr = (string)info.GetValue("facespr", typeof(string));
            hairspr = (string)info.GetValue("hairspr", typeof(string));
            hailcol = (string)info.GetValue("hailcol", typeof(string));
            armor = (string)info.GetValue("armor", typeof(string));
            headgear = (string)info.GetValue("headgear", typeof(string));
            weapon = (string)info.GetValue("weapon", typeof(string));

        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("Name", Name);
            info.AddValue("IP", IP);
            info.AddValue("AccountID", AccountID);
            info.AddValue("CharacterID", CharacterID);
            info.AddValue("Action", Action);
            info.AddValue("PositionX", PositionX);
            info.AddValue("PositionY", PositionY);
            info.AddValue("spritename", spritename);
            info.AddValue("spritestate", spritestate);
            info.AddValue("direction", direction);
            info.AddValue("prevspriteframe", prevspriteframe);
            info.AddValue("maxspriteframe", maxspriteframe);
            info.AddValue("attackSprite", attackSprite);
            info.AddValue("spriteEffect", spriteEffect);
            info.AddValue("mapName", mapName);
            info.AddValue("skincol", skincol);
            info.AddValue("facespr", facespr);
            info.AddValue("hairspr", hairspr);
            info.AddValue("hailcol", hailcol);
            info.AddValue("armor", armor);
            info.AddValue("headgear", headgear);
            info.AddValue("weapon", weapon);
        }
    }
}
