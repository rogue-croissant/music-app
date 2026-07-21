using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class CurrentTrackResponse
    {
        [JsonPropertyName("item")]
        public Tracks Item { get; set; }

        [JsonPropertyName("is_playing")]
        public bool IsPlaying { get; set; }

        [JsonPropertyName("progress_ms")]
        public int Progress { get; set; }
    }
}
