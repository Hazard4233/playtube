using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using Console = System.Console;
using Uri = Android.Net.Uri;

namespace PlayTube
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", LaunchMode = LaunchMode.SingleInstance, ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Locale | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class VideoFullScreenActivity : AppCompatActivity
    {
        #region Variables Basic

        private ProgressBar ProgressBar;
        private VideoView PostVideoView;
        private string VideoUrl;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                //Set Full screen 
                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.HideNavigation;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

                Window.AddFlags(WindowManagerFlags.Fullscreen);
                 
                //newUiOptions |= (int)SystemUiFlags.LowProfile;
                //newUiOptions |= (int)SystemUiFlags.Immersive;

                //ScreenOrientation.Portrait >>  Make to run your application only in portrait mode
                //ScreenOrientation.Landscape >> Make to run your application only in LANDSCAPE mode 
                //RequestedOrientation = ScreenOrientation.Landscape;
                 
                SetContentView(Resource.Layout.VideoFullScreenLayout);

                VideoUrl = Intent.GetStringExtra("videoUrl") ?? "";
                //var VideoDuration = Intent.GetStringExtra("videoDuration") ?? "";

                //Get Value And Set Toolbar
                InitComponent(); 
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                var media = new MediaController(this);
                media.Show(5000);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progress_bar);
                ProgressBar.Visibility = ViewStates.Visible;
                 
                PostVideoView = FindViewById<VideoView>(Resource.Id.videoView);
                PostVideoView.Completion += PostVideoViewOnCompletion;
                PostVideoView.SetMediaController(media);
                media.Visibility = ViewStates.Gone;
                PostVideoView.Prepared += PostVideoViewOnPrepared;
                //PostVideoView.CanSeekBackward();
                //PostVideoView.CanSeekForward();
                PostVideoView.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(AudioUsageKind.Media).SetContentType(AudioContentType.Movie).Build());

                if (VideoUrl.Contains("http"))
                {
                    PostVideoView.SetVideoURI(Uri.Parse(VideoUrl)); 
                }
                else
                {
                    var file = Uri.FromFile(new File(VideoUrl));
                    PostVideoView.SetVideoPath(file.Path);
                }

                //TabbedMainActivity.GetInstance()?.SetOnWakeLock();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void PostVideoViewOnPrepared(object sender, EventArgs e)
        {
            try
            {
                PostVideoView.Start();
                ProgressBar.Visibility = ViewStates.Invisible;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void PostVideoViewOnCompletion(object sender, EventArgs e)
        {
            try
            {
                PostVideoView.Pause();
                OnBackPressed();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
         
        public override void OnBackPressed()
        {
            try
            {
                PostVideoView?.StopPlayback();
                PostVideoView = null;

                //TabbedMainActivity.GetInstance()?.SetOffWakeLock();

                base.OnBackPressed();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                base.OnBackPressed();
            }
        }

    }
}