using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace P4G_Encount_Music_Editor
{
    abstract class EncounterMenuOptions : IMenuOption
    {
        public abstract string Name { get; }

        protected GameProps _currentGame;

        public void Run(GameProps game)
        {
            if (!File.Exists(game.EncountFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{Name}] Requires the game's ({game.Name}) ENCOUNT tbl!");
                Console.WriteLine($"Missing file: {game.EncountFile}");
                Console.ResetColor();
            }
            else
            {
                _currentGame = game;
                RunEncount();
            }
        }

        protected abstract void RunEncount();

        protected Encounter[] GetEncountersList()
        {
            List<Encounter> allEncounters = new List<Encounter>();

            bool useBigEndian = _currentGame.IsBigEndian;
            Console.WriteLine("Parsing ENCOUNT.TBL");

            try
            {
                using BinaryReader reader = new BinaryReader(File.Open(_currentGame.EncountFile, FileMode.Open));
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
                Console.WriteLine($"Couldn't parse ENCOUNT.TBL file! File: {_currentGame.EncountFile}");
            }

            return allEncounters.ToArray();
        }
    }
}
