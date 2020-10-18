using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers.Binary;
using static P4G_Encount_Music_Editor.Enums;
using System.Text;
using System.Linq;

namespace P4G_Encount_Music_Editor
{
    class Program
    {
        public struct Encounter
        {
            public byte[] Flags { get; set; }
            public ushort Field04 { get; set; }
            public ushort Field06 { get; set; }
            public EnemiesID[] Units { get; set; }
            public ushort FieldId { get; set; }
            public ushort RoomId { get; set; }
            public ushort MusicId { get; set; }
        }

        private static string currentDir = null;
        private static string originalFolderDir = null;
        private static string moddedFolderDir = null;
        private static string presetsFolderDir = null;

        private static PresetHandler presetHandler = new PresetHandler();
        private static ConfigHandler config = new ConfigHandler();

        static void Main(string[] args)
        {
            //ParseArkFile();
            SetPaths();
            EditEncount();
        }

        private static void ParseArkFile()
        {
            string arkFile = $@"{Directory.GetCurrentDirectory()}\Encounter_Place_ID.txt";
            string inEncountPath = $@"{Directory.GetCurrentDirectory()}\original\ENCOUNT.TBL";

            Encounter[] allEncounters = GetEncountersList(inEncountPath);

            string[] arkLines = File.ReadAllLines(arkFile);

            Dictionary<string, List<int>> encountersList = new Dictionary<string, List<int>>();

            foreach(string line in arkLines)
            {
                if (!line.Contains('-') || line.Length < 3 || line.StartsWith("//"))
                    continue;

                int encounterIndex = Int32.Parse(line.Split(" - ")[0].Split(' ')[2]);
                // get full encounter location/description
                string encounterLocation = line.Split(" - ")[1];
                // remove question marks
                //encounterLocation = encounterLocation.Replace('?', '\0');
                if (encounterLocation.StartsWith('?'))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Encounter {encounterIndex} is unknown!");
                    //Console.ReadLine();
                    Console.ResetColor();
                    continue;
                }

                encounterLocation = encounterLocation.Replace('?', '\0');

                string locationName = $"{encounterLocation.Split(' ')[0]}{(encounterLocation.Split(' ')[0].ToLower().Equals("heaven") ? "" : $" {encounterLocation.Split(' ')[1]}")}";
                locationName = locationName.Trim('\0');

                Console.WriteLine($"Encounter Index: {encounterIndex} Location: {locationName}");
                if (encountersList.ContainsKey(locationName))
                {
                    encountersList[locationName].Add(encounterIndex);
                }
                else
                {
                    encountersList.Add(locationName, new List<int>());
                    encountersList[locationName].Add(encounterIndex);
                }
            }

            StringBuilder parsedFile = new StringBuilder();

            foreach (string locationKey in encountersList.Keys)
            {
                string collectionFile = $@"{Directory.GetCurrentDirectory()}\collections\{locationKey}.enc";
                StringBuilder collectionText = new StringBuilder();

                parsedFile.AppendLine($"{locationKey.ToUpper()}\n#================================================");
                collectionText.AppendLine($"//{locationKey.ToUpper()}\n//================================================");

                foreach (int encountId in encountersList[locationKey])
                {
                    Encounter currentEncounter = allEncounters[encountId];
                    parsedFile.AppendLine($"Encounter Index: {encountId}");
                    collectionText.AppendLine($"//Encounter Index:\n{encountId}");
                    
                    parsedFile.Append("#Encounter Units: ");
                    collectionText.Append("//Encounter Units: ");

                    foreach (EnemiesID enemy in currentEncounter.Units)
                    {
                        if (enemy != EnemiesID.h000)
                        {
                            parsedFile.Append($"{enemy.ToString()}, ");
                            collectionText.Append($"{enemy.ToString()}, ");
                        }
                    }
                    parsedFile.Append('\n');
                    collectionText.Append("\n\n");
                }
                parsedFile.Append('\n');
                //collectionText.Append('\n');
                File.WriteAllText(collectionFile, collectionText.ToString());
            }

            File.WriteAllText($"{arkFile}.parsed", parsedFile.ToString());
        }

        private static void SetPaths()
        {
            currentDir = Directory.GetCurrentDirectory();
            originalFolderDir = $@"{currentDir}\original";
            moddedFolderDir = $@"{currentDir}\modded";
            presetsFolderDir = $@"{currentDir}\presets";

            // create folders if needed
            try
            {
                Directory.CreateDirectory(originalFolderDir);
                Directory.CreateDirectory(moddedFolderDir);
                Directory.CreateDirectory(presetsFolderDir);
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
            if (!File.Exists(inEncountPath))
            {
                Console.WriteLine($"Missing original ENCOUNT.TBL! File: {inEncountPath}");
                return;
            }

            string outEncountPath = $@"{moddedFolderDir}\ENCOUNT.TBL";

            Encounter[] allBattles = GetEncountersList(inEncountPath);

            int menuSelection = 0;

            do
            {
                Console.WriteLine("P4G_Encount_Music_Editor");
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Run Preset");
                Console.WriteLine("2. Rebuild Config");
                Console.WriteLine("3. Output Encounter List");
                Console.WriteLine("0. Exit and Save");

                menuSelection = PromptInt("Menu Selection");
                switch (menuSelection)
                {
                    case 1:
                        presetHandler.RunPreset(allBattles);
                        break;
                    case 2:
                        config.RebuildPatch();
                        break;
                    case 3:
                        OutputEncounterList(allBattles);
                        break;
                    default:
                        break;
                }

            } while (menuSelection != 0);

            // write edited encounter tbl
            using (BinaryWriter writer = new BinaryWriter(File.Open(outEncountPath, FileMode.Create)))
            {
                using BinaryReader reader = new BinaryReader(File.Open(inEncountPath, FileMode.Open));
                UInt32 size = reader.ReadUInt32();
                writer.Write(size);
                foreach (Encounter battle in allBattles)
                {
                    writer.Write(battle.Flags);
                    writer.Write(battle.Field04);
                    writer.Write(battle.Field06);
                    foreach (EnemiesID enemy in battle.Units)
                    {
                        writer.Write((ushort)enemy);
                    }
                    writer.Write(battle.FieldId);
                    writer.Write(battle.RoomId);
                    writer.Write(battle.MusicId);
                }
                reader.BaseStream.Seek(size, SeekOrigin.Current);
                writer.Write(reader.ReadBytes((int)(reader.BaseStream.Length - (size + 4))));
            }
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
                    foreach(EnemiesID enemy in encounters[i].Units)
                    {
                        listText.AppendLine(enemy.ToString());
                    }
                }

                File.WriteAllText(listFilePath, listText.ToString());
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

            return theNumber;
        }

        private static Encounter[] GetEncountersList(string encountPath)
        {
            List<Encounter> allEncounters = new List<Encounter>();

            Console.WriteLine("Parsing ENCOUNT.TBL");

            try
            {
                using BinaryReader reader = new BinaryReader(File.Open(encountPath, FileMode.Open));
                // 4 byte size integer
                UInt32 size = reader.ReadUInt32();

                for (int bytesRead = 4; bytesRead < size; bytesRead += 24)
                {
                    Encounter currentEncounter = new Encounter();
                    currentEncounter.Flags = reader.ReadBytes(4);
                    currentEncounter.Field04 = reader.ReadUInt16();
                    currentEncounter.Field06 = reader.ReadUInt16();
                    currentEncounter.Units = new EnemiesID[] { (EnemiesID)reader.ReadUInt16(), (EnemiesID)reader.ReadUInt16(), (EnemiesID)reader.ReadUInt16(), (EnemiesID)reader.ReadUInt16(), (EnemiesID)reader.ReadUInt16() };
                    currentEncounter.FieldId = reader.ReadUInt16();
                    currentEncounter.RoomId = reader.ReadUInt16();
                    currentEncounter.MusicId = reader.ReadUInt16();
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

        public static void DisplayEnemiesList(EnemiesID[] enemies)
        {
            foreach (EnemiesID enemy in enemies)
            {
                Console.WriteLine($"ID: {(int)enemy} Name: {enemy}");
            }
        }
    }
}
