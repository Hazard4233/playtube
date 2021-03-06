﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using PlayTube.Activities.Tabbes;
using PlayTube.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Video;
using PlayTubeClient.RestCalls;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Fragment = Android.Support.V4.App.Fragment;

namespace PlayTube.Activities.Videos
{
    public class VideosByTypeFragment : Fragment
    {
        #region Variables Basic

        private VideoRowAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private TabbedMainActivity GlobalContext;
        private AdView MAdView;
        private string TypeVideo = "";
        
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            GlobalContext = (TabbedMainActivity)Activity;
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater.Inflate(Resource.Layout.RecyclerDefaultLayout, container, false);

                TypeVideo = Arguments.GetString("Type") ?? "";
                 
                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters(); 

                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                MAdapter.ItemClick += MAdapterOnItemClick;

                //Get Data Api
                GetVideoByType();

                AdsGoogle.Ad_Interstitial(this.Activity);

                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
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
                    try
                    {
                        GlobalContext.FragmentNavigatorBack();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                MAdView = view.FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                MAdapter = new VideoRowAdapter(Activity)
                {
                    VideoList = new ObservableCollection<VideoObject>()
                };
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<VideoObject>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    GlobalContext.SetToolBar(toolbar, TypeVideo + " " + Context.GetText(Resource.String.Lbl_Videos));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
 
        #endregion

        #region Events
 
        private void MAdapterOnItemClick(object sender, VideoRowAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = MAdapter.GetItem(e.Position);
                if (item == null) return;

                GlobalContext.StartPlayVideo(item);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                //Get Data Api
                MAdapter.VideoList.Clear();
                MAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.VideoList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void GetVideoByType()
        {
            try
            {
                if (TypeVideo.Contains("Top"))
                { 
                    var checkList = GlobalContext.MainVideoFragment.MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.TopVideos);
                    if (checkList != null)
                    {
                        MAdapter.VideoList = new ObservableCollection<VideoObject>(checkList.TopVideoList);

                        var lastId = checkList.TopVideoList.LastOrDefault()?.Id ?? "0";
                        StartApiService(lastId);
                    } 
                }
                else if (TypeVideo.Contains("Latest"))
                {
                    var checkList = GlobalContext.MainVideoFragment.MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.LatestVideos);
                    if (checkList != null)
                    {
                        MAdapter.VideoList = new ObservableCollection<VideoObject>(checkList.LatestVideoList);

                        var lastId = checkList.LatestVideoList.LastOrDefault()?.Id ?? "0";
                        StartApiService("0" , lastId);
                    } 
                }
                else if (TypeVideo.Contains("Fav"))
                {
                    var checkList = GlobalContext.MainVideoFragment.MAdapter.MainVideoList.FirstOrDefault(q => q.Type == ItemType.FavVideos);
                    if (checkList != null)
                    {
                        MAdapter.VideoList = new ObservableCollection<VideoObject>(checkList.FavVideoList);

                        var lastId = checkList.FavVideoList.LastOrDefault()?.Id ?? "0";
                        StartApiService("0", "0", lastId);
                    } 
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void StartApiService( string topOffset = "0", string latestOffset = "0", string favOffset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(topOffset, latestOffset, favOffset) });
        }

        private async Task LoadDataAsync( string topOffset = "0", string latestOffset = "0", string favOffset = "0")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                int countList = MAdapter.VideoList.Count;
                  
                var (apiStatus, respond) = await RequestsAsync.Video.Get_Videos_Http("0", topOffset, latestOffset, favOffset, "25");
                if (apiStatus != 200 || !(respond is GetVideosObject result) || result.DataResult == null)
                {
                    MainScrollEvent.IsLoading = false; 
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    int respondList = 0;
                    if (TypeVideo.Contains("Top"))
                    {
                        result.DataResult.Top = AppTools.ListFilter(new List<VideoObject>(result.DataResult.Top));
                        respondList = result.DataResult.Top.Count;
                    } 
                    else if (TypeVideo.Contains("Latest"))
                    {
                        result.DataResult.Latest = AppTools.ListFilter(new List<VideoObject>(result.DataResult.Latest));
                        respondList = result.DataResult.Latest.Count;
                    }  
                    else if (TypeVideo.Contains("Fav"))
                    {
                        result.DataResult.Fav = AppTools.ListFilter(new List<VideoObject>(result.DataResult.Fav));
                        respondList = result.DataResult.Fav.Count;
                    }  
                    if (respondList > 0)
                    { 
                        if (countList > 0)
                        {
                            if (TypeVideo.Contains("Top"))
                            { 
                                foreach (var item in from item in result.DataResult.Top let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
                                {
                                    MAdapter.VideoList.Add(item);
                                }
                            }
                            else if (TypeVideo.Contains("Latest"))
                            { 
                                foreach (var item in from item in result.DataResult.Latest let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
                                {
                                    MAdapter.VideoList.Add(item);
                                }
                            }
                            else if (TypeVideo.Contains("Fav"))
                            { 
                                foreach (var item in from item in result.DataResult.Fav let check = MAdapter.VideoList.FirstOrDefault(a => a.VideoId == item.VideoId) where check == null select item)
                                {
                                    MAdapter.VideoList.Add(item);
                                }
                            } 
                            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                        else
                        {
                            if (TypeVideo.Contains("Top"))
                            { 
                                MAdapter.VideoList = new ObservableCollection<VideoObject>(result.DataResult.Top);
                            }
                            else if (TypeVideo.Contains("Latest"))
                            {
                                MAdapter.VideoList = new ObservableCollection<VideoObject>(result.DataResult.Latest);
                            }
                            else if (TypeVideo.Contains("Fav"))
                            {
                                MAdapter.VideoList = new ObservableCollection<VideoObject>(result.DataResult.Fav);
                            }
                            
                            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.VideoList.Count > 10 && !MRecycler.CanScrollVertically(1))
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

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.VideoList.Count > 0)
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
                GetVideoByType();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
      
        #endregion 
    }
}   