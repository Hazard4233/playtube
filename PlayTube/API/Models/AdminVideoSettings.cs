using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace PlayTube.API.Models
{
    public partial class AdminVideoSettings
    {
        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("videos_data")]
        public UserSettingVideo[] VideosData { get; set; }
    }

    public partial class UserSettingVideo
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("rvs_id")]
        public int RvsId { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }
}