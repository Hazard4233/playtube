using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.Net.Wifi.Aware;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Bumptech.Glide;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Com.Google.Android.Exoplayer2.Util;
using Newtonsoft.Json;
using PlayTube.Activities.ResponseComments;
using PlayTube.Activities.Tabbes;
using PlayTube.API;
using PlayTube.API.Models;
using PlayTube.Helpers;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTubeClient.Classes.Global;
using Plugin.Share;
using Plugin.Share.Abstractions;
using static Android.Views.View;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace PlayTube.Activities.UserReponse
{
    [Activity(Label = "MediaPlayerActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MediaPlayerActivity : AppCompatActivity
    {
        private static MediaPlayerActivity Instance;
        public PlayerView SimpleExoPlayerView;
        ImageView imgAudio;
        FrameLayout frameLayout;
        public List<RvDatum> mediaList = new List<RvDatum>();
        public RvDatum selectedMedia = new RvDatum();
        string videoId;
        OnSwipeTouchListener swipListener;
        ProgressBar progressBar;
        ImageView imgProfile;
        TextView txtUserName, txtdays, txtDescription;
        public SimpleExoPlayer Player;
        private ImageView btnLike, btnComment, btnShare;
        public TextView txtLikeNumber, txtCommentNumber, txtShareNumber;
        private UserDataObject userObject;
        private FrameLayout CommentsLayout;
        //private TextView txtHideComment;
        public ResponseComments.ResponseCommentsFragment CommentsFragment;
        //private readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();
        //private IDataSourceFactory DefaultDataMediaFactory;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.SetFlags(WindowManagerFlags.Fullscreen,
                WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.MediaPlayerLayout);
            SimpleExoPlayerView = FindViewById<PlayerView>(Resource.Id.exo_media_player_view);
            SimpleExoPlayerView.SetShutterBackgroundColor(Color.Transparent);
            Instance = this;

            imgAudio = FindViewById<ImageView>(Resource.Id.imgAudio);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            frameLayout = FindViewById<FrameLayout>(Resource.Id.frameLayout);
            mediaList = JsonConvert.DeserializeObject<List<RvDatum>>(Intent.GetStringExtra("mediaList"));
            selectedMedia = JsonConvert.DeserializeObject<RvDatum>(Intent.GetStringExtra("selectedMedia"));
            videoId = Intent.GetStringExtra("videoId");
            imgProfile = FindViewById<ImageView>(Resource.Id.imgProfile);
            imgProfile.Click += UserProfile_Click;
            txtUserName = FindViewById<TextView>(Resource.Id.txtUserName);
            txtUserName.Click += UserProfile_Click;
            txtDescription = FindViewById<TextView>(Resource.Id.txtDescription);
            txtdays = FindViewById<TextView>(Resource.Id.txtdays);
            btnLike = FindViewById<ImageView>(Resource.Id.btnLike);
            btnLike.Click += btnLike_Click;
            btnLike.Tag = "0";
            txtLikeNumber = FindViewById<TextView>(Resource.Id.txtLikeNumber);
            btnComment = FindViewById<ImageView>(Resource.Id.btnComment);
            btnComment.Click += btnComment_Click;
            txtCommentNumber = FindViewById<TextView>(Resource.Id.txtCommentNumber);
            btnShare = FindViewById<ImageView>(Resource.Id.btnShare);
            btnShare.Click += btnShare_Click;
            txtShareNumber = FindViewById<TextView>(Resource.Id.txtShareNumber);

            CommentsLayout = FindViewById<FrameLayout>(Resource.Id.ResponseCommentsLayout);
            CommentsLayout.LayoutParameters.Height = (int)(Resources.DisplayMetrics.HeightPixels / 2);
            //txtHideComment = FindViewById<TextView>(Resource.Id.txtHideResponseComments);
            //txtHideComment.Click += txtHideComment_Click;
            CommentsFragment = new ResponseComments.ResponseCommentsFragment();
            FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
            ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
            ftvideo.AddToBackStack(null);
            ftvideo.Add(CommentsLayout.Id, CommentsFragment, null).Hide(CommentsFragment).Commit();

            swipListener = new OnSwipeTouchListener(this);
            //DefaultDataMediaFactory = new DefaultDataSourceFactory(this, Util.GetUserAgent(this, AppSettings.ApplicationName), BandwidthMeter);
            TabbedMainActivity.GetInstance().VideoActionsController.ReleaseVideo();
            PlayAsync();
        }
        private async void UserProfile_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedMedia.UserID != UserDetails.UserId)
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                        {
                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                    });

                        //  send a Post request  
                        var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=get_channel_info&channel_id=" + selectedMedia.UserID;
                        var result = await client.PostAsync(uri, formContent);

                        if (result.IsSuccessStatusCode)
                        {
                            // handling the answer  
                            var resultString = await result.Content.ReadAsStringAsync();
                            var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                            if (jConfigObject["api_status"].ToString() == "200" && jConfigObject["data"] != null)
                            {
                                userObject = (UserDataObject)jConfigObject["data"].ToObject(typeof(UserDataObject));
                                OnBackPressed();
                                //TabbedMainActivity.GetInstance().ShowUserChannelFragment(userObject, selectedMedia.UserID);
                            }
                        }
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async void btnLike_Click(object sender, EventArgs e)
        {
            try
            {
                var ActivityContext = TabbedMainActivity.GetInstance();
                if (Helpers.Utils.Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        try
                        {
                            //If User Liked
                            if (btnLike.Tag.ToString() == "0")
                            {
                                btnLike.Tag = "1";
                                btnLike.SetImageResource(Resource.Drawable.like_blue);

                                if (!txtLikeNumber.Text.Contains("K") && !txtLikeNumber.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(txtLikeNumber.Text);
                                    x++;
                                    txtLikeNumber.Text = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                }

                                int index = mediaList.FindIndex(x => x.RvId == selectedMedia.RvId);
                                mediaList[index].IsLiked = "1";
                                mediaList[index].Likes = (Convert.ToDouble(mediaList[index].Likes) + 1).ToString();
                                //AddToLiked
                                //ActivityContext.LibrarySynchronizer.AddToLiked(VideoDataHandler);

                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short).Show();

                                //Send API Request here for Like
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                        new KeyValuePair<string, string>("video_id", selectedMedia.RvId),
                                        new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                                        new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                                    });

                                    //  send a Post request  
                                    var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=like_response_videos";
                                    var result = await client.PostAsync(uri, formContent);
                                    //if (result.IsSuccessStatusCode)
                                    //{
                                    //    // handling the answer  
                                    //    var resultString = await result.Content.ReadAsStringAsync();
                                    //    var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                    //    if (jConfigObject["api_status"].ToString() == "200" && jConfigObject["data"] != null)
                                    //    {
                                    //    }
                                    //}
                                }

                            }
                            else
                            {
                                btnLike.Tag = "0";
                                btnLike.SetImageResource(Resource.Drawable.like_white);
                                
                                if (!txtLikeNumber.Text.Contains("K") && !txtLikeNumber.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(txtLikeNumber.Text);
                                    if (x > 0)
                                        x--;
                                    else
                                        x = 0;

                                    txtLikeNumber.Text = x.ToString(System.Globalization.CultureInfo.CurrentCulture);
                                }

                                int index = mediaList.FindIndex(x => x.RvId == selectedMedia.RvId);
                                mediaList[index].IsLiked = "0";
                                var y = Convert.ToDouble(mediaList[index].Likes);
                                if (y > 0)  y--;
                                else    y = 0;
                                mediaList[index].Likes = y.ToString();

                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short).Show();

                                //Send API Request here for Remove Like
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                        new KeyValuePair<string, string>("video_id", selectedMedia.RvId),
                                        new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                                        new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                                    });

                                    //  send a Post request  
                                    var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=like_response_videos";
                                    var result = await client.PostAsync(uri, formContent);

                                    //if (result.IsSuccessStatusCode)
                                    //{
                                    //    // handling the answer  
                                    //    var resultString = await result.Content.ReadAsStringAsync();
                                    //    var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                    //    if (jConfigObject["api_status"].ToString() == "200" && jConfigObject["data"] != null)
                                    //    {
                                    //    }
                                    //}
                                }
                            }
                            await ActivityContext.VideoDataWithEventsLoader.ResponseFragment.GetResponseList(ActivityContext.VideoData.VideoId);
                            ActivityContext.VideoDataWithEventsLoader.ResponseFragment.GetUserResponse(ActivityContext.VideoData.VideoId);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(ActivityContext, ActivityContext.VideoDataWithEventsLoader.VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(this.GetText(Resource.String.Lbl_Warning), this.GetText(Resource.String.Lbl_Please_sign_in_Like), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        private async void btnShare_Click(object sender, EventArgs e)
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported)
                {
                    return;
                }

                await CrossShare.Current.Share(new ShareMessage
                {
                    Text = string.IsNullOrEmpty(selectedMedia.Description) ? "" : selectedMedia.Description,
                    Url = System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.VideoLocation)
                });
                
                await Task.Delay(TimeSpan.FromSeconds(5));
                string msg = UserResponseAPI.ShareVideoResponse(selectedMedia.RvId);
                if (msg.Contains("ok"))
                {
                    var x = Convert.ToDouble(txtShareNumber.Text);
                    x++;
                    txtShareNumber.Text = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    int index = mediaList.FindIndex(x => x.RvId == selectedMedia.RvId);
                    mediaList[index].Shares = (Convert.ToDouble(mediaList[index].Shares) + 1).ToString();
                        
                    var ActivityContext = TabbedMainActivity.GetInstance();
                    await ActivityContext.VideoDataWithEventsLoader.ResponseFragment.GetResponseList(ActivityContext.VideoData.VideoId);
                    ActivityContext.VideoDataWithEventsLoader.ResponseFragment.GetUserResponse(ActivityContext.VideoData.VideoId);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        private void btnComment_Click(object sender, EventArgs e)
        {
            try
            {
                //txtHideComment.Visibility = ViewStates.Visible;
                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();

                if (!CommentsFragment.IsAdded)
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    ftvideo.AddToBackStack(null);
                    ftvideo.Add(CommentsLayout.Id, CommentsFragment, null).Commit();
                }
                else
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    ftvideo.Show(CommentsFragment).Commit();
                }

                CommentsFragment.StartApiService(selectedMedia.RvId, "0");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        public static MediaPlayerActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void ShowReplyResponseCommentFragment(dynamic comment, string type)
        {
            try
            {
                ReplyResponseCommentBottomSheet replyFragment = new ReplyResponseCommentBottomSheet();
                Bundle bundle = new Bundle();

                bundle.PutString("Type", type);
                bundle.PutString("Object", JsonConvert.SerializeObject(comment));
                replyFragment.Arguments = bundle;

                replyFragment.Show(SupportFragmentManager, replyFragment.Tag);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        //private void txtHideComment_Click(object sender, EventArgs e)
        //{
        //    txtHideComment.Visibility = ViewStates.Gone;
        //    FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
        //    ftvideo.AddToBackStack(null);
        //    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
        //    ftvideo.Hide(CommentsFragment).Commit();
        //}
        private void InitializePlayer()
        {
            try
            {

                AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                var trackSelector = new DefaultTrackSelector(trackSelectionFactory);

                var newParameters = new DefaultTrackSelector.ParametersBuilder()
                    .SetMaxVideoSizeSd()
                    .Build();

                trackSelector.SetParameters(newParameters);

                //DefaultLoadControl.Builder builder = new DefaultLoadControl.Builder();

                //builder.SetBufferDurationsMs(DefaultLoadControl.DefaultMinBufferMs, 60000, DefaultLoadControl.DefaultBufferForPlaybackMs, DefaultLoadControl.DefaultBufferForPlaybackAfterRebufferMs);
                //DefaultLoadControl loadControl = builder.CreateDefaultLoadControl();
                //                
                Player = ExoPlayerFactory.NewSimpleInstance(this, trackSelector);

            }
            catch (System.Exception)
            {

            }
        }
        //private IMediaSource CreateCacheMediaSource(IMediaSource videoSource, Android.Net.Uri videoUrL)
        //{
        //    try
        //    {
        //        var url = System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.VideoLocation);
        //        var file = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(url);

        //        SimpleCache cache = new SimpleCache(this.CacheDir, new LeastRecentlyUsedCacheEvictor(1024 * 1024 * 10));
        //        CacheDataSourceFactory cacheDataSource = new CacheDataSourceFactory(cache, DefaultDataMediaFactory);

        //        if (!string.IsNullOrEmpty(file))
        //        {
        //            videoUrL = Android.Net.Uri.Parse(file);

        //            videoSource = GetMediaSourceFromUrl(videoUrL, "normal");
        //            return videoSource;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //        return null;
        //    }
        //}

        public void PlayAsync()
        {

            try
            {
                progressBar.Visibility = ViewStates.Visible;
                if (Player != null)
                {
                    Player.Stop();
                    Player.Release();
                }
                InitializePlayer();
                if (selectedMedia.VideoLocation.Contains(".mp4"))
                {
                    imgAudio.Visibility = ViewStates.Gone;
                    SimpleExoPlayerView.Visibility = ViewStates.Visible;
                    SimpleExoPlayerView.ResizeMode = AspectRatioFrameLayout.ResizeModeZoom;
                    SimpleExoPlayerView.Player = Player;
                    SimpleExoPlayerView.UseController = false;
                    swipListener.SwipeEvent -= SwipListener_SwipeEvent;
                    swipListener.SwipeEvent += SwipListener_SwipeEvent;
                    SimpleExoPlayerView.SetOnTouchListener(swipListener);
                }
                else
                {
                    imgAudio.Visibility = ViewStates.Visible;
                    SimpleExoPlayerView.Visibility = ViewStates.Gone;
                    swipListener.SwipeEvent -= SwipListener_SwipeEvent;
                    swipListener.SwipeEvent += SwipListener_SwipeEvent;
                    imgAudio.SetOnTouchListener(swipListener);
                    Glide.With(this).Load(System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.Thumbnail)).Into(imgAudio);
                }
                var url = Android.Net.Uri.Parse(System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.VideoLocation));
                var VideoSource = GetMediaSourceFromUrl(url, "normal");
                Player.Prepare(VideoSource);
                PlayerListener playerListener = new PlayerListener(this);
                playerListener.MediaPlayCompleted = () =>
                {
                    if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                    {
                        UserResponseAPI.ViewVideoResponse(selectedMedia.RvId);
                        TabbedMainActivity.GetInstance().VideoDataWithEventsLoader
                        .ResponseFragment.UserResponseAdapter.UserResponseList
                        .Where(x => x.VideoLocation == selectedMedia.VideoLocation)
                        .FirstOrDefault().RvvId = string.Empty;
                        TabbedMainActivity.GetInstance().VideoDataWithEventsLoader
                        .ResponseFragment.UserResponseAdapter
                        .NotifyItemChanged(mediaList.FindIndex(x => x.VideoLocation == selectedMedia.VideoLocation));
                    }
                };
                Player.AddListener(playerListener);
                Player.PlayWhenReady = true;
                Glide.With(this).Load(UserResponseURL.DirectoryUserSettings + selectedMedia.Avatar).Into(imgProfile);
                txtUserName.Text = selectedMedia.Username;
                txtdays.Text = Helpers.DateTimeHelper.GetDateTimeString(selectedMedia.Timestamp);
                txtDescription.Text = selectedMedia.Description;
                txtLikeNumber.Text = selectedMedia.Likes;
                txtShareNumber.Text = selectedMedia.Shares;
                txtCommentNumber.Text = selectedMedia.Comments;

                if (selectedMedia.IsLiked == "1") // true
                {
                    btnLike.Tag = "1";
                    btnLike.SetImageResource(Resource.Drawable.like_blue);
                }
                else
                {
                    btnLike.Tag = "0";
                    btnLike.SetImageResource(Resource.Drawable.like_white);
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private IMediaSource GetMediaSourceFromUrl(Android.Net.Uri uri, string tag)
        {
            try
            {
                var mBandwidthMeter = new DefaultBandwidthMeter();
                DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(this, Util.GetUserAgent(this, AppSettings.ApplicationName), mBandwidthMeter);
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(this, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(this, AppSettings.ApplicationName), new DefaultBandwidthMeter()));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(this, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(this, AppSettings.ApplicationName), null));
                int type = Util.InferContentType(uri, null);
                var src = type switch
                {
                    C.TypeSs => new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
                    C.TypeDash => new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
                    C.TypeHls => new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
                    C.TypeOther => new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
                    _ => new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri)
                };
                return src;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }


        private void PlayPause()
        {
            try
            {
                if (Player != null)
                {
                    Player.PlayWhenReady = !Player.PlayWhenReady;
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private void PlayPrevious()
        {
            try
            {
                if (mediaList != null && mediaList.Count > 0)
                {
                    int index = mediaList.FindIndex(x => x.RvId == selectedMedia.RvId);
                    index -= 1;
                    if (index >= 0)
                    {
                        selectedMedia = mediaList[index];
                        PlayAsync();
                    }
                    else
                    {
                        Toast.MakeText(this, "No more response available to play", ToastLength.Long);
                        this.Finish();
                    }
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        public void PlayNext()
        {
            try
            {
                if (mediaList != null && mediaList.Count > 0)
                {
                    int index = mediaList.FindIndex(x => x.RvId == selectedMedia.RvId);
                    index += 1;
                    if (mediaList.Count >= index + 1)
                    {
                        selectedMedia = mediaList[index];
                        PlayAsync();
                    }
                    else
                    {
                        Toast.MakeText(this, "No more response available to play", ToastLength.Long);
                        this.Finish();
                    }
                }
            }
            catch (System.Exception ex)
            {

            }
        }
        private void SwipListener_SwipeEvent(SwipeEnum swipe)
        {
            try
            {
                if (swipe == SwipeEnum.Top)
                {
                    PlayNext();
                }
                else if (swipe == SwipeEnum.Bottom)
                {
                    PlayPrevious();
                }
                else if (swipe == SwipeEnum.SingleTab)
                {
                    PlayPause();
                }
            }
            catch (System.Exception ex)
            {

            }
        }


        protected async override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                Player.Stop();
                Player.Release();
                TabbedMainActivity.GetInstance().VideoDataWithEventsLoader
                    .ResponseFragment.UserResponseAdapter.NotifyDataSetChanged();
                if(userObject != null)
                    TabbedMainActivity.GetInstance().ShowUserChannelFragment(userObject, selectedMedia.UserID);
            }
            catch (System.Exception ex)
            {

            }
        }
    }

    internal class PlayerListener : Java.Lang.Object, IPlayerEventListener, PlayerControlView.IVisibilityListener
    {
        public Action MediaPlayCompleted;
        private MediaPlayerActivity mediaPlayerActivity;
        private readonly ProgressBar LoadingProgressBar;
        public PlayerListener(MediaPlayerActivity mediaPlayerActivity)
        {
            this.mediaPlayerActivity = mediaPlayerActivity;
            LoadingProgressBar = mediaPlayerActivity.FindViewById<ProgressBar>(Resource.Id.progressBar);
        }

        public void OnLoadingChanged(bool isLoading)
        {

        }

        public void OnPlaybackParametersChanged(PlaybackParameters playbackParameters)
        {

        }

        public void OnPlayerError(ExoPlaybackException error)
        {

        }

        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            try
            {
                if (LoadingProgressBar == null)
                    return;

                if (playbackState == Player.StateEnded)
                {
                    MediaPlayCompleted?.Invoke();
                    mediaPlayerActivity.PlayNext();
                }
                else if (playbackState == Player.StateReady)
                {
                    //var videoFormat = mediaPlayerActivity.Player.VideoFormat;
                    //if (videoFormat.Width > videoFormat.Height)
                    //    mediaPlayerActivity.SimpleExoPlayerView.ResizeMode = AspectRatioFrameLayout.ResizeModeFit;
                    //else
                    //    mediaPlayerActivity.SimpleExoPlayerView.ResizeMode = AspectRatioFrameLayout.ResizeModeZoom;
                    //LoadingProgressBar.Visibility = ViewStates.Invisible;
                }
                else if (playbackState == Player.StateBuffering)
                {
                    LoadingProgressBar.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void OnPositionDiscontinuity(int reason)
        {

        }

        public void OnRepeatModeChanged(int repeatMode)
        {

        }

        public void OnSeekProcessed()
        {

        }

        public void OnShuffleModeEnabledChanged(bool shuffleModeEnabled)
        {

        }

        public void OnTimelineChanged(Timeline timeline, Java.Lang.Object manifest, int reason)
        {

        }

        public void OnTracksChanged(TrackGroupArray trackGroups, TrackSelectionArray trackSelections)
        {

        }

        public void OnVisibilityChange(int visibility)
        {

        }
    }
}