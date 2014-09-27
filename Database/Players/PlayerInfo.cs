using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RelayServer.Database.Items;

namespace RelayServer.Database.Players
{
    public enum PlayerBattleInfo
    {
        ATK,
        DEF,
        MATK,
        MDEF,
        ASPD,
        HIT,
        FLEE,
        MAXHP,
        MAXSP,
    }

    public enum PlayerStats
    {
        Strength,
        Agility,
        Vitality,
        Intelligence,
        Dexterity,
        Luck
    }

    [Serializable]
    public class PlayerInfo
    {
        public long AccountID;              // Account
        public long CharacterID;            // Character
        public bool Online;                 // Character

        public Equipment equipment;         // Equipment
        public Inventory inventory;         // Inventory
        public QuickSlotBar quickslotbar;   // QuickSlotBar
        public SkillTree skilltree;         // Skillbar

        #region texture properties

        public string head_sprite;
        public string body_sprite;
        public string hands_sprite;
        public string hair_sprite;
        public string faceset_sprite;

        public Vector2[] spriteOfset = new Vector2[10]; // refer to playersprite -> spritepath
        public List<spriteOffset> list_offsets = new List<spriteOffset>();

        public Color hair_color = Color.Red;
        public Color skin_color = new Color(255, 206, 180);

        public string headgear_sprite
        {
            get
            {
                string equip = null;

                if (equipment.item_list.FindAll(delegate(Item item) { return item.Slot == ItemSlot.Headgear; }).Count > 0)
                    equip = equipment.item_list.Find(delegate(Item item) { return item.Slot == ItemSlot.Headgear; }).equipSpritePath;

                if (equip != null)
                    return equip;
                else
                    return null;
            }
        }
        public string costume_sprite
        {
            get
            {
                string equip = null;

                if (equipment.item_list.FindAll(delegate(Item item) { return item.Slot == ItemSlot.Bodygear; }).Count > 0)
                    equip = equipment.item_list.Find(delegate(Item item) { return item.Slot == ItemSlot.Bodygear; }).equipSpritePath;

                if (equip != null)
                    return equip;
                else
                    return null;
            }
        }
        public string weapon_sprite
        {
            get
            {
                string equip = null;

                if (equipment.item_list.FindAll(delegate(Item item) { return item.Slot == ItemSlot.Weapon; }).Count > 0)
                    equip = equipment.item_list.Find(delegate(Item item) { return item.Slot == ItemSlot.Weapon; }).equipSpritePath;

                if (equip != null)
                    return equip;
                else
                    return null;
            }
        }

        public Vector2 Position;

        #endregion

        #region properties
        private string name, gender, jobclass;
        private int maxhp, hp, maxsp, sp, exp, nlexp, lvl, gold;
        private int str, agi, vit, intel, dex, luk;
        #endregion

        #region general info
        // General Info
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public string Gender
        {
            get { return this.gender; }
            set { this.gender = value; }
        }
        public string Jobclass
        {
            get { return this.jobclass; }
            set { this.jobclass = value; }
        }
        public int Exp
        {
            get { return this.exp; }
            set { this.exp = value; }
        }
        public int NextLevelExp
        {
            get { return this.nlexp; }
            set { this.nlexp = value; }
        }
        public int Level
        {
            get { return this.lvl; }
            set { this.lvl = value; }
        }
        public int Gold
        {
            get { return this.gold; }
            set { this.gold = value; }
        }
        #endregion

        #region battleinfo
        // Battle Info
        // for more info http://irowiki.org/wiki/ATK#Status_ATK
        public int ATK
        {
            get { return (int)(StatusATK * 2 + WeaponATK + EquipATK + MasteryATK) + b_atk; }
        }

        public int MATK
        {
            get { return (int)(intel + (intel / 2) + (dex / 5) + (luk / 3) + (lvl / 4)) + b_matk; }
        }

        public int DEF
        {
            get { return (int)(HardDef + SoftDef) + b_def; }
        }

        // for equipment screen
        public int BattleDEF
        {
            get { return (int)((4000 + HardDef) / (4000 + HardDef * 10)); }
        }

        // for Battle calculation
        public int SoftDef
        {
            get { return (int)((lvl / 2) + (vit / 2) + (agi / 2)); }
        }

        // for Battle calculation
        public int HardDef
        {
            get
            {
                int defmod = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.Type == ItemType.Armor; }))
                {
                    defmod += item.DEF;
                }
                return defmod;
            }
        }
        public float DamageReduced
        {
            get
            {
                return (1 - (600 / (HardDef + RefineDef + 600))) * 100;
            }
        }
        public int RefineDef
        {
            get
            {
                int refmod = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.Type == ItemType.Armor; }))
                {
                    refmod += item.RefinementLevel;
                }
                return refmod;
            }
        }
        public int MDEF
        {
            get { return (int)(intel + vit / 5 + dex / 5 + lvl / 4) + b_mdef; }
        }
        public int HIT
        {
            get { return (int)(this.Level + this.dex + 175) + b_hit; }
        }
        public int FLEE
        {
            get { return (int)((this.Level + this.agi + 100) / 5) + b_flee; }
        }
        public int HP
        {
            get { return this.hp; }
            set { this.hp = value; }
        }
        public int MAXHP
        {
            get { return this.maxhp; }
            set { this.maxhp = value; }
        }
        public int SP
        {
            get { return this.sp; }
            set { this.sp = value; }
        }
        public int MAXSP
        {
            get { return this.maxsp; }
            set { this.maxsp = value; }
        }
        public int StatusATK
        {
            get { return (int)(str + (dex / 5) + (luk / 3) + (lvl / 4)); }
        }
        public int WeaponATK
        {
            get { return (int)((BaseWeaponATK + Variance + STRBonus + RefinementBonus) * SizePenalty / 100); }
        }
        public int SizePenalty
        {
            get { return 100; } // to do monster size table
        }
        public int EquipATK
        {
            get { return 0; } // to do (buffs + cards etc)
        }
        public int MasteryATK
        {
            get { return 0; } // to do
        }
        public int Variance
        {
            get
            {
                int WeaponLevel = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.Type == ItemType.Weapon; }))
                {
                    WeaponLevel += item.WeaponLevel;
                }
                return (int)(0.05f * WeaponLevel * BaseWeaponATK);
            }
        }
        public int BaseWeaponATK
        {
            get
            {
                int atkmod = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.Type == ItemType.Weapon; }))
                {
                    atkmod += item.ATK;
                }
                return atkmod;
            }
        }
        public int RefinementBonus
        {
            get
            {
                int atkmod = 0;
                foreach (Item item in equipment.item_list.FindAll(delegate(Item item) { return item.Type == ItemType.Weapon; }))
                {
                    atkmod += item.RefinementLevel;
                }
                return atkmod;
            }
        }
        public int STRBonus
        {
            get { return (int)((200 * str) / 200); }
        }
        public int ASPD
        {
            get { return (int)((Math.Sqrt(Math.Pow(agi, 2) / 2) + Math.Sqrt(Math.Pow(dex, 2) / 5)) / 4) + b_aspd; }
        }
        #endregion

        #region status updates (buffs)

        public int b_def { get; set; }
        public int b_atk { get; set; }
        public int b_matk { get; set; }
        public int b_mdef { get; set; }
        public int b_flee { get; set; }
        public int b_aspd { get; set; }
        public int b_hit { get; set; }

        #endregion

        #region levelup properties

        public int Statpoints { get; set; }
        public int Skillpoints { get; set; }

        #endregion

        #region player stats
        // Player Stats
        public int Strength
        {
            get { return this.str; }
            set { this.str = value; }
        }
        public int Agility
        {
            get { return this.agi; }
            set { this.agi = value; }
        }
        public int Vitality
        {
            get { return this.vit; }
            set { this.vit = value; }
        }
        public int Intelligence
        {
            get { return this.intel; }
            set { this.intel = value; }
        }
        public int Dexterity
        {
            get { return this.dex; }
            set { this.dex = value; }
        }
        public int Luck
        {
            get { return this.luk; }
            set { this.luk = value; }
        }
        #endregion

        #region constructor

        public PlayerInfo()
        {
            this.name = "New Player";
            this.jobclass = "Archer";

            // temporary parameters these should eventually be imported from the Monster Database
            this.HP = 2000;
            this.MAXHP = 2000;
            this.SP = 500;
            this.MAXSP = 500;
            this.Level = 1;
            this.Exp = 0;
            this.NextLevelExp = (int)(Level ^ 4 + (1000 * Level));
            this.Strength = 10;
            this.Dexterity = 10;
            this.Luck = 10;
            this.Agility = 10;

            this.Skillpoints = 20;
            this.Statpoints = 20;

            this.body_sprite = @"gfx\player\body\torso\";
            this.head_sprite = @"gfx\player\body\head\";
            this.faceset_sprite = @"gfx\player\faceset\face0\";
            this.hair_sprite = @"gfx\player\hairset\hair0\";
            this.hands_sprite = @"gfx\player\body\hands\";

            this.equipment = new Equipment();
            this.inventory = new Inventory();
            this.quickslotbar = new QuickSlotBar();
            this.skilltree = new SkillTree();
        }
        #endregion

    }
}
