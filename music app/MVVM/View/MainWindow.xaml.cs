using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Music_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string msg = "Do you look like a twat?";
            MessageBoxResult result = MessageBox.Show(msg, "my app", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if(result==MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            MessageBox.Show("bitch ass");
        }
        private void btnPauseAndPlay_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Pause and Play", "my app", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void btnForwardSkip_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Forward", "my app", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void btnBackwardSkip_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Backward", "my app", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState==WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnplaylist_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Playlist button eish", "my app", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
          
               if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
                
            
                
        }
    }
}