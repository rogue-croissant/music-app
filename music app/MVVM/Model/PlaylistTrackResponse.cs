using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class PlaylistTrackResponse
    {
        [JsonPropertyName("items")]
        public List<PlaylistItem> Items { get; set; }
    }
}
