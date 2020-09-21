using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PlayTube.Activities.UserReponse
{
	public class PreviewCaptureStateCallback : CameraCaptureSession.StateCallback
	{
		VideoRecorderActivitiy activity;
		RecordResponseManager manager;
		public PreviewCaptureStateCallback(VideoRecorderActivitiy act, RecordResponseManager _manager)
		{
			activity = act;
			manager = _manager;
		}
		public override void OnConfigured(CameraCaptureSession session)
		{
			manager.previewSession = session;
			manager.updatePreview();

		}

		public override void OnConfigureFailed(CameraCaptureSession session)
		{
			if (null != activity)
				Toast.MakeText(activity, "Failed", ToastLength.Short).Show();
		}
	}
}