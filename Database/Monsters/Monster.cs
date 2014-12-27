using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RelayServer.Database.Monsters
{
    public class Monster
    {
        public int monsterID { get; set; }
        public string monsterName { get; set; }
        public string monsterSprite { get; set; }

        public int Level { get; set; }
        public int EXP { get; set; }
        public int HP { get; set; }
        public int Hit { get; set; }
        public int Flee { get; set; }
        public int DEF { get; set; }
        public int ATK { get; set; }
        public int Magic { get; set; }
        public int Speed { get; set; }
        public string Size { get; set; }
        public string Mode { get; set; }

        public int drop01Item { get; set; }
        public int drop01Chance { get; set; }
        public int drop02Item { get; set; }
        public int drop02Chance { get; set; }
        public int drop03Item { get; set; }
        public int drop03Chance { get; set; }
        public int drop04Item { get; set; }
        public int drop04Chance { get; set; }
        public int drop05Item { get; set; }
        public int drop05Chance { get; set; }

        public Vector2 sizeMod
        {
            get
            {
                switch (this.Size)
                {
                    case "small":
                        return new Vector2(5, 15);
                    case "medium":
                        return Vector2.Zero;
                    case "big":
                        return new Vector2(-5, -15);
                    case "huge":
                        return new Vector2(-10, -30);
                    default:
                        return Vector2.Zero;
                }
            }
        }

        public static Monster create(int identifier, string name, string sprite)
        {
            var results = new Monster();

            results.monsterID = identifier;
            results.monsterName = name;
            results.monsterSprite = sprite;

            return results;
        }
    }
}
