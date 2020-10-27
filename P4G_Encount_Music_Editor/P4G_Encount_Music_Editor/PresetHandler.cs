using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static P4G_Encount_Music_Editor.Program;

namespace P4G_Encount_Music_Editor
{
    class PresetHandler
    {
        private string currentDir = null;
        private string presetsFolderDir = null;
        private ConfigHandler config = new ConfigHandler();

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

                    string item = lineArgs[0];
                    string command = lineArgs[1];

                    // parse command to get new music id
                    ushort newMusicId = ParseCommand(command);

                    // line is a command
                    if (lineArgs[0].StartsWith('.'))
                    {
                        string collection = item.Substring(1);
                        RunCollectionCommand(encounters, collection, newMusicId);
                    }
                    else
                    {
                        int encounterIndex = Int32.Parse(lineArgs[0]);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Direct Edit");
                        Console.ResetColor();
                        Console.WriteLine($"Encounter Index: {encounterIndex} Song Index: {newMusicId}");
                        encounters[encounterIndex].MusicId = newMusicId;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Problem reading preset file! Enter any key to return to menu...");
                Console.ReadLine();
            }
        }

        private ushort ParseCommand(string command)
        {
            ushort musicId = 0;

            if (command.StartsWith("random"))
            {
                try
                {
                    // regex for parsing functions: https://stackoverflow.com/questions/18906514/regex-for-matching-functions-and-capturing-their-arguments/18908330
                    var functionMatch = Regex.Match(command, @"\b[^()]+\((.*)\)$");
                    string innerArgs = functionMatch.Groups[1].Value;
                    var argMatches = Regex.Matches(innerArgs, @"([^,]+\(.+?\))|([^,]+)");
                    string arg1 = argMatches[0].Value;
                    string arg2 = argMatches[1].Value;

                    ushort minIndex = ushort.Parse(arg1);
                    ushort maxIndex = ushort.Parse(arg2);

                    musicId = config.GetRandomSetIndex(minIndex, maxIndex, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem parsing random args!");
                }
            }
            else if(command.StartsWith("advantage"))
            {
                try
                {
                    // regex for parsing functions: https://stackoverflow.com/questions/18906514/regex-for-matching-functions-and-capturing-their-arguments/18908330
                    var functionMatch = Regex.Match(command, @"\b[^()]+\((.*)\)$");
                    string innerArgs = functionMatch.Groups[1].Value;
                    var argMatches = Regex.Matches(innerArgs, @"([^,]+\(.+?\))|([^,]+)");
                    string arg1 = argMatches[0].Value;
                    string arg2 = argMatches[1].Value;

                    ushort minIndex = ushort.Parse(arg1);
                    ushort maxIndex = ushort.Parse(arg2);

                    musicId = config.GetRandomSetIndex(minIndex, maxIndex, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem parsing random args!");
                }
            }
            else
            {
                try
                {
                    musicId = ushort.Parse(command);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Could not parse song index!");
                }
            }

            return musicId;
        }

        private void RunCollectionCommand(Encounter[] encounters, string collectionName, ushort waveIndex)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Collection {collectionName.ToUpper()}:");
            Console.ResetColor();

            switch (collectionName)
            {
                case "all":
                    Console.WriteLine($"Setting All Encounter Music IDs to: {waveIndex}");
                    for (int i = 0, total = encounters.Length; i < total; i++)
                    {
                        encounters[i].MusicId = waveIndex;
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

                        int numEncounters = 0;

                        foreach (string line in collectionLines)
                        {
                            // skip comment lines and new lines
                            if (line.StartsWith('/') || line.StartsWith('\n') || line.Length < 1)
                                continue;
                            
                            try
                            {
                                int encounterIndex = Int32.Parse(line);
                                //Console.WriteLine($"Encounter Index: {encounterIndex} Song Index: {waveIndex}");
                                encounters[encounterIndex].MusicId = waveIndex;
                                numEncounters++;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                Console.WriteLine($"Could not parse Encounter Index in collection: {collectionName}");
                            }
                        }

                        Console.WriteLine($"Set {numEncounters} encounters to Song Index: {waveIndex}");
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
