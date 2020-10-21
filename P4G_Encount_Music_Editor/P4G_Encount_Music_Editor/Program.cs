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
                Directory.CreateDirectory($@"{currentDir}\collections");
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
                Console.WriteLine("\nP4G_Encount_Music_Editor");
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Run Preset");
                Console.WriteLine("2. Output Encounter List");
                Console.WriteLine("0. Save and Exit");

                menuSelection = PromptInt("Menu Selection");
                switch (menuSelection)
                {
                    case 1:
                        presetHandler.RunPreset(allBattles);
                        break;
                    case 2:
                        OutputEncounterList(allBattles);
                        break;
                    default:
                        break;
                }

            } while (menuSelection != 0);

            try
            {
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

                // rebuild config patch
                config.BuildPatch();

                string aemPatcherPath = $@"{currentDir}\Aem_TBL_Patcher.exe";
                if (File.Exists(aemPatcherPath))
                {
                    Console.WriteLine("Aem TBL Patcher detected! Create tblpatches now?");
                    Console.WriteLine("0. Yes\n1. No");
                    int choice = PromptInt("Choice");
                    if (choice == 0)
                    {
                        Process aemPatcher = Process.Start(new ProcessStartInfo(aemPatcherPath));
                        aemPatcher.WaitForExit();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem saving modified ENCOUNT.TBL!");
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
