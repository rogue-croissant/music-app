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
        public Tracks item { get; set; }

        [JsonPropertyName("actions")]
        public Actions actions { get; set; }

        [JsonPropertyName("is_playing")]
        public bool isPlaying { get; set; }

        [JsonPropertyName("progress_ms")]
        public int progress { get; set; }
    }
}
