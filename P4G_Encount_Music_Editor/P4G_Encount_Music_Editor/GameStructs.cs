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
        public GameTitle Game { get; }
        public string EncountFile { get; }
        public bool IsBigEndian { get; }

        public GameProps(GameTitle game)
        {
            string currentDir = Directory.GetCurrentDirectory();
            Game = game;
            EncountFile = $@"{currentDir}\{game}\ENCOUNT.TBL";
            IsBigEndian = game == GameTitle.P5;
        }

        public int TotalEncounters()
        {
            return Game switch
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
