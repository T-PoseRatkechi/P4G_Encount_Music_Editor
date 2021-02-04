using System;
using System.Collections.Generic;
using System.Text;

namespace P4G_Encount_Music_Editor
{
    class ConsolePrompt
    {
        public static int PromptInt(string name)
        {
            int theNumber = -1;

            while (theNumber < 0)
            {
                Console.Write($"Enter {name} (number): ");
                string input = Console.ReadLine();
                try
                {
                    theNumber = Int32.Parse(input);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't parse number!");
                }
            }

            Console.WriteLine();

            return theNumber;
        }

        public static bool PromptYN(string name)
        {
            string theString = null;

            while (theString == null || (!theString.Equals("y") && !theString.Equals("n")))
            {
                theString = PromptString(name);
            }

            if (theString.ToLower().Equals("y"))
                return true;
            else
                return false;
        }

        public static string PromptString(string name)
        {
            string theString = null;

            while (theString == null)
            {
                Console.Write($"Enter {name} (string): ");
                string input = Console.ReadLine();

                if (input != null && input.Length > 0)
                {
                    theString = input;
                }
            }

            return theString;
        }
    }
}
