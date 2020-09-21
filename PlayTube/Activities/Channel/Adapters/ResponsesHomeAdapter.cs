using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
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
    public class ResponsesHomeAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<AdminVideoResponse> FollowingResponses = new ObservableCollection<AdminVideoResponse>();
        public ObservableCollection<AdminVideoResponse> ForYouResponses = new ObservableCollection<AdminVideoResponse>();
        Activity activity;
        ResponsesEnum VideoEnum;
        public ResponsesHomeAdapter(Activity _activity, ResponsesEnum enumVideo)
        {
            activity = _activity;
            VideoEnum = enumVideo;
        }
        public override int ItemCount
        {
            get
            {
                if (VideoEnum == ResponsesEnum.FollowingResponses)
                    return FollowingResponses != null ? FollowingResponses.Count : 0;
                else
                    return ForYouResponses != null ? ForYouResponses.Count : 0;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is ResponsesVideoHolder viewHolder)
            {
                if (VideoEnum == ResponsesEnum.FollowingResponses)
                {
                    viewHolder.checkBoxApprove.Visibility = ViewStates.Gone;
                    var item = FollowingResponses[position];
                    if (item.Thumbnail.StartsWith("https"))
                        Glide.With(activity).Load(item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    else
                        Glide.With(activity).Load(UserResponseURL.DirectoryURL + item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    viewHolder.txtdays.Visibility = ViewStates.Gone;
                    viewHolder.txtVideoTitle.Visibility = ViewStates.Gone;
                    //viewHolder.txtVideoTitle.Text = item.Username;
                    viewHolder.cardMain.Click -= FollowCardMain_Click;
                    viewHolder.cardMain.Click += FollowCardMain_Click;
                    viewHolder.txtdays.Text = Helpers.DateTimeHelper.GetDateTimeString(item.Timestamp);
                    viewHolder.cardMain.Tag = new JavaObjectWrapper<AdminVideoResponse>() { Obj = item };
                    DisplayMetrics displayMetrics = new DisplayMetrics();
                    activity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
                    viewHolder.imgVideoThumbnail.LayoutParameters.Height = displayMetrics.WidthPixels * 2 / 3;
                    viewHolder.imgVideoThumbnail.RequestLayout();
                }
                else
                {
                    viewHolder.checkBoxApprove.Visibility = ViewStates.Gone;
                    var item = ForYouResponses[position];
                    if (item.Thumbnail.StartsWith("https"))
                        Glide.With(activity).Load(item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    else
                        Glide.With(activity).Load(UserResponseURL.DirectoryURL + item.Thumbnail).Into(viewHolder.imgVideoThumbnail);
                    viewHolder.txtdays.Visibility = ViewStates.Gone;
                    viewHolder.txtVideoTitle.Visibility = ViewStates.Gone;
                    //viewHolder.txtVideoTitle.Text = item.Username;
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
            Intent intent = new Intent(this.activity, typeof(HomeResponsePlayerActivity));
            intent.PutExtra("mediaList", JsonConvert.SerializeObject(ForYouResponses));
            intent.PutExtra("selectedMedia", JsonConvert.SerializeObject(item));
            intent.PutExtra("type", "ForYou");
            intent.PutExtra("swipeAction", "None");
            activity.StartActivity(intent);
            //activity.OverridePendingTransition(Resource.Animation.response_slide_in, Resource.Animation.response_slide_hold);
            
        }
        private void FollowCardMain_Click(object sender, EventArgs e)
        {
            var btn = (sender as CardView);
            var item = (btn.Tag as JavaObjectWrapper<AdminVideoResponse>).Obj;
            Intent intent = new Intent(this.activity, typeof(HomeResponsePlayerActivity));
            intent.PutExtra("mediaList", JsonConvert.SerializeObject(FollowingResponses));
            intent.PutExtra("selectedMedia", JsonConvert.SerializeObject(item));
            intent.PutExtra("type", "Follow");
            intent.PutExtra("swipeAction", "None");
            activity.StartActivity(intent);
            //activity.OverridePendingTransition(Resource.Animation.response_slide_in, Resource.Animation.response_slide_hold);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.AdminVideoList, parent, false);
            var vh = new ResponsesVideoHolder(itemView);
            return vh;
        }
    }
    public class ResponsesVideoHolder : RecyclerView.ViewHolder
    {
        public ImageView imgVideoThumbnail;
        public TextView txtVideoTitle;
        public CheckBox checkBoxApprove;
        public CardView cardMain;
        public TextView txtdays;
        public ResponsesVideoHolder(View view) : base(view)
        {
            imgVideoThumbnail = view.FindViewById<ImageView>(Resource.Id.imgVideoThumbnail);
            txtVideoTitle = view.FindViewById<TextView>(Resource.Id.txtVideoTitle);
            checkBoxApprove = view.FindViewById<CheckBox>(Resource.Id.checkBoxApprove);
            cardMain = view.FindViewById<CardView>(Resource.Id.cardMain);
            txtdays = view.FindViewById<TextView>(Resource.Id.txtdays);
        }
    }
}