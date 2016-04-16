using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AutoUpdaterForWPF
{
    /// <summary>
    /// Interaction logic for AutoUpdaterWindow.xaml
    /// </summary>
    public partial class AutoUpdaterWindow : Window
    {
        public AutoUpdaterWindow(string appName, VersionData proposedVersion)
        {
            InitializeComponent();

            DataContext = proposedVersion;
            Browser.Source = new Uri(proposedVersion.ReleaseNotes);
            Blurb.Content = "An update to " + appName + " is available:";
        }

        private VersionData ViewModel
        {
            get { return DataContext as VersionData; }
        }

        private void update_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void notThisTime_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
