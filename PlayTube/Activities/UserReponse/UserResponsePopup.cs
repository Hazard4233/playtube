using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using PlayTube.Helpers.Controller;

namespace PlayTube.Activities.UserReponse
{
    public class UserResponsePopup : Android.Support.V4.App.DialogFragment
    {
        TextView txtAudio, txtVideo;
        string VideoId;
        VideoController VideoController;
        IYouTubePlayer YoutubePlayer;
        public UserResponseFragment ResponseFragment;

        public UserResponsePopup(string videoId, VideoController videoController, UserResponseFragment _responseFragment, IYouTubePlayer youtubePlayer = null)
        {
            VideoId = videoId;
            VideoController = videoController;
            YoutubePlayer = youtubePlayer;
            ResponseFragment = _responseFragment;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.UserResponsePopup, container, false);
            InitView(view);
            return view;
        }

        private void InitView(View view)
        {
            txtAudio = view.FindViewById<TextView>(Resource.Id.txtAudio);
            txtVideo = view.FindViewById<TextView>(Resource.Id.txtVideo);
            txtAudio.Click += TxtAudio_Click;
            txtVideo.Click += TxtVideo_Click;
        }

        private void TxtVideo_Click(object sender, EventArgs e)
        {
            UserResponseBottomSheet mFragment = new UserResponseBottomSheet(UserResponse.Video, VideoId, VideoController, ResponseFragment, YoutubePlayer);
            mFragment.Show(this.FragmentManager, mFragment.Tag);
            this.Dismiss();
        }

        private void TxtAudio_Click(object sender, EventArgs e)
        {
            UserResponseBottomSheet mFragment = new UserResponseBottomSheet(UserResponse.Audio, VideoId, VideoController, ResponseFragment, YoutubePlayer);
            mFragment.Show(this.FragmentManager, mFragment.Tag);
            this.Dismiss();
        }
    }
}