using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class PlaylistItem
    {
        [JsonPropertyName("item")]
        public Tracks ItemPlaylist { get; set; }
}
}
