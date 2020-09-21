using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using PlayTube.API;
using PlayTube.API.Models;
using PlayTube.Helpers.CacheLoaders;
using WoWonder.Helpers;

namespace PlayTube.Activities.UserReponse
{
    public class UserResponseListAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<RvDatum> UserResponseList = new ObservableCollection<RvDatum>();
        public override int ItemCount => UserResponseList != null ? UserResponseList.Count : 0;
        Activity context;

        string VideoId;

        public UserResponseListAdapter(Activity _context, string videoId)
        {
            context = _context;
            VideoId = videoId;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override int GetItemViewType(int position)
        {
            return position;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as UserResponseListViewHolder;
            var item = UserResponseList[position];
            holder.txtUserName.Text = item.Username;
            //GlideImageLoader.LoadImage(context, UserResponseURL.AvtaarURL, holder.imgProfile, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
            Glide.With(context).Load(UserResponseURL.DirectoryURL + item.Thumbnail).Into(holder.imgThumbnail);
            Glide.With(context).Load(UserResponseURL.DirectoryUserSettings + item.Avatar).Into(holder.imgProfile);
            if (item.RvvId == null)
            {
                holder.shape_layout.SetBackgroundResource(Resource.Drawable.circular_bordershape);
                //holder.frameLayout.Background = context.Resources.GetDrawable(Resource.Drawable.circular_bordershape);
                //holder.frameLayout.BackgroundTintList = context.Resources.GetColorStateList(Android.Resource.Color.White);
            }
            DisplayMetrics displayMetrics = new DisplayMetrics();
            context.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);
            holder.cardMain.LayoutParameters.Height = displayMetrics.WidthPixels * 1 / 2;
            holder.cardMain.RequestLayout();
            holder.cardMain.Click -= CardMain_Click;
            holder.cardMain.Click += CardMain_Click;
            holder.cardMain.Tag = new JavaObjectWrapper<RvDatum>() { Obj = item };
        }

        private void CardMain_Click(object sender, EventArgs e)
        {
            var btn = (sender as CardView);
            var item = (btn.Tag as JavaObjectWrapper<RvDatum>).Obj;
            Intent intent = new Intent(this.context, typeof(MediaPlayerActivity));
            intent.PutExtra("mediaList", JsonConvert.SerializeObject(UserResponseList));
            intent.PutExtra("selectedMedia", JsonConvert.SerializeObject(item));
            intent.PutExtra("videoId", VideoId);
            context.StartActivity(intent);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.UserResposneList, parent, false);
            var vh = new UserResponseListViewHolder(itemView);
            return vh;
        }

    }
    public class UserResponseListViewHolder : RecyclerView.ViewHolder
    {
        public CardView cardMain { get; set; }
        public ImageView imgThumbnail { get; set; }
        public ImageView imgProfile { get; set; }
        public RelativeLayout shape_layout { get; set; }
        public TextView txtUserName { get; set; }
        public FrameLayout frameLayout { get; set; }
        public UserResponseListViewHolder(View view) : base(view)
        {
            cardMain = view.FindViewById<CardView>(Resource.Id.cardMain);
            imgThumbnail = view.FindViewById<ImageView>(Resource.Id.imgThumbnail);
            imgProfile = view.FindViewById<ImageView>(Resource.Id.imgProfile);
            txtUserName = view.FindViewById<TextView>(Resource.Id.txtUserName);
            shape_layout = view.FindViewById<RelativeLayout>(Resource.Id.shape_layout);
            frameLayout = view.FindViewById<FrameLayout>(Resource.Id.frameLayout);
        }
    }
}