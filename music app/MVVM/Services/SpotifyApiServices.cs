using Music_app.MVVM.Model;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Music_app.MVVM.Services
{
    public class SpotifyApiServices
    {
        private readonly HttpClient _client;
        public string trackUrl = "https://api.spotify.com/v1/me/tracks";
        public string playlistUrl = "https://api.spotify.com/v1/me/playlists";
        public string currentTrackUrl = "https://api.spotify.com/v1/me/player/currently-playing";


        public SpotifyApiServices(string accessToken)//my constructor to recive my access token
        {
            _client = new HttpClient();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public async Task<List<Tracks>> GetTrackAsync()
        {
            HttpResponseMessage response = await _client.GetAsync(trackUrl);

            response.EnsureSuccessStatusCode();//If the status code isn't in the 200 range, throw an exception immediately.

            string json = await response.Content.ReadAsStringAsync();
            

            SavedTrackResponse result = JsonSerializer.Deserialize<SavedTrackResponse>(json);

            List<Tracks> tracks = new List<Tracks>();

            foreach (SavedTracks item in result.Items)  //you can write this foreach loop as a LINQ query
            {                                           //return result.Items
                tracks.Add(item.Track);                 // .Select(item=> item.Track)
            }                                           //.ToList();
                                                      
                return tracks;                        

        }

        public async Task<List<Playlist>> GetPlaylistAsync()
        {
            HttpResponseMessage response = await _client.GetAsync(playlistUrl);

            response.EnsureSuccessStatusCode();//If the status code isn't in the 200 range, throw an exception immediately.

            string json = await response.Content.ReadAsStringAsync();
            

            PlaylistResponse result = JsonSerializer.Deserialize<PlaylistResponse>(json);

            return result.Items;

        }

        public async Task<List<Tracks>> GetPlaylistTracksAsync(string playlistId)
        {
            string playlistTrackUrl = $"https://api.spotify.com/v1/playlists/{playlistId}/items";
          
            HttpResponseMessage response = await _client.GetAsync(playlistTrackUrl);

            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            PlaylistTrackResponse result = JsonSerializer.Deserialize<PlaylistTrackResponse>(json);

            List<Tracks> tracks = new List<Tracks>();

            foreach (PlaylistItem item in result.Items)
            {
                tracks.Add(item.ItemPlaylist);
            }

            return tracks;
        }

        public async Task<Tracks> GetCurrentTrackAsync()
        {
            HttpResponseMessage respose = await _client.GetAsync(currentTrackUrl);
            respose.EnsureSuccessStatusCode();

            string json = await respose.Content.ReadAsStringAsync();

            CurrentTrackResponse result = JsonSerializer.Deserialize<CurrentTrackResponse>(json);

            return result.item;
        }
    }
    
}