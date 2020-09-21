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
    public class MyCameraStateCallback : CameraDevice.StateCallback
    {
        VideoRecorderActivitiy activity;
        RecordResponseManager recordManager;
        public MyCameraStateCallback(VideoRecorderActivitiy act, RecordResponseManager _recordManager)
        {
            activity = act;
            recordManager = _recordManager;
        }
        public override void OnOpened(CameraDevice camera)
        {
            recordManager.cameraDevice = camera;
            recordManager.startPreview();
            recordManager.cameraOpenCloseLock.Release();
            if (null != activity.textureView)
                recordManager.configureTransform(activity.textureView.Width, activity.textureView.Height);
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            recordManager.cameraOpenCloseLock.Release();
            camera.Close();
            recordManager.cameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            recordManager.cameraOpenCloseLock.Release();
            camera.Close();
            recordManager.cameraDevice = null;
            if (null != activity)
                activity.Finish();
        }


    }
}