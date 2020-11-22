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
            string patchesDir = $@"{outputFolder}\tblpatches-2";

            if (!Directory.Exists(patchesDir))
                Directory.CreateDirectory(patchesDir);

            int offsetStart = 4;
            int encounterSize = 24;
            int musicOffset = 22;

            for (int i = 0, total = encounters.Length; i < total; i++)
            {
                Encounter currentEnc = encounters[i];
                long currentEncOffset = offsetStart + (encounterSize * i) + musicOffset;
                string patchFile = $@"{patchesDir}\ENC_{currentEncOffset:X}.tblpatch";

                using BinaryWriter writer = new BinaryWriter(File.Open(patchFile, FileMode.Create));

                writer.Write(Encoding.ASCII.GetBytes("ENC"));
                writer.Write(BitConverter.GetBytes(currentEncOffset).Reverse().ToArray());
                writer.Write(BitConverter.GetBytes(currentEnc.MusicId));

                Console.WriteLine($"Created tblpatch\n{patchFile}\nOffset: {currentEncOffset}");
            }
        }
    }
}
