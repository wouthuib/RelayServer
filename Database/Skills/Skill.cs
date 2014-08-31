using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayServer.Database.Skills
{
    public enum SkillType
    {
        Active, Passive,
    };

    public enum SkillClass
    {
        Warrior, Magician, Bowman, Thief, Pirate, All
    };

    [Serializable]
    public class Skill
    {
        // General properties
        public int ID { get; set; }
        public string Name { get; set; }

        // Casting properties
        public int MagicCost { get; set; }
        public float CastTime { get; set; }
        public float CooldownTime { get; set; }

        // Skill unlocks other skills in skill tree
        public int Level { get; set; }
        public int MaxLevel { get; set; }
        public string[] UnlockSkill { get; set; }
        public int[] UnlockLevel { get; set; }

        public SkillType Type { get; set; }
        public SkillClass Class { get; set; }

        public static Skill create(int identifier, string name, SkillType type)
        {
            var skill = new Skill();

            skill.ID = identifier;
            skill.Name = name;
            skill.Type = type;
            skill.UnlockSkill = new string[4];
            skill.UnlockLevel = new int[4];
            skill.Level = 0;

            return skill;
        }
    }
}
