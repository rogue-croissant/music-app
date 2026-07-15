using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class PlaylistResponse
    {
        [JsonPropertyName("items")]
        public List<Playlist> Items { get; set; }
    }
}
