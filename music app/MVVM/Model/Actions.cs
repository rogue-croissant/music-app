using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class Actions
    {
        [JsonPropertyName("interrupting_playback")]
        public bool interrupting_playback {  get; set; }

        [JsonPropertyName("pausing")]
        public bool pausing { get; set; }

        [JsonPropertyName("resuming")]
        public bool resuming { get; set; }

        [JsonPropertyName("seeking")]
        public bool seeking { get; set; }

        [JsonPropertyName("skipping_next")]
        public bool skipping_next { get; set; }

        [JsonPropertyName("skipping_prev")]
        public bool skipping_prev { get; set; }

        [JsonPropertyName("toggling_repeat_context")]
        public bool toggling_repeat_context { get; set; }

        [JsonPropertyName("toggling_shuffle")]
        public bool toggling_shuffle { get; set; }

        [JsonPropertyName("toggling_repeat_track")]
        public bool toggling_repeat_track { get; set; }

        [JsonPropertyName("transferring_playback")]
        public bool transferring_playback { get; set; }

    }
}
