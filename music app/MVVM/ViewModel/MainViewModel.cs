using Music_app.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_app.MVVM.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<Tracks> Tracks { get; set; }
        public ObservableCollection<Playlist> Playlists { get; set; }
        public ObservableCollection<Tracks> PlaylistTracks { get; set; }

        public MainViewModel()
        {
            Tracks = new ObservableCollection<Tracks>();
            Playlists = new ObservableCollection<Playlist>();
            PlaylistTracks = new ObservableCollection<Tracks>();


        }
    }
}
