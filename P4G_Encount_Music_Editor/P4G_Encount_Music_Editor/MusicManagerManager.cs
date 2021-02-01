using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace P4G_Encount_Music_Editor
{
    public class idtowaveindex
    {
        public conversionObject[] conversions { get; set; }
    }

    public class conversionObject
    {
        public string trackID { get; set; }
        public string waveIndex { get; set; }
        public string uwusID { get; set; }
    }

    public class songnames
    {
        public nameObject[] names { get; set; }
    }

    public class nameObject
    {
        public string id { get; set; }
        public string trackName { get; set; }
    }

    public class originalTracks
    {
        public trackObject[] tracks { get; set; }
    }

    public class trackObject
    {
        public string id { get; set; }
        public string fileName { get; set; }
        public string songName { get; set; }
    }


    class MusicManagerManager
    {
        private string currentDir = null;

        public MusicManagerManager()
        {
            currentDir = Directory.GetCurrentDirectory();
        }

        public void ExtendSongsList()
        {
            int newNumSongs = ConsolePrompt.PromptInt("New Total Songs");

            string originalConversionFile = $@"{currentDir}\music manager\idtowaveindex_original.json";
            string outputConversionFile = $@"{currentDir}\music manager\idtowaveindex.json";

            string originalNamesFile = $@"{currentDir}\music manager\songnames_original.json";
            string outputNamesFile = $@"{currentDir}\music manager\songnames.json";

            idtowaveindex originalConversions = ParseConversionFile(originalConversionFile);
            if (originalConversions == null)
                return;

            songnames originalNames = ParseNamesFile(originalNamesFile);
            if (originalNames == null)
                return;

            int normalNumSongs = originalConversions.conversions.Length;

            // new array of conversion objects with new total
            conversionObject[] newConversionList = new conversionObject[newNumSongs];
            nameObject[] newNamesList = new nameObject[newNumSongs];

            // copy original list to new list
            Array.Copy(originalConversions.conversions, newConversionList, originalConversions.conversions.Length);
            Array.Copy(originalNames.names, newNamesList, originalNames.names.Length);

            int startingIndex = 886;
            for (int i = normalNumSongs, total = newNumSongs, indexCounter = 0; i < total; i++, indexCounter++)
            {
                conversionObject newConversion = new conversionObject();
                newConversion.trackID = i.ToString();
                newConversion.uwusID = null;
                newConversion.waveIndex = (startingIndex + indexCounter).ToString();
                newConversionList[i] = newConversion;

                nameObject newName = new nameObject();
                newName.id = i.ToString();
                newName.trackName = $"(BGME) Song Index {newConversion.waveIndex}";
                newNamesList[i] = newName;
            }

            originalConversions.conversions = newConversionList;
            originalNames.names = newNamesList;

            string newConversionText = JsonSerializer.Serialize<idtowaveindex>(originalConversions, new JsonSerializerOptions { WriteIndented = true });
            string newNamesText = JsonSerializer.Serialize<songnames>(originalNames, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputConversionFile, newConversionText);
            File.WriteAllText(outputNamesFile, newNamesText);
        }

        private originalTracks ParseOriginalTracks(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Original tracks file missing! File: {filePath}");
                return null;
            }

            try
            {
                string tracksJsonString = File.ReadAllText(filePath);
                originalTracks originalTracksObject = JsonSerializer.Deserialize<originalTracks>(tracksJsonString);
                return originalTracksObject;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"Problem parsing original tracks file! File: {filePath}");
                return null;
            }
        }

        private songnames ParseNamesFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Song names file missing! File: {filePath}");
                return null;
            }

            try
            {
                string songnamesJsonString = File.ReadAllText(filePath);
                songnames songnamesObject = JsonSerializer.Deserialize<songnames>(songnamesJsonString);
                return songnamesObject;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"Problem parsing song names file! File: {filePath}");
                return null;
            }
        }

        private idtowaveindex ParseConversionFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Conversion file missing! File: {filePath}");
                return null;
            }

            try
            {
                string idtowaveJsonString = File.ReadAllText(filePath);
                idtowaveindex idtowaveObject = JsonSerializer.Deserialize<idtowaveindex>(idtowaveJsonString);
                return idtowaveObject;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"Problem parsing conversion file! File: {filePath}");
                return null;
            }
        }
    }
}
