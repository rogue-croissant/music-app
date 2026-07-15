using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class SavedTracks
    {
        [JsonPropertyName("added at")]
        public DateTime AddedAt { get; set; }
        [JsonPropertyName("track")]
        public Tracks Track { get; set; }
    }
}
