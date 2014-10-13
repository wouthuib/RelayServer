using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace RelayServer.Static
{
    public class OutputManager
    {
        private static OutputManager instance;

        private Object thisLock = new Object();

        private OutputManager()
        {
        }

        public static OutputManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new OutputManager();

                return instance;
            }
        }

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

        public void WriteLog(string logMessage)
        {
            lock (thisLock)
            {
                using (StreamWriter logwriter = File.AppendText("log.txt"))
                {
                    logwriter.Write("\r\nLog Entry : ");
                    logwriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                        DateTime.Now.ToLongDateString());
                    logwriter.WriteLine("\n");
                    logwriter.WriteLine("{0} \n", logMessage);
                    logwriter.WriteLine("-------------------------------");
                    logwriter.Flush();
                    logwriter.Close();
                }
            }
        }
    }
}
