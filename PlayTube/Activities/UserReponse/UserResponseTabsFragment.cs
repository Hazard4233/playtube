using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.Content.Res;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PlayTube.Activities.Videos;

namespace PlayTube.Activities.UserReponse
{
    public class UserResponseTabsFragment : Fragment, TabLayout.IOnTabSelectedListener
    {
        public NextToFragment NextFragment;
        public UserResponseFragment ResponseFragment;
        public TabLayout tabLayout;
        public ViewPager viewPager;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.ResponseTabsLayout, container, false);

            //Get Value And Set Toolbar
            InitComponent(view);

            return view;
        }

        public void OnTabReselected(TabLayout.Tab tab)
        {

        }

        public void OnTabSelected(TabLayout.Tab tab)
        {
            if (tabLayout.SelectedTabPosition == 0)
            {
                setTabBG(Resource.Drawable.tab_left_select, Resource.Drawable.tab_right_unselect);
            }
            else
            {
                setTabBG(Resource.Drawable.tab_left_unselect, Resource.Drawable.tab_right_select);
            }
        }
        private void setTabBG(int tab1, int tab2)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBeanMr1)
            {
                ViewGroup tabStrip = (ViewGroup)tabLayout.GetChildAt(0);
                View tabView1 = tabStrip.GetChildAt(0);
                View tabView2 = tabStrip.GetChildAt(1);
                if (tabView1 != null)
                {
                    int paddingStart = tabView1.PaddingStart;
                    int paddingTop = tabView1.PaddingTop;
                    int paddingEnd = tabView1.PaddingEnd;
                    int paddingBottom = tabView1.PaddingBottom;
                    ViewCompat.SetBackground(tabView1, AppCompatResources.GetDrawable(this.Context, tab1));
                    ViewCompat.SetPaddingRelative(tabView1, paddingStart, paddingTop, paddingEnd, paddingBottom);
                }

                if (tabView2 != null)
                {
                    int paddingStart = tabView2.PaddingStart;
                    int paddingTop = tabView2.PaddingTop;
                    int paddingEnd = tabView2.PaddingEnd;
                    int paddingBottom = tabView2.PaddingBottom;
                    ViewCompat.SetBackground(tabView2, AppCompatResources.GetDrawable(this.Context, tab2));
                    ViewCompat.SetPaddingRelative(tabView2, paddingStart, paddingTop, paddingEnd, paddingBottom);
                }
            }
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {

        }

        private void InitComponent(View view)
        {
            tabLayout = (TabLayout)view.FindViewById(Resource.Id.tabs);
            tabLayout.AddTab(tabLayout.NewTab().SetText("Response"), 0, true);
            tabLayout.AddTab(tabLayout.NewTab().SetText("Related Video"), 1, false);
            viewPager = (ViewPager)view.FindViewById(Resource.Id.viewpager);
            LinearLayoutManager mLayoutManager = new LinearLayoutManager(this.Activity);
            mLayoutManager.Orientation = LinearLayoutManager.Vertical;
            //mRecyclerView.setLayoutManager(mLayoutManager);
            viewPager.Adapter = new PagerAdapter(this.FragmentManager, tabLayout.TabCount, NextFragment, ResponseFragment);
            tabLayout.SetOnTabSelectedListener(this);
            //viewPager.AddOnPageChangeListener(new TabLayout.TabLayoutOnPageChangeListener(tabLayout));
            tabLayout.SetupWithViewPager(viewPager);
            setTabBG(Resource.Drawable.tab_left_select, Resource.Drawable.tab_right_unselect);
        }
    }
    public class PagerAdapter : FragmentStatePagerAdapter
    {
        private string[] tabTitles = new string[] { "Response", "Relative Video" };
        public NextToFragment NextFragment;
        public UserResponseFragment ResponseFragment;
        int mNumOfTabs;
        public PagerAdapter(FragmentManager fm, int NumOfTabs, NextToFragment nextFragment, UserResponseFragment responseFragment) : base(fm)
        {
            this.mNumOfTabs = NumOfTabs;
            NextFragment = nextFragment;
            ResponseFragment = responseFragment;
        }
        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(tabTitles[position]);
        }

        public override int Count => mNumOfTabs;

        public override Fragment GetItem(int position)
        {
            switch (position)
            {
                case 0:
                    return ResponseFragment;
                case 1:
                    return NextFragment;
                default:
                    return null;
            }
        }
    }
}