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
            // bit shift left to make room for advantage bit flag
            ushort maxValue = (ushort)(maxIndex << 1);

            // array to contain set's min and max value, adding 1 if advantage flag true
            ushort[] setValues = new ushort[] { minIndex, (ushort)(isAdvantage ? (maxValue + 1) : (maxValue)) };

            // see if range already exists in dictionary
            // TODO: Possibly test if this causes problems with similar indexes. Shouldn't since +1 is given to advantage indexes
            KeyValuePair<ushort, ushort[]> dictionaryMatch = randSets.FirstOrDefault(sets => sets.Value.SequenceEqual(setValues));

            // add entry for index range to dictionary
            if (dictionaryMatch.Value == null)
            {
                // limit of 47 set values
                if (randSets.Count < 48)
                {
                    int totalSets = randSets.Count;
                    randomSetIndex = (ushort)(8192 + totalSets);
                    randSets.Add(randomSetIndex, setValues);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Set Created:");
                    Console.ResetColor();
                    Console.WriteLine($"SetID: {totalSets} | MinValue: {minIndex} | MaxValue: {maxIndex}) | WaveIndex: {randomSetIndex}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Max 47 sets limit reached! Defaulting to first set!");
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

        public ushort[] GetSetByKey(ushort key)
        {
            if (randSets.ContainsKey(key))
            {
                return randSets[key];
            }
            else
            {
                Console.WriteLine($"Key: {key} not found in list of sets!");
                return null;
            }
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
            string newPatchFile = $@"{currentDir}\BGME Config Package\patches\BGME_Config.patch";

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
                Console.WriteLine($"New BGME_Config Patch Created!\nFile: {newPatchFile}");
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
