using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace P4G_Encount_Music_Editor
{
    class CollectionCreationOption : IMenuOption
    {
        public string Name => "Collection Creation";

        public void Run(GameProps game)
        {
            if (!File.Exists(game.EncountFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Collection Creation requires the game's ({game.Name}) ENCOUNT tbl!");
                Console.WriteLine($"Missing file: {game.EncountFile}");
                Console.ResetColor();
                return;
            }
        }
    }
}
