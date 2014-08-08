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

        public void addMonster(Monster addItem)
        {
            monster_list.Add(addItem);
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

                        this.addMonster(Monster.create(Convert.ToInt32(values[0]), name_vals[1], values[2]));

                        // Link monster to monster database
                        Monster monster = this.getMonster(Convert.ToInt32(values[0]));

                        // Trim and fix sprite path
                        monster.monsterSprite = @"" + Regex.Replace(values[2], "\"", "");
                        //monster.monsterSprite = Regex.Replace(monster.monsterSprite, " ", "");

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

                        // Monster Drops
                        monster.drop01Item = Convert.ToInt32(values[13]);
                        monster.drop01Chance = Convert.ToInt32(values[14]);
                        monster.drop02Item = Convert.ToInt32(values[15]);
                        monster.drop02Chance = Convert.ToInt32(values[16]);
                        monster.drop03Item = Convert.ToInt32(values[17]);
                        monster.drop03Chance = Convert.ToInt32(values[18]);
                        monster.drop04Item = Convert.ToInt32(values[19]);
                        monster.drop04Chance = Convert.ToInt32(values[20]);
                        monster.drop05Item = Convert.ToInt32(values[21]);
                        monster.drop05Chance = Convert.ToInt32(values[22]);

                    }
                    catch (Exception ee)
                    {
                        string exception = ee.ToString();
                    }
                }
            }
        }

        public void saveItem(string dir, string file)
        {
            Type itemType = typeof(Monster);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            using (var writer = new StreamWriter(Path.Combine(dir, file)))
            {
                // writer.WriteLine(string.Join("; ", props.Select(p => p.Name)));

                foreach (var monster in monster_list)
                {
                    foreach (PropertyInfo propertyInfo in monster.GetType().GetProperties())
                    {
                        if (propertyInfo.Name != "itemID")
                            writer.Write("; ");

                        var getvalue = propertyInfo.GetValue(monster, null);

                        writer.Write(getvalue.ToString());
                    }

                    writer.WriteLine(";");
                }
            }
        }
    }
}
