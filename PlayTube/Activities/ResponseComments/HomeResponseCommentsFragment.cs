﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using PlayTube.Activities.ResponseComments.Adapters;
using PlayTube.Activities.Default;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.UserReponse;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.ViewExtensions;
using PlayTubeClient.Classes.Comment;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.RestCalls;
using Fragment = Android.Support.V4.App.Fragment;
using VideoLibrary;
using System.Runtime.CompilerServices;
using PlayTube.API.Models;

namespace PlayTube.Activities.ResponseComments
{
    public class HomeResponseCommentsFragment : Fragment
    {
        #region Variables Basic

        private SwipeRefreshLayout SwipeRefreshLayout;
        private HomeResponsePlayerFragment MainContext;
        private View MainView;
        private RelativeLayout RootView;
        private EmojiconEditText EmojiconEditTextView;
        private AppCompatImageView Emojiicon;
        private CircleButton SendButton;
        private RecyclerView MRecycler;
        private LinearLayoutManager MLayoutManager;
        public ResponseCommentsAdapter MAdapter;
        private ProgressBar ProgressBarLoader;
        private View Inflated;
        private ViewStub EmptyStateLayout;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private string VideoId;
        private RelativeLayout CommentButtomLayout;
        private HomeResponseCommentClickListener CommentClickListener;

        #endregion

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                HasOptionsMenu = true;
                if (TabbedMainActivity.GetInstance().MyResponsesFragment.activePage == 1)
                    MainContext = TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses;
                else
                    MainContext = TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses;
                //MainContext = HomeResponsePlayerFragment.GetInstance();
                CommentClickListener = new HomeResponseCommentClickListener(Activity, "Comment");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Activity.Window.SetSoftInputMode(SoftInput.AdjustResize);

                MainView = inflater.Inflate(Resource.Layout.CommentsFragment_Layout, container, false);

                InitComponent();
                SetRecyclerViewAdapters();

                return MainView;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public override async void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                if (MainContext.ActivityType == "Follow")
                {
                    //TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses =
                    //new ObservableCollection<AdminVideoResponse>(API.UserResponseAPI.GetFollowingResponseVideos().VideoResponse);
                    //var y = MAdapter.CommentList.Count;
                    //TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater.FollowingResponses
                    //    .Where(x => x.Id == Int32.Parse(VideoId)).FirstOrDefault().Comments = y.ToString();
                    //TabbedMainActivity.GetInstance().MyResponsesFragment.FollowingResponses.adpater
                    //.NotifyItemChanged(MainContext.mediaList.FindIndex(x => x.Id == Int32.Parse(VideoId)));
                }
                else
                {
                    //TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses =
                    //new ObservableCollection<AdminVideoResponse>(API.UserResponseAPI.GetForYouResponseVideos().VideoResponse);
                    //var y = MAdapter.CommentList.Count;
                    //TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater.ForYouResponses
                    //    .Where(x => x.Id == Int32.Parse(VideoId)).FirstOrDefault().Comments = y.ToString();
                    //TabbedMainActivity.GetInstance().MyResponsesFragment.ForYouResponses.adpater
                    //.NotifyItemChanged(MainContext.mediaList.FindIndex(x => x.Id == Int32.Parse(VideoId)));
                }
                //TabbedMainActivity.GetInstance().MyChannelFragment.latestResponse.adpater.LatestResponse =
                //new ObservableCollection<AdminVideoResponse>(API.UserResponseAPI.GetAdminVideos().VideoResponse);
            }
            catch (System.Exception ex)
            {

            }
        }

        #region Functions

        private void InitComponent()
        {
            try
            {
                RootView = MainView.FindViewById<RelativeLayout>(Resource.Id.root);
                Emojiicon = MainView.FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojiconEditTextView = MainView.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                SendButton = MainView.FindViewById<CircleButton>(Resource.Id.sendButton);
                MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.commentRecyler);
                ProgressBarLoader = MainView.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
                EmptyStateLayout = MainView.FindViewById<ViewStub>(Resource.Id.viewStub);
                CommentButtomLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.commentonButtom);
                SwipeRefreshLayout = MainView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);

                SwipeRefreshLayout.SetDefaultStyle();
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                EmojiconEditTextView.RequestFocus();

                ProgressBarLoader.Visibility = ViewStates.Visible;

                EmojIconActions emojis = new EmojIconActions(Activity, RootView, EmojiconEditTextView, Emojiicon);
                emojis.ShowEmojIcon();

                SendButton.Click += SendButton_Click;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new ResponseCommentsAdapter(Activity);
                MLayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(MLayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.NestedScrollingEnabled = true;
                MAdapter.ReplyClick += CommentsAdapter_ReplyClick;
                MAdapter.AvatarClick += CommentsAdapter_AvatarClick;
                MAdapter.ItemLongClick += MAdapterOnItemLongClick;

                RecyclerViewOnScrollListener recyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
                MainScrollEvent = recyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                MRecycler.AddOnScrollListener(recyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.CommentList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                StartApiService(VideoId, "0");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Open Profile User
        private void CommentsAdapter_AvatarClick(object sender, AvatarCommentAdapterClickEventArgs e)
        {
            try
            {
                TabbedMainActivity.GetInstance().ShowUserChannelFragment(e.Class.CommentUserData, e.Class.UserId.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Add New Comment
        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (UserDetails.IsLogin)
                {
                    if (!string.IsNullOrEmpty(EmojiconEditTextView.Text))
                    {
                        if (Methods.CheckConnectivity())
                        {
                            if (MAdapter.CommentList.Count == 0)
                            {
                                EmptyStateLayout.Visibility = ViewStates.Gone;
                                MRecycler.Visibility = ViewStates.Visible;
                            }

                            //Comment Code
                            string time = Methods.Time.TimeAgo(DateTime.Now, false);
                            int unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                            string time2 = unixTimestamp.ToString();
                            string message = EmojiconEditTextView.Text;
                            var postId = MAdapter.CommentList.FirstOrDefault(a => a.VideoId == Convert.ToInt32(VideoId))?.PostId ?? 0;

                            CommentObject comment = new CommentObject
                            {
                                Text = message,
                                TextTime = time,
                                UserId = Convert.ToInt32(UserDetails.UserId),
                                Id = Convert.ToInt32(time2),
                                IsCommentOwner = true,
                                VideoId = Convert.ToInt32(VideoId),
                                CommentUserData = new UserDataObject
                                {
                                    Avatar = UserDetails.Avatar,
                                    Username = UserDetails.Username,
                                    Name = UserDetails.FullName,
                                    Cover = UserDetails.Cover
                                },
                                CommentReplies = new List<ReplyObject>(),
                                DisLikes = 0,
                                IsDislikedComment = 0,
                                IsLikedComment = 0,
                                Likes = 0,
                                Pinned = "",
                                PostId = postId,
                                RepliesCount = 0,
                                Time = unixTimestamp
                            };

                            MAdapter.CommentList.Add(comment);
                            int index = MAdapter.CommentList.IndexOf(comment);
                            MAdapter.NotifyItemInserted(index);
                            MRecycler.ScrollToPosition(MAdapter.CommentList.IndexOf(MAdapter.CommentList.Last()));

                            var y = MAdapter.CommentList.Count;
                            MainContext.txtCommentNumber.Text = y.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            int index2 = MainContext.mediaList.FindIndex(x => x.Id == MainContext.selectedMedia.Id);
                            MainContext.mediaList[index2].Comments = y.ToString();
                            //Api request
                            Task.Run(async () =>
                            {
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                                        new KeyValuePair<string, string>("video_id", VideoId),
                                        new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                                        new KeyValuePair<string, string>("text", message),
                                        new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                                    });

                                    //  send a Post request  
                                    var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=add_response_comment";
                                    var result = await client.PostAsync(uri, formContent);
                                    if (result.IsSuccessStatusCode)
                                    {
                                        // handling the answer  
                                        var resultString = await result.Content.ReadAsStringAsync();
                                        var jConfigObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                                        if (jConfigObject["api_status"].ToString() == "200")
                                        {
                                            var dataComment = MAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
                                            if (dataComment != null)
                                                dataComment.Id = Int32.Parse(jConfigObject["id"].ToString());
                                        }
                                        else
                                        {
                                            Methods.DisplayReportResult(Activity, "An unknown error occurred. Please try again later");
                                        }
                                    }
                                }
                                //var (respondCode, respond) = await RequestsAsync.Comments.Add_Comment_Http(VideoId, message);
                                //if (respondCode.Equals(200))
                                //{
                                //    if (respond is MessageIdObject Object)
                                //    {
                                //        var dataComment = MAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
                                //        if (dataComment != null)
                                //            dataComment.Id = Object.Id;
                                //    }
                                //}
                                //else Methods.DisplayReportResult(Activity, respond);
                            });

                            //Hide keyboard
                            EmojiconEditTextView.Text = "";
                            EmojiconEditTextView.ClearFocus();
                        }
                        else
                        {
                            Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                    }
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, null, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning),
                        Activity.GetText(Resource.String.Lbl_Please_sign_in_comment),
                        Activity.GetText(Resource.String.Lbl_Yes),
                        Activity.GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void CommentsAdapter_ReplyClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                var item = e.Class;
                if (item != null)
                {
                    MainContext.ShowReplyResponseCommentFragment(item, "video");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        private void MAdapterOnItemLongClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = MAdapter.GetItem(e.Position);
                if (item == null) return;

                CommentClickListener?.MoreCommentPostClick(item);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion Events

        #region Load Data Api 

        public void StartApiService(string videoId, string offset)
        {
            VideoId = videoId;
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {

                if (UserDetails.IsLogin)
                {
                    if (MainScrollEvent.IsLoading)
                        return;

                    MainScrollEvent.IsLoading = true;

                    int countList = MAdapter.CommentList.Count;

                    using (var client = new System.Net.Http.HttpClient())
                    {
                        var formContent = new System.Net.Http.FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("server_key", "0913cbbd8c729a5db4db40e4aa267a17"),
                            new KeyValuePair<string, string>("type", "fetch_comments"),
                            new KeyValuePair<string, string>("video_id", VideoId),
                            new KeyValuePair<string, string>("offset", offset),
                            new KeyValuePair<string, string>("limit", "20"),
                            new KeyValuePair<string, string>("user_id", UserDetails.UserId),
                            new KeyValuePair<string, string>("s", UserDetails.AccessToken)
                        });

                        //  send a Post request  
                        //var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=get_response_video_comments&video_id=" + VideoId + "&limit=20&offset=" + offset;
                        var uri = PlayTubeClient.Client.WebsiteUrl + "/api/v1.0/?type=response_comments";
                        var result2 = await client.PostAsync(uri, formContent);

                        if (result2.IsSuccessStatusCode)
                        {
                            // handling the answer  
                            var resultString = await result2.Content.ReadAsStringAsync();
                            var jObject = Newtonsoft.Json.Linq.JObject.Parse(resultString);
                            if (jObject["api_status"].ToString() != "200" || jObject["data"] == null)
                            {
                                MainScrollEvent.IsLoading = false;
                                Methods.DisplayReportResult(Activity, jObject["errors"]["error_text"].ToString());
                            }
                            else
                            {
                                //List<CommentObject> ListComments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CommentObject>>(jObject["data"].ToString());
                                List<CommentObject> ListComments = (List<CommentObject>)jObject["data"].ToObject(typeof(List<CommentObject>));
                                var respondList = ListComments.Count;
                                if (respondList > 0)
                                {
                                    if (countList > 0)
                                    {
                                        foreach (var item in from item in ListComments let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                        {
                                            MAdapter.CommentList.Insert(0, item);
                                        }

                                        Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                                    }
                                    else
                                    {
                                        MAdapter.CommentList = new ObservableCollection<CommentObject>(ListComments);
                                        Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                                    }
                                }
                                else if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                {
                                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short).Show();
                                }
                            }

                            Activity.RunOnUiThread(ShowEmptyPage);
                        }
                    }
                    //var (apiStatus, respond) = await RequestsAsync.Comments.Get_Video_Comments_Http(VideoId, "20", offset);
                    //if (apiStatus != 200 || !(respond is GetCommentsObject result) || result.ListComments == null)
                    //{
                    //    MainScrollEvent.IsLoading = false;
                    //    Methods.DisplayReportResult(Activity, respond);
                    //}
                    //else
                    //{
                    //    var respondList = result.ListComments.Count;
                    //    if (respondList > 0)
                    //    {
                    //        if (countList > 0)
                    //        {
                    //            foreach (var item in from item in result.ListComments let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                    //            {
                    //                MAdapter.CommentList.Insert(0, item);
                    //            }

                    //            Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                    //        }
                    //        else
                    //        {
                    //            MAdapter.CommentList = new ObservableCollection<CommentObject>(result.ListComments);
                    //            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                    //        }
                    //    }
                    //    else if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
                    //    {
                    //        Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short).Show();
                    //    }
                    //}

                    //Activity.RunOnUiThread(ShowEmptyPage);
                }
                else
                {
                    Activity.RunOnUiThread(() =>
                    {
                        try
                        {
                            if (ProgressBarLoader.Visibility == ViewStates.Visible)
                                ProgressBarLoader.Visibility = ViewStates.Gone;

                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated = EmptyStateLayout.Inflate();
                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.Login);
                            if (!x.EmptyStateButton.HasOnClickListeners)
                            {
                                x.EmptyStateButton.Click += null;
                                x.EmptyStateButton.Click += LoginButtonOnClick;
                            }

                            EmptyStateLayout.Visibility = ViewStates.Visible;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
            MainScrollEvent.IsLoading = false;
        }
        //private async Task LoadDataAsync(string offset = "0")
        //{
        //    if (Methods.CheckConnectivity())
        //    {

        //        if (UserDetails.IsLogin)
        //        {
        //            if (MainScrollEvent.IsLoading)
        //                return;

        //            MainScrollEvent.IsLoading = true;

        //            int countList = MAdapter.CommentList.Count;

        //            var (apiStatus, respond) = await RequestsAsync.Comments.Get_Video_Comments_Http(VideoId, "20", offset);
        //            if (apiStatus != 200 || !(respond is GetCommentsObject result) || result.ListComments == null)
        //            {
        //                MainScrollEvent.IsLoading = false;
        //                Methods.DisplayReportResult(Activity, respond);
        //            }
        //            else
        //            {
        //                var respondList = result.ListComments.Count;
        //                if (respondList > 0)
        //                {
        //                    if (countList > 0)
        //                    {
        //                        foreach (var item in from item in result.ListComments let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
        //                        {
        //                            MAdapter.CommentList.Insert(0, item);
        //                        }

        //                        Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
        //                    }
        //                    else
        //                    {
        //                        MAdapter.CommentList = new ObservableCollection<CommentObject>(result.ListComments);
        //                        Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
        //                    }
        //                }
        //                else if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
        //                {
        //                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short).Show();
        //                }
        //            }

        //            Activity.RunOnUiThread(ShowEmptyPage);
        //        }
        //        else
        //        {
        //            Activity.RunOnUiThread(() =>
        //            {
        //                try
        //                {
        //                    if (ProgressBarLoader.Visibility == ViewStates.Visible)
        //                        ProgressBarLoader.Visibility = ViewStates.Gone;

        //                    MRecycler.Visibility = ViewStates.Gone;

        //                    Inflated = EmptyStateLayout.Inflate();
        //                    EmptyStateInflater x = new EmptyStateInflater();
        //                    x.InflateLayout(Inflated, EmptyStateInflater.Type.Login);
        //                    if (!x.EmptyStateButton.HasOnClickListeners)
        //                    {
        //                        x.EmptyStateButton.Click += null;
        //                        x.EmptyStateButton.Click += LoginButtonOnClick;
        //                    }

        //                    EmptyStateLayout.Visibility = ViewStates.Visible;
        //                }
        //                catch (Exception e)
        //                {
        //                    Console.WriteLine(e);
        //                }
        //            });
        //        }
        //    }
        //    else
        //    {
        //        Inflated = EmptyStateLayout.Inflate();
        //        EmptyStateInflater x = new EmptyStateInflater();
        //        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
        //        if (!x.EmptyStateButton.HasOnClickListeners)
        //        {
        //            x.EmptyStateButton.Click += null;
        //            x.EmptyStateButton.Click += EmptyStateButtonOnClick;
        //        }

        //        Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
        //    }
        //    MainScrollEvent.IsLoading = false;
        //}

        private void LoginButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Activity.StartActivity(new Intent(Activity, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                if (MAdapter.CommentList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoComments);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService(VideoId, "0");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    TabbedMainActivity.GetInstance()?.FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.CommentList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id.ToString()) && !MainScrollEvent.IsLoading)
                    StartApiService(VideoId, item.Id.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

    }
}