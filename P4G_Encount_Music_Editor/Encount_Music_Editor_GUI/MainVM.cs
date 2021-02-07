using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encount_Music_Editor_GUI
{
    public class MainVM
    {
        private EncountItemsList _encountItemsList = new EncountItemsList();
        public EncountItemsList EncountItemsCollection { get => _encountItemsList; }

        public void LoadPreset(string fileName)
        {
            Trace.WriteLine($"{fileName}");
        }
    }
}
