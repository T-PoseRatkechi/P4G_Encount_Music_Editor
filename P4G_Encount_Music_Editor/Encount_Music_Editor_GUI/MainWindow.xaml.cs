using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Encount_Music_Editor_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainVM mainVM = new MainVM();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainVM;
        }

        private void MenuLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".preset";
            dlg.Filter = "Preset(*.preset)|*.preset";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string fileName = dlg.FileName;
                mainVM.LoadPreset(fileName);
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                mainVM.UpdateCurrentItem(item);
            }
        }
    }
}
