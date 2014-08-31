using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RelayServer.Database.Skills
{
    public sealed class SkillStore
    {
        public List<Skill> skill_list { get; set; }

        private static SkillStore instance;
        private SkillStore()
        {
            skill_list = new List<Skill>();
        }

        public static SkillStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SkillStore();
                }
                return instance;
            }
        }

        public void addSkill(Skill addSkill)
        {
            skill_list.Add(addSkill);
        }

        public void removeSkill(string name)
        {
            skill_list.Remove(new Skill() { Name = name });

        }

        public Skill getSkill(int ID)
        {
            return this.skill_list.Find(delegate(Skill skill) { return skill.ID == ID; });
        }

        public Skill getSkill(string Name)
        {
            return this.skill_list.Find(delegate(Skill skill) { return skill.Name == Name; });
        }

        public bool hasSkillRequirement(int ID)
        {
            for (int i = 0; i < 4; i++)
                if (skill_list.Find(delegate(Skill skill) { return skill.ID == ID; }).UnlockSkill[i] != null)
                    return true;
            // no requirements found
            return false;
        }

        public void loadSkills(string dir, string file)
        {
            using (var reader = new StreamReader(Path.Combine(dir, file)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');

                    try
                    {
                        if (values[0] != "[0] - ID")
                        {
                            // create new skill in skilllist
                            this.addSkill(Skill.create(Convert.ToInt32(values[0]), TrimParameter(values[1]), (SkillType)Enum.Parse(typeof(SkillType), values[14])));

                            // Link new Skill object to Skill database
                            Skill skill = this.getSkill(Convert.ToInt32(values[0]));

                            // magic casting properties
                            skill.MagicCost = Convert.ToInt32(values[2]);
                            skill.CastTime = Convert.ToInt32(values[3]) * 0.01f;
                            skill.CooldownTime = Convert.ToInt32(values[4]) * 0.01f;

                            // Skill Level properties
                            skill.MaxLevel = Convert.ToInt32(values[5]);

                            // Skill Requirement properties
                            for (int i = 0; i < 4; i++)
                            {
                                if (TrimParameter(values[6 + (i * 2)]) != "")
                                {
                                    skill.UnlockSkill[i] = TrimParameter(values[6 + (i * 2)]);
                                    skill.UnlockLevel[i] = Convert.ToInt32(values[7 + (i * 2)]);
                                }
                            }

                            skill.Class = (SkillClass)Enum.Parse(typeof(SkillClass), values[15]);

                        }
                    }
                    catch
                    {
                        throw new Exception("Error loading skilltable, please check the entries!");
                    }
                }
            }
        }

        public string StringtoPath(string path)
        {
            path = Regex.Replace(path, "\"", "");
            path = Regex.Replace(path, " ", "");
            path = Regex.Replace(path, @"\t|\n|\r", "");

            return @"" + path;
        }

        public string TrimParameter(string param)
        {
            param = Regex.Replace(param, "0", "");
            param = Regex.Replace(param, "\"", "");
            param = Regex.Replace(param, @"\t|\n|\r", "");

            return param;
        }

    }
}
