using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RelayServer.WorldObjects.Structures
{
    public class OutputManager
    {
        public static void WriteLine(string text, string[] args = null)
        {
            string strip = Regex.Replace(text, "\t", "");

            if (strip.Contains("Error") || strip.Contains("error"))
                Console.ForegroundColor = ConsoleColor.Red;
            else if (strip.StartsWith("Info") || strip.StartsWith("-"))
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
                Console.ForegroundColor = ConsoleColor.Gray;

            if (args != null)
                Console.WriteLine(text, args);
            else
                Console.WriteLine(text);

            Console.ResetColor();
        }

        public static void Write(string text, string[] args = null)
        {
            string strip = Regex.Replace(text, "\t", "");

            if (strip.StartsWith("Error") || strip.StartsWith("error"))
                Console.ForegroundColor = ConsoleColor.Red;
            else if (strip.StartsWith("Info") || strip.StartsWith("-"))
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
                Console.ForegroundColor = ConsoleColor.Gray;

            if (args != null)
                Console.Write(text, args);
            else
                Console.Write(text);

            Console.ResetColor();
        }
    }
}
