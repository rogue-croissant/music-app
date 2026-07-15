using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class Tracks
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("duration_ms")]
        public int DurationMs { get; set; }

        [JsonPropertyName("artists")]
        public List<Artist> Artists { get; set; }

        [JsonPropertyName("album")]
        public Album Album { get; set; }

        [JsonPropertyName("uri")]
        public string Uri { get; set; }


    }
}
