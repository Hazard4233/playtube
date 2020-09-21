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
using Android.Util;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Newtonsoft.Json;
using PlayTube.Activities.Channel.Tab;
using PlayTube.Activities.UserReponse;
using PlayTube.API;
using PlayTube.API.Models;
using WoWonder.Helpers;

namespace PlayTube.Activities.Channel.Adapters
{
    public class AdminVideoistAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<UserSettingVideo> AdminVideoList = new ObservableCollection<UserSettingVideo>();
        public ObservableCollection<AdminVideoResponse> LatestResponse = new ObservableCollection<AdminVideoResponse>();
        Activity activity;
        AdminVideoEnum VideoEnum;
        public AdminVideoistAdapter(Activity _activity, AdminVideoEnum enumVideo)
        {
            activity = _activity;
            VideoEnum = enumVideo;
        }
        public override int ItemCount
        {
            get
            {
                if (VideoEnum == AdminVideoEnum.UserSettings)
                    return AdminVideoList != null ? AdminVideoList.Count : 0;
                else
                    return LatestResponse != null ? LatestResponse.Count : 0;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is AdminVideoistVideHolder viewHolder)
            {

                if (VideoEnum == AdminVideoEnum.UserSettings)
                {
                    var item = AdminVideoList[position];
                    if (item.Thumbnail.StartsWith("https"))
                        Glide.With(activity).Load(item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    else
                        Glide.With(activity).Load(UserResponseURL.DirectoryUserSettings + item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    viewHolder.txtVideoTitle.Text = item.Title;
                    DisplayMetrics displayMetrics = new DisplayMetrics();
                    activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                    viewHolder.imgVideoThumbnail.LayoutParameters.Height = displayMetrics.WidthPixels * 1 / 3;
                    viewHolder.imgVideoThumbnail.RequestLayout();
                    viewHolder.checkBoxApprove.Visibility = ViewStates.Visible;
                    if (item.Status == 0)
                        viewHolder.checkBoxApprove.Checked = false;
                    else
                        viewHolder.checkBoxApprove.Checked = true;
                    viewHolder.txtdays.Visibility = ViewStates.Gone;
                    viewHolder.checkBoxApprove.CheckedChange -= CheckBoxApprove_CheckedChange;
                    viewHolder.checkBoxApprove.CheckedChange += CheckBoxApprove_CheckedChange;
                    viewHolder.checkBoxApprove.Tag = new JavaObjectWrapper<int>() { Obj = item.RvsId };
                }
                else
                {
                    viewHolder.checkBoxApprove.Visibility = ViewStates.Gone;
                    var item = LatestResponse[position];
                    if (item.Thumbnail.StartsWith("https"))
                        Glide.With(activity).Load(item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    else
                        Glide.With(activity).Load(UserResponseURL.DirectoryURL + item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    viewHolder.txtdays.Visibility = ViewStates.Visible;
                    viewHolder.txtVideoTitle.Text = item.Username;
                    viewHolder.cardMain.Click -= CardMain_Click;
                    viewHolder.cardMain.Click += CardMain_Click;
                    viewHolder.txtdays.Text = Helpers.DateTimeHelper.GetDateTimeString(item.Timestamp);
                    viewHolder.cardMain.Tag = new JavaObjectWrapper<AdminVideoResponse>() { Obj = item };
                    DisplayMetrics displayMetrics = new DisplayMetrics();
                    activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                    viewHolder.imgVideoThumbnail.LayoutParameters.Height = displayMetrics.WidthPixels * 2 / 3;
                    viewHolder.imgVideoThumbnail.RequestLayout();
                }
            }
        }

        private void CardMain_Click(object sender, EventArgs e)
        {
            var btn = (sender as CardView);
            var item = (btn.Tag as JavaObjectWrapper<AdminVideoResponse>).Obj;
            Intent intent = new Intent(this.activity, typeof(LatestResponsePlayerActivity));
            intent.PutExtra("mediaList", JsonConvert.SerializeObject(LatestResponse));
            intent.PutExtra("selectedMedia", JsonConvert.SerializeObject(item));
            activity.StartActivity(intent);
        }

        private void CheckBoxApprove_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            var btn = (sender as CheckBox);
            var item = (btn.Tag as JavaObjectWrapper<int>).Obj;
            UserResponseAPI.ApproveUserSettings(item);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.AdminVideoList, parent, false);
            var vh = new AdminVideoistVideHolder(itemView);
            return vh;
        }
    }
    public class AdminVideoistVideHolder : RecyclerView.ViewHolder
    {
        public ImageView imgVideoThumbnail;
        public TextView txtVideoTitle;
        public CheckBox checkBoxApprove;
        public CardView cardMain;
        public TextView txtdays;
        public AdminVideoistVideHolder(View view) : base(view)
        {
            imgVideoThumbnail = view.FindViewById<ImageView>(Resource.Id.imgVideoThumbnail);
            txtVideoTitle = view.FindViewById<TextView>(Resource.Id.txtVideoTitle);
            checkBoxApprove = view.FindViewById<CheckBox>(Resource.Id.checkBoxApprove);
            cardMain = view.FindViewById<CardView>(Resource.Id.cardMain);
            txtdays = view.FindViewById<TextView>(Resource.Id.txtdays);
        }
    }
}