using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MusicEditorLibrary.Presets;

namespace Encount_Music_Editor_GUI
{
    public class MainVM
    {
        private EncountItemsList _encountItemsList = new EncountItemsList();
        public EncountItemsList EncountItemsCollection { get => _encountItemsList; }

        private EncountItem _currentEncountItem = new EncountItem();
        public EncountItem CurrentEncountItem { get => _currentEncountItem; }

        public void LoadPreset(string fileName)
        {
            Trace.WriteLine($"Loading Preset: {fileName}");
            _encountItemsList.Clear();
            PresetCommand[] presetCommands = PresetUtils.GetPresetCommands(fileName);

            foreach (PresetCommand command in presetCommands)
            {
                _encountItemsList.Add(new EncountItem(command));
            }
        }

        public void UpdateCurrentItem(ListViewItem item)
        {
            var encountItem = item.Content as EncountItem;
            if (encountItem != null)
            {
                _currentEncountItem.Name = encountItem.Name;
                _currentEncountItem.Type = encountItem.Type;
            }
        }
    }
}
