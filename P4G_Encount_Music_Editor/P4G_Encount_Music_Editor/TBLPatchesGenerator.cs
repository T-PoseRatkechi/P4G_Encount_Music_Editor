using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static P4G_Encount_Music_Editor.Program;

namespace P4G_Encount_Music_Editor
{
    class TBLPatchesGenerator
    {
        public void GenerateTBLPatches(string outputFolder, Encounter[] encounters)
        {
            string patchesDir = $@"{outputFolder}\tblpatches";

            if (!Directory.Exists(patchesDir))
                Directory.CreateDirectory(patchesDir);

            int offsetStart = 4;
            int encounterSize = 24;
            int musicOffset = 22;

            StringBuilder patchesLog = new StringBuilder();

            for (int i = 0, total = encounters.Length; i < total; i++)
            {
                Encounter currentEnc = encounters[i];
                long currentEncOffset = offsetStart + (encounterSize * i) + musicOffset;
                string patchFile = $@"{patchesDir}\ENC_{currentEncOffset:X}.tblpatch";

                using BinaryWriter writer = new BinaryWriter(File.Open(patchFile, FileMode.Create));

                writer.Write(Encoding.ASCII.GetBytes("ENC"));
                writer.Write(BitConverter.GetBytes(currentEncOffset).Reverse().ToArray());
                writer.Write(BitConverter.GetBytes(currentEnc.MusicId));

                patchesLog.AppendLine($"Offset: {currentEncOffset:X} Value: {currentEnc.MusicId}");
            }

            File.WriteAllText($@"{outputFolder}\TBL Patches.log", patchesLog.ToString());
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{encounters.Length} tblpatches created!");
            Console.ResetColor();
        }
    }
}
