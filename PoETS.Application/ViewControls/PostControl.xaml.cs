using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PoETS.Application.Controls {
    /// <summary>
    /// Interaction logic for PostControl.xaml
    /// </summary>
    public partial class PostControl : UserControl {
        public Uri PMLink = null;
        public Uri PostLink = null;

        public PostControl() {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void btnShowBrowser_Click(object sender, RoutedEventArgs e) {
            if (PMLink != null) {
                Process.Start(new ProcessStartInfo(PostLink.AbsoluteUri));
            }
            e.Handled = true;
        }

        private void btnPM_Click(object sender, RoutedEventArgs e) {

        }
    }
}
