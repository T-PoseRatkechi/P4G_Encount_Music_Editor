using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers.Binary;
using static P4G_Encount_Music_Editor.EnemyEnums;
using System.Text;
using System.Linq;

namespace P4G_Encount_Music_Editor
{
    class Program
    {
        private static GameProps currentGame;

        static void Main(string[] args)
        {
            Console.WriteLine("Encount Music Editor");

            SetupGame();
            SetupFolders();
            MainMenu();
        }

        private static void SetupGame()
        {
            int selectedGameInput = -1;

            do
            {
                Console.WriteLine("Select Persona Game");

                // display list of available games as choices, starting at 1
                foreach (GameTitle game in Enum.GetValues(typeof(GameTitle)))
                {
                    Console.WriteLine($"{(int)game + 1}. {game}");
                }

                // save temp choice
                int tempChoice = ConsolePrompt.PromptInt("Game");

                // save and exit prompt if game choice is valid
                if (Enum.IsDefined(typeof(GameTitle), tempChoice - 1))
                    selectedGameInput = tempChoice - 1;

            }
            while (selectedGameInput < 0);

            GameTitle selectedGame = (GameTitle)selectedGameInput;

            currentGame = new GameProps(selectedGame);
        }

        private static void MainMenu()
        {
            IMenuOption[] menuOptions = new IMenuOption[]
            {
                new RunPresetOption(),
                new CollectionCreationOption(),
                new OutputListOption()
            };

            do
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{currentGame.Name} Encount Music Editor");
                Console.ResetColor();

                for (int i = 0, total = menuOptions.Length; i < total; i++)
                {
                    Console.WriteLine($"{i + 1}. {menuOptions[i].Name}");
                }
                Console.WriteLine("0. Exit");

                int choice = ConsolePrompt.PromptInt("Choice");

                if (choice == 0)
                    return;
                else if (choice - 1 < menuOptions.Length)
                    menuOptions[choice - 1].Run(currentGame);
            }
            while (true);
        }

        private static void SetupFolders()
        {
            string currentDir = Directory.GetCurrentDirectory();

            string collectionsFolder = $@"{currentDir}\collections";
            string presetsFolder = $@"{currentDir}\presets";

            // create folders if needed
            try
            {
                // app folders
                Directory.CreateDirectory(collectionsFolder);
                Directory.CreateDirectory(presetsFolder);

                // game folders
                Directory.CreateDirectory(currentGame.PackageFolder);
                Directory.CreateDirectory(currentGame.TblPatchesFolder);
                Directory.CreateDirectory(currentGame.PatchesFolder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem creating folders...");
            }
        }
    }
}
