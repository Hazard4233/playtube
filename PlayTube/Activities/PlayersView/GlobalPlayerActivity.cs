using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AFollestad.MaterialDialogs;
using Com.Google.Android.Youtube.Player;
using PlayTube.Activities.Comments;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Controller;
using PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Util;
using Com.Github.Library.Bubbleview;
using Com.Luseen.Autolinklibrary;
using Java.Lang;
using Newtonsoft.Json;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Payment;
using PlayTube.PaymentGoogle;
using PlayTube.SQLite;
using PlayTubeClient;
using PlayTubeClient.Classes.Video;
using PlayTubeClient.RestCalls;
using Xamarin.PayPal.Android;
using Exception = System.Exception;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;
using Fragment = Android.Support.V4.App.Fragment;
using Math = System.Math;
using PlayTube.Activities.UserReponse;
using PlayTube.Helpers;
using PlayTube.API;
using System.Net.Http;
using RestSharp.Extensions;
using PlayTube.API.Models;

namespace PlayTube.Activities.PlayersView
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode, LaunchMode = LaunchMode.SingleInstance, ResizeableActivity = true, SupportsPictureInPicture = true)]
    public class GlobalPlayerActivity : AppCompatActivity, AppBarLayout.IOnOffsetChangedListener, IYouTubePlayerOnInitializedListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    {
        #region Variables Basic

        private TextView ShareIconView, AddToIconView, ShowMoreDescriptionIconView, TextChannelName, EditIconView, TxtSubscribeCount;
        private ImageView ImageChannelView, LikeIconView, UnLikeIconView, ViewMoreCommentSection;
        private Button SubscribeChannelButton;
        private LinearLayout VideoDescriptionLayout, LikeButton, UnLikeButton, ShareButton, AddToButton, EditButton;
        private TextView VideoTitle, VideoLikeCount, VideoUnLikeCount, VideoChannelViews, VideoPublishDate, VideoCategory, UpNextTextview;
        private AutoLinkTextView VideoDescription;
        private TextSanitizer TextSanitizerAutoLink;
        private VideoObject VideoDataHandler;
        private VideoDataWithEventsLoader.VideoEnumTypes VideoType;
        private LinearLayout PaymentLayout;
        private LinearLayout DonateButton, RentButton;
        private string TypeDialog, PayType;
        private Switch AutoNextswitch;
        private FrameLayout VideoButtomLayout;
        private RelativeLayout CommentButtomLayout, CircleLayout;
        private BubbleLinearLayout SubscribeCountLayout;
        public CommentsFragment CommentsFragment;
        public UserResponseTabsFragment UserResponseTabs;
        public IYouTubePlayer YoutubePlayer { get; set; }
        public LibrarySynchronizer LibrarySynchronizer;
        public VideoController VideoActionsController;
        private AppBarLayout AppBarLayoutView;
        private CoordinatorLayout CoordinatorLayoutView;
        private YouTubePlayerSupportFragment YouTubeFragment;
        private RestrictedVideoFragment RestrictedVideoPlayerFragment;
        private ThirdPartyPlayersFragment ThirdPartyPlayersFragment;
        private string VideoIdYoutube;
        private Button btnAddAudioResponse, btnAddVideoResponse;
        private Handler handler;
        private Action VideoTracking;
        public int StartTime = 0, EndTime = 0;

        private static GlobalPlayerActivity Instance;
        private bool OnStopCalled;
        public static bool OnOpenPage;
        private InitPayPalPayment InitPayPalPayment;
        private InitInAppBillingPayment BillingPayment;
        private Button JoinButton;
        private TextView txtHideComment;


        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.GlobalPlayerLayout);

                Instance = this;
                OnStopCalled = false;
                OnOpenPage = true;

                var videoObject = Intent.GetStringExtra("VideoObject");
                if (!string.IsNullOrEmpty(videoObject))
                    VideoDataHandler = JsonConvert.DeserializeObject<VideoObject>(videoObject);

                SetVideoPlayerFragmentAdapters();
                InitComponent();

                if (VideoDataHandler == null)
                    return;

                LoadVideoData(VideoDataHandler);
                StartPlayVideo(VideoDataHandler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
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

        protected override void OnStop()
        {
            try
            {
                OnStopCalled = true;
                base.OnStop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                VideoActionsController?.SetStopvideo();

                if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                    YoutubePlayer?.Pause();

                OnOpenPage = false;

                InitPayPalPayment?.StopPayPalService();
                BillingPayment?.DisconnectInAppBilling();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            try
            {
                base.OnNewIntent(intent);

                //Called onNewIntent
                VideoActionsController.SetStopvideo();

                if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                    YoutubePlayer?.Pause();

                var videoObject = intent.GetStringExtra("VideoObject");
                if (!string.IsNullOrEmpty(videoObject))
                    VideoDataHandler = JsonConvert.DeserializeObject<VideoObject>(videoObject);

                InitComponent();

                if (VideoDataHandler == null)
                    return;

                LoadVideoData(VideoDataHandler);
                StartPlayVideo(VideoDataHandler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Back Pressed

        private bool IsPipModeEnabled = true;
        public override void OnBackPressed()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture) && IsPipModeEnabled)
                {
                    switch (VideoType)
                    {
                        case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                            EnterPipMode();
                            break;
                        case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                            base.OnBackPressed();
                            break;
                    }
                }
                else
                {
                    base.OnBackPressed();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                base.OnBackPressed();
            }
        }

        private void EnterPipMode()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        Rational rational = new Rational(450, 250);
                        PictureInPictureParams.Builder builder = new PictureInPictureParams.Builder();
                        builder.SetAspectRatio(rational);
                        EnterPictureInPictureMode(builder.Build());
                    }
                    else
                    {
                        var param = new PictureInPictureParams.Builder().Build();
                        EnterPictureInPictureMode(param);
                    }

                    new Handler().PostDelayed(CheckPipPermission, 30);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void CheckPipPermission()
        {
            IsPipModeEnabled = IsInPictureInPictureMode;
            if (!IsInPictureInPictureMode)
            {
                OnBackPressed();
            }
        }

        private void BackIcon_Click(object sender, EventArgs e)
        {
            try
            {
                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var param = new PictureInPictureParams.Builder().Build();
                            EnterPictureInPictureMode(param);
                        }
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        FinishActivityAndTask();
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                CoordinatorLayoutView = FindViewById<CoordinatorLayout>(Resource.Id.parent);
                AppBarLayoutView = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayoutView.AddOnOffsetChangedListener(this);

                LikeIconView = FindViewById<ImageView>(Resource.Id.Likeicon);
                UnLikeIconView = FindViewById<ImageView>(Resource.Id.UnLikeicon);
                ShareIconView = FindViewById<TextView>(Resource.Id.Shareicon);
                AddToIconView = FindViewById<TextView>(Resource.Id.AddToicon);
                EditIconView = FindViewById<TextView>(Resource.Id.editIcon);
                JoinButton = FindViewById<Button>(Resource.Id.JoinButton);
                JoinButton.Click += JoinButton_Click;

                ShowMoreDescriptionIconView = FindViewById<TextView>(Resource.Id.video_ShowDiscription);
                VideoDescriptionLayout = FindViewById<LinearLayout>(Resource.Id.videoDescriptionLayout);
                ImageChannelView = FindViewById<ImageView>(Resource.Id.Image_Channel);
                TextChannelName = FindViewById<TextView>(Resource.Id.ChannelName);
                SubscribeChannelButton = FindViewById<Button>(Resource.Id.SubcribeButton);
                txtHideComment = FindViewById<TextView>(Resource.Id.txtHideComment);
                txtHideComment.Click += TxtHideComment_Click;

                SubscribeCountLayout = FindViewById<BubbleLinearLayout>(Resource.Id.bubble_layout);
                TxtSubscribeCount = FindViewById<TextView>(Resource.Id.subcriberscount);

                LikeButton = FindViewById<LinearLayout>(Resource.Id.LikeButton);
                UnLikeButton = FindViewById<LinearLayout>(Resource.Id.UnLikeButton);
                ShareButton = FindViewById<LinearLayout>(Resource.Id.ShareButton);
                AddToButton = FindViewById<LinearLayout>(Resource.Id.AddToButton);
                EditButton = FindViewById<LinearLayout>(Resource.Id.editButton);

                LikeButton.Tag = "0";
                UnLikeButton.Tag = "0";

                PaymentLayout = FindViewById<LinearLayout>(Resource.Id.PaymentLayout);
                RentButton = FindViewById<LinearLayout>(Resource.Id.RentButton);
                DonateButton = FindViewById<LinearLayout>(Resource.Id.DonateButton);

                RentButton.Visibility = ViewStates.Gone;
                DonateButton.Visibility = ViewStates.Gone;

                VideoTitle = FindViewById<TextView>(Resource.Id.video_Titile);
                VideoLikeCount = FindViewById<TextView>(Resource.Id.LikeNumber);
                VideoUnLikeCount = FindViewById<TextView>(Resource.Id.UnLikeNumber);
                VideoChannelViews = FindViewById<TextView>(Resource.Id.Channelviews);
                VideoPublishDate = FindViewById<TextView>(Resource.Id.videoDate);
                VideoDescription = FindViewById<AutoLinkTextView>(Resource.Id.videoDescriptionTextview);
                VideoCategory = FindViewById<TextView>(Resource.Id.videoCategorytextview);

                VideoButtomLayout = FindViewById<FrameLayout>(Resource.Id.videoButtomLayout);
                CommentButtomLayout = FindViewById<RelativeLayout>(Resource.Id.CommentButtomLayout);
                UpNextTextview = FindViewById<TextView>(Resource.Id.UpNextTextview);
                ViewMoreCommentSection = FindViewById<ImageView>(Resource.Id.viewMoreCommentsection);
                AutoNextswitch = FindViewById<Switch>(Resource.Id.AutoNextswitch);
                AutoNextswitch.Checked = UserDetails.AutoNext;
                AutoNextswitch.Visibility = ViewStates.Gone;

                CircleLayout = FindViewById<RelativeLayout>(Resource.Id.circlelayout);

                btnAddAudioResponse = FindViewById<Button>(Resource.Id.btnAddAudioResponse);
                btnAddVideoResponse = FindViewById<Button>(Resource.Id.btnAddVideoResponse);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIconView, IonIconsFonts.ReplyAll);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AddToIconView, IonIconsFonts.PlusCircled);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDownB);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EditIconView, IonIconsFonts.AndroidCreate);

                TextSanitizerAutoLink = new TextSanitizer(VideoDescription, this);

                VideoActionsController = new VideoController(this, "GlobalPlayer");
                VideoActionsController.ExoBackButton.Click += BackIcon_Click;

                btnAddAudioResponse.Click += BtnAddAudioResponse_Click;
                btnAddVideoResponse.Click += BtnAddVideoResponse_Click;

                LibrarySynchronizer = new LibrarySynchronizer(this);
                InitPayPalPayment = new InitPayPalPayment(this);
                BillingPayment = new InitInAppBillingPayment(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void TxtHideComment_Click(object sender, EventArgs e)
        {
            txtHideComment.Visibility = ViewStates.Gone;
            ViewMoreCommentSection.Visibility = ViewStates.Gone;
            CommentButtomLayout.Visibility = ViewStates.Visible;
            FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
            ftvideo.AddToBackStack(null);
            ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
            ftvideo.Hide(CommentsFragment).Show(UserResponseTabs.NextFragment).Commit();
        }

        private void JoinButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(UserDetails.AccessToken))
            {
                UserResponsePopup mFragment = new UserResponsePopup(VideoDataHandler.VideoId, this.VideoActionsController, UserResponseTabs.ResponseFragment);
                mFragment.Show(SupportFragmentManager, mFragment.Tag);
            }
            else
            {
                Snackbar.Make(this.Window.DecorView, "You are not logged in!", Snackbar.LengthLong);
            }
        }

        private void BtnAddVideoResponse_Click(object sender, EventArgs e)
        {
            UserResponseBottomSheet mFragment = new UserResponseBottomSheet(UserResponse.Video, VideoDataHandler.VideoId, this.VideoActionsController, UserResponseTabs.ResponseFragment, YoutubePlayer);
            mFragment.Show(this.SupportFragmentManager, mFragment.Tag);
        }

        private void BtnAddAudioResponse_Click(object sender, EventArgs e)
        {
            UserResponseBottomSheet mFragment = new UserResponseBottomSheet(UserResponse.Audio, VideoDataHandler.VideoId, this.VideoActionsController, UserResponseTabs.ResponseFragment, YoutubePlayer);
            mFragment.Show(this.SupportFragmentManager, mFragment.Tag);
        }

        public static GlobalPlayerActivity GetInstance()
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    LikeButton.Click += LikeButtonOnClick;
                    UnLikeButton.Click += UnLikeButtonOnClick;
                    ShareButton.Click += ShareButtonOnClick;
                    AddToButton.Click += AddToButtonOnClick;
                    SubscribeChannelButton.Click += SubscribeChannelButtonOnClick;
                    TextChannelName.Click += ImageChannelViewOnClick;
                    VideoCategory.Click += VideoCategoryOnClick;
                    ImageChannelView.Click += ImageChannelViewOnClick;
                    ShowMoreDescriptionIconView.Click += ShowMoreDescriptionIconViewOnClick;
                    EditButton.Click += EditButtonOnClick;
                    RentButton.Click += RentButtonOnClick;
                    DonateButton.Click += DonateButtonOnClick;
                    CommentButtomLayout.Click += CommentButtomLayout_Click;
                    ViewMoreCommentSection.Click += ViewMoreCommentSectionClick;
                    UpNextTextview.Click += ViewMoreCommentSectionClick;
                    AutoNextswitch.CheckedChange += AutoNextswitchOnCheckedChange;
                }
                else
                {
                    LikeButton.Click -= LikeButtonOnClick;
                    UnLikeButton.Click -= UnLikeButtonOnClick;
                    ShareButton.Click -= ShareButtonOnClick;
                    AddToButton.Click -= AddToButtonOnClick;
                    SubscribeChannelButton.Click -= SubscribeChannelButtonOnClick;
                    TextChannelName.Click -= ImageChannelViewOnClick;
                    VideoCategory.Click -= VideoCategoryOnClick;
                    ImageChannelView.Click -= ImageChannelViewOnClick;
                    ShowMoreDescriptionIconView.Click -= ShowMoreDescriptionIconViewOnClick;
                    EditButton.Click -= EditButtonOnClick;
                    RentButton.Click -= RentButtonOnClick;
                    DonateButton.Click -= DonateButtonOnClick;
                    CommentButtomLayout.Click -= CommentButtomLayout_Click;
                    ViewMoreCommentSection.Click -= ViewMoreCommentSectionClick;
                    UpNextTextview.Click -= ViewMoreCommentSectionClick;
                    AutoNextswitch.CheckedChange -= AutoNextswitchOnCheckedChange;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void EditButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent();
                intent.PutExtra("Open", "EditVideo");
                intent.PutExtra("ItemDataVideo", JsonConvert.SerializeObject(VideoDataHandler));
                SetResult(Result.Ok, intent);

                if (VideoType == VideoDataWithEventsLoader.VideoEnumTypes.Normal)
                    VideoActionsController.SetStopvideo();
                else if (VideoType == VideoDataWithEventsLoader.VideoEnumTypes.Youtube)
                    YoutubePlayer.Pause();

                FinishActivityAndTask();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void AutoNextswitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                UserDetails.AutoNext = AutoNextswitch.Checked;
                MainSettings.AutoNext.Edit().PutBoolean(MainSettings.PrefKeyAutoNext, UserDetails.AutoNext).Commit();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void CommentButtomLayout_Click(object sender, EventArgs e)
        {
            try
            {
                UpNextTextview.Text = GetString(Resource.String.Lbl_Comments);
                UpNextTextview.Tag = GetString(Resource.String.Lbl_Comments);
                ViewMoreCommentSection.Visibility = ViewStates.Visible;
                AutoNextswitch.Visibility = ViewStates.Gone;
                CommentButtomLayout.Visibility = ViewStates.Gone;
                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();

                if (!CommentsFragment.IsAdded)
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    ftvideo.AddToBackStack(null);
                    ftvideo.Add(VideoButtomLayout.Id, CommentsFragment, null).Commit();
                }
                else
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    //ftvideo.Hide(UserResponseTabs.NextFragment).Show(CommentsFragment).Commit();
                }

                CommentsFragment.StartApiService(VideoDataHandler.Id, "0");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ViewMoreCommentSectionClick(object sender, EventArgs e)
        {
            try
            {
                UpNextTextview.Text = GetString(Resource.String.Lbl_NextTo);
                UpNextTextview.Tag = GetString(Resource.String.Lbl_NextTo);
                ViewMoreCommentSection.Visibility = ViewStates.Gone;
                AutoNextswitch.Visibility = ViewStates.Visible;
                CommentButtomLayout.Visibility = ViewStates.Visible;

                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
                ftvideo.AddToBackStack(null);
                ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                ftvideo.Hide(CommentsFragment).Show(UserResponseTabs.NextFragment).Commit();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

        }

        private void AddToButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "AddTo";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                var arrayAdapter = new List<string> { GetString(Resource.String.Lbl_Addto_playlist) };

                var check = ListUtils.WatchLaterVideosList.FirstOrDefault(q => q.Videos?.VideoAdClass.Id == VideoDataHandler.Id);
                arrayAdapter.Add(check == null ? GetString(Resource.String.Lbl_Addto_watchlater) : GetString(Resource.String.Lbl_RemoveFromWatchLater));

                dialogList.Title(GetString(Resource.String.Lbl_Add_To));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetString(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ShareButtonOnClick(object sender, EventArgs e)
        {
            LibrarySynchronizer.ShareVideo(VideoDataHandler);
        }

        private void UnLikeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        if (UnLikeButton.Tag.ToString() == "0")
                        {
                            UnLikeButton.Tag = "1";
                            UnLikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                            LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));

                            if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                            {
                                var x = Convert.ToDouble(VideoUnLikeCount.Text);
                                x++;
                                VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                            }

                            if (LikeButton.Tag.ToString() == "1")
                            {
                                LikeButton.Tag = "0";
                                if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(VideoUnLikeCount.Text);
                                    if (x > 0)
                                    {
                                        x--;
                                    }
                                    else
                                    {
                                        x = 0;
                                    }

                                    VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                                }
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Video_UnLiked), ToastLength.Short).Show();

                            //Send API Request here for dislike
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.Add_Like_Dislike_Videos_Http(VideoDataHandler.Id, "dislike") });
                        }
                        else
                        {
                            UnLikeButton.Tag = "0";


                            UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                            var x = Convert.ToDouble(VideoUnLikeCount.Text);
                            if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                            {
                                if (x > 0)
                                {
                                    x--;
                                }
                                else
                                {
                                    x = 0;
                                }
                                VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                            }

                            //Send API Request here for Remove UNLike
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.Add_Like_Dislike_Videos_Http(VideoDataHandler.Id, "dislike") });

                        }
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Dislike), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void LikeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        try
                        {
                            //If User Liked
                            if (LikeButton.Tag.ToString() == "0")
                            {
                                LikeButton.Tag = "1";
                                LikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));


                                UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                                if (!VideoLikeCount.Text.Contains("K") && !VideoLikeCount.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(VideoLikeCount.Text);
                                    x++;
                                    VideoLikeCount.Text = x.ToString(CultureInfo.InvariantCulture);
                                }

                                if (UnLikeButton.Tag.ToString() == "1")
                                {
                                    UnLikeButton.Tag = "0";
                                    if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                                    {
                                        var x = Convert.ToDouble(VideoUnLikeCount.Text);
                                        if (x > 0)
                                        {
                                            x--;
                                        }
                                        else
                                        {
                                            x = 0;
                                        }
                                        VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                                    }
                                }


                                //AddToLiked
                                LibrarySynchronizer.AddToLiked(VideoDataHandler);

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short).Show();

                                //Send API Request here for Like
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.Add_Like_Dislike_Videos_Http(VideoDataHandler.Id, "like") });

                            }
                            else
                            {
                                LikeButton.Tag = "0";

                                LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                                UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                                if (!VideoLikeCount.Text.Contains("K") && !VideoLikeCount.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(VideoLikeCount.Text);
                                    if (x > 0)
                                    {
                                        x--;
                                    }
                                    else
                                    {
                                        x = 0;
                                    }

                                    VideoLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                                }

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short).Show();

                                //Send API Request here for Remove UNLike
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.Add_Like_Dislike_Videos_Http(VideoDataHandler.Id, "like") });
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Like), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void VideoCategoryOnClick(object sender, EventArgs e)
        {
            try
            {

                Intent intent = new Intent();
                intent.PutExtra("Open", "VideosByCategory");
                intent.PutExtra("CatId", VideoDataHandler.CategoryId);
                intent.PutExtra("CatName", VideoDataHandler.CategoryName);
                SetResult(Result.Ok, intent);

                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        VideoActionsController.SetStopvideo();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        YoutubePlayer.Pause();
                        break;
                }
                FinishActivityAndTask();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SubscribeChannelButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        if (!string.IsNullOrEmpty(VideoDataHandler.Owner.SubscriberPrice) && VideoDataHandler.Owner.SubscriberPrice != "0")
                        {
                            if (SubscribeChannelButton.Tag.ToString() == "PaidSubscribe")
                            {
                                TypeDialog = "PaidSubscribe";

                                //This channel is paid, You must pay to subscribe
                                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                                dialog.Title(Resource.String.Lbl_Warning);
                                dialog.Content(GetText(Resource.String.Lbl_ChannelIsPaid));
                                dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                                dialog.AlwaysCallSingleChoiceCallback();
                                dialog.Build().Show();
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribe";
                                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribe);
                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                //Remove The Video to Subscribed Videos Database
                                var sqlEntity = new SqLiteDatabase();
                                sqlEntity.RemoveSubscriptionsChannel(VideoDataHandler.Owner.Id);
                                sqlEntity.Dispose();

                                //Send API Request here for UnSubscribed
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Add_Subscribe_To_Channel_Http(VideoDataHandler.Owner.Id) });

                                // Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            if (SubscribeChannelButton.Tag.ToString() == "Subscribe")
                            {
                                SubscribeChannelButton.Tag = "Subscribed";
                                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribed);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                //Add The Video to  Subcribed Videos Database
                                Events_Insert_SubscriptionsChannel();

                                //Send API Request here for Subcribe
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Add_Subscribe_To_Channel_Http(VideoDataHandler.Owner.Id) });

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short).Show();
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribe";
                                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribe);
                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                //Remove The Video to Subcribed Videos Database
                                var sqlEntity = new SqLiteDatabase();
                                sqlEntity.RemoveSubscriptionsChannel(VideoDataHandler.Owner.Id);
                                sqlEntity.Dispose();

                                //Send API Request here for UnSubcribe
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Add_Subscribe_To_Channel_Http(VideoDataHandler.Owner.Id) });

                                // Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short).Show();
                            }
                        }
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Subcribed), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ImageChannelViewOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent();
                intent.PutExtra("Open", "UserProfile");
                intent.PutExtra("UserObject", JsonConvert.SerializeObject(VideoDataHandler.Owner));
                SetResult(Result.Ok, intent);

                FinishActivityAndTask();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ShowMoreDescriptionIconViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (VideoDescriptionLayout.Tag.ToString() == "Open")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDownB);
                    VideoDescriptionLayout.Visibility = ViewStates.Gone;
                    VideoDescriptionLayout.Tag = "closed";
                    VideoTitle.Text = Methods.FunString.DecodeString(VideoDataHandler.Title);
                }
                else
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowUpB);
                    VideoDescriptionLayout.Visibility = ViewStates.Visible;
                    VideoDescriptionLayout.Tag = "Open";
                    VideoTitle.Text = Methods.FunString.DecodeString(VideoDataHandler.Title);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Menu >> WatchLater
        private async void OnMenuAddWatchLaterClick(VideoObject videoObject)
        {
            try
            {
                if (UserDetails.IsLogin)
                {
                    if (Methods.CheckConnectivity())
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Video.AddToWatchLaterVideos_Http(VideoDataHandler.Id);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageCodeObject result)
                            {
                                if (result.SuccessType.Contains("Removed"))
                                {
                                    LibrarySynchronizer.RemovedFromWatchLater(videoObject);
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_RemovedFromWatchLater), ToastLength.Short).Show();
                                }
                                else if (result.SuccessType.Contains("Added"))
                                {
                                    LibrarySynchronizer.AddToWatchLater(videoObject);
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_AddedToWatchLater), ToastLength.Short).Show();
                                }
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(this, videoObject, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_WatchLater), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Menu >> Playlist
        private void OnMenuAddPlaylistClick(VideoObject videoObject)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        PopupDialogController dialog = new PopupDialogController(this, videoObject, "PlayList");
                        dialog.ShowPlayListDialog();
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, videoObject, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_playlist), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Events_Insert_SubscriptionsChannel()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                if (VideoDataHandler.Owner != null)
                    sqlEntity.Insert_One_SubscriptionChannel(VideoDataHandler.Owner);

                sqlEntity.Dispose();

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Rent
        private void RentButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.ShowInAppBilling && Client.IsExtended && AppSettings.VideoRentalPriceStatic)
                {
                    BillingPayment.SetConnInAppBilling();
                    BillingPayment.InitInAppBilling("RentVideo");
                }
                else
                {
                    TypeDialog = "Payment_RentVideo";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    if (AppSettings.ShowPaypal)
                        arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));

                    if (AppSettings.ShowCreditCard)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetString(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Donate
        private void DonateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "Donate";

                new MaterialDialog.Builder(this)
                    .Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light)
                    .Title(Resource.String.Lbl_Donate)
                    .InputType(InputTypes.ClassNumber | InputTypes.NumberVariationNormal)
                    .PositiveText(GetText(Resource.String.Btn_Send)).OnPositive(this)
                    .NegativeText(Resource.String.Lbl_Cancel).OnNegative(this)
                    .Input("0.00", "", this)
                    .Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region PictureInPictur

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                var percentage = ((float)Math.Abs(verticalOffset) / appBarLayout.TotalScrollRange);
                Console.WriteLine(percentage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode, Configuration newConfig)
        {
            try
            {
                CoordinatorLayoutView.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;

                if (VideoActionsController?.ControlView != null)
                    VideoActionsController.ControlView.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;

                if (isInPictureInPictureMode)
                {
                    // ...
                    switch (VideoType)
                    {
                        case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                            //VideoActionsController?.SetStopvideo();
                            break;
                        case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                            YoutubePlayer.Play();
                            break;
                    }
                }
                else
                {
                    if (OnStopCalled)
                    {
                        switch (VideoType)
                        {
                            case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                                VideoActionsController?.SetStopvideo();
                                break;
                            case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                                YoutubePlayer.Pause();
                                break;
                        }

                        FinishActivityAndTask();
                    }
                }

                base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnUserLeaveHint()
        {
            try
            {
                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var param = new PictureInPictureParams.Builder().Build();
                            EnterPictureInPictureMode(param);
                        }
                        base.OnUserLeaveHint();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region YouTube Player

        public void OnInitializationFailure(IYouTubePlayerProvider p0, YouTubeInitializationResult p1)
        {
            try
            {
                if (p1.IsUserRecoverableError)
                    p1.GetErrorDialog(this, 1).Show();
                else
                    Toast.MakeText(this, p1.ToString(), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnInitializationSuccess(IYouTubePlayerProvider p0, IYouTubePlayer player, bool wasRestored)
        {
            try
            {
                if (YoutubePlayer == null)
                    YoutubePlayer = player;

                handler = new Handler();
                VideoTracking = () =>
                {
                    try
                    {
                        //For every 1 second, check the current time and endTime
                        if (EndTime != 0 && YoutubePlayer.CurrentTimeMillis > EndTime)
                        {
                            //handler.RemoveCallbacks(VideoTracking);
                            YoutubePlayer.Pause(); //and Pause the video
                            YoutubePlayer.SeekToMillis(StartTime);
                            YoutubePlayer.Play();
                            handler.PostDelayed(VideoTracking, 1000);
                        }
                        else
                            handler.PostDelayed(VideoTracking, 1000);
                    } catch(Exception) { }
                };
                handler.PostDelayed(VideoTracking, 1000);

                if (!wasRestored)
                {
                    if (StartTime != 0)
                    {
                        YoutubePlayer.LoadVideo(VideoIdYoutube, StartTime);
                        handler.PostDelayed(VideoTracking, 1000);
                    }
                    else
                    {
                        YoutubePlayer.LoadVideo(VideoIdYoutube);
                        handler.PostDelayed(VideoTracking, 1000);
                    }
                    //YoutubePlayer.LoadVideo(VideoIdYoutube);
                    //YoutubePlayer.AddFullscreenControlFlag(YouTubePlayer.FullscreenFlagControlOrientation  | YouTubePlayer.FullscreenFlagControlSystemUi  | YouTubePlayer.FullscreenFlagCustomLayout); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Fun Video 
        public List<Fragment> VideoFrameLayoutFragments = new List<Fragment>();

        private void SetVideoPlayerFragmentAdapters()
        {
            try
            {
                CommentsFragment = new CommentsFragment();
                UserResponseTabs = new UserResponseTabsFragment();
                UserResponseTabs.NextFragment = new NextToFragment();
                UserResponseTabs.ResponseFragment = new UserResponseFragment();

                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
                ftvideo.Add(Resource.Id.videoButtomLayout, UserResponseTabs, UserResponseTabs.Id.ToString() + DateTime.Now).Commit();

                if (YouTubeFragment == null)
                {
                    FragmentTransaction ft = SupportFragmentManager.BeginTransaction();
                    YouTubeFragment = new YouTubePlayerSupportFragment();
                    YouTubeFragment.Initialize(AppSettings.YoutubePlayerKey, this);
                    ft.Add(Resource.Id.root, YouTubeFragment, YouTubeFragment.Id.ToString() + DateTime.Now).Commit();

                    if (!VideoFrameLayoutFragments.Contains(YouTubeFragment))
                        VideoFrameLayoutFragments.Add(YouTubeFragment);
                }
                if (ThirdPartyPlayersFragment == null)
                {
                    FragmentTransaction ft1 = SupportFragmentManager.BeginTransaction();
                    ThirdPartyPlayersFragment = new ThirdPartyPlayersFragment();
                    ft1.Add(Resource.Id.root, ThirdPartyPlayersFragment, DateTime.Now.ToString(CultureInfo.InvariantCulture)).Commit();
                    if (!VideoFrameLayoutFragments.Contains(ThirdPartyPlayersFragment))
                        VideoFrameLayoutFragments.Add(ThirdPartyPlayersFragment);
                }
                if (RestrictedVideoPlayerFragment == null)
                {
                    FragmentTransaction ft2 = SupportFragmentManager.BeginTransaction();
                    RestrictedVideoPlayerFragment = new RestrictedVideoFragment();
                    ft2.Add(Resource.Id.root, RestrictedVideoPlayerFragment, DateTime.Now.ToString(CultureInfo.InvariantCulture)).Commit();
                    if (!VideoFrameLayoutFragments.Contains(RestrictedVideoPlayerFragment))
                        VideoFrameLayoutFragments.Add(RestrictedVideoPlayerFragment);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void LoadVideoData(VideoObject videoObject)
        {
            try
            {
                if (videoObject == null)
                    return;

                VideoDataHandler = videoObject;
                SetVideoType(VideoDataHandler);

                //Run fast data fetch from the server 
                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetVideosInfoAsJson(videoObject.VideoId) });

                GlideImageLoader.LoadImage(this, videoObject.Owner.Avatar, ImageChannelView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);
                TextChannelName.Text = videoObject.Owner.Username;
                VideoLikeCount.Text = videoObject.LikesPercent;
                VideoUnLikeCount.Text = videoObject.DislikesPercent;
                VideoChannelViews.Text = videoObject.Views + " " + GetText(Resource.String.Lbl_Views) + " | " + Methods.Time.ReplaceTime(videoObject.TimeAgo);
                VideoPublishDate.Text = GetText(Resource.String.Lbl_Published_on) + " " + videoObject.TimeDate;
                VideoCategory.Text = CategoriesController.GetCategoryName(videoObject);

                TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                if (string.IsNullOrEmpty(videoObject.Owner.SubCount))
                    videoObject.Owner.SubCount = "0";

                TxtSubscribeCount.Text = videoObject.Owner.SubCount;

                TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(videoObject.Description));

                EditButton.Visibility = videoObject.IsOwner != null && videoObject.IsOwner.Value ? ViewStates.Visible : ViewStates.Gone;

                //Reset Views
                LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                LikeButton.Tag = "0";
                UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                UnLikeButton.Tag = "0";
                VideoLikeCount.Text = "0";
                VideoUnLikeCount.Text = "0";

                SubscribeChannelButton.Tag = "Subscribe";
                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribe);
                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                icon.Bounds = new Rect(10, 10, 10, 7);
                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                //Close the description panel
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDownB);

                VideoDescriptionLayout.Tag = "closed";
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);

                //Clear all data 
                if (CommentsFragment != null && CommentsFragment.MAdapter != null)
                {
                    CommentsFragment.MAdapter.CommentList?.Clear();
                    UserResponseTabs.NextFragment.MAdapter.VideoList?.Clear();
                    CommentsFragment.MAdapter.NotifyDataSetChanged();
                    UserResponseTabs.NextFragment.MAdapter.NotifyDataSetChanged();
                    CommentsFragment.StartApiService(videoObject.Id, "0");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SetVideoType(VideoObject videoObject)
        {
            try
            {
                VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Normal;

                if (videoObject.VideoLocation.Contains("Youtube") || videoObject.VideoLocation.Contains("youtu") || videoObject.VideoType == "VideoObject/youtube")
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Youtube;
                else if (!string.IsNullOrEmpty(videoObject.Vimeo))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Vimeo;
                else if (!string.IsNullOrEmpty(videoObject.Daily))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.DailyMotion;
                else if (!string.IsNullOrEmpty(videoObject.Ok))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Ok;
                else if (!string.IsNullOrEmpty(videoObject.Twitch))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Twitch;
                else if (!string.IsNullOrEmpty(videoObject.Facebook))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Facebook;
                else if (videoObject.IsOwner != null && (videoObject.AgeRestriction == "2" && videoObject.IsOwner.Value == false))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.AgeRestricted;
                else if (!string.IsNullOrEmpty(videoObject.GeoBlocking) && videoObject.IsOwner == false)
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.GeoBlocked;

                videoObject.VideoType = VideoType.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task GetVideosInfoAsJson(string videoId)
        {
            try
            {
                var token = UserDetails.AccessToken;
                var (apiStatus, respond) = await RequestsAsync.Video.API_Get_Videos_Details_Http(videoId, UserDetails.AndroidId);
                if (apiStatus == 200)
                {
                    if (respond is GetVideosDetailsObject result)
                    {
                        var updater = ListUtils.GlobalViewsVideosList.FirstOrDefault(a => a.VideoId == videoId);
                        if (updater != null)
                        {
                            ListUtils.GlobalViewsVideosList.Add(updater);
                            SetNewDataVideo(updater);
                        }
                        else
                        {
                            ListUtils.GlobalViewsVideosList.Add(result.DataResult);
                            SetNewDataVideo(result.DataResult);
                        }

                        result.DataResult.SuggestedVideos = AppTools.ListFilter(result.DataResult.SuggestedVideos);
                        ListUtils.ArrayListPlay = new ObservableCollection<VideoObject>(result.DataResult.SuggestedVideos);
                        UserResponseTabs.NextFragment.LoadDataAsync(new ObservableCollection<VideoObject>(result.DataResult.SuggestedVideos));

                        if (ListUtils.AdsVideoList.Count > 0)
                        {
                            if (result.DataResult.VideoAd.VideoAdClass != null)
                                ListUtils.AdsVideoList.Add(result.DataResult.VideoAd.VideoAdClass);
                        }
                        else
                        {
                            ListUtils.AdsVideoList = new ObservableCollection<VideoAdDataObject>();

                            if (result.DataResult.VideoAd.VideoAdClass != null)
                                ListUtils.AdsVideoList.Add(result.DataResult.VideoAd.VideoAdClass);
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async void SetNewDataVideo(VideoObject videoObject)
        {
            try
            {
                if (videoObject == null)
                    return;

                VideoDataHandler = videoObject;
                SetVideoType(VideoDataHandler);

                VideoChannelViews.Text = videoObject.Views + " " + GetText(Resource.String.Lbl_Views) + " | " + Methods.Time.ReplaceTime(videoObject.TimeAgo);
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);

                if (videoObject.IsLiked == "1") // true
                {

                    LikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    LikeButton.Tag = "1";
                }
                else
                {

                    LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    LikeButton.Tag = "0";
                }

                if (videoObject.IsDisliked == "1") // true
                {
                    LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    UnLikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    UnLikeButton.Tag = "1";
                }
                else
                {
                    UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    UnLikeButton.Tag = "0";
                }

                SubscribeChannelButton.Visibility = videoObject.IsOwner != null && videoObject.IsOwner.Value ? ViewStates.Invisible : ViewStates.Visible;
                SubscribeCountLayout.Visibility = videoObject.IsOwner != null && videoObject.IsOwner.Value ? ViewStates.Invisible : ViewStates.Visible;
                EditButton.Visibility = videoObject.IsOwner != null && videoObject.IsOwner.Value ? ViewStates.Visible : ViewStates.Gone;

                VideoLikeCount.Text = videoObject.Likes;
                VideoUnLikeCount.Text = videoObject.Dislikes;
                VideoPublishDate.Text = GetText(Resource.String.Lbl_Published_on) + " " + videoObject.TimeDate;
                VideoCategory.Text = CategoriesController.GetCategoryName(videoObject);

                //Verified 
                TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
                TextChannelName.Text = AppTools.GetNameFinal(videoObject.Owner);
                GlideImageLoader.LoadImage(this, videoObject.Owner.Avatar, ImageChannelView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                if (string.IsNullOrEmpty(videoObject.Owner.SubCount))
                    videoObject.Owner.SubCount = "0";

                TxtSubscribeCount.Text = videoObject.Owner.SubCount;

                //Rent
                if (!string.IsNullOrEmpty(videoObject.RentPrice) && videoObject.RentPrice != "0" && AppSettings.RentVideosSystem)
                {
                    RentButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    RentButton.Visibility = ViewStates.Gone;
                }

                //Donate
                if (!string.IsNullOrEmpty(videoObject.Owner.DonationPaypalEmail) && AppSettings.DonateVideosSystem)
                {
                    DonateButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    DonateButton.Visibility = ViewStates.Gone;
                }

                var file = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(videoObject.VideoId);
                if (!string.IsNullOrEmpty(file))
                {
                    VideoActionsController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
                    VideoActionsController.DownloadIcon.Tag = "Downloaded";
                    LibrarySynchronizer.AddToWatchOffline(videoObject);
                }

                if (videoObject.Owner.Id != UserDetails.UserId)
                {
                    UserDataObject channel = await ApiRequest.GetChannelData(this, videoObject.Owner.Id);
                    if (channel != null)
                    {
                        videoObject.Owner = channel;

                        if (!string.IsNullOrEmpty(videoObject.Owner.SubscriberPrice) && videoObject.Owner.SubscriberPrice != "0")
                        {
                            if (videoObject.Owner.IsSubscribedToChannel == "0")
                            {
                                //This channel is paid, You must pay to subscribe
                                SubscribeChannelButton.Tag = "PaidSubscribe";

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
                                var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
                                Console.WriteLine(currency);
                                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribe) + " " + currencyIcon + videoObject.Owner.SubscriberPrice;
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribed";

                                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribed);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                            }
                        }
                        else
                        {
                            SubscribeChannelButton.Tag = videoObject.Owner.IsSubscribedToChannel == "0" ? "Subscribe" : "Subscribed";

                            if (SubscribeChannelButton.Tag.ToString() == "Subscribed")
                            {
                                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribed);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                            }
                            else if (SubscribeChannelButton.Tag.ToString() == "Subscribe")
                            {
                                SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribe);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                            }
                        }

                        //Verified 
                        TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
                    }
                }
                else
                {
                    UserDataObject channel = ListUtils.MyChannelList.FirstOrDefault();
                    if (channel == null) return;
                    videoObject.Owner = channel;
                    //Verified 
                    TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void StartPlayVideo(VideoObject videoObject)
        {
            try
            {
                if (videoObject != null)
                {
                    VideoDataHandler = videoObject;

                    LoadVideoData(videoObject);
                    await UserResponseTabs.ResponseFragment.GetResponseList(videoObject.VideoId);
                    UserResponseTabs.ResponseFragment.GetUserResponse(videoObject.VideoId);

                    VideoActionsController.ExoBackButton.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                    VideoActionsController.ExoBackButton.Tag = "Open";

                    if (VideoType == VideoDataWithEventsLoader.VideoEnumTypes.AgeRestricted)
                    {
                        GlobalVideosRelease("All");
                        CustomNavigationController.BringFragmentToTop(RestrictedVideoPlayerFragment, SupportFragmentManager, VideoFrameLayoutFragments);
                        RestrictedVideoPlayerFragment.LoadRestriction("AgeRestriction", videoObject.Thumbnail, videoObject);
                    }
                    else if (VideoType == VideoDataWithEventsLoader.VideoEnumTypes.GeoBlocked)
                    {
                        GlobalVideosRelease("All");
                        CustomNavigationController.BringFragmentToTop(RestrictedVideoPlayerFragment, SupportFragmentManager, VideoFrameLayoutFragments);
                        RestrictedVideoPlayerFragment.LoadRestriction("GeoRestriction", videoObject.Thumbnail, videoObject);
                    }
                    else if (VideoType == VideoDataWithEventsLoader.VideoEnumTypes.Facebook || VideoType == VideoDataWithEventsLoader.VideoEnumTypes.Twitch | VideoType == VideoDataWithEventsLoader.VideoEnumTypes.DailyMotion | VideoType == VideoDataWithEventsLoader.VideoEnumTypes.Ok | VideoType == VideoDataWithEventsLoader.VideoEnumTypes.Vimeo)
                    {
                        GlobalVideosRelease("All");
                        CustomNavigationController.BringFragmentToTop(ThirdPartyPlayersFragment, SupportFragmentManager, VideoFrameLayoutFragments);
                        ThirdPartyPlayersFragment.SetVideoIframe(videoObject);
                    }
                    else if (VideoType == VideoDataWithEventsLoader.VideoEnumTypes.Youtube)
                    {
                        VideoIdYoutube = videoObject.VideoLocation.Split(new[] { "v=" }, StringSplitOptions.None).LastOrDefault();
                        GlobalVideosRelease("Youtube");
                        CustomNavigationController.BringFragmentToTop(YouTubeFragment, SupportFragmentManager, VideoFrameLayoutFragments);
                        ////////////////////////////////////////
                        StartTime = 0;
                        EndTime = 0;
                        using (var client = new HttpClient())
                        {
                            //string server_key = "de6c8d5e3922f0e7076e1a7654730320c2417190-7d5128ae2b8906362192a8502d6ecc90-48738715";
                            var formContent = new FormUrlEncodedContent(new[]
                            {
                                    new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                    new KeyValuePair<string, string>("android_id", UserDetails.AndroidId),
                                });

                            //  send a Post request  
                            var uri = Client.WebsiteUrl + "/api/v1.0/?type=get_video_details&video_id=" + videoObject.VideoId;
                            var result = await client.PostAsync(uri, formContent);

                            if (result.IsSuccessStatusCode)
                            {
                                // handling the answer  
                                var resultString = await result.Content.ReadAsStringAsync();
                                var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                if (jConfigObject["api_status"].ToString() == "200" && jConfigObject["data"] != null)
                                {
                                    if (!string.IsNullOrEmpty(jConfigObject["data"]["start_time"].ToString()))
                                    {
                                        string start = jConfigObject["data"]["start_time"].ToString();
                                        int pos1 = start.IndexOf(':');
                                        int minutes1 = int.Parse(start.Substring(0, (pos1 == 1 ? pos1 : pos1 - 1)));
                                        int seconds1 = int.Parse(start.Substring(pos1 + 1));
                                        StartTime = ((minutes1 * 60) + seconds1) * 1000;
                                    }

                                    if (!string.IsNullOrEmpty(jConfigObject["data"]["end_time"].ToString()))
                                    {
                                        string end = jConfigObject["data"]["end_time"].ToString();
                                        int pos2 = end.IndexOf(':');
                                        int minutes2 = int.Parse(end.Substring(0, (pos2 == 1 ? pos2 : pos2 - 1)));
                                        int seconds2 = int.Parse(end.Substring(pos2 + 1));
                                        EndTime = ((minutes2 * 60) + seconds2) * 1000;
                                    }
                                }
                            }
                        }
                        ///////////////////////////////////////////
                        if (StartTime != 0)
                        {
                            YoutubePlayer?.LoadVideo(VideoIdYoutube, StartTime);
                            handler.PostDelayed(VideoTracking, 1000);
                        }
                        else
                        {
                            YoutubePlayer?.LoadVideo(VideoIdYoutube);
                            handler.PostDelayed(VideoTracking, 1000);
                        }
                    }
                    else
                    {
                        GlobalVideosRelease("exo");
                        CustomNavigationController.BringFragmentToTop(null, SupportFragmentManager, VideoFrameLayoutFragments);
                        VideoActionsController.PlayVideo(videoObject.VideoLocation, videoObject, RestrictedVideoPlayerFragment, this);
                    }

                    TabbedMainActivity.GetInstance().SetOnWakeLock();
                    LibrarySynchronizer.AddToRecentlyWatched(videoObject);

                    #region Old Code
                    //if (videoObject.IsOwner != null && (videoObject.AgeRestriction == "2" && videoObject.IsOwner.Value == false))
                    //{
                    //    FragmentTransaction ft = SupportFragmentManager.BeginTransaction();

                    //    VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                    //    VideoActionsController.ReleaseVideo();
                    //    if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                    //        YoutubePlayer?.Pause();

                    //    if (RestrictedVideoPlayerFragment == null)
                    //    {
                    //        Bundle bundle = new Bundle();
                    //        bundle.PutString("type", "AgeRestriction");
                    //        bundle.PutString("image", videoObject.Thumbnail);
                    //        bundle.PutString("Object", JsonConvert.SerializeObject(videoObject));

                    //        RestrictedVideoPlayerFragment = new RestrictedVideoFragment { Arguments = bundle };
                    //    }

                    //    if (RestrictedVideoPlayerFragment.IsAdded)
                    //    {
                    //        if (YouTubeFragment != null && YouTubeFragment.IsAdded)
                    //            ft.Hide(YouTubeFragment);

                    //        ft.Show(RestrictedVideoPlayerFragment).Commit();
                    //        RestrictedVideoPlayerFragment.LoadRestriction("AgeRestriction", videoObject.Thumbnail, videoObject);
                    //    }
                    //    else
                    //    {
                    //        if (YouTubeFragment != null && YouTubeFragment.IsAdded)
                    //            ft.Hide(YouTubeFragment);

                    //        ft.Add(Resource.Id.root, RestrictedVideoPlayerFragment, DateTime.Now.ToString(CultureInfo.InvariantCulture)).Commit();
                    //    }
                    //}
                    //else if (!string.IsNullOrEmpty(videoObject.GeoBlocking) && videoObject.IsOwner == false)
                    //{
                    //    FragmentTransaction ft = SupportFragmentManager.BeginTransaction();

                    //    VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                    //    VideoActionsController.ReleaseVideo();
                    //    if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                    //        YoutubePlayer?.Pause();

                    //    if (RestrictedVideoPlayerFragment == null)
                    //    {
                    //        Bundle bundle = new Bundle();
                    //        bundle.PutString("type", "GeoRestriction");
                    //        bundle.PutString("image", videoObject.Thumbnail);
                    //        bundle.PutString("Object", JsonConvert.SerializeObject(videoObject));

                    //        RestrictedVideoPlayerFragment = new RestrictedVideoFragment { Arguments = bundle };
                    //    }

                    //    if (RestrictedVideoPlayerFragment.IsAdded)
                    //    {
                    //        if (YouTubeFragment != null && YouTubeFragment.IsAdded)
                    //            ft.Hide(YouTubeFragment);

                    //        ft.Show(RestrictedVideoPlayerFragment).Commit();
                    //        RestrictedVideoPlayerFragment.LoadRestriction("GeoRestriction", videoObject.Thumbnail, videoObject);
                    //    }
                    //    else
                    //    {
                    //        if (YouTubeFragment != null && YouTubeFragment.IsAdded)
                    //            ft.Hide(YouTubeFragment);

                    //        ft.Add(Resource.Id.root, RestrictedVideoPlayerFragment, DateTime.Now.ToString(CultureInfo.InvariantCulture)).Commit();
                    //    }
                    //}
                    //else if (!string.IsNullOrEmpty(videoObject.Vimeo) || !string.IsNullOrEmpty(videoObject.Twitch) || !string.IsNullOrEmpty(videoObject.Daily) || !string.IsNullOrEmpty(videoObject.Ok) || !string.IsNullOrEmpty(videoObject.Facebook))
                    //{
                    //    var videoType = string.Empty;
                    //    var videoId = string.Empty;

                    //    if (!string.IsNullOrEmpty(videoObject.Vimeo))
                    //    {
                    //        videoType = "Vimeo";
                    //        videoId = videoObject.Vimeo;
                    //    }
                    //    else if (!string.IsNullOrEmpty(videoObject.Twitch))
                    //    {
                    //        videoType = "Twitch";
                    //        videoId = videoObject.Twitch;
                    //    }
                    //    else if (!string.IsNullOrEmpty(videoObject.Daily))
                    //    {
                    //        videoType = "Daily";
                    //        videoId = videoObject.Daily;
                    //    }

                    //    else if (!string.IsNullOrEmpty(videoObject.Ok))
                    //    {
                    //        videoType = "Ok";
                    //        videoId = videoObject.Ok;
                    //    }

                    //    else if (!string.IsNullOrEmpty(videoObject.Facebook))
                    //    {
                    //        videoType = "Facebook";
                    //        videoId = videoObject.Facebook;
                    //    }


                    //    if (ThirdPartyPlayersFragment == null)
                    //    {
                    //        Bundle bundle = new Bundle();
                    //        bundle.PutString("type", videoType);
                    //        bundle.PutString("imageUrl", videoObject.Thumbnail);
                    //        bundle.PutString("videoId", videoId);

                    //        ThirdPartyPlayersFragment = new ThirdPartyPlayersFragment { Arguments = bundle };
                    //    }

                    //    FragmentTransaction ft2 = SupportFragmentManager.BeginTransaction();
                    //    if (ThirdPartyPlayersFragment.IsAdded)
                    //    {
                    //        if (YouTubeFragment != null && YouTubeFragment.IsAdded)
                    //            ft2.Hide(YouTubeFragment);

                    //        ft2.Show(ThirdPartyPlayersFragment).Commit();
                    //        ThirdPartyPlayersFragment.SetVideoIframe(videoObject);
                    //    }
                    //    else
                    //    {
                    //        if (YouTubeFragment != null && YouTubeFragment.IsAdded)
                    //            ft2.Hide(YouTubeFragment);

                    //        ft2.Add(Resource.Id.root, ThirdPartyPlayersFragment, DateTime.Now.ToString(CultureInfo.InvariantCulture)).Commit();
                    //    }
                    //}
                    //else if (videoObject.VideoType == "VideoObject/youtube" || videoObject.VideoLocation.Contains("Youtube") || videoObject.VideoLocation.Contains("youtu"))
                    //{
                    //    var ft = SupportFragmentManager.BeginTransaction();

                    //    VideoIdYoutube = videoObject.VideoLocation.Split(new[] { "v=" }, StringSplitOptions.None).LastOrDefault();

                    //    if (RestrictedVideoPlayerFragment != null && RestrictedVideoPlayerFragment.IsAdded)
                    //        ft.Hide(RestrictedVideoPlayerFragment);

                    //    if (YouTubeFragment == null)
                    //    {
                    //        YouTubeFragment = new YouTubePlayerSupportFragment();
                    //        YouTubeFragment.Initialize(AppSettings.YoutubePlayerKey, this);
                    //        ft.Add(Resource.Id.root, YouTubeFragment, YouTubeFragment.Id.ToString() + DateTime.Now).Commit();

                    //        VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                    //        VideoActionsController.ReleaseVideo();
                    //    }
                    //    else
                    //    {
                    //        VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                    //        VideoActionsController.ReleaseVideo();

                    //        if (YouTubeFragment.IsAdded)
                    //            ft.Show(YouTubeFragment).Commit();
                    //        else
                    //        {
                    //            YouTubeFragment = new YouTubePlayerSupportFragment();
                    //            ft.Add(Resource.Id.root, YouTubeFragment, YouTubeFragment.Id.ToString() + DateTime.Now).Commit();
                    //        }
                    //        YouTubeFragment.View.Visibility = ViewStates.Visible;
                    //        YoutubePlayer?.LoadVideo(VideoIdYoutube);
                    //    }
                    //}
                    //else
                    //{
                    //    FragmentTransaction ft = SupportFragmentManager.BeginTransaction();

                    //    if (RestrictedVideoPlayerFragment != null && RestrictedVideoPlayerFragment.IsAdded)
                    //        ft.Hide(RestrictedVideoPlayerFragment);

                    //    if (YouTubeFragment != null)
                    //    {
                    //        if (YouTubeFragment.IsAdded)
                    //        {

                    //            if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                    //                YoutubePlayer?.Pause();

                    //            ft.Hide(YouTubeFragment).AddToBackStack(null).Commit();
                    //            YouTubeFragment.View.Visibility = ViewStates.Gone;

                    //            if (VideoActionsController.SimpleExoPlayerView.Visibility == ViewStates.Gone)
                    //                VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Visible;
                    //        }
                    //    }

                    //    VideoActionsController.PlayVideo(videoObject.VideoLocation, videoObject, RestrictedVideoPlayerFragment, this);
                    //}
                    #endregion

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void GlobalVideosRelease(string exepttype)
        {
            try
            {
                if (exepttype == "exo")
                {
                    if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                        YoutubePlayer?.Pause();

                    if (VideoActionsController.SimpleExoPlayerView.Visibility == ViewStates.Gone)
                        VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Visible;
                }
                if (exepttype == "Youtube")
                {
                    VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                    VideoActionsController.ReleaseVideo();
                    YouTubeFragment.View.Visibility = ViewStates.Visible;
                }

                if (exepttype == "All")
                {
                    if (YoutubePlayer != null && YoutubePlayer.IsPlaying)
                        YoutubePlayer?.Pause();
                    YouTubeFragment.View.Visibility = ViewStates.Gone;

                    VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                    VideoActionsController.ReleaseVideo();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (TypeDialog == "AddTo")
                {
                    if (itemString.ToString() == GetString(Resource.String.Lbl_Addto_playlist))
                    {
                        OnMenuAddPlaylistClick(VideoDataHandler);
                    }
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_Addto_watchlater) || itemString.ToString() == GetString(Resource.String.Lbl_RemoveFromWatchLater))
                    {
                        OnMenuAddWatchLaterClick(VideoDataHandler);
                    }
                }
                else if (TypeDialog == "Payment_RentVideo")
                {
                    string price = AppSettings.VideoRentalPriceStatic && AppSettings.VideoRentalPrice > 0
                        ? AppSettings.VideoRentalPrice.ToString()
                        : VideoDataHandler.RentPrice;

                    if (itemString.ToString() == GetString(Resource.String.Btn_Paypal))
                    {
                        PayType = "RentVideo";
                        InitPayPalPayment.BtnPaypalOnClick(price, "RentVideo");
                    }
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_CreditCard))
                    {
                        Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                        intent.PutExtra("Price", price);
                        intent.PutExtra("payType", "RentVideo");
                        intent.PutExtra("Id", VideoDataHandler.Id);
                        StartActivity(intent);
                    }
                }
                else if (TypeDialog == "Payment_DonateVideo")
                {
                    if (itemString.ToString() == GetString(Resource.String.Btn_Paypal))
                    {
                        PayType = "DonateVideo";
                        InitPayPalPayment.BtnPaypalOnClick(stNUmber, "DonateVideo");
                    }
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_CreditCard))
                    {
                        Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                        intent.PutExtra("Price", stNUmber);
                        intent.PutExtra("payType", "DonateVideo");
                        StartActivity(intent);
                    }
                }
                else if (TypeDialog == "Payment_PaidSubscribe")
                {
                    if (itemString.ToString() == GetString(Resource.String.Btn_Paypal))
                    {
                        PayType = "SubscriberVideo";
                        InitPayPalPayment.BtnPaypalOnClick(VideoDataHandler.Owner.SubscriberPrice, "SubscriberVideo");
                    }
                    else if (itemString.ToString() == GetString(Resource.String.Lbl_CreditCard))
                    {
                        Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                        intent.PutExtra("Price", VideoDataHandler.Owner.SubscriberPrice);
                        intent.PutExtra("payType", "SubscriberVideo");
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (TypeDialog == "PaidSubscribe")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        TypeDialog = "Payment_PaidSubscribe";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        if (AppSettings.ShowPaypal)
                            arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));

                        if (AppSettings.ShowCreditCard)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetString(Resource.String.Lbl_Close)).OnNegative(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public string stNUmber;
        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            try
            {
                if (p1.Length() > 0)
                {
                    if (Methods.CheckConnectivity())
                    {
                        stNUmber = p1.ToString();

                        TypeDialog = "Payment_DonateVideo";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        if (AppSettings.ShowPaypal)
                            arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));

                        if (AppSettings.ShowCreditCard)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetString(Resource.String.Lbl_Close)).OnNegative(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Permissions && Result

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                BillingPayment?.Handler?.HandleActivityResult(requestCode, resultCode, data);
                if (requestCode == 201)
                {
                    if (requestCode == 201)
                    {
                        if (resultCode == Result.Ok)
                        {
                            UserResponsePostDescription mFragment = new UserResponsePostDescription(data, this, 2);
                            mFragment.Show(SupportFragmentManager, mFragment.Tag);
                            //Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                            //Android.App.AlertDialog alert = dialog.Create();
                            //alert.SetTitle("Confirmation");
                            //alert.SetMessage("Are you sure you want to upload the selected file?");
                            //alert.SetButton("OK", async (c, ev) =>
                            //{
                            //    var array = VideoHelper.GetSelectedMediaData(this, data.Data);
                            //    await UploadSelectedResponse(array);
                            //});
                            //alert.SetButton2("CANCEL", (c, ev) => { alert.Hide(); });
                            //alert.Show();
                        }
                    }
                }
                else if (requestCode == 2000)
                {
                    if (resultCode == Result.Ok)
                    {
                        VideoActionsController.RestartPlayAfterShrinkScreen();
                    }
                }
                else if (requestCode == InitPayPalPayment?.PayPalDataRequestCode)
                {
                    switch (resultCode)
                    {
                        case Result.Ok:
                            var confirmObj = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
                            PaymentConfirmation configuration = Android.Runtime.Extensions.JavaCast<PaymentConfirmation>(confirmObj);
                            if (configuration != null)
                            {
                                //string createTime = configuration.ProofOfPayment.CreateTime;
                                //string intent = configuration.ProofOfPayment.Intent;
                                //string paymentId = configuration.ProofOfPayment.PaymentId;
                                //string state = configuration.ProofOfPayment.State;
                                //string transactionId = configuration.ProofOfPayment.TransactionId;
                                if (PayType == "SubscriberVideo")
                                {
                                    if (Methods.CheckConnectivity())
                                    {
                                        SetSubscribeChannelWithPaid();
                                    }
                                    else
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                }
                                else if (PayType == "RentVideo")
                                {
                                    if (Methods.CheckConnectivity())
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.Video.RentVideo_Http(VideoDataHandler.Id).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                Toast.MakeText(this, GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long).Show();
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    }
                                }
                                else if (PayType == "DonateVideo")
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_DonatedSuccessfully), ToastLength.Long).Show();
                                }
                            }
                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long).Show();
                            break;
                    }
                }
                else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long).Show();
                }
                else if (requestCode == 1001 && resultCode == Result.Ok)
                {
                    if (Methods.CheckConnectivity())
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Video.RentVideo_Http(VideoDataHandler.Id).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long).Show();
                            });
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public async System.Threading.Tasks.Task UploadSelectedResponse(byte[] bytesArray, string Description)
        {
            try
            {
                this.RunOnUiThread(() => LoadingView.Show(this, "Uploading..."));
                if (bytesArray != null)
                {
                    string msg = string.Empty;
                    if (UserResponseBottomSheet.userResponse == UserResponse.Video)
                        msg = await UserResponseAPI.UploadUserResponse(UserResponseBottomSheet.videoId, bytesArray, InputSelectedType.Selected, Description);
                    else
                        msg = await UserResponseAPI.UploadAudioUserResponse(UserResponseBottomSheet.videoId, bytesArray, InputSelectedType.Selected, Description);
                    var tabbedActivity = TabbedMainActivity.GetInstance();
                    if (tabbedActivity != null)
                    {
                        int UnseenResponseCount = UserResponseAPI.GetUnseenResponseNotification();
                        if (UnseenResponseCount > 0)
                        {
                            tabbedActivity.MyChannelFragment.UnSeenReponse = UnseenResponseCount;
                            tabbedActivity.FragmentBottomNavigator.txtUnSeenCount.Visibility = ViewStates.Visible;
                            tabbedActivity.FragmentBottomNavigator.txtUnSeenCount.Text = UnseenResponseCount.ToString();
                        }
                        else
                            tabbedActivity.FragmentBottomNavigator.txtUnSeenCount.Visibility = ViewStates.Gone;
                    }
                    if (msg.Contains("ok"))
                        this.RunOnUiThread(() => Toast.MakeText(this, "Response Uploaded successfully", ToastLength.Short).Show());
                    else
                        this.RunOnUiThread(() => Toast.MakeText(this, msg, ToastLength.Short).Show());
                }
                await UserResponseTabs.ResponseFragment.GetResponseList(UserResponseBottomSheet.videoId);
                UserResponseTabs.ResponseFragment.GetUserResponse(UserResponseBottomSheet.videoId);
            }
            catch (System.Exception ex)
            {

            }
            finally
            {
                this.RunOnUiThread(() => LoadingView.Hide());
            }
        }

        #endregion

        public async void SetSubscribeChannelWithPaid()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    SubscribeChannelButton.Tag = "Subscribed";
                    SubscribeChannelButton.Text = GetText(Resource.String.Btn_Subscribed);

                    //Color
                    SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                    //icon
                    Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                    icon.Bounds = new Rect(10, 10, 10, 7);
                    SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                    //Add The Video to  Subscribed Videos Database
                    Events_Insert_SubscriptionsChannel();

                    //Send API Request here for Subscribed
                    (int apiStatus, var respond) = await RequestsAsync.Global.Add_Subscribe_To_Channel_Http(VideoDataHandler.Owner.Id, "paid");
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short).Show();
                            });
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void FinishActivityAndTask()
        {
            try
            {
                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        VideoActionsController.ReleaseVideo();
                        //MoveTaskToBack(true);
                        FinishAndRemoveTask();
                        Finish();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        YoutubePlayer?.Pause();
                        Finish();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}