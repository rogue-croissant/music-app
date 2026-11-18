using AnimatedImage.Wpf;
using Microsoft.Web.WebView2.Core;
using Music_app.MVVM.Model;
using Music_app.MVVM.Services;
using Music_app.MVVM.ViewModel;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
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
using System.Windows.Threading;



namespace Music_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Playlist currentPlaylist;
        private MainViewModel viewModel;
        public SpotifyApiServices apiService;
        private DispatcherTimer playerTimer;
        private bool updatingTrack = false;
        private string? currentTrackId = null;
        private bool userDragging = false;
        private bool seeking = false;
        private bool isSpotify = false;

        private int myVolume = 0;
        private bool updatingVolume = false;

        private string mySongName;
        private string myArtistName;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            DataContext = viewModel;

            Loaded += MainWindow_Loaded;

            songProgress.IsEnabled = false;
            btnBackwardSkip.IsEnabled = false;
            btnForwardSkip.IsEnabled = false;
            btnPauseAndPlay.IsEnabled = false;


            //code must be moved so that it only plays when the user is logged in and playing a song, otherwise it will be a static image
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/MVVM/View/procreateIcons/Animate.gif", UriKind.RelativeOrAbsolute);
            image.EndInit();
            //    ImageBehavior.SetAnimatedSource(themeArt, image);

            rbtnEnglish.IsChecked = true;
            rbtnSpotify.IsChecked = true;
            isSpotify = true;
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

        private async void btnPauseAndPlay_Click(object sender, RoutedEventArgs e)
        {
          
 
            await SpotifyPlayer.ExecuteScriptAsync("togglePlayer();");

            CurrentTrackResponse current = await apiService.GetCurrentTrackAsync();

            btnPauseAndPlay.Tag = current.IsPlaying ? FindResource("PlayIcon") : FindResource("PauseIcon");

        }
        private async void btnForwardSkip_Click(object sender, RoutedEventArgs e)
        {
            await SpotifyPlayer.ExecuteScriptAsync("forwardSkip();");
        }
        private async void btnBackwardSkip_Click(object sender, RoutedEventArgs e)
        {
            await SpotifyPlayer.ExecuteScriptAsync("backwardSkip();");
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
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

                if (isSpotify)
                {
                    rbtnYouTube.IsEnabled = false;
                    SpotifyAuthServices authService = new SpotifyAuthServices();

                    string accessToken = await authService.AuthenticateUserAsync();
                    await SpotifyPlayer.ExecuteScriptAsync($"window.accessToken='{accessToken}'; createPlayer();");
                    Clipboard.SetText(accessToken);
                    btnLogIn.Visibility = Visibility.Collapsed;
                    btnLogOut.Visibility = Visibility.Visible;

                    apiService = new SpotifyApiServices(accessToken);

                    List<Tracks> userTrack = await apiService.GetTrackAsync();
                    List<Playlist> userPlaylist = await apiService.GetPlaylistAsync();
                    UserProfile profile = await apiService.GetUserProfileAsync();

                    lblUsername.Content = profile.Name;


                    viewModel.Tracks.Clear();

                    foreach (Tracks tracks in userTrack)
                    {
                        viewModel.Tracks.Add(tracks);

                    }

                    foreach (Playlist playlist in userPlaylist)
                    {
                        viewModel.Playlists.Add(playlist);
                    }
                    string deviceId = "";

                    while (string.IsNullOrWhiteSpace(deviceId))
                    {
                        await Task.Delay(200);

                        deviceId = await SpotifyPlayer.ExecuteScriptAsync("window.deviceId");
                        deviceId = deviceId.Trim('"');
                    }

                    bool foundDevice = false;
                    int myCount = 0;
                   
                    while (!foundDevice)
                    {
                        var devices = await apiService.GetDevicesAsync();

                        foundDevice = devices.Any(d => d.Id == deviceId);
                        myVolume = devices[1].VolumePercentage;
                        MessageBox.Show("volume in initial: " + myVolume.ToString());
                        
                        if (!foundDevice) { 
                            await Task.Delay(300);
                            myCount++;
                        }
                    }

                    // Check the playback state BEFORE transferring
                    CurrentTrackResponse? state = await apiService.GetCurrentTrackAsync();
                    bool wasPlaying = state?.IsPlaying ?? false;

                    // Transfer playback
                    await apiService.PutTransferPlaybackAsync(deviceId);
                    await UpdateCurrentVolume(myVolume);
                    updateVolumeSlider(myVolume);
                    // If Spotify was paused, keep your player paused too
                    if (!wasPlaying)
                    {
                        await SpotifyPlayer.ExecuteScriptAsync("window.player.pause();");
                    }

                    await UpdateCurrentTrackUi();
                    StartPlayerTimer();

                    btnPlaylists.Visibility = Visibility.Visible;
                    btnVolume.Visibility = Visibility.Visible;

                    lblPlayerName.Margin = new Thickness(0, 0, 85, 0);

                    songProgress.IsEnabled = true;
                    btnBackwardSkip.IsEnabled = true;
                    btnForwardSkip.IsEnabled = true;
                    btnPauseAndPlay.IsEnabled = true;
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
        private async void btnLogOut_Click(object sender, RoutedEventArgs e)
        {

            playerTimer.Stop();
            await SpotifyPlayer.ExecuteScriptAsync("logoutPlayer()");

            apiService = null;
            currentTrackId = null;

            viewModel.PlaylistTracks.Clear();
            viewModel.Playlists.Clear();
            viewModel.Tracks.Clear();



            clearLogin();
            
           
            songProgress.IsEnabled = false;
            btnBackwardSkip.IsEnabled = false;
            btnForwardSkip.IsEnabled = false;
            btnPauseAndPlay.IsEnabled = false;

            btnPlaylists.Visibility = Visibility.Collapsed;
            btnLogIn.Visibility = Visibility.Visible;
            btnLogOut.Visibility = Visibility.Collapsed;
            
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           Playlist selectedPlaylist = (Playlist)((ListView)sender).SelectedItem;
            if (selectedPlaylist == null)
                return;
            
            popupTracks.IsOpen = false;
            currentPlaylist = selectedPlaylist;
            List<Tracks> playlistTracks= await apiService.GetPlaylistTracksAsync(selectedPlaylist.Id);
            viewModel.PlaylistTracks.Clear();
            foreach (Tracks track in playlistTracks)
            {
                viewModel.PlaylistTracks.Add(track);
            }

            await Task.Delay(20);

            popupTracks.IsOpen = true;

        }

        public void UpdateSongInformation(Tracks current)
        {
            string artist = "";
            if (current == null) return;

          //  lblSongName.Content = current.Name;
            mySongName = current.Name;
    
            for (int i = 0; i < current.Artists.Count; i++)
            {
                if (i == 0)
                {
                    artist = current.Artists[i].Name;
                }
                else
                {
                    artist = artist + ", " + current.Artists[i].Name;
                }
            }

           //lblAuthor.Content = artist;
            myArtistName = artist;
            myContent.Text = (String)FindResource("NowPlaying");
            txtArtist.Text = artist;
            txtSongName.Text = mySongName;
            if (current.Album.Images.Count > 0)
            {
                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(current.Album.Images[0].Url);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
               

                imgAlbumArt.ImageSource = bitmap;
            }
        }
        public void updateSongProgress(CurrentTrackResponse current)
        {
          /*   BitmapImage myBitmap = new BitmapImage();
             myBitmap.BeginInit();
            myBitmap.StreamSource = File.OpenRead(@"C:\Users\USER-PC\source\repos\music app\Music app\MVVM\View\procreateIcons\cdPlayer1.png");
            myBitmap.CacheOption = BitmapCacheOption.OnLoad;
         
                myBitmap.EndInit();*/

            
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(@"C:\Users\USER-PC\source\repos\music app\Music app\MVVM\View\procreateIcons\Cd_Animation.gif");
            image.EndInit();
            ImageBehavior.SetAnimatedSource(imgPlayerCdState, image);
         //   imgPlayerCdState.Source = myBitmap;


            TimeSpan duration = TimeSpan.FromMilliseconds(current.Item.DurationMs);
            TimeSpan progress = TimeSpan.FromMilliseconds(current.Progress);
            lblDuration.Content = duration.ToString(@"mm\:ss");
            lblProgress.Content = progress.ToString(@"mm\:ss");

            songProgress.Minimum = 0;
            songProgress.Maximum = current.Item.DurationMs;

           if (!userDragging)
            {
                songProgress.Value = current.Progress;
            }
          
                userDragging = false;
               
        }
        public async Task UpdateCurrentVolume(int volume) {
            
            if (updatingVolume)
                return;
            try
            {
                updatingVolume = true;
               
                MessageBox.Show("volume in updateCurrentVolume(): " + volume);
               
                if (volumeSlider.Value != volume)
                {
                    _ = apiService.PutSeekVolumeAsync((int)volumeSlider.Value);
                   
                }
                
            }
            catch
            {
                MessageBox.Show("Loser");

            }
            finally
            {
                updatingVolume = false;
            }
            
            await Task.Delay(2000);
        }

        public void updateVolumeSlider(int currentVolume)
        {
            if (!userDragging)
            {
                volumeSlider.Value = currentVolume;

            }
            userDragging = false;
            
        }
        private async Task UpdateCurrentTrackUi()
        {
            if (updatingTrack)
                return;

            try { 
                updatingTrack = true;

            CurrentTrackResponse? current = await apiService.GetCurrentTrackAsync();

                if (current == null)
                {
                    return;
                }
                if (current.Item.Id != currentTrackId)
                {
                    currentTrackId = current.Item.Id;
                    UpdateSongInformation(current.Item);
                }

                if (seeking)
                    return;

                updateSongProgress(current);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                updatingTrack = false;
            }
        }

        private void StartPlayerTimer()
        {
            playerTimer = new DispatcherTimer();

            playerTimer.Interval = TimeSpan.FromSeconds(1);

            playerTimer.Tick += async (s, e) =>
            {
                await UpdateCurrentTrackUi();
            };

            playerTimer.Start();
        }
        private async void lvPlaylistTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Tracks selectedTrack = (Tracks)((ListView)sender).SelectedItem;
            string artist = "";
            int selectedIndex = lvPlaylistTracks.SelectedIndex;

           
            if (selectedTrack == null)
                return;

            try
            {
                await SpotifyPlayer.ExecuteScriptAsync($"playPlaylist('{currentPlaylist.Uri}', {selectedIndex});");
                await Task.Delay(1000);
                await UpdateCurrentTrackUi();
 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            userDragging = true;
        }

        private async void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            seeking = true;
            await apiService.SeekPlaybackAsync((int)songProgress.Value);
            await Task.Delay(1000);

            seeking = false;
            
        }

        private void popupTracks_Closed(object sender, EventArgs e)
        {
            lvPlaylist.SelectedItem = null;
        }

        private void btnEnglish_Click(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary(); 

            if (myContent.Text != (String)FindResource("Connect"))
            {
                myContent.Text = (String)FindResource("NowPlaying");
            }
        }
        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (rbtnAfrikaans.IsChecked)
            {
                case false:
                    dict.Source = new Uri("..\\Resources\\EnglishStringResource.xaml", UriKind.Relative);
                    break;
                case true:
                    dict.Source = new Uri("..\\Resources\\AfrikaansStringResource.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\EnglishStringResource.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }

        private void btnAfrikaans_Click(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary();
            if (myContent.Text != (String)FindResource("Connect"))
            {
                myContent.Text = (String)FindResource("NowPlaying");
            }
        }

        private void rbtnSpotify_Click(object sender, RoutedEventArgs e)
        {
            isSpotify = true;
        }

        private void rbtnYouTube_Click(object sender, RoutedEventArgs e)
        {
            isSpotify = false;
        }

        private void clearLogin()
        {
           
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri("C:\\Users\\USER-PC\\source\\repos\\music app\\Music app\\MVVM\\View\\Uramichi oniisan KOTORI.jpg", UriKind.RelativeOrAbsolute);
            image.EndInit();
            imgAlbumArt.ImageSource = image;
           


            myContent.Text = (String)FindResource("Connect");
            txtSongName.Text = (String)FindResource("SongName");
            txtArtist.Text = (String)FindResource("Artist");
            lblProgress.Content = "00:00";
            lblDuration.Content = "00:00";
            songProgress.Value = 0;
            btnPlaylists.Visibility = Visibility.Collapsed;
            lblPlayerName.Margin = new Thickness(0, 0, 120, 0);
        }

        private async void VolumeSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            seeking = true;
            await UpdateCurrentVolume((int)volumeSlider.Value);
            await Task.Delay(500);

            seeking = false;
        }
    }
}
