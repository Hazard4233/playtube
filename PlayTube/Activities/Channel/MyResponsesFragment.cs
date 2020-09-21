using System;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using PlayTube.Activities.Channel.Tab;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Tabbes;
using PlayTube.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Fragment = Android.Support.V4.App.Fragment;
using PlayTube.API;
using PlayTube.Activities.UserReponse;

namespace PlayTube.Activities.Channel
{
    public class MyResponsesFragment : Fragment
    {
        #region Variables Basic

        private TabLayout Tabs;
        public ViewPager ViewPagerView;
        //private TabbedMainActivity MainContext;
        public HomeResponsePlayerFragment ForYouResponses;
        public HomeResponsePlayerFragment FollowingResponses;
        public int activePage = 1;
        
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //MainContext = (TabbedMainActivity)Activity;
            //HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater.Inflate(Resource.Layout.ResponsesHome_Fragment, container, false);

                //Get Value And Set Toolbar
                InitComponent(view);
                InitTab();
                
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                Tabs = view.FindViewById<TabLayout>(Resource.Id.HomeResponseTabs);
                ViewPagerView = view.FindViewById<ViewPager>(Resource.Id.HomeResponseViewpager);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void InitTab()
        {
            try
            {
                ViewPagerView.OffscreenPageLimit = 1;
                SetUpViewPager(ViewPagerView);
                Tabs.SetupWithViewPager(ViewPagerView);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public override void OnResume()
        {
            base.OnResume();
        }

        #endregion

        #region Tab

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                //FollowingResponses = new ResponsesFragment(ResponsesEnum.FollowingResponses, false);
                //ForYouResponses = new ResponsesFragment(ResponsesEnum.ForYouResponses, true);
                FollowingResponses = new HomeResponsePlayerFragment("Follow", false);
                ForYouResponses = new HomeResponsePlayerFragment("ForYou", true);

                MainTabAdapter adapter = new MainTabAdapter(Activity.SupportFragmentManager);
                adapter.AddFragment(ForYouResponses, "For You");
                adapter.AddFragment(FollowingResponses, "Following");

                viewPager.PageSelected += ViewPagerOnPageSelected;
                viewPager.Adapter = adapter;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ViewPagerOnPageSelected(object sender, ViewPager.PageSelectedEventArgs page)
        {
            try
            {
                var p = page.Position;
                if (p == 0)
                {
                    if (!ForYouResponses.loadData)
                        ForYouResponses.StartApiService();
                    if(FollowingResponses.Player != null)
                        FollowingResponses.Player.PlayWhenReady = false;
                    if (ForYouResponses.Player != null)
                        ForYouResponses.Player.PlayWhenReady = true;
                    activePage = 1;
                }
                else if (p == 1)
                {
                    if(!FollowingResponses.loadData)
                        FollowingResponses.StartApiService();
                    if (ForYouResponses.Player != null)
                        ForYouResponses.Player.PlayWhenReady = false;
                    if (FollowingResponses.Player != null)
                        FollowingResponses.Player.PlayWhenReady = true;
                    activePage = 2;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

    }
}