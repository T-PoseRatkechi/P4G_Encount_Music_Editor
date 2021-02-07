using System;
using System.Collections.Generic;
using System.IO;

namespace MusicEditorLibrary.Presets
{
    public enum CommandType
    {
        DirectEdit,
        RandomSet,
        AdvantageSet
    }

    public struct PresetCommand
    {
        public CommandType Type { get; }
        public string Name { get; }
        public ushort[] Values { get; }

        public PresetCommand(CommandType type, string name, ushort[] values)
        {
            Type = type;
            Name = name;
            Values = values;
        }
    }

    public static class PresetUtils
    {
        public static PresetCommand[] GetPresetCommands(string presetFile)
        {
            string[] presetLines = File.ReadAllLines(presetFile);
            List<PresetCommand> presetCommands = new List<PresetCommand>();

            foreach (string line in presetLines)
            {
                if (line.StartsWith("//") || line.Length < 3)
                    continue;

                string[] values = line.Split('=');
                string type = values[1];

                if (type.Contains("random"))
                    presetCommands.Add(new PresetCommand(CommandType.RandomSet, line, new ushort[2] { 100, 500 }));
                else if (type.Contains("advantage"))
                    presetCommands.Add(new PresetCommand(CommandType.AdvantageSet, line, new ushort[2] { 200, 250 }));
                else
                    presetCommands.Add(new PresetCommand(CommandType.DirectEdit, line, new ushort[2] { 300, 400 }));
            }

            return presetCommands.ToArray();
        }
    }
}
