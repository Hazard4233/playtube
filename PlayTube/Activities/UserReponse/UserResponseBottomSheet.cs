using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.UserReponse;

namespace PlayTube.Activities.UserReponse
{
    public enum UserResponse
    {
        Audio,
        Video
    }
    public enum InputSelectedType
    {
        Selected,
        Recorded
    }
    public class UserResponseBottomSheet : BottomSheetDialogFragment
    {
        const string camPermission = Manifest.Permission.Camera;
        const string micPermission = Manifest.Permission.RecordAudio;
        public static UserResponse userResponse { get; set; }

        private TextView txtLocalStorage, txtRecordLive, txtGalary;

        public PlayTube.Helpers.Controller.VideoController VideoController;
        IYouTubePlayer YoutubePlayer;
        public static string videoId { get; set; }

        public UserResponseFragment ResponseFragment;

        public UserResponseBottomSheet(UserResponse response, string _videoId, PlayTube.Helpers.Controller.VideoController _videoController, UserResponseFragment _responseFragment
            , IYouTubePlayer youtubePayer = null)
        {
            userResponse = response;
            VideoController = _videoController;
            YoutubePlayer = youtubePayer;
            videoId = _videoId;
            ResponseFragment = _responseFragment;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                //    View view = inflater.Inflate(Resource.Layout.ButtomSheetJobhFilter, container, false);
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater.Inflate(Resource.Layout.UserResponseSheetLayout, container, false);

                InitComponent(view);


                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private void InitComponent(View view)
        {
            txtLocalStorage = view.FindViewById<TextView>(Resource.Id.txtLocalStorage);
            txtLocalStorage.Visibility = ViewStates.Gone;
            txtRecordLive = view.FindViewById<TextView>(Resource.Id.txtRecordLive);
            txtGalary = view.FindViewById<TextView>(Resource.Id.txtGalary);
            txtGalary.Visibility = ViewStates.Visible;
            txtGalary.Click += TxtGalary_Click;
            //txtLocalStorage.Click += TxtLocalStorage_Click;
            txtRecordLive.Click += TxtRecordLive_Click;
        }

        private void TxtRecordLive_Click(object sender, EventArgs e)
        {
            this.Dismiss();
            VideoRecorderActivitiy.ResponseFragment = ResponseFragment;
            if (userResponse == UserResponse.Video)
            {
                if (ContextCompat.CheckSelfPermission(this.Activity, camPermission) == (int)Permission.Granted &&
                   ContextCompat.CheckSelfPermission(this.Activity, micPermission) == (int)Permission.Granted)
                {
                    Intent intent = new Intent(this.Context, typeof(VideoRecorderActivitiy));
                    intent.PutExtra("UserResponseType", (int)userResponse);
                    intent.PutExtra("videoId", videoId);
                    StartActivity(intent);
                }
                else
                {
                    RecordResponseManager.GetCameraAndMicCompatPermission(this.Activity, new string[] { camPermission, micPermission });
                }
            }
            else if (userResponse == UserResponse.Audio)
            {
                if (ContextCompat.CheckSelfPermission(this.Activity, micPermission) == (int)Permission.Granted)
                {
                    Intent intent = new Intent(this.Context, typeof(VideoRecorderActivitiy));
                    intent.PutExtra("UserResponseType", (int)userResponse);
                    intent.PutExtra("videoId", videoId);
                    StartActivity(intent);
                }
                else
                {
                    RecordResponseManager.GetCameraAndMicCompatPermission(this.Activity, new string[] { camPermission });
                }
            }
            if (VideoController != null)
            {
                VideoController.SetStopvideo();
                VideoController.ReleaseVideo();
            }
            if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                YoutubePlayer?.Pause();
        }

        private void TxtLocalStorage_Click(object sender, EventArgs e)
        {

        }

        private void TxtGalary_Click(object sender, EventArgs e)
        {
            this.Dismiss();
            if (userResponse == UserResponse.Video)
            {
                Intent contentSelectionIntent = Android.OS.Environment.GetExternalStorageState(null).Equals(Android.OS.Environment.MediaMounted) ? new Intent(Intent.ActionPick, MediaStore.Video.Media.ExternalContentUri) : new Intent(Intent.ActionPick, MediaStore.Video.Media.InternalContentUri);
                contentSelectionIntent.SetType("video/*");
                contentSelectionIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
                this.Activity.StartActivityForResult(Intent.CreateChooser(contentSelectionIntent, "Select videos"), 201);
            }
            else if (userResponse == UserResponse.Audio)
            {
                Intent contentSelectionIntent = Android.OS.Environment.GetExternalStorageState(null).Equals(Android.OS.Environment.MediaMounted) ? new Intent(Intent.ActionPick, MediaStore.Video.Media.ExternalContentUri) : new Intent(Intent.ActionPick, MediaStore.Video.Media.InternalContentUri);
                contentSelectionIntent.SetType("audio/*");
                contentSelectionIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
                this.Activity.StartActivityForResult(Intent.CreateChooser(contentSelectionIntent, "Select audios"), 201);
            }
        }
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}