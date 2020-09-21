using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PlayTube.Activities.Channel.Adapters;
using PlayTube.API;
using PlayTube.API.Models;
using Fragment = Android.Support.V4.App.Fragment;
using Android.Support.V4.Widget;
using PlayTube.Helpers.Utils;
using Android.Graphics;
using PlayTube.Helpers.Controller;
using System.Threading.Tasks;
using System.Net;

namespace PlayTube.Activities.Channel.Tab
{
    public enum ResponsesEnum
    {
        FollowingResponses,
        ForYouResponses
    }
    public class ResponsesFragment : Fragment
    {
        public ObservableCollection<AdminVideoResponse> FollowingResponses = new ObservableCollection<AdminVideoResponse>();
        public ObservableCollection<AdminVideoResponse> ForYouResponses = new ObservableCollection<AdminVideoResponse>();
        RecyclerView recyclerViewUserSettigs;
        public SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerViewOnScrollListener MainScrollEvent;
        public ViewStub EmptyStateLayout;
        public View Inflated;
        public ResponsesHomeAdapter adpater;
        private bool loadData = false; 
        ResponsesEnum VideoEnum;
        public ResponsesFragment(ResponsesEnum _videoEnum, bool data)
        {
            VideoEnum = _videoEnum;
            loadData = data;
            //GetAdminVideoList();
        }
        public void GetAdminVideoList()
        {
            if (VideoEnum == ResponsesEnum.FollowingResponses)
            {
                //FollowingResponses = new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetFollowingResponseVideos().VideoResponse);
                FollowingResponses = new ObservableCollection<AdminVideoResponse>();
            }
            else
            {
                //ForYouResponses = new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetForYouResponseVideos().VideoResponse);
                ForYouResponses = new ObservableCollection<AdminVideoResponse>();
            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.ResponsesLayout, container, false);
            InitComponent(view);
            SetRecyclerView(VideoEnum);

            SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

            //Get Data Api
            if (loadData)
                StartApiService();

            return view;
        }
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }

        private void InitComponent(View view)
        {
            recyclerViewUserSettigs = view.FindViewById<RecyclerView>(Resource.Id.recyclerViewResponses);
            EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStubResponses);

            SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayoutResponses);
            SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
            SwipeRefreshLayout.Refreshing = true;
            SwipeRefreshLayout.Enabled = true;
            SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
        }
        public void SetRecyclerView(ResponsesEnum _videoEnum)
        {
            GridLayoutManager manager = new GridLayoutManager(this.Context, 2);
            recyclerViewUserSettigs.SetLayoutManager(manager);
            if (_videoEnum == ResponsesEnum.FollowingResponses)
            {
                adpater = new ResponsesHomeAdapter(this.Activity, _videoEnum);
                adpater.FollowingResponses = FollowingResponses;
                recyclerViewUserSettigs.SetAdapter(adpater);
            }
            else
            {
                adpater = new ResponsesHomeAdapter(this.Activity, _videoEnum);
                adpater.ForYouResponses = ForYouResponses;
                recyclerViewUserSettigs.SetAdapter(adpater);
            }

            RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(manager);
            MainScrollEvent = xamarinRecyclerViewOnScrollListener;
            MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
            recyclerViewUserSettigs.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
            MainScrollEvent.IsLoading = false;
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                //Get Data Api
                if (VideoEnum == ResponsesEnum.FollowingResponses)
                    adpater.FollowingResponses.Clear();
                else
                    adpater.ForYouResponses.Clear();
                adpater.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Code get last id where LoadMore >>
                AdminVideoResponse item;
                if (VideoEnum == ResponsesEnum.FollowingResponses)
                    item = adpater.FollowingResponses.LastOrDefault();
                else
                    item = adpater.ForYouResponses.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.VideoId) && !MainScrollEvent.IsLoading)
                    StartApiService(item.VideoId);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Load Data Api 
        public void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                int countList;
                AdminVideoResponseModel response;
                if (VideoEnum == ResponsesEnum.FollowingResponses) {
                    countList = adpater.FollowingResponses.Count;
                    response = await UserResponseAPI.GetFollowingResponseVideos();
                }
                else {
                    countList = adpater.ForYouResponses.Count;
                    response = await UserResponseAPI.GetForYouResponseVideos();
                }

                if (VideoEnum == ResponsesEnum.FollowingResponses)
                {
                    var videoList = response.VideoResponse.ToList();
                    var respondList = videoList.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in videoList let check = adpater.FollowingResponses.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
                            {
                                adpater.FollowingResponses.Add(item);
                            }

                            Activity.RunOnUiThread(() => { adpater.NotifyItemRangeInserted(countList, adpater.FollowingResponses.Count - countList); });
                        }
                        else
                        {
                            adpater.FollowingResponses = new ObservableCollection<AdminVideoResponse>(response.VideoResponse);
                            Activity.RunOnUiThread(() => { adpater.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (((VideoEnum == ResponsesEnum.FollowingResponses && adpater.FollowingResponses.Count > 10) || (VideoEnum == ResponsesEnum.ForYouResponses && adpater.ForYouResponses.Count > 10)) && !recyclerViewUserSettigs.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short).Show();
                    }
                } else
                {
                    var videoList = response.VideoResponse.ToList();
                    var respondList = videoList.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in videoList let check = adpater.ForYouResponses.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
                            {
                                adpater.ForYouResponses.Add(item);
                            }

                            Activity.RunOnUiThread(() => { adpater.NotifyItemRangeInserted(countList, adpater.ForYouResponses.Count - countList); });
                        }
                        else
                        {
                            adpater.ForYouResponses = new ObservableCollection<AdminVideoResponse>(response.VideoResponse);
                            Activity.RunOnUiThread(() => { adpater.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (((VideoEnum == ResponsesEnum.FollowingResponses && adpater.FollowingResponses.Count > 10) || (VideoEnum == ResponsesEnum.ForYouResponses && adpater.ForYouResponses.Count > 10)) && !recyclerViewUserSettigs.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreVideos), ToastLength.Short).Show();
                    }
                }

                Activity.RunOnUiThread(ShowEmptyPage);
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

        public void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                
                if((VideoEnum == ResponsesEnum.FollowingResponses && adpater.FollowingResponses.Count > 0) || (VideoEnum == ResponsesEnum.ForYouResponses && adpater.ForYouResponses.Count > 0))
                {
                    recyclerViewUserSettigs.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    recyclerViewUserSettigs.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoVideo);
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
                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        #endregion
    }
}