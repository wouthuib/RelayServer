using System;
using RelayServer.Database.Skills;
using RelayServer.Database.Items;

namespace RelayServer.Database.Players
{
    [Serializable]
    public class QuickSlotBar
    {
        #region properties

        public Object[] quickslot = new Object[12];
        public bool visible = true;

        #endregion

        public QuickSlotBar()
        {
        }

        public Object Quickslot(int slot)
        {
            if (quickslot[slot] != null)
            {
                if (quickslot[slot] is Skill)
                    return quickslot[slot] as Skill;
                else if (quickslot[slot] is Item)
                    return quickslot[slot] as Item;
                else
                    return null;
            }
            else
                return null;
        }

        public Skill[] QuickSlotSkill
        {
            get { return quickslot as Skill[]; }
            set { quickslot = value as Skill[]; }
        }

        public Item[] QuickSlotItem
        {
            get { return quickslot as Item[]; }
            set { quickslot = value as Item[]; }
        }


        public int getSlot(string objectname)
        {
            for (int i = 0; i < quickslot.Length; i++)
            {
                if (quickslot[i] != null)
                {
                    if (quickslot[i] is Skill) // cast 1
                    {
                        Skill skill = quickslot[i] as Skill; // cast 2

                        if (skill.Name == objectname)
                            return i;
                    }
                    else if (quickslot[i] is Item) // cast 1
                    {
                        Item item = quickslot[i] as Item; // cast 2

                        if (item.itemName == objectname)
                            return i;
                    }
                }

            }

            return -1;
        }
    }
}
