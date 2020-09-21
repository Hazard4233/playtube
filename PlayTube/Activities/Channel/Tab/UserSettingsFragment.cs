using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PlayTube.Activities.Channel.Adapters;
using PlayTube.API;
using PlayTube.API.Models;
using Fragment = Android.Support.V4.App.Fragment;

namespace PlayTube.Activities.Channel.Tab
{
    public enum AdminVideoEnum
    {
        UserSettings,
        LatestResponse
    }
    public class UserSettingsFragment : Fragment
    {

        ObservableCollection<UserSettingVideo> AdminVideoList = new ObservableCollection<UserSettingVideo>();
        ObservableCollection<AdminVideoResponse> LatestResponse = new ObservableCollection<AdminVideoResponse>();
        RecyclerView recyclerViewUserSettigs;
        public AdminVideoistAdapter adpater;
        AdminVideoEnum VideoEnum;
        public UserSettingsFragment(AdminVideoEnum _videoEnum)
        {
            VideoEnum = _videoEnum;
            GetAdminVideoList();
        }
        public void GetAdminVideoList()
        {
            if (VideoEnum == AdminVideoEnum.UserSettings)
            {
                AdminVideoList = new ObservableCollection<UserSettingVideo>(UserResponseAPI.GetAdminUserSettings().VideosData);
            }
            else
            {
                LatestResponse = new ObservableCollection<AdminVideoResponse>(UserResponseAPI.GetAdminVideos().VideoResponse);
            }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.UserSettingsLayout, container, false);
            InitComponent(view);
            SetRecyclerView(VideoEnum);
            return view;
        }
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }

        private void InitComponent(View view)
        {
            recyclerViewUserSettigs = view.FindViewById<RecyclerView>(Resource.Id.recyclerViewUserSettigs);
        }
        public void SetRecyclerView(AdminVideoEnum _videoEnum)
        {
            GridLayoutManager manager = new GridLayoutManager(this.Context, 2);
            recyclerViewUserSettigs.SetLayoutManager(manager);
            if (_videoEnum == AdminVideoEnum.UserSettings)
            {
                adpater = new AdminVideoistAdapter(this.Activity, _videoEnum);
                adpater.AdminVideoList = AdminVideoList;
                recyclerViewUserSettigs.SetAdapter(adpater);
            }
            else
            {
                adpater = new AdminVideoistAdapter(this.Activity, _videoEnum);
                adpater.LatestResponse = LatestResponse;
                recyclerViewUserSettigs.SetAdapter(adpater);
            }
        }
    }
}