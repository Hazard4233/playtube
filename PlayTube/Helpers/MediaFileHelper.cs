using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using PlayTube.Activities.SettingsPreferences;

namespace PlayTube.Helpers
{
    public enum MediaDownlaodStatus
    {
        Success,
        Fail,
        Cancel
    }
    public class MediaFileHelper
    {
        public delegate void DownloadComplete(MediaDownlaodStatus status, byte[] array);
        public event DownloadComplete DownloadCompleted;
        public WebClient webClient;
        public void DownloadMediaFile(string url)
        {
            try
            {
                webClient = new WebClient();
                webClient.DownloadDataCompleted -= WebClient_DownloadDataCompleted;
                webClient.DownloadDataCompleted += WebClient_DownloadDataCompleted;
                webClient.DownloadDataAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                DownloadCompleted(MediaDownlaodStatus.Fail, null);
            }
        }

        private void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (DownloadCompleted != null)
                DownloadCompleted(MediaDownlaodStatus.Success, e.Result);
        }

        public void CancelDownload()
        {
            webClient?.CancelAsync();
            webClient.DownloadDataCompleted -= WebClient_DownloadDataCompleted;
        }

        public static FileDescriptor GetMediaFileDescriptor(Context context, byte[] byteArray)
        {
            try
            {
                // create temp file that will hold byte array
                Java.IO.File tempMp3 = Java.IO.File.CreateTempFile("demo", "mp4", context.CacheDir);
                tempMp3.DeleteOnExit();
                FileOutputStream fos = new FileOutputStream(tempMp3);
                fos.Write(byteArray);
                fos.Close();
                FileInputStream fis = new FileInputStream(tempMp3);
                return fis.FD;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}