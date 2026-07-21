using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class UserProfile
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("display_name")]
        public string Name { get; set; }

        [JsonPropertyName("images")]
        public List<Image> ProfileImage { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
