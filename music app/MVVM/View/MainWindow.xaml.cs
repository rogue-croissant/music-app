using Microsoft.Web.WebView2.Core;
using Music_app.MVVM.Model;
using Music_app.MVVM.Services;
using Music_app.MVVM.ViewModel;
using SpotifyAPI.Web.Auth;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private string username = "user1";
        private MainViewModel viewModel;
        public SpotifyApiServices apiService;
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
            this.Closing += MainWindow_Closing;

            viewModel = new MainViewModel();
            DataContext = viewModel;

            Loaded += MainWindow_Loaded;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await SpotifyPlayer.EnsureCoreWebView2Async();

            SpotifyPlayer.NavigationCompleted += SpotifyPlayer_NavigationCompleted;

            string path = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "WebPlayer",
                "index.html");

            SpotifyPlayer.CoreWebView2.Navigate(
                new Uri(path).AbsoluteUri);
        }
        private async void SpotifyPlayer_NavigationCompleted(object sender,CoreWebView2NavigationCompletedEventArgs e)
        {
            string html = await SpotifyPlayer.ExecuteScriptAsync("document.documentElement.outerHTML");
            
            string scripts = await SpotifyPlayer.ExecuteScriptAsync(@"Array.from(document.scripts).map(s => s.src).join('\n');");
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string msg = "Do you look like a twat?";
            MessageBoxResult result = MessageBox.Show(msg, "my app", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
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

            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }

        }

        private async void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SpotifyAuthServices authService = new SpotifyAuthServices();
               
                string accessToken = await authService.AuthenticateUserAsync();
                await SpotifyPlayer.ExecuteScriptAsync($"window.accessToken='{accessToken}'; createPlayer();");

                btnLogIn.Visibility = Visibility.Collapsed;
                btnLogOut.Visibility = Visibility.Visible;

                apiService = new SpotifyApiServices(accessToken);
           
                List<Tracks> userTrack = await apiService.GetTrackAsync();
              
                List<Playlist> userPlaylist = await apiService.GetPlaylistAsync();
                

                viewModel.Tracks.Clear();

                foreach (Tracks tracks in userTrack)
                {
                    viewModel.Tracks.Add(tracks);

                }

                foreach (Playlist playlist in userPlaylist)
                {
                    viewModel.Playlists.Add(playlist);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
         
            Playlist selectedPlaylist = (Playlist)((ListView)sender).SelectedItem;
            if (selectedPlaylist == null)
                return;
            
            popupTracks.IsOpen = true;
           
            List<Tracks> playlistTracks= await apiService.GetPlaylistTracksAsync(selectedPlaylist.Id);
            viewModel.PlaylistTracks.Clear();
            foreach (Tracks track in playlistTracks)
            {
                viewModel.PlaylistTracks.Add(track);
            }

        }

        private async void lvPlaylistTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tracks selectedTrack = (Tracks)((ListView)sender).SelectedItem;
            string artist = "";
            
            if (selectedTrack == null)
                return;

            try
            {
             await SpotifyPlayer.ExecuteScriptAsync($"playTrack('{selectedTrack.Uri}')");
                lblSongName.Content = selectedTrack.Name;
                artist = "";
                for (int i = 0; i < selectedTrack.Artists.Count; i++)
                {
                    if (i == 0)
                    {
                        artist = selectedTrack.Artists[i].Name;
                    }
                    else
                    {
                        artist = artist + ", " + selectedTrack.Artists[i].Name;
                    }
                }

                lblAuthor.Content = artist;

                if (selectedTrack.Album.Images.Count > 0)
                {
                    BitmapImage bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedTrack.Album.Images[0].Url);
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmap.CacheOption = BitmapCacheOption.OnDemand;
                    bitmap.EndInit();

                    imgAlbumArt.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
