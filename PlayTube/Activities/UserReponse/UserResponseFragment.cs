using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using PlayTube.API;
using PlayTube.API.Models;
using PlayTube.Helpers.Controller;

namespace PlayTube.Activities.UserReponse
{
    public class UserResponseFragment : Fragment
    {
        RecyclerView recyclerViewResponse;
        public UserResponseListAdapter UserResponseAdapter;
        ObservableCollection<RvDatum> ResponseList;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.UserResponseLayout, container, false);

            //Get Value And Set Toolbar
            InitComponent(view);

            return view;
        }

        private void InitComponent(View view)
        {
            recyclerViewResponse = view.FindViewById<RecyclerView>(Resource.Id.recyclerViewResponse);
        }

        public async Task GetResponseList(string videoId)
        {
            var response = await UserResponseAPI.GetUserResponse(videoId);
            ResponseList = new ObservableCollection<RvDatum>(response);
        }

        public void GetUserResponse(string videoId)
        {
            try
            {
                RecyclerView.LayoutManager mLayoutManager = new GridLayoutManager(this.Context, 3);
                recyclerViewResponse.SetLayoutManager(mLayoutManager);
                UserResponseAdapter = new UserResponseListAdapter(this.Activity, videoId);
                UserResponseAdapter.UserResponseList = ResponseList;
                recyclerViewResponse.SetAdapter(UserResponseAdapter);
                ViewCompat.SetNestedScrollingEnabled(recyclerViewResponse, false);
            }
            catch (Exception ex)
            {

            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
        }
    }
}