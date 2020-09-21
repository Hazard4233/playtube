using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using NUnit.Framework;

using Java.Lang;
using Java.Util;
using Java.IO;
using Java.Util.Concurrent;
using PlayTube.Helpers;
using Android.App;
using System;
using Android.Content;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android;
using PlayTube.API;
using Android.Gms.Gcm;
using Android.Support.Design.Widget;
using PlayTube.Activities.Tabbes;
using Android.Support.V7.App;

namespace PlayTube.Activities.UserReponse
{
    [Activity(Label = "VideoRecorderActivitiy", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class VideoRecorderActivitiy : AppCompatActivity
    {
        public ImageView imgThumbnail, imgUpload, imgSwitchCamera, imgPlayPauseMedia;
        private TextView txtStop;

        FrameLayout mainFrame;
        ImageView imgAudio;

        public static UserResponseFragment ResponseFragment;


        public TextView txtTime;
        private FrameLayout framePlayPause;
        public AutoFitTextureView textureView;
        string videoId;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            VideoManager.cameraDevice?.Close();
            VideoManager.StopRecorder();
        }

        private UserResponse UserResponseType;

        RecordResponseManager VideoManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen,
                WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.VideoRecorderLayout);

            imgThumbnail = FindViewById<ImageView>(Resource.Id.imgThumbnail);
            imgUpload = FindViewById<ImageView>(Resource.Id.imgUpload);
            imgSwitchCamera = FindViewById<ImageView>(Resource.Id.imgSwitchCamera);
            imgPlayPauseMedia = FindViewById<ImageView>(Resource.Id.imgPlayPauseMedia);
            txtStop = FindViewById<TextView>(Resource.Id.txtStop);
            txtTime = FindViewById<TextView>(Resource.Id.txtTime);
            framePlayPause = FindViewById<FrameLayout>(Resource.Id.framePlayPause);
            mainFrame = FindViewById<FrameLayout>(Resource.Id.mainFrame);
            imgAudio = FindViewById<ImageView>(Resource.Id.imgAudio);
            textureView = FindViewById<AutoFitTextureView>(Resource.Id.texture);
            framePlayPause.Click += FramePlayPause_Click;
            txtStop.Click += TxtStop_Click;
            imgUpload.Click += ImgUpload_Click;
            txtTime.Text = "00:30";

            UserResponseType = (UserResponse)this.Intent.GetIntExtra("UserResponseType", 0);
            videoId = this.Intent.GetStringExtra("videoId");

            VideoManager = new RecordResponseManager(this, UserResponseType);
            if (UserResponseType == UserResponse.Video)
            {
                imgAudio.Visibility = ViewStates.Gone;
                mainFrame.SetBackgroundResource(Resource.Color.gnt_black);
                VideoManager.StartBackgroundThread();
                if (textureView.IsAvailable)
                    VideoManager.openCamera(textureView.Width, textureView.Height, VideoManager.LensFacing);
                else
                    textureView.SurfaceTextureListener = VideoManager.surfaceTextureListener;

                imgSwitchCamera.Click += ImgSwitchCamera_Click;
                textureView.Visibility = ViewStates.Visible;
                imgSwitchCamera.Visibility = ViewStates.Visible;
            }
            else
            {
                textureView.Visibility = ViewStates.Gone;
                imgSwitchCamera.Visibility = ViewStates.Gone;
                imgAudio.Visibility = ViewStates.Visible;
                mainFrame.SetBackgroundResource(Resource.Color.gnt_white);
            }

            var pathStr = VideoManager.GetFilePath();
        }

        private void ImgUpload_Click(object sender, EventArgs e)
        {
            UserResponseLiveDescription mFragment = new UserResponseLiveDescription(this);
            mFragment.Show(SupportFragmentManager, mFragment.Tag);
            //await UploadRecordedResponse();
            //var tabbedActivity = TabbedMainActivity.GetInstance();
            //if (tabbedActivity != null)
            //{
            //    int UnseenResponseCount = UserResponseAPI.GetUnseenResponseNotification();
            //    if (UnseenResponseCount > 0)
            //    {
            //        tabbedActivity.MyChannelFragment.UnSeenReponse = UnseenResponseCount;
            //        tabbedActivity.FragmentBottomNavigator.txtUnSeenCount.Visibility = ViewStates.Visible;
            //        tabbedActivity.FragmentBottomNavigator.txtUnSeenCount.Text = UnseenResponseCount.ToString();
            //    }
            //    else
            //        tabbedActivity.FragmentBottomNavigator.txtUnSeenCount.Visibility = ViewStates.Gone;
            //}
        }

        public async System.Threading.Tasks.Task UploadRecordedResponse(string Description)
        {
            try
            {
                this.RunOnUiThread(() => LoadingView.Show(this, "Uploading..."));
                var binaryData = VideoManager.GetVideoData();
                if (VideoManager.IsRecorderStop && binaryData != null)
                {
                    string msg = string.Empty;
                    if (UserResponseType == UserResponse.Video)
                        msg = await UserResponseAPI.UploadUserResponse(videoId, binaryData, InputSelectedType.Recorded, Description);
                    else
                        msg = await UserResponseAPI.UploadAudioUserResponse(videoId, binaryData, InputSelectedType.Recorded, Description);
                    if (ResponseFragment != null)
                    {
                        await ResponseFragment.GetResponseList(videoId);
                        ResponseFragment.GetUserResponse(videoId);
                    }
                    if (msg.Contains("ok"))
                        this.RunOnUiThread(() => Toast.MakeText(this, "Response Uploaded successfully", ToastLength.Short).Show());
                    else
                        this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
                }
            }
            catch (System.Exception ex)
            {

            }
            finally
            {
                this.RunOnUiThread(() => LoadingView.Hide());
            }
        }

        private void ImgSwitchCamera_Click(object sender, EventArgs e)
        {
            VideoManager?.SwitchCamera();
        }

        private void TxtStop_Click(object sender, EventArgs e)
        {
            VideoManager.StopRecorder();
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (UserResponseType == UserResponse.Video)
            {
                VideoManager.StartBackgroundThread();
                if (textureView.IsAvailable)
                    VideoManager.openCamera(textureView.Width, textureView.Height, VideoManager.LensFacing);
                else
                    textureView.SurfaceTextureListener = VideoManager.surfaceTextureListener;
            }
        }

        private void FramePlayPause_Click(object sender, EventArgs e)
        {
            if (VideoManager != null)
            {
                if (VideoManager.IsRecorderPause && !VideoManager.IsRecorderPlaying)
                {
                    imgPlayPauseMedia.SetImageResource(Resource.Drawable.pause);
                    VideoManager.ResumeRecording();
                }
                else if (!VideoManager.IsRecorderPlaying)
                {
                    imgPlayPauseMedia.SetImageResource(Resource.Drawable.pause);
                    VideoManager.StartRecoring();
                }
                else
                {
                    imgPlayPauseMedia.SetImageResource(Resource.Drawable.whitePlay);
                    VideoManager.PauseRecording();
                }
            }
        }
    }
}