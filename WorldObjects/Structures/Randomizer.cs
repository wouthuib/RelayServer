using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelayServer.WorldObjects.Structures
{
    // Singleton randomizer to provide unique values
    public class Randomizer
    {
        private static Randomizer instance;
        private Random rand;

        private Randomizer()
        {
            rand = new Random();
        }

        public int generateRandom(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static Randomizer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Randomizer();
                }
                return instance;
            }
        }
    }
}
