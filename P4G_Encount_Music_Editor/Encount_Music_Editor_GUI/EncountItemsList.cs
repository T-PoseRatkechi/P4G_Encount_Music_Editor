using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MusicEditorLibrary.Presets;

namespace Encount_Music_Editor_GUI
{
    public class EncountItemsList : ObservableCollection<EncountItem>
    {
        public EncountItemsList() : base()
        {

        }
    }

    public class EncountItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _Name = String.Empty;
        private string _Type = String.Empty;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ushort[] Values { get; set; }

        public EncountItem(PresetCommand presetCommand)
        {
            Name = presetCommand.Name;
            Type = presetCommand.Type.ToString();
            Values = presetCommand.Values;
        }

        public EncountItem()
        {
            Name = string.Empty;
            Type = string.Empty;
            Values = null;
        }
    }
}
