using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Encount_Music_Editor_GUI
{

    public class EncountItemsList : ObservableCollection<EncountItem>
    {
        public EncountItemsList() : base()
        {
            Add(new EncountItem("Test Name", "Random"));
            Add(new EncountItem("Test Name", "Random"));
            Add(new EncountItem("Test Name", "Random"));
        }
    }

    public class EncountItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public string Type { get; set; }

        public EncountItem(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}
