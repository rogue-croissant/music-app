using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Music_app.MVVM.Model
{
    public class MyDeviceResponse
    {
        [JsonPropertyName("devices")]
        public List<MyDevice> Devices { get; set; }
    }

}
