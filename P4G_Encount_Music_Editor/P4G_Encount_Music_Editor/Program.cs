using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers.Binary;
using static P4G_Encount_Music_Editor.Enums;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace P4G_Encount_Music_Editor
{
    class Program
    {
        private const int Game_P4G = 1;
        private const int Game_P5 = 2;

        public struct Encounter
        {
            public byte[] Flags { get; set; }
            public ushort Field04 { get; set; }
            public ushort Field06 { get; set; }
            public ushort[] Units { get; set; }
            public ushort FieldId { get; set; }
            public ushort RoomId { get; set; }
            public ushort MusicId { get; set; }
        }

        private static string currentDir = null;
        private static string originalFolderDir = null;
        private static string moddedFolderDir = null;
        private static string presetsFolderDir = null;
        private static string packageFolderDir = null;

        private static PresetHandler presetHandler = new PresetHandler();
        private static ConfigHandler config = new ConfigHandler();
        private static TBLPatchesGenerator tblpatcher = new TBLPatchesGenerator();

        private static int gameID = 0;

        static void Main(string[] args)
        {
            SetPaths();

            //Console.WriteLine("Which Persona game are you editing?");
            //gameID = PromptInt("Persona (4/5)");
            //if (gameID != 4 && gameID != 5)
            //    return;

            if (!PromptGame())
                return;

            EditEncount();

            Console.WriteLine("Enter any key to exit...");
            Console.ReadKey();
        }

        private static bool PromptGame()
        {
            Console.WriteLine("Which Persona game are you editing?");
            Console.WriteLine("1. Persona 4 Golden");
            Console.WriteLine("2. Persona 5");
            gameID = PromptInt("Game Selection");
            if (gameID < 1 || gameID > 2)
            {
                Console.WriteLine("Invalid selection!");
                Console.WriteLine("Enter any key to exit...");
                Console.ReadKey();
                return false;
            }

            return true;
        }

        private static void SetPaths()
        {
            currentDir = Directory.GetCurrentDirectory();
            originalFolderDir = $@"{currentDir}\original";
            moddedFolderDir = $@"{currentDir}\modded";
            presetsFolderDir = $@"{currentDir}\presets";
            packageFolderDir = $@"{currentDir}\BGME Config Package";

            // create folders if needed
            try
            {
                Directory.CreateDirectory(originalFolderDir);
                Directory.CreateDirectory(moddedFolderDir);
                Directory.CreateDirectory(presetsFolderDir);
                Directory.CreateDirectory($@"{currentDir}\collections");
                Directory.CreateDirectory(packageFolderDir);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem creating folders...");
            }
        }

        private static void EditEncount()
        {
            string inEncountPath = $@"{originalFolderDir}\ENCOUNT.TBL";
            bool encountExists = File.Exists(inEncountPath);
            if (!encountExists)
            {
                Console.WriteLine($"Missing original ENCOUNT.TBL! File: {inEncountPath}");
                if (gameID == Game_P5)
                {
                    return;
                }
                Console.WriteLine($"Some features will be disabled!");
            }

            string outEncountPath = $@"{moddedFolderDir}\ENCOUNT.TBL";

            Encounter[] allBattles = encountExists ? GetEncountersList(inEncountPath) : new Encounter[944];

            int menuSelection = 0;

            do
            {
                Console.WriteLine("\nP4G_Encount_Music_Editor");
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Run Preset");
                if (encountExists)
                {
                    Console.WriteLine("2. Output Encounter List");
                    Console.WriteLine("3. Collection Creation");
                }
                Console.WriteLine("0. Save and Exit");

                menuSelection = PromptInt("Menu Selection");
                switch (menuSelection)
                {
                    case 1:
                        presetHandler.RunPreset(allBattles);
                        break;
                    case 2:
                        if (encountExists)
                            OutputEncounterList(allBattles);
                        break;
                    case 3:
                        if (encountExists)
                            CollectionCreation(allBattles);
                        break;
                    default:
                        break;
                }

            } while (menuSelection != 0);

            try
            {
                if (encountExists)
                {
                    // write edited encounter tbl
                    using (BinaryWriter writer = new BinaryWriter(File.Open(outEncountPath, FileMode.Create)))
                    {
                        using BinaryReader reader = new BinaryReader(File.Open(inEncountPath, FileMode.Open));
                        UInt32 size = reader.ReadUInt32();
                        writer.Write(size);
                        foreach (Encounter battle in allBattles)
                        {
                            if (gameID == Game_P5)
                            {
                                writer.Write(battle.Flags);
                                writer.Write(GetReverseEndianessBytes(battle.Field04));
                                writer.Write(GetReverseEndianessBytes(battle.Field06));
                                foreach (ushort enemy in battle.Units)
                                {
                                    writer.Write(GetReverseEndianessBytes(enemy));
                                }
                                writer.Write(GetReverseEndianessBytes(battle.FieldId));
                                writer.Write(GetReverseEndianessBytes(battle.RoomId));
                                writer.Write(GetReverseEndianessBytes(battle.MusicId));
                            }
                            else
                            {
                                writer.Write(battle.Flags);
                                writer.Write(battle.Field04);
                                writer.Write(battle.Field06);
                                foreach (P4_EnemiesID enemy in battle.Units)
                                {
                                    writer.Write((ushort)enemy);
                                }
                                writer.Write(battle.FieldId);
                                writer.Write(battle.RoomId);
                                writer.Write(battle.MusicId);
                            }
                        }
                        reader.BaseStream.Seek(size, SeekOrigin.Current);
                        writer.Write(reader.ReadBytes((int)(reader.BaseStream.Length - (size + 4))));
                    }
                }

                // create tbl patches
                try
                {
                    tblpatcher.GenerateTBLPatches(packageFolderDir, allBattles);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine($"Problem generating tblpatches!");
                }

                // rebuild config patch
                config.BuildPatch();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem building output!");
            }
        }

        private static byte[] GetReverseEndianessBytes(ushort value)
        {
            byte[] theBytes = BitConverter.GetBytes(value);
            Array.Reverse(theBytes);
            return theBytes;
        }

        private static void OutputEncounterList(Encounter[] encounters)
        {
            try
            {
                string listFilePath = $@"{currentDir}\encounters.txt";

                StringBuilder listText = new StringBuilder();

                for (int i = 0, total = encounters.Length; i < total; i++)
                {
                    listText.AppendLine($"Encounter Index: {i} Song Index: {encounters[i].MusicId}");
                    foreach(P4_EnemiesID enemy in encounters[i].Units)
                    {
                        listText.AppendLine(enemy.ToString());
                    }
                }

                File.WriteAllText(listFilePath, listText.ToString());
                Console.WriteLine($"Parsed encounter list written to: {listFilePath}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem writing encounter list!");
            }
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

            Console.WriteLine();

            return theNumber;
        }

        private static bool PromptYN(string name)
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

        private static string PromptString(string name)
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

        private static void CollectionCreation(Encounter[] encounters)
        {
            Console.WriteLine("Collection Creation");
            Console.WriteLine("Enter search term \"inaba\" to exit and save matches to file...");

            Dictionary<ushort, string> encounterMatches = new Dictionary<ushort, string>();

            while (true)
            {
                string searchString = PromptString("Search Term").ToLower();
                if (searchString.Equals("inaba"))
                    break;

                string[] searchTerms = searchString.Split(' ');
                bool searchByOccurence = false;

                if (searchTerms.Length == 1)
                {
                    Console.WriteLine("Match encounters that contain only ONE instance (y) or any amount (n) of Search Term?");
                    searchByOccurence = PromptYN("(y/n)");
                }

                int totalMatches = 0;
                for (int i = 0, total = encounters.Length; i < total; i++)
                {
                    Encounter currentEncounter = encounters[i];

                    bool foundMatch = false;

                    if (searchTerms.Length == 1)
                    {
                        if (searchByOccurence)
                        {
                            if (ContainsUnitTerm(currentEncounter.Units, searchTerms[0], 1))
                            {
                                Console.WriteLine("Found match!");
                                foundMatch = true;
                            }
                        }
                        else
                        {
                            if (ContainsUnitTerm(currentEncounter.Units, searchTerms[0]))
                            {
                                Console.WriteLine("Found match!");
                                foundMatch = true;
                            }
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Searching for multiple terms...");
                        foundMatch = true;
                        // if multiple terms, check for each term in current encounter units
                        foreach (var term in searchTerms)
                        {
                            int count = searchTerms.Count(s => s.Equals(term));
                            // if encounter does not contain a term match not made
                            if (!ContainsUnitTerm(currentEncounter.Units, term, count))
                            {
                                foundMatch = false;
                                break;
                            }
                        }
                    }

                    // add to encounters list if match was found
                    if (foundMatch)
                    {
                        totalMatches++;
                        ushort matchKey = (ushort)i;
                        if (!encounterMatches.ContainsKey(matchKey))
                        {
                            Console.WriteLine($"EncounterID: {i} - Added match to collection list!");
                            StringBuilder enemiesList = new StringBuilder();
                            enemiesList.Append("//");
                            foreach (ushort enemyId in currentEncounter.Units)
                                enemiesList.Append($"{GetEnemyName(enemyId)}, ");
                            enemiesList.Append('\n');

                            encounterMatches.Add((ushort)i, enemiesList.ToString());
                        }
                    }
                }

                Console.WriteLine($"Total Matches: {totalMatches}");
            }

            string collectionName = PromptString("Collection Name (Lowercase)").ToLower();
            string collectionFilePath = $@"{currentDir}\collections\{collectionName}.enc";
            bool addToFile = false;

            if (File.Exists(collectionFilePath))
            {
                Console.WriteLine("Collection exists! Append to collection (y) or overwrite (n)?");
                addToFile = PromptYN("(y/n)");
            }

            // write or overwrite collection file
            if (!File.Exists(collectionFilePath) || !addToFile)
            {
                StringBuilder collectionText = new StringBuilder();

                foreach (var match in encounterMatches)
                {
                    collectionText.AppendLine(match.Key.ToString());
                    collectionText.AppendLine(match.Value);
                }

                try
                {
                    File.WriteAllText(collectionFilePath, collectionText.ToString());
                    Console.WriteLine($"Collectione created! File: {collectionFilePath}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem writing collection to file!");
                }
            }
            else
            {
                // add to existing collection
                try
                {
                    string[] originalCollectionLines = File.ReadAllLines(collectionFilePath);
                    StringBuilder newCollectionLines = new StringBuilder();

                    // list of existing ids
                    List<ushort> existingIds = new List<ushort>();

                    // parse collection for existing ids
                    foreach (string line in originalCollectionLines)
                    {
                        newCollectionLines.AppendLine(line);
                        if (line.StartsWith("/") || line.Length < 1)
                            continue;
                        existingIds.Add(ushort.Parse(line));
                    }

                    foreach (var match in encounterMatches)
                    {
                        if (!existingIds.Contains(match.Key))
                        {
                            newCollectionLines.AppendLine(match.Key.ToString());
                            newCollectionLines.AppendLine($"{match.Value}");
                            Console.WriteLine($"EncounterID: {match.Key} added!");
                        }
                    }

                    File.WriteAllText(collectionFilePath, newCollectionLines.ToString());
                    Console.WriteLine($"Collectione edited! File: {collectionFilePath}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Problem adding to collection!");
                }
            }
        }

        private static bool ContainsUnitTerm(ushort[] units, string term)
        {
            foreach (ushort unit in units)
            {
                string unitName = GetEnemyName(unit).ToLower();
                if (unitName.Contains(term))
                    return true;
            }

            return false;
        }

        private static bool ContainsUnitTerm(ushort[] units, string term, int count)
        {
            int matchCount = 0;
            foreach (ushort unit in units)
            {
                string unitName = GetEnemyName(unit).ToLower();
                if (unitName.Contains(term))
                    matchCount++;
            }

            if (matchCount == count)
                return true;
            else
                return false;
        }

        private static string GetEnemyName(ushort enemyId)
        {
            if (gameID == Game_P4G)
                return ((P4_EnemiesID)enemyId).ToString();
            if (gameID == Game_P5)
                return ((P5_EnemiesID)enemyId).ToString();
            else
                return null;
        }

        private static Encounter[] GetEncountersList(string encountPath)
        {
            List<Encounter> allEncounters = new List<Encounter>();

            bool useBigEndian = false;
            if (gameID == Game_P5)
            {
                //Console.WriteLine("Reading Encount.tbl in Big Endian!");
                useBigEndian = true;
            }

            Console.WriteLine("Parsing ENCOUNT.TBL");

            try
            {
                using BinaryReader reader = new BinaryReader(File.Open(encountPath, FileMode.Open));
                // 4 byte size integer
                UInt32 size = useBigEndian ? BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4)) : reader.ReadUInt32();
                //Console.WriteLine($"Encount Size: {size}");

                for (int bytesRead = 4; bytesRead < size; bytesRead += 24)
                {
                    Encounter currentEncounter = new Encounter();
                    currentEncounter.Flags = reader.ReadBytes(4);
                    currentEncounter.Field04 = useBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)) : reader.ReadUInt16();
                    currentEncounter.Field06 = useBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)) : reader.ReadUInt16();
                    if (useBigEndian)
                        currentEncounter.Units = new ushort[] { BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)), BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)), BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)), BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)), BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)) };
                    else
                        currentEncounter.Units = new ushort[] { reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16() };
                    currentEncounter.FieldId = useBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)) : reader.ReadUInt16();
                    currentEncounter.RoomId = useBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)) : reader.ReadUInt16();
                    currentEncounter.MusicId = useBigEndian ? BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)) : reader.ReadUInt16();

                    allEncounters.Add(currentEncounter);
                }

                Console.WriteLine($"Total Encounters: {allEncounters.Count}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"Couldn't parse ENCOUNT.TBL file! File: {encountPath}");
            }

            return allEncounters.ToArray();
        }

        public static void DisplayEnemiesList(P4_EnemiesID[] enemies)
        {
            foreach (P4_EnemiesID enemy in enemies)
            {
                Console.WriteLine($"ID: {(int)enemy} Name: {enemy}");
            }
        }
    }
}
