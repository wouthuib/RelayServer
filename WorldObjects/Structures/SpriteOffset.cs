using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayServer.WorldObjects.Structures
{
    public struct spriteOffset
    {
        public string Name;
        public int ID, X, Y;

        public spriteOffset(int id, string name, int x, int y)
        {
            this.ID = id;
            this.Name = name;
            this.X = x;
            this.Y = y;
        }
    }
}
