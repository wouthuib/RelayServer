using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RelayServer.Database.Monsters
{
    public sealed class MonsterStore
    {
        public List<Monster> monster_list { get; set; }

        private static MonsterStore instance;
        private MonsterStore()
        {
            monster_list = new List<Monster>();
        }

        public static MonsterStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MonsterStore();
                }
                return instance;
            }
        }

        public void addMonster(Monster addMonster)
        {
            monster_list.Add(addMonster);
        }

        public void removeMonster(string name)
        {
            monster_list.Remove(new Monster() { monsterName = name });
        }

        public Monster getMonster(int ID)
        {
            return this.monster_list.Find(delegate(Monster mob) { return mob.monsterID == ID; });
        }

        public void loadMonster(string dir, string file)
        {
            using (var reader = new StreamReader(Path.Combine(dir, file)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    try
                    {
                        string[] name_vals = values[1].Split('"');

                        this.addMonster(Monster.create(Convert.ToInt32(values[0]), name_vals[0], values[2]));

                        // Link monster to monster database
                        Monster monster = this.getMonster(Convert.ToInt32(values[0]));

                        // Trim and fix sprite path
                        monster.monsterSprite = @"" + Regex.Replace(values[2], "\"", "");
                        monster.monsterSprite = Regex.Replace(monster.monsterSprite, " ", "");

                        // Monster battle inforation
                        monster.Level = Convert.ToInt32(values[3]);
                        monster.EXP = Convert.ToInt32(values[4]);
                        monster.HP = Convert.ToInt32(values[5]);
                        monster.Hit = Convert.ToInt32(values[6]);
                        monster.Flee = Convert.ToInt32(values[7]);
                        monster.DEF = Convert.ToInt32(values[8]);
                        monster.ATK = Convert.ToInt32(values[9]);
                        monster.Magic = Convert.ToInt32(values[10]);
                        monster.Speed = Convert.ToInt32(values[11]);
                        monster.Size = Regex.Replace(values[12].ToString(), " ", "");
                        monster.Mode = Regex.Replace(values[13].ToString(), " ", "");

                        // Monster Drops
                        monster.drop01Item = Convert.ToInt32(values[14]);
                        monster.drop01Chance = Convert.ToInt32(values[15]);
                        monster.drop02Item = Convert.ToInt32(values[16]);
                        monster.drop02Chance = Convert.ToInt32(values[17]);
                        monster.drop03Item = Convert.ToInt32(values[18]);
                        monster.drop03Chance = Convert.ToInt32(values[19]);
                        monster.drop04Item = Convert.ToInt32(values[20]);
                        monster.drop04Chance = Convert.ToInt32(values[21]);
                        monster.drop05Item = Convert.ToInt32(values[22]);
                        monster.drop05Chance = Convert.ToInt32(values[23]);

                    }
                    catch (Exception ee)
                    {
                        string exception = ee.ToString();
                    }
                }
            }
        }
    }
}
