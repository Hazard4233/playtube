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
    public class DateTimeHelper
    {
        public static string GetDateTimeString(DateTimeOffset dateTime)
        {
            TimeSpan time = new TimeSpan();
            if (DateTime.UtcNow > dateTime)
                time = DateTime.UtcNow.Subtract(dateTime.DateTime);
            else
                time = dateTime.Subtract(DateTime.UtcNow);
            if (time.Days > 0)
                return time.Days + " days ago";
            else if (time.Hours > 0)
                return time.Hours + " hrs ago";
            else if (time.Minutes > 0)
                return time.Minutes + " min ago";
            else if (time.Seconds > 0)
                return time.Seconds + " sec ago";
            else
                return "";
        }
    }
}