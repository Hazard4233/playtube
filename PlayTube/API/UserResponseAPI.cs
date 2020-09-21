using Newtonsoft.Json;
using PlayTube.Activities.UserReponse;
using PlayTube.API.Models;
using PlayTube.Helpers.Models;
using PlayTubeClient;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayTube.API
{
    public class UserResponseURL
    {
        public static string BaseURL = "https://fanstory.tv/video-response/video-response-files/server/api/";
        public static string UploadVideoResponseURL = BaseURL + "upload_video.php";
        public static string UploadAudioResponseURL = BaseURL + "upload_audio.php";
        public static string GetUserResponseURL = BaseURL + "get_response_videos.php";
        public static string ViewVideoResponseURL = BaseURL + "view_video_response.php";
        public static string ShareVideoResponseURL = BaseURL + "share_video_response.php";
        public static string AdminVideoResponseURL = BaseURL + "get_admin_response_videos.php";
        public static string FollowingVideoResponseURL = BaseURL + "get_following_response_videos.php";
        public static string ForYouVideoResponseURL = BaseURL + "get_foryou_response_videos.php";
        public static string AdminUserSettingURL = BaseURL + "get_response_videos_setting.php";
        public static string ApproveUserSettings = BaseURL + "toggle_video_response_setting.php";
        public static string AcceptRejectLatestResponse = BaseURL + "rv_toggle_status.php";
        public static string DeleteLatestResponse = BaseURL + "delete_response_video.php";

        public static string UnseenResponseURL = BaseURL + "get_admin_response_videos_notifications.php";

        public static string DirectoryURL = "https://fanstory.tv/video-response/content/";
        public static string AvtaarURL = "https://fanstory.tv/upload/photos/d-avatar.jpg";

        public static string DirectoryUserSettings = "https://fanstory.tv/";
    }
    public class UserResponseAPI
    {
        public static async Task<string> UploadUserResponse(string videoId, byte[] uploadFileBinary,
            InputSelectedType selectedType, string Description, string thumbNail = null)
        {
            string msg = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        var values = new[]
                        {
                            new KeyValuePair<string, string>("video_id", videoId),
                            new KeyValuePair<string, string>("session_id", UserDetails.AccessToken),
                            new KeyValuePair<string, string>("inputType", selectedType == InputSelectedType.Selected ? "uploaded-video" : "recorded-video"),
                            new KeyValuePair<string, string>("description", Description)
                        };
                        foreach (var keyValuePair in values)
                        {
                            content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
                        }
                        var fileContent = new StreamContent(new MemoryStream(uploadFileBinary));
                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = selectedType == InputSelectedType.Selected ? "uploadedVideo": "recordedVideo",
                            FileName = "test.mp4"
                        };
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        content.Add(fileContent, selectedType == InputSelectedType.Selected ? "uploadedVideo" : "recordedVideo");
                        client.Timeout = Timeout.InfiniteTimeSpan;
                        var result = await client.PostAsync(UserResponseURL.UploadVideoResponseURL, content);
                        var response = await result.Content.ReadAsStringAsync();
                        var responseModel = JsonConvert.DeserializeObject<ResponseModel>(response);
                        msg = responseModel.msg;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = "Something went wrong, please try again later";
            }
            return msg;
        }
        public static async Task<string> UploadAudioUserResponse(string videoId, byte[] uploadFileBinary,
            InputSelectedType selectedType, string Description, string thumbNail = null)
        {
            string msg = string.Empty;
            try
            {
                var filename = "test.mp3";
                var client = new RestClient(UserResponseURL.UploadAudioResponseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("video_id", videoId);
                request.AddParameter("session_id", UserDetails.AccessToken);
                request.AddParameter("description", Description);
                request.AddFileBytes("audio", uploadFileBinary, filename, "application/octet-stream");
                if (selectedType == InputSelectedType.Selected)
                    request.AddParameter("inputType", "uploaded-audio");
                else
                    request.AddParameter("inputType", "recorded-audio");
                IRestResponse response = await client.ExecuteAsync(request);
                var responseModel = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
                if (responseModel != null)
                    msg = responseModel.msg;
            }
            catch (Exception ex)
            {
                msg = "Something went wrong, please try again later";
            }
            return msg;
        }
        public static async Task<List<RvDatum>> GetUserResponse(string videoId)
        {
            List<RvDatum> dataList = new List<RvDatum>();
            try
            {
                var client = new RestClient(UserResponseURL.GetUserResponseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("video_id", videoId);
                if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                    request.AddParameter("session_id", UserDetails.AccessToken);
                IRestResponse response = client.Execute(request);
                var responseModel = JsonConvert.DeserializeObject<UserResponseModel>(response.Content);
                dataList = responseModel.RvData.ToList();
            }
            catch (Exception ex)
            {

            }
            return dataList;
        }

        public static string ViewVideoResponse(string rv_id)
        {
            try
            {
                var client = new RestClient(UserResponseURL.ViewVideoResponseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("response_video_id", rv_id);
                request.AddParameter("session_id", UserDetails.AccessToken);
                IRestResponse response = client.Execute(request);
                var responseModel = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
                return responseModel.msg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static string ShareVideoResponse(string rv_id)
        {
            try
            {
                var client = new RestClient(UserResponseURL.ShareVideoResponseURL);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddParameter("response_video_id", rv_id);
                //request.AddParameter("session_id", UserDetails.AccessToken);
                IRestResponse response = client.Execute(request);
                var responseModel = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
                return responseModel.msg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static AdminVideoResponseModel GetAdminVideos()
        {
            var client = new RestClient(UserResponseURL.AdminVideoResponseURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = client.Execute(request);
            var reponseData = JsonConvert.DeserializeObject<AdminVideoResponseModel>(response.Content);
            return reponseData;
        }

        public static async Task<AdminVideoResponseModel> GetFollowingResponseVideos()
        {
            var client = new RestClient(UserResponseURL.FollowingVideoResponseURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = await client.ExecuteAsync(request);
            var reponseData = JsonConvert.DeserializeObject<AdminVideoResponseModel>(response.Content);
            return reponseData;
        }
        public static async Task<AdminVideoResponseModel> GetForYouResponseVideos()
        {
            var client = new RestClient(UserResponseURL.ForYouVideoResponseURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                request.AddParameter("user_id", UserDetails.UserId);
            //request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = await client.ExecuteAsync(request);
            var reponseData = JsonConvert.DeserializeObject<AdminVideoResponseModel>(response.Content);
            return reponseData;
        }
        public static AdminVideoSettings GetAdminUserSettings()
        {
            var client = new RestClient(UserResponseURL.AdminUserSettingURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = client.Execute(request);
            var reponseData = JsonConvert.DeserializeObject<AdminVideoSettings>(response.Content);
            return reponseData;
        }

        public static string ApproveUserSettings(int rsvId)
        {
            var client = new RestClient(UserResponseURL.ApproveUserSettings);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("id", rsvId);
            request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = client.Execute(request);
            var reponseData = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
            return reponseData.msg;
        }

        public static int AcceptRejectResponse(int userResponseId)
        {
            var client = new RestClient(UserResponseURL.AcceptRejectLatestResponse);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("id", userResponseId);
            request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = client.Execute(request);
            var reponseData = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
            return reponseData.response;
        }
        public static async Task<string> DeleteResponse(int userResponseId)
        {
            var client = new RestClient(UserResponseURL.DeleteLatestResponse);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("id", userResponseId);
            request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = await client.ExecuteAsync(request);
            var reponseData = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
            return reponseData.msg;
        }


        public static int GetUnseenResponseNotification()
        {
            var client = new RestClient(UserResponseURL.UnseenResponseURL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("session_id", UserDetails.AccessToken);
            IRestResponse response = client.Execute(request);
            var reponseData = JsonConvert.DeserializeObject<ResponseModel>(response.Content);
            return reponseData.response;
        }
    }
}