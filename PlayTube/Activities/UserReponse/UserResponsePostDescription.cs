using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;

namespace PlayTube.Activities.UserReponse
{
    public class UserResponsePostDescription : Android.Support.V4.App.DialogFragment
    {
        private Intent intentData;
        private Context mainContext;
        private TabbedMainActivity tabbed;
        private GlobalPlayerActivity global;
        private EditText editDescription;
        private Button btnCancel, btnPost;
        public UserResponsePostDescription(Intent data, Context context, int type)
        {
            intentData = data;
            mainContext = context;
            if (type == 1)
                tabbed = (TabbedMainActivity)context;
            else if(type == 2)
                global = (GlobalPlayerActivity)context;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.UserResponsePostDescription, container, false);
            InitView(view);
            editDescription.RequestFocus();
            Action DescriptionFocus = () =>
            {
                editDescription.RequestFocus();

                InputMethodManager imm = (InputMethodManager)mainContext.GetSystemService(Context.InputMethodService);
                imm.ShowSoftInput(editDescription, ShowFlags.Implicit);
            };
            view.PostDelayed(DescriptionFocus, 100);
            //InputMethodManager imm = (InputMethodManager)mainContext.GetSystemService(Context.InputMethodService);
            //imm.ShowSoftInput(editDescription, ShowFlags.Implicit);
            //imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            return view;
        }

        private void InitView(View view)
        {
            editDescription = view.FindViewById<EditText>(Resource.Id.editDescription);
            btnCancel = view.FindViewById<Button>(Resource.Id.btnCancel);
            btnPost = view.FindViewById<Button>(Resource.Id.btnPost);
            btnPost.Click += btnPost_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnPost_Click(object sender, EventArgs e)
        {
            Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(mainContext);
            Android.App.AlertDialog alert = dialog.Create();
            alert.SetTitle("Confirmation");
            alert.SetMessage("Are you sure you want to upload the selected file?");
            alert.SetButton("OK", async (c, ev) =>
            {
                var array = Helpers.VideoHelper.GetSelectedMediaData((Activity)mainContext, intentData.Data);
                
                if(tabbed != null)
                    await tabbed.UploadSelectedResponse(array, editDescription.Text);
                else if(global != null)
                    await global.UploadSelectedResponse(array, editDescription.Text);

                alert.Hide();
                this.Dismiss();
            });
            alert.SetButton2("CANCEL", (c, ev) => { alert.Hide(); });
            alert.Show();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dismiss();
        }
    }
}