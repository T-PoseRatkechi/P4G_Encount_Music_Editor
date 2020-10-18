using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

    class MusicManagerManager
    {
        private static string currentDir = null;

        public MusicManagerManager()
        {
            currentDir = Directory.GetCurrentDirectory();
        }


    }
}
