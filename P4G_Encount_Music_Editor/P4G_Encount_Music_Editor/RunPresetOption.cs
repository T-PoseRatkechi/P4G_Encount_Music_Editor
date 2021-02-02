using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace P4G_Encount_Music_Editor
{
    class RunPresetOption : IMenuOption
    {
        public string Name => "Run Preset";

        private readonly struct PresetConfig
        {
            public BGMEConfig BgmeConfig { get; }
            public Dictionary<string, ushort> SetIndexNames { get; }

            public PresetConfig(BGMEConfig bgme, Dictionary<string, ushort> setIndexNames)
            {
                BgmeConfig = bgme;
                SetIndexNames = setIndexNames;
            }
        }

        private string currentDir = null;
        private string presetsFolderDir = null;

        public RunPresetOption()
        {
            currentDir = Directory.GetCurrentDirectory();
            presetsFolderDir = $@"{currentDir}\presets";
        }

        private TBLPatchGenerator patchGenerator = new TBLPatchGenerator();

        public void Run(GameProps game)
        {
            PresetConfig config = new PresetConfig(new BGMEConfig(), new Dictionary<string, ushort>());

            // store list of encounter music ids
            ushort[] encountersMusicIds = new ushort[game.TotalEncounters()];

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
                    if (line.StartsWith("//") || line.Length < 3)
                        continue;

                    string[] lineArgs = line.Split("=");

                    string item = lineArgs[0];
                    string command = lineArgs[1];

                    // parse command to get new music id
                    ushort newMusicId = ParseCommand(ref config, command);

                    // line is a collection command
                    if (lineArgs[0].StartsWith('.'))
                    {
                        string collection = item.Substring(1);
                        RunCollectionCommand(encountersMusicIds, collection, newMusicId);
                    }
                    // line is an alias command
                    else if (lineArgs[0].StartsWith('_'))
                    {
                        string setName = lineArgs[0];
                        RunAliasCommand(ref config, setName, newMusicId);
                    }
                    else
                    {
                        int encounterIndex = Int32.Parse(lineArgs[0]);

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Direct Edit");
                        Console.ResetColor();
                        Console.WriteLine($"Encounter Index: {encounterIndex} Song Index: {newMusicId}");
                        encountersMusicIds[encounterIndex] = newMusicId;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Problem reading preset file! Enter any key to return to menu...");
                return;
            }

            try
            {
                // create preset tbl patches
                EmptyFolder(game.TblPatchesFolder);
                patchGenerator.GeneratePatch(game.TblPatchesFolder, game, encountersMusicIds);

                // create inaba patch for p4g
                if (game.Name == GameTitle.P4G)
                {
                    EmptyFolder(game.PatchesFolder);
                    config.BgmeConfig.BuildPatch(game.PatchesFolder);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Aemulus Package Created: {game.PackageFolder}");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem creating package!");
            }
        }

        private void EmptyFolder(string folder)
        {
            string[] folderFiles = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

            try
            {
                foreach (string file in folderFiles)
                    File.Delete(file);
            }
            catch (DirectoryNotFoundException) { }
        }

        private ushort ParseCommand(ref PresetConfig config, string command)
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

                    musicId = config.BgmeConfig.GetRandomSetIndex(minIndex, maxIndex, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem parsing random args!");
                }
            }
            else if (command.StartsWith("advantage"))
            {
                try
                {
                    // regex for parsing functions: https://stackoverflow.com/questions/18906514/regex-for-matching-functions-and-capturing-their-arguments/18908330
                    var functionMatch = Regex.Match(command, @"\b[^()]+\((.*)\)$");
                    string innerArgs = functionMatch.Groups[1].Value;
                    var argMatches = Regex.Matches(innerArgs, @"([^,]+\(.+?\))|([^,]+)");
                    string arg1 = argMatches[0].Value;
                    string arg2 = argMatches[1].Value;

                    ushort minIndex = config.SetIndexNames[arg1];
                    ushort maxIndex = config.SetIndexNames[arg2];

                    musicId = config.BgmeConfig.GetRandomSetIndex(minIndex, maxIndex, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem parsing random args!");
                }
            }
            else if (command.StartsWith('_'))
            {
                musicId = (ushort)(config.SetIndexNames[command] + 8192);
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

        private void RunAliasCommand(ref PresetConfig config, string name, ushort waveIndex)
        {
            if (!config.SetIndexNames.ContainsKey(name))
            {
                int setIndex = waveIndex - 8192;
                config.SetIndexNames.Add(name, (ushort)setIndex);
                ushort[] aliasSet = config.BgmeConfig.GetSetByKey(waveIndex);
                if (aliasSet != null)
                    Console.WriteLine($"{name} set to SetID: {setIndex} ({aliasSet[0]}, {aliasSet[1]})");
            }
        }

        private void RunCollectionCommand(ushort[] encountersMusicIds, string collectionName, ushort waveIndex)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Collection {collectionName.ToUpper()}:");
            Console.ResetColor();

            switch (collectionName)
            {
                case "all":
                    Console.WriteLine($"Setting All Encounter Music IDs to: {waveIndex}");
                    for (int i = 0, total = encountersMusicIds.Length; i < total; i++)
                    {
                        encountersMusicIds[i] = waveIndex;
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
                                encountersMusicIds[encounterIndex] = waveIndex;
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
            if (allPresets.Length < 1)
                return null;

            // display all presets
            Console.WriteLine("Available Presets");
            for (int i = 0, total = allPresets.Length; i < total; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileNameWithoutExtension(allPresets[i])}");
            }

            // get valid preset choice
            int presetSelection = -1;
            do
            {
                int tempChoice = ConsolePrompt.PromptInt("Preset Selection");
                if (tempChoice > allPresets.Length)
                    Console.WriteLine("Invalid selection!");
                else
                    presetSelection = tempChoice;

            } while (presetSelection < 1);

            return allPresets[presetSelection - 1];
        }
    }
}
