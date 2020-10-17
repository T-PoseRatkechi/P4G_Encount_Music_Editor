using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static P4G_Encount_Music_Editor.Program;

namespace P4G_Encount_Music_Editor
{
    class PresetHandler
    {
        private string currentDir = null;
        private string presetsFolderDir = null;

        public PresetHandler()
        {
            currentDir = Directory.GetCurrentDirectory();
            presetsFolderDir = $@"{currentDir}\presets";
        }

        public void RunPreset(Encounter[] encounters)
        {
            // get preset file path
            string presetFile = SelectPresetFile();
            if (presetFile == null)
                return;

            try
            {
                string[] presetLines = File.ReadAllLines(presetFile);

                foreach (string line in presetLines)
                {
                    // skip comment lines
                    if (line.StartsWith("//") || line.Length <= 3)
                        continue;

                    string[] lineArgs = line.Split("=");

                    // line is a command
                    if (lineArgs[0].StartsWith('.'))
                    {
                        string collection = lineArgs[0].Substring(1);
                        string command = lineArgs[1];
                        RunCollectionCommand(encounters, collection, lineArgs[1]);
                    }
                    else
                    {
                        int encounterIndex = Int32.Parse(lineArgs[0]);
                        ushort songIndex = UInt16.Parse(lineArgs[1]);

                        Console.WriteLine($"Encounter Index: {encounterIndex} Song Index: {songIndex}");
                        encounters[encounterIndex].MusicId = songIndex;
                    }
                    /*
                    int encounterIndex = Int32.Parse(line.Replace(' ', '\0').Split("=")[0]);
                    ushort songIndex = UInt16.Parse(line.Replace(' ', '\0').Split("=")[1]);

                    Console.WriteLine($"Encounter Index: {encounterIndex} Song Index: {songIndex}");
                    encounters[encounterIndex].MusicId = songIndex;
                    */
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem reading preset file!");
            }

            Console.WriteLine("Encounter preset set. Enter to return to menu...");
            Console.ReadLine();
        }

        private void RunCollectionCommand(Encounter[] encounters, string collectionName, string command)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Collection {collectionName.ToUpper()}:");
            Console.ResetColor();

            // store new music id to set to encounters
            ushort newMusicId = 0;

            // random set
            if (command.StartsWith('.'))
            {
                if (command.StartsWith(".random"))
                {
                    try
                    {
                        ushort randSetIndex = UInt16.Parse(command.Split('-')[1]);
                        newMusicId = (ushort)(randSetIndex + 8192);
                        Console.WriteLine($"Using Random Set: {randSetIndex}, Song Index: {newMusicId}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Problem parsing random set Song Index!");
                    }
                }
            }
            else
            {
                try
                {
                    newMusicId = UInt16.Parse(command);
                    Console.WriteLine($"Using Song Index: {newMusicId}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem parsing Song Index!");
                }
            }

            switch (collectionName)
            {
                case "all":
                    Console.WriteLine($"Setting All Encounter Music IDs to: {newMusicId}");
                    for (int i = 0, total = encounters.Length; i < total; i++)
                    {
                        encounters[i].MusicId = newMusicId;
                    }
                    break;
                default:
                    try
                    {
                        string collectionFilePath = $@"{currentDir}\collections\{collectionName}.enc";
                        // exit if collection file is mising
                        if (!File.Exists(collectionFilePath))
                        {
                            Console.WriteLine($"Collection file doesn't exist! File: {collectionFilePath}");
                            return;
                        }

                        string[] collectionLines = File.ReadAllLines(collectionFilePath);

                        foreach (string line in collectionLines)
                        {
                            // skip comment lines and new lines
                            if (line.StartsWith('/') || line.StartsWith('\n') || line.Length < 1)
                                continue;
                            
                            try
                            {
                                int encounterIndex = Int32.Parse(line);
                                Console.WriteLine($"Encounter Index: {encounterIndex} Song Index: {newMusicId}!");
                                encounters[encounterIndex].MusicId = newMusicId;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                Console.WriteLine($"Could not parse Encounter Index in collection: {collectionName}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine("Problem parsing collection!");
                    }
                    break;
            }
        }

        private string SelectPresetFile()
        {
            string[] allPresets = null;

            try
            {
                allPresets = Directory.GetFiles(presetsFolderDir, "*.preset", SearchOption.AllDirectories);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem reading presets from folder!");
                return null;
            }

            // exit if no presets were found
            if (allPresets.Length <= 0)
                return null;

            // display all presets
            Console.WriteLine("Available Presets");
            for (int i = 0, total = allPresets.Length; i < total; i++)
            {
                Console.WriteLine($"{i}. {Path.GetFileNameWithoutExtension(allPresets[i])}");
            }

            // get valid preset choice
            int presetSelection = -1;
            do
            {
                int tempChoice = PromptInt("Preset Selection");
                if (tempChoice >= allPresets.Length)
                    Console.WriteLine("Invalid selection!");
                else
                    presetSelection = tempChoice;

            } while (presetSelection < 0);

            return allPresets[presetSelection];
        }

        private static int PromptInt(string name)
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
                    Console.WriteLine("Couldn't parse numer!");
                }
            }

            return theNumber;
        }
    }
}
