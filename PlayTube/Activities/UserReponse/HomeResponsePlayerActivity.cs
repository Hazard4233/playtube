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
using PlayTube.Activities.Channel.Tab;
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
    [Activity(Label = "HomeResponsePlayerActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class HomeResponsePlayerActivity : AppCompatActivity
    {
//        private static HomeResponsePlayerActivity Instance;
//        public SimpleExoPlayer Player;
//        public PlayerView SimpleExoPlayerView;
//        ISurfaceHolder surfaceHolder;
//        ImageView imgAudio;
//        FrameLayout frameLayout;
//        LinearLayout linearButton;
//        Button btnAcceptReject, btnDelete;
//        public List<AdminVideoResponse> mediaList = new List<AdminVideoResponse>();
//        public AdminVideoResponse selectedMedia = new AdminVideoResponse();
//        OnSwipeTouchListener swipListener;
//        ProgressBar progressBar;
//        ImageView imgProfile;
//        TextView txtUserName, txtdays, txtDescription;
//        private ImageView btnLike, btnComment, btnShare;
//        public TextView txtLikeNumber, txtCommentNumber, txtShareNumber;
//        private UserDataObject userObject;
//        private FrameLayout CommentsLayout;
//        //private TextView txtHideComment;
//        public ResponseComments.HomeResponseCommentsFragment CommentsFragment;
//        public string ActivityType;
//        //private readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();
//        //private IDataSourceFactory DefaultDataMediaFactory;


//        protected override void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);

//            RequestWindowFeature(WindowFeatures.NoTitle);
//            Window.SetFlags(WindowManagerFlags.Fullscreen,
//                WindowManagerFlags.Fullscreen);

//            Instance = this;

//            if (Intent.GetStringExtra("swipeAction") == "SwipeUp")
//                OverridePendingTransition(Resource.Animation.response_slide_out, Resource.Animation.response_slide_hold);
//            else if (Intent.GetStringExtra("swipeAction") == "SwipeDown")
//                OverridePendingTransition(Resource.Animation.response_slide_in, Resource.Animation.response_slide_hold);

//            SetContentView(Resource.Layout.MediaPlayerLayout);
//            linearButton = FindViewById<LinearLayout>(Resource.Id.linearButton);
//            btnAcceptReject = FindViewById<Button>(Resource.Id.btnAcceptReject);
//            btnDelete = FindViewById<Button>(Resource.Id.btnDelete);
//            SimpleExoPlayerView = FindViewById<PlayerView>(Resource.Id.exo_media_player_view);
//            SimpleExoPlayerView.SetShutterBackgroundColor(Color.Transparent);
//            imgAudio = FindViewById<ImageView>(Resource.Id.imgAudio);
//            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
//            frameLayout = FindViewById<FrameLayout>(Resource.Id.frameLayout);
//            imgProfile = FindViewById<ImageView>(Resource.Id.imgProfile);
//            imgProfile.Click += UserProfile_Click;
//            txtUserName = FindViewById<TextView>(Resource.Id.txtUserName);
//            txtUserName.Click += UserProfile_Click;
//            txtDescription = FindViewById<TextView>(Resource.Id.txtDescription);
//            txtdays = FindViewById<TextView>(Resource.Id.txtdays);
//            mediaList = JsonConvert.DeserializeObject<List<AdminVideoResponse>>(Intent.GetStringExtra("mediaList"));
//            selectedMedia = JsonConvert.DeserializeObject<AdminVideoResponse>(Intent.GetStringExtra("selectedMedia"));
//            ActivityType = Intent.GetStringExtra("type");
//            //linearButton.Visibility = ViewStates.Visible;
//            //btnAcceptReject.Visibility = ViewStates.Visible;
//            //btnDelete.Visibility = ViewStates.Visible;
//            //btnAcceptReject.Click -= BtnAcceptReject_Click;
//            //btnAcceptReject.Click += BtnAcceptReject_Click;
//            //btnDelete.Click -= BtnDelete_Click;
//            //btnDelete.Click += BtnDelete_Click;

//            btnLike = FindViewById<ImageView>(Resource.Id.btnLike);
//            btnLike.Click += btnLike_Click;
//            btnLike.Tag = "0";
//            txtLikeNumber = FindViewById<TextView>(Resource.Id.txtLikeNumber);
//            btnComment = FindViewById<ImageView>(Resource.Id.btnComment);
//            btnComment.Click += btnComment_Click;
//            txtCommentNumber = FindViewById<TextView>(Resource.Id.txtCommentNumber);
//            btnShare = FindViewById<ImageView>(Resource.Id.btnShare);
//            btnShare.Click += btnShare_Click;
//            txtShareNumber = FindViewById<TextView>(Resource.Id.txtShareNumber);

//            CommentsLayout = FindViewById<FrameLayout>(Resource.Id.ResponseCommentsLayout);
//            CommentsLayout.LayoutParameters.Height = (int)(Resources.DisplayMetrics.HeightPixels / 2);
//            //txtHideComment = FindViewById<TextView>(Resource.Id.txtHideResponseComments);
//            //txtHideComment.Click += txtHideComment_Click;
//            CommentsFragment = new HomeResponseCommentsFragment();
//            FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
//            ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
//            ftvideo.AddToBackStack(null);
//            ftvideo.Add(CommentsLayout.Id, CommentsFragment, null).Hide(CommentsFragment).Commit();

//            swipListener = new OnSwipeTouchListener(this);
//            //DefaultDataMediaFactory = new DefaultDataSourceFactory(this, Util.GetUserAgent(this, AppSettings.ApplicationName), BandwidthMeter);

//            Play();
//        }

//        private async void UserProfile_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                if (selectedMedia.UserID != UserDetails.UserId)
//                {
//                    using (var client = new System.Net.Http.HttpClient())
//                    {
//                        var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
//                        {
//                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
//                    });

//                        //  send a Post request  
//                        var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=get_channel_info&channel_id=" + selectedMedia.UserID;
//                        var result = await client.PostAsync(uri, formContent);

//                        if (result.IsSuccessStatusCode)
//                        {
//                            // handling the answer  
//                            var resultString = await result.Content.ReadAsStringAsync();
//                            var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
//                            if (jConfigObject["api_status"].ToString() == "200" && jConfigObject["data"] != null)
//                            {
//                                userObject = (UserDataObject)jConfigObject["data"].ToObject(typeof(UserDataObject));
//                                OnBackPressed();
//                                //TabbedMainActivity.GetInstance().ShowUserChannelFragment(userObject, selectedMedia.UserID);
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//        }
//        private async void btnLike_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                var ActivityContext = TabbedMainActivity.GetInstance();
//                if (Helpers.Utils.Methods.CheckConnectivity())
//                {
//                    if (UserDetails.IsLogin)
//                    {
//                        try
//                        {
//                            //If User Liked
//                            if (btnLike.Tag.ToString() == "0")
//                            {
//                                btnLike.Tag = "1";
//                                btnLike.SetImageResource(Resource.Drawable.like_blue);

//                                if (!txtLikeNumber.Text.Contains("K") && !txtLikeNumber.Text.Contains("M"))
//                                {
//                                    var x = Convert.ToDouble(txtLikeNumber.Text);
//                                    x++;
//                                    txtLikeNumber.Text = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
//                                }

//                                int index = mediaList.FindIndex(x => x.Id == selectedMedia.Id);
//                                mediaList[index].IsLiked = "1";
//                                var y = Convert.ToDouble(mediaList[index].Likes);
//                                y++;
//                                mediaList[index].Likes = y.ToString();

//                                if (ActivityType == "Follow")
//                                {
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Likes = y.ToString();
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().IsLiked = "1";
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater
//                                    .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                                }
//                                else
//                                {
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Likes = y.ToString();
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().IsLiked = "1";
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater
//                                    .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                                }

//                                //AddToLiked
//                                //ActivityContext.LibrarySynchronizer.AddToLiked(VideoDataHandler);

//                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short).Show();

//                                //Send API Request here for Like
//                                using (var client = new System.Net.Http.HttpClient())
//                                {
//                                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
//                                    {
//                                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
//                                        new KeyValuePair<string, string>("video_id", selectedMedia.Id.ToString()),
//                                        new KeyValuePair<string, string>("user_id", UserDetails.UserId),
//                                        new KeyValuePair<string, string>("s", UserDetails.AccessToken)
//                                    });

//                                    //  send a Post request  
//                                    var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=like_response_videos";
//                                    var result = await client.PostAsync(uri, formContent);
//                                    //if (result.IsSuccessStatusCode)
//                                    //{
//                                    //    // handling the answer  
//                                    //    var resultString = await result.Content.ReadAsStringAsync();
//                                    //    var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
//                                    //    if (jConfigObject["api_status"].ToString() == "200" && jConfigObject["data"] != null)
//                                    //    {
//                                    //    }
//                                    //}
//                                }

//                            }
//                            else
//                            {
//                                btnLike.Tag = "0";
//                                btnLike.SetImageResource(Resource.Drawable.like_white);

//                                if (!txtLikeNumber.Text.Contains("K") && !txtLikeNumber.Text.Contains("M"))
//                                {
//                                    var x = Convert.ToDouble(txtLikeNumber.Text);
//                                    if (x > 0)
//                                        x--;
//                                    else
//                                        x = 0;

//                                    txtLikeNumber.Text = x.ToString(System.Globalization.CultureInfo.CurrentCulture);
//                                }

//                                int index = mediaList.FindIndex(x => x.Id == selectedMedia.Id);
//                                mediaList[index].IsLiked = "0";
//                                var y = Convert.ToDouble(mediaList[index].Likes);
//                                if (y > 0) y--;
//                                else y = 0;
//                                mediaList[index].Likes = y.ToString();
//                                if (ActivityType == "Follow")
//                                {
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Likes = y.ToString();
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().IsLiked = "0";
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater
//                                    .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                                }
//                                else
//                                {
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Likes = y.ToString();
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses
//                                    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().IsLiked = "0";
//                                    TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater
//                                    .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                                }

//                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short).Show();

//                                //Send API Request here for Remove Like
//                                using (var client = new System.Net.Http.HttpClient())
//                                {
//                                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
//                                    {
//                                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
//                                        new KeyValuePair<string, string>("video_id", selectedMedia.Id.ToString()),
//                                        new KeyValuePair<string, string>("user_id", UserDetails.UserId),
//                                        new KeyValuePair<string, string>("s", UserDetails.AccessToken)
//                                    });

//                                    //  send a Post request  
//                                    var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=like_response_videos";
//                                    var result = await client.PostAsync(uri, formContent);

//                                    //if (result.IsSuccessStatusCode)
//                                    //{
//                                    //    // handling the answer  
//                                    //    var resultString = await result.Content.ReadAsStringAsync();
//                                    //    var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
//                                    //    if (jConfigObject["api_status"].ToString() == "200" && jConfigObject["data"] != null)
//                                    //    {
//                                    //    }
//                                    //}
//                                }
//                            }
//                            //if (ActivityType == "Follow")
//                            //{
//                            //    //TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses =
//                            //    //new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetFollowingResponseVideos().VideoResponse);
//                            //    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses
//                            //    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Likes = y.ToString();
//                            //    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater
//                            //    .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                            //}
//                            //else
//                            //{
//                            //    //TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses =
//                            //    //new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetForYouResponseVideos().VideoResponse);
//                            //    TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.ForYouResponses
//                            //    .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Likes = y.ToString();
//                            //    TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater
//                            //    .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                            //}
//                        }
//                        catch (Exception exception)
//                        {
//                            Console.WriteLine(exception);
//                        }
//                    }
//                    else
//                    {
//                        PopupDialogController dialog = new PopupDialogController(ActivityContext, ActivityContext.VideoDataWithEventsLoader.VideoDataHandler, "Login");
//                        dialog.ShowNormalDialog(this.GetText(Resource.String.Lbl_Warning), this.GetText(Resource.String.Lbl_Please_sign_in_Like), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
//                    }
//                }
//                else
//                {
//                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
//                }
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine(exception);
//            }
//        }
//        private async void btnShare_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                //Share Plugin same as video
//                if (!CrossShare.IsSupported)
//                {
//                    return;
//                }

//                await CrossShare.Current.Share(new ShareMessage
//                {
//                    Text = string.IsNullOrEmpty(selectedMedia.Description) ? "" : selectedMedia.Description,
//                    Url = System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.VideoLocation)
//                });

//                await Task.Delay(TimeSpan.FromSeconds(5));
//                string msg = UserResponseAPI.ShareVideoResponse(selectedMedia.Id.ToString());
//                if (msg.Contains("ok"))
//                {
//                    var x = Convert.ToDouble(txtShareNumber.Text);
//                    x++;
//                    txtShareNumber.Text = x.ToString(System.Globalization.CultureInfo.InvariantCulture);
//                    int index = mediaList.FindIndex(x => x.Id == selectedMedia.Id);
//                    mediaList[index].Shares = (Convert.ToDouble(mediaList[index].Shares) + 1).ToString();

//                    if (ActivityType == "Follow")
//                    {
//                        //TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses =
//                        //new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetFollowingResponseVideos().VideoResponse);
//                        TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses
//                        .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Shares = mediaList[index].Shares;
//                        TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater
//                        .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                    }
//                    else
//                    {
//                        //TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses =
//                        //new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetForYouResponseVideos().VideoResponse);
//                        TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses
//                        .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Shares = mediaList[index].Shares;
//                        TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater
//                        .NotifyItemChanged(mediaList.FindIndex(x => x.Id == selectedMedia.Id));
//                    }
//                }
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine(exception);
//            }
//        }
//        private void btnComment_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                //txtHideComment.Visibility = ViewStates.Visible;
//                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();

//                if (!CommentsFragment.IsAdded)
//                {
//                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
//                    ftvideo.AddToBackStack(null);
//                    ftvideo.Add(CommentsLayout.Id, CommentsFragment, null).Commit();
//                }
//                else
//                {
//                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
//                    ftvideo.Show(CommentsFragment).Commit();
//                }

//                CommentsFragment.StartApiService(selectedMedia.Id.ToString(), "0");
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine(exception);
//            }
//        }
//        public static HomeResponsePlayerActivity GetInstance()
//        {
//            try
//            {
//                return Instance;
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                return null;
//            }
//        }

//        public void ShowReplyResponseCommentFragment(dynamic comment, string type)
//        {
//            try
//            {
//                HomeReplyCommentBottomSheet replyFragment = new HomeReplyCommentBottomSheet();
//                Bundle bundle = new Bundle();

//                bundle.PutString("Type", type);
//                bundle.PutString("Object", JsonConvert.SerializeObject(comment));
//                replyFragment.Arguments = bundle;

//                replyFragment.Show(SupportFragmentManager, replyFragment.Tag);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//            }
//        }
//        //private void txtHideComment_Click(object sender, EventArgs e)
//        //{
//        //    txtHideComment.Visibility = ViewStates.Gone;
//        //    FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
//        //    ftvideo.AddToBackStack(null);
//        //    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
//        //    ftvideo.Hide(CommentsFragment).Commit();
//        //}

//        private void BtnDelete_Click(object sender, EventArgs e)
//        {
//            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
//            Android.App.AlertDialog alert = dialog.Create();
//            alert.SetTitle("Confirmation");
//            alert.SetMessage("Are you sure you want to delete the response?");
//            alert.SetButton("OK", (c, ev) =>
//            {
//                DeleteLatestVideo().FireAndForgetSafeAsync();
//            });
//            alert.SetButton2("CANCEL", (c, ev) => { alert.Hide(); });
//            alert.Show();
//        }

//        public async Task DeleteLatestVideo()
//        {
//            try
//            {
//                this.RunOnUiThread(() => LoadingView.Show(this, "Deleting..."));
//                var deleteResponse = await UserResponseAPI.DeleteResponse(selectedMedia.Id);
//                TabbedMainActivity.GetInstance().MyChannelFragment.latestResponse.adpater.LatestResponse =
//                new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetAdminVideos().VideoResponse);
//                this.RunOnUiThread(() => Toast.MakeText(this, "Deleted Successfully", ToastLength.Long));
//            }
//            catch (Exception ex)
//            {

//            }
//            finally
//            {
//                this.RunOnUiThread(() => LoadingView.Hide());
//                this.Finish();
//            }
//        }

//        protected override void OnStart()
//        {
//            base.OnStart();
//            var tabActivity = TabbedMainActivity.GetInstance();
//            if (tabActivity != null)
//            {
//                tabActivity.VideoActionsController.SetStopvideo();
//            }
//        }

//        //protected override void OnPause()
//        //{
//        //    OverridePendingTransition(Resource.Animation.response_slide_hold, Resource.Animation.response_slide_out);
//        //    base.OnPause();
//        //}

//        private void BtnAcceptReject_Click(object sender, EventArgs e)
//        {
//            var responseId = UserResponseAPI.AcceptRejectResponse(selectedMedia.Id);
//            if (responseId == 1)
//            {
//                btnAcceptReject.Text = "Reject";
//                btnAcceptReject.Background = Resources.GetDrawable(Resource.Drawable.Shape_Radius_Btn);
//                btnAcceptReject.BackgroundTintList = Resources.GetColorStateList(Resource.Color.gnt_red);
//            }
//            else
//            {
//                btnAcceptReject.Text = "Accept";
//                btnAcceptReject.Background = Resources.GetDrawable(Resource.Drawable.Shape_Radius_Btn);
//                btnAcceptReject.BackgroundTintList = Resources.GetColorStateList(Android.Resource.Color.HoloGreenLight);
//            }
//            TabbedMainActivity.GetInstance().MyChannelFragment.latestResponse.adpater.LatestResponse
//                .Where(x => x.Id == selectedMedia.Id).FirstOrDefault().Status = responseId;
//        }

//        private void InitializePlayer()
//        {
//            try
//            {
//                if (selectedMedia.Status == 1)
//                {
//                    btnAcceptReject.Text = "Reject";
//                    btnAcceptReject.Background = Resources.GetDrawable(Resource.Drawable.Shape_Radius_Btn);
//                    btnAcceptReject.BackgroundTintList = Resources.GetColorStateList(Resource.Color.gnt_red);
//                }
//                else
//                {
//                    btnAcceptReject.Text = "Accept";
//                    btnAcceptReject.Background = Resources.GetDrawable(Resource.Drawable.Shape_Radius_Btn);
//                    btnAcceptReject.BackgroundTintList = Resources.GetColorStateList(Android.Resource.Color.HoloGreenLight);
//                }
//                AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
//                var trackSelector = new DefaultTrackSelector(trackSelectionFactory);

//                var newParameters = new DefaultTrackSelector.ParametersBuilder()
//                    .SetMaxVideoSizeSd()
//                    .Build();

//                trackSelector.SetParameters(newParameters);

//                //DefaultLoadControl.Builder builder = new DefaultLoadControl.Builder();

//                //builder.SetBufferDurationsMs(DefaultLoadControl.DefaultMinBufferMs, 60000, DefaultLoadControl.DefaultBufferForPlaybackMs, DefaultLoadControl.DefaultBufferForPlaybackAfterRebufferMs);
//                //DefaultLoadControl loadControl = builder.CreateDefaultLoadControl();
//                //                
//                Player = ExoPlayerFactory.NewSimpleInstance(this, trackSelector);

//            }
//            catch (System.Exception)
//            {

//            }
//        }
//        //private IMediaSource CreateCacheMediaSource(IMediaSource videoSource, Android.Net.Uri videoUrL)
//        //{
//        //    try
//        //    {
//        //        var url = System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.VideoLocation);
//        //        var file = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(url);

//        //        SimpleCache cache = new SimpleCache(this.CacheDir, new LeastRecentlyUsedCacheEvictor(1024 * 1024 * 10));
//        //        CacheDataSourceFactory cacheDataSource = new CacheDataSourceFactory(cache, DefaultDataMediaFactory);

//        //        if (!string.IsNullOrEmpty(file))
//        //        {
//        //            videoUrL = Android.Net.Uri.Parse(file);

//        //            videoSource = GetMediaSourceFromUrl(videoUrL, "normal");
//        //            return videoSource;
//        //        }
//        //        else
//        //        {
//        //            return null;
//        //        }
//        //    }
//        //    catch (Exception exception)
//        //    {
//        //        Console.WriteLine(exception);
//        //        return null;
//        //    }
//        //}

//        public void Play()
//        {

//            try
//            {
//                progressBar.Visibility = ViewStates.Visible;
//                if (Player != null)
//                {
//                    Player.Stop();
//                    Player.Release();
//                }
//                InitializePlayer();
//                if (selectedMedia.VideoLocation.Contains(".mp4"))
//                {
//                    imgAudio.Visibility = ViewStates.Gone;
//                    SimpleExoPlayerView.Visibility = ViewStates.Visible;
//                    SimpleExoPlayerView.ResizeMode = AspectRatioFrameLayout.ResizeModeZoom;
//                    SimpleExoPlayerView.Player = Player;
//                    SimpleExoPlayerView.UseController = false;
//                    swipListener.SwipeEvent -= SwipListener_SwipeEvent;
//                    swipListener.SwipeEvent += SwipListener_SwipeEvent;
//                    SimpleExoPlayerView.SetOnTouchListener(swipListener);
//                }
//                else
//                {
//                    imgAudio.Visibility = ViewStates.Visible;
//                    SimpleExoPlayerView.Visibility = ViewStates.Gone;
//                    swipListener.SwipeEvent -= SwipListener_SwipeEvent;
//                    swipListener.SwipeEvent += SwipListener_SwipeEvent;
//                    imgAudio.SetOnTouchListener(swipListener);
//                    Glide.With(this).Load(System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.Thumbnail)).Into(imgAudio);
//                }
//                var url = Android.Net.Uri.Parse(System.IO.Path.Combine(UserResponseURL.DirectoryURL, selectedMedia.VideoLocation));
//                var VideoSource = GetMediaSourceFromUrl(url, "normal");
//                Player.Prepare(VideoSource);
//                HomePlayerListener playerListener = new HomePlayerListener(this);
//                playerListener.MediaPlayCompleted = () =>
//                {
//                    if (!string.IsNullOrEmpty(UserDetails.AccessToken))
//                    {
//                        UserResponseAPI.ViewVideoResponse(selectedMedia.Id.ToString());
//                    }
//                };
//                Player.AddListener(playerListener);
//                Player.PlayWhenReady = true;
//                Glide.With(this).Load(UserResponseURL.DirectoryUserSettings + selectedMedia.Avatar).Into(imgProfile);
//                txtUserName.Text = selectedMedia.Username;
//                txtdays.Text = Helpers.DateTimeHelper.GetDateTimeString(selectedMedia.Timestamp);
//                txtDescription.Text = selectedMedia.Description;
//                txtLikeNumber.Text = selectedMedia.Likes;
//                txtShareNumber.Text = selectedMedia.Shares;
//                txtCommentNumber.Text = selectedMedia.Comments;

//                if (selectedMedia.IsLiked == "1") // true
//                {
//                    btnLike.Tag = "1";
//                    btnLike.SetImageResource(Resource.Drawable.like_blue);
//                }
//                else
//                {
//                    btnLike.Tag = "0";
//                    btnLike.SetImageResource(Resource.Drawable.like_white);
//                }
//            }
//            catch (System.Exception ex)
//            {

//            }
//        }

//        private IMediaSource GetMediaSourceFromUrl(Android.Net.Uri uri, string tag)
//        {
//            try
//            {
//                var mBandwidthMeter = new DefaultBandwidthMeter();
//                DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(this, Util.GetUserAgent(this, AppSettings.ApplicationName), mBandwidthMeter);
//                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(this, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(this, AppSettings.ApplicationName), new DefaultBandwidthMeter()));
//                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(this, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(this, AppSettings.ApplicationName), null));
//                int type = Util.InferContentType(uri, null);
//                var src = type switch
//                {
//                    C.TypeSs => new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
//                    C.TypeDash => new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
//                    C.TypeHls => new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
//                    C.TypeOther => new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
//                    _ => new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri)
//                };
//                return src;
//            }
//            catch (System.Exception e)
//            {
//                Console.WriteLine(e);
//                return null;
//            }
//        }



//        private void PlayPause()
//        {
//            try
//            {
//                if (Player != null)
//                {
//                    Player.PlayWhenReady = !Player.PlayWhenReady;
//                }
//            }
//            catch (System.Exception ex)
//            {

//            }
//        }

//        private void PlayPrevious()
//        {
//            try
//            {
//                if (mediaList != null && mediaList.Count > 0)
//                {
//                    int index = mediaList.FindIndex(x => x.Id == selectedMedia.Id);
//                    index -= 1;
//                    if (index >= 0)
//                    {
//                        selectedMedia = mediaList[index];
//                        Play();
//                    }
//                    else
//                    {
//                        Toast.MakeText(this, "No more response available to play", ToastLength.Long);
//                        this.Finish();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {

//            }
//        }

//        public void PlayNext()
//        {
//            try
//            {
//                if (mediaList != null && mediaList.Count > 0)
//                {
//                    int index = mediaList.FindIndex(x => x.Id == selectedMedia.Id);
//                    index += 1;
//                    if (mediaList.Count >= index + 1)
//                    {
//                        selectedMedia = mediaList[index];
//                        Play();
//                    }
//                    else
//                    {
//                        Toast.MakeText(this, "No more response available to play", ToastLength.Long);
//                        this.Finish();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {

//            }
//        }
//        private void SwipListener_SwipeEvent(SwipeEnum swipe)
//        {
//            try
//            {
//                if (swipe == SwipeEnum.Top)
//                {
//                    try
//                    {
//                        if (mediaList != null && mediaList.Count > 0)
//                        {
//                            int index = mediaList.FindIndex(x => x.Id == selectedMedia.Id);
//                            index += 1;
//                            if (mediaList.Count >= index + 1)
//                            {
//                                var item = mediaList[index];
//                                Intent intent = new Intent(this, typeof(HomeResponsePlayerActivity));
//                                intent.PutExtra("mediaList", JsonConvert.SerializeObject(mediaList));
//                                intent.PutExtra("selectedMedia", JsonConvert.SerializeObject(item));
//                                intent.PutExtra("type", ActivityType);
//                                intent.PutExtra("swipeAction", "SwipeUp");
//                                this.Finish();
//                                StartActivity(intent);
//                            }
//                            else
//                            {
//                                Toast.MakeText(this, "No more response available to play", ToastLength.Long);
//                                this.Finish();
//                            }
//                        }
//                    }
//                    catch (Exception ex)
//                    {

//                    }

//                    //PlayNext();
//                }
//                else if (swipe == SwipeEnum.Bottom)
//                {
//                    try
//                    {
//                        if (mediaList != null && mediaList.Count > 0)
//                        {
//                            int index = mediaList.FindIndex(x => x.Id == selectedMedia.Id);
//                            index -= 1;
//                            if (index >= 0)
//                            {
//                                var item = mediaList[index];
//                                Intent intent = new Intent(this, typeof(HomeResponsePlayerActivity));
//                                intent.PutExtra("mediaList", JsonConvert.SerializeObject(mediaList));
//                                intent.PutExtra("selectedMedia", JsonConvert.SerializeObject(item));
//                                intent.PutExtra("type", ActivityType);
//                                intent.PutExtra("swipeAction", "SwipeDown");
//                                this.Finish();
//                                StartActivity(intent);
//                            }
//                            else
//                            {
//                                Toast.MakeText(this, "No more response available to play", ToastLength.Long);
//                                this.Finish();
//                            }
//                        }
//                    }
//                    catch (Exception ex)
//                    {

//                    }

//                    //PlayPrevious();
//                }
//                else if (swipe == SwipeEnum.SingleTab)
//                {
//                    PlayPause();
//                }
//            }
//            catch (System.Exception ex)
//            {

//            }
//        }
//        protected override void OnDestroy()
//        {
//            try
//            {
//                base.OnDestroy();
//                //TabbedMainActivity.GetInstance().MyChannelFragment.latestResponse.adpater.NotifyDataSetChanged();
//                if (Player != null)
//                {
//                    Player.Stop();
//                    Player.Release();
//                }
//                if (userObject != null)
//                    TabbedMainActivity.GetInstance().ShowUserChannelFragment(userObject, selectedMedia.UserID);
//            }
//            catch (Exception ex)
//            {

//            }
//        }

    }
//    internal class HomePlayerListener : Java.Lang.Object, IPlayerEventListener, PlayerControlView.IVisibilityListener
//    {
//        public Action MediaPlayCompleted;
//        private HomeResponsePlayerActivity mediaPlayerActivity;
//        private readonly ProgressBar LoadingProgressBar;
//        public HomePlayerListener(HomeResponsePlayerActivity mediaPlayerActivity)
//        {
//            this.mediaPlayerActivity = mediaPlayerActivity;
//            LoadingProgressBar = mediaPlayerActivity.FindViewById<ProgressBar>(Resource.Id.progressBar);
//        }


//        public void Dispose()
//        {

//        }

//        public void OnLoadingChanged(bool isLoading)
//        {

//        }

//        public void OnPlaybackParametersChanged(PlaybackParameters playbackParameters)
//        {

//        }

//        public void OnPlayerError(ExoPlaybackException error)
//        {

//        }

//        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
//        {
//            try
//            {
//                if (LoadingProgressBar == null)
//                    return;

//                if (playbackState == Player.StateEnded)
//                {
//                    MediaPlayCompleted?.Invoke();
//                    mediaPlayerActivity.Play();
//                    //mediaPlayerActivity.PlayNext();
//                }
//                else if (playbackState == Player.StateReady)
//                {
//                    //var videoFormat = mediaPlayerActivity.Player.VideoFormat;
//                    //if (videoFormat.Width > videoFormat.Height)
//                    //    mediaPlayerActivity.SimpleExoPlayerView.ResizeMode = AspectRatioFrameLayout.ResizeModeFit;
//                    //else
//                    //    mediaPlayerActivity.SimpleExoPlayerView.ResizeMode = AspectRatioFrameLayout.ResizeModeZoom;

//                    //LoadingProgressBar.Visibility = ViewStates.Invisible;
//                }
//                else if (playbackState == Player.StateBuffering)
//                {
//                    LoadingProgressBar.Visibility = ViewStates.Visible;

//                }
//            }
//            catch (Exception exception)
//            {
//                Console.WriteLine(exception);
//            }
//        }

//        public void OnPositionDiscontinuity(int reason)
//        {

//        }

//        public void OnRepeatModeChanged(int repeatMode)
//        {

//        }

//        public void OnSeekProcessed()
//        {

//        }

//        public void OnShuffleModeEnabledChanged(bool shuffleModeEnabled)
//        {

//        }

//        public void OnTimelineChanged(Timeline timeline, Java.Lang.Object manifest, int reason)
//        {

//        }

//        public void OnTracksChanged(TrackGroupArray trackGroups, TrackSelectionArray trackSelections)
//        {

//        }

//        public void OnVisibilityChange(int visibility)
//        {

//        }
//    }
}