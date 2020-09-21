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
    public partial class AdminVideoResponseModel
    {
        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("video_response")]
        public AdminVideoResponse[] VideoResponse { get; set; }
    }

    public partial class AdminVideoResponse
    {
        [JsonProperty("user_id")]
        public string UserID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("video_location")]
        public string VideoLocation { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("video_id")]
        public string VideoId { get; set; }

        [JsonProperty("likes", NullValueHandling = NullValueHandling.Ignore)]
        public string Likes { get; set; }

        [JsonProperty("is_liked", NullValueHandling = NullValueHandling.Ignore)]
        public string IsLiked { get; set; }

        [JsonProperty("shares", NullValueHandling = NullValueHandling.Ignore)]
        public string Shares { get; set; }

        [JsonProperty("comments", NullValueHandling = NullValueHandling.Ignore)]
        public string Comments { get; set; }
    }
}