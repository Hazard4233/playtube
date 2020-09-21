using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PlayTube.Helpers
{
    public class LoadingView
    {
        static ProgressDialog progress;
        public static void Show(Context context, string message)
        {
            Hide();
            progress = new ProgressDialog(context);
            progress.Indeterminate = true;
            progress.SetProgressStyle(ProgressDialogStyle.Spinner);
            progress.SetMessage(message);
            progress.SetCancelable(false);
            progress.Show();
        }
        public static void Hide()
        {
            if (progress != null)
            {
                progress.Hide();
                progress.Dismiss();
            }
        }
    }
}