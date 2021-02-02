using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace P4G_Encount_Music_Editor
{
    enum GameTitle
    {
        P4G,
        P5
    }

    readonly struct GameProps
    {
        public GameTitle Name { get; }
        public string EncountFile { get; }
        public bool IsBigEndian { get; }
        public int EntrySize { get; }
        public int StartingOffset { get; }
        public string PackageFolder { get; }
        public string TblPatchesFolder { get; }
        public string PatchesFolder { get; }

        public GameProps(GameTitle game)
        {
            string currentDir = Directory.GetCurrentDirectory();
            Name = game;
            EncountFile = $@"{currentDir}\{game}\ENCOUNT.TBL";
            IsBigEndian = game == GameTitle.P5;
            EntrySize = 24;
            StartingOffset = 22;

            PackageFolder = game == GameTitle.P4G ? $@"{currentDir}\BGME Config Package" : $@"{currentDir}\Encount Music Package";
            TblPatchesFolder = $@"{PackageFolder}\tblpatches";
            PatchesFolder = $@"{PackageFolder}\patches";
        }

        public int TotalEncounters()
        {
            return Name switch
            {
                GameTitle.P4G => 944,
                GameTitle.P5 => 1000,
                _ => 0,
            };
        }
    }

    struct Encounter
    {
        public byte[] Flags { get; set; }
        public ushort Field04 { get; set; }
        public ushort Field06 { get; set; }
        public ushort[] Units { get; set; }
        public ushort FieldId { get; set; }
        public ushort RoomId { get; set; }
        public ushort MusicId { get; set; }
    }
}
