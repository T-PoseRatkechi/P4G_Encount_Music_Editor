using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace P4G_Encount_Music_Editor
{
    class ConfigHandler
    {
        private string currentDir = null;
        private static Dictionary<ushort, ushort[]> randSets = new Dictionary<ushort, ushort[]>();

        public ConfigHandler()
        {
            currentDir = Directory.GetCurrentDirectory();
        }

        public ushort GetRandomSetIndex(ushort minIndex, ushort maxIndex, bool isAdvantage)
        {
            ushort randomSetIndex = 0;

            // random index range for comparison
            ushort[] randomRange = new ushort[] { minIndex, (ushort)(isAdvantage ? (maxIndex * 2 + 1) : (maxIndex * 2)) };

            // see if range already exists in dictionary
            KeyValuePair<ushort, ushort[]> dictionaryMatch = randSets.FirstOrDefault(sets => sets.Value.SequenceEqual(randomRange));

            // add entry for index range to dictionary
            if (dictionaryMatch.Value == null)
            {
                Console.WriteLine("New Random Set detected!");
                // limit of 15 possible random sets
                if (randSets.Count < 32)
                {
                    int totalSets = randSets.Count;
                    randomSetIndex = (ushort)(8192 + totalSets);
                    randSets.Add(randomSetIndex, randomRange);
                    Console.WriteLine($"Random Set added!\nRandSet ID: {totalSets}, Range: [{minIndex}, {maxIndex}), Wave Index: {randomSetIndex}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Max 15 randomized sets limit reached! Defaulting to first set!");
                    Console.ResetColor();
                    randomSetIndex = 8192;
                }
            }
            else
            {
                // set key from match as randSetIndex to use
                randomSetIndex = dictionaryMatch.Key;
            }

            return randomSetIndex;
        }

        public void BuildPatch()
        {
            // set file paths
            string patchFilePath = $@"{currentDir}\original\BGME_Config.patch";

            // exit early if missing one of the required files
            if (!File.Exists(patchFilePath))
            {
                Console.WriteLine($"Missing original BGME_Config.patch! File: {patchFilePath}");
                return;
            }

            // new patch file path
            string newPatchFile = $@"{currentDir}\BGME Aemulus Package\patches\BGME_Config.patch";

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(newPatchFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newPatchFile));
                }

                byte[] patchBytes = File.ReadAllBytes(patchFilePath);
                int startOffset = 20;

                //string[] randomSetsLines = File.ReadAllLines(randomSetsFilePath);

                foreach (KeyValuePair<ushort, ushort[]> set in randSets)
                {
                    // calculate randset index for patch
                    int randSetIndex = set.Key - 8192;

                    // parse min and max song indexes
                    ushort minIndex = set.Value[0];
                    ushort maxIndex = set.Value[1];

                    // convert min and max to bytes
                    byte[] minBytes = BitConverter.GetBytes(minIndex);
                    byte[] maxBytes = BitConverter.GetBytes(maxIndex);

                    // display
                    //Console.WriteLine($"Random Set: {randSetIndex}");
                    //Console.WriteLine($"Min Index: {minIndex} Bytes: {BitConverter.ToString(minBytes)}");
                    //Console.WriteLine($"Max Index: {maxIndex} Bytes: {BitConverter.ToString(maxBytes)}");

                    Array.Copy(minBytes, 0, patchBytes, startOffset + 4 * randSetIndex, minBytes.Length);
                    Array.Copy(maxBytes, 0, patchBytes, startOffset + 2 + 4 * randSetIndex, maxBytes.Length);
                }

                File.WriteAllBytes(newPatchFile, patchBytes);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"New BGME_Config Patch Created! File: {newPatchFile}");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Problem rebuilding BGME_Config patch!");
                Console.ReadLine();
            }
        }
    }
}
