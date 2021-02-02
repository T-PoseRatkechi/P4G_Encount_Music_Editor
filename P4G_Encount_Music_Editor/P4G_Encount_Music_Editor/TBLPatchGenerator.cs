using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using static P4G_Encount_Music_Editor.Program;

namespace P4G_Encount_Music_Editor
{
    class TBLPatchGenerator
    {
        private struct PatchEdit
        {
            public string tbl { get; set; }
            public int section { get; set; }
            public int offset { get; set; }
            public string data { get; set; }
        }

        private struct Patch
        {
            public int Version { get; set; }
            public PatchEdit[] Patches { get; set; }
        }

        public void GeneratePatch(string outputFolder, GameProps game, ushort[] encountMusicIds)
        {
            List<PatchEdit> musicPatches = new List<PatchEdit>();

            for (int i = 0, total = encountMusicIds.Length; i < total; i++)
            {
                int currentOffset = (i * game.EntrySize) + game.StartingOffset;
                byte[] currentMusicBytes = BitConverter.GetBytes(encountMusicIds[i]);

                if (game.IsBigEndian)
                    Array.Reverse(currentMusicBytes);

                musicPatches.Add(new PatchEdit()
                {
                    tbl = "ENCOUNT",
                    section = 0,
                    data = PatchDataFormatter.ByteArrayToHexText(currentMusicBytes),
                    offset = currentOffset
                });
            }

            Patch encountPatch = new Patch()
            {
                Version = 1,
                Patches = musicPatches.ToArray()
            };

            string patchFilePath = $@"{outputFolder}\tblpatches\EncountMusicPatches.tbp";
            
            // create sub tblpatches folder if missing
            if (!Directory.GetParent(patchFilePath).Exists)
                Directory.CreateDirectory(Path.GetDirectoryName(patchFilePath));

            File.WriteAllText(patchFilePath, JsonSerializer.Serialize(encountPatch, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"TBL Patch Created: {patchFilePath}");
        }
    }
}
