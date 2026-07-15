using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class Album
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("images")]
        public List<Image> Images { get; set; }
       
    }
}
