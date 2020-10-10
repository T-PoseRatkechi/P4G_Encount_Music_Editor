using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers.Binary;
using static P4G_Encount_Music_Editor.Enums;

namespace P4G_Encount_Music_Editor
{
    class Program
    {
        private struct Encounter
        {
            public byte[] Flags { get; set; }
            public ushort Field04 { get; set; }
            public ushort Field06 { get; set; }
            public EnemiesID[] Units { get; set; }
            public ushort FieldId { get; set; }
            public ushort RoomId { get; set; }
            public ushort MusicId { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            EditEncount();
        }

        private static void EditEncount()
        {
            string currentDir = Directory.GetCurrentDirectory();
            string inEncountPath = $@"{currentDir}\ENCOUNT.TBL";
            string outEncountPath = $@"{currentDir}\modded\ENCOUNT.TBL";

            List<Encounter> allEncounters = new List<Encounter>();

            using (BinaryReader reader = new BinaryReader(File.Open(inEncountPath, FileMode.Open)))
            {
                // skip first 4 bytes
                UInt32 size = reader.ReadUInt32();

                for (int bytesRead = 4; bytesRead < size; bytesRead+=24)
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
            }

            Console.WriteLine($"Total Encounters: {allEncounters.Count}");
            Console.WriteLine($"Displaying First 5 Encounters");

            for (int i = 0; i < 5; i++)
            {
                Encounter currentEncounter = allEncounters[i];
                Console.WriteLine($"ID: {i} MusicId: {currentEncounter.MusicId}");
                DisplayEnemiesList(currentEncounter.Units);
            }

            Console.WriteLine($"Randomizing Encounter MusicIds");
            Encounter[] allBattles = allEncounters.ToArray();

            Random rand = new Random();
            for (int i = 0, total = allEncounters.Count; i < total; i++)
            {
                allBattles[i].MusicId = (ushort) (886 + rand.Next(0,10));
            }

            Console.WriteLine($"Creating New Encount");

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

        private static Encounter[] GetEncountersList(string encountPath)
        {
            List<Encounter> allEncounters = new List<Encounter>();

            Console.WriteLine("Parsing ENCOUNT.TBL...");

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

            return allEncounters.Count <= 0 ? null : allEncounters.ToArray();
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
