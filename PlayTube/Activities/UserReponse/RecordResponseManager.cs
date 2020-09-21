using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using NUnit.Framework;

using Java.Lang;
using Java.Util;
using Java.Util.Concurrent;
using PlayTube.Helpers;
using Android.App;
using System;
using Android.Content;
using System.Collections.Generic;
using Manifest = Android.Manifest;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Content.PM;
using System.Linq;
using System.IO;
using System.Net.Http;
using Android.Provider;
using Android.Database;

namespace PlayTube.Activities.UserReponse
{
    public enum CameraFacing
    {
        FRONT,
        BACK
    }
    public class RecordResponseManager
    {
        public static readonly string CAMERA_FRONT = "1";
        public static readonly string CAMERA_BACK = "0";
        private string cameraId = CAMERA_BACK;
        private MyCameraStateCallback stateListener;

        public CameraFacing LensFacing { get; set; } = CameraFacing.FRONT;

        public CameraCaptureSession previewSession;

        public CaptureRequest.Builder builder;
        private CaptureRequest.Builder previewBuilder;

        public CameraDevice cameraDevice;
        private SparseIntArray ORIENTATIONS = new SparseIntArray();


        public Size videoSize;
        private Size previewSize;
        private HandlerThread backgroundThread;
        private Handler backgroundHandler;
        CameraManager manager;
        public MySurfaceTextureListener surfaceTextureListener;

        public Semaphore cameraOpenCloseLock = new Semaphore(1);

        System.Timers.Timer timer;

        private int count = 30;

        public bool IsRecorderPlaying = false;
        public bool IsRecorderPause = false;
        public bool IsRecorderStop = false;

        public Android.Media.MediaRecorder mediaRecorder;
        private VideoRecorderActivitiy activity;

        private UserResponse userResponse;

        string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/PlayTube/Video";

        public RecordResponseManager(VideoRecorderActivitiy act, UserResponse _userResponse)
        {
            activity = act;
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
            ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);
            userResponse = _userResponse;
            if (_userResponse == UserResponse.Video)
            {
                surfaceTextureListener = new MySurfaceTextureListener(this);
                stateListener = new MyCameraStateCallback(act, this);
            }
            else if (_userResponse == UserResponse.Audio)
            {
                SetUpRecoder();
            }
        }
        public void SetUpRecoder()
        {
            try
            {
                if (null == activity)
                    return;
                if (userResponse == UserResponse.Video)
                {
                    mediaRecorder = new MediaRecorder();
                    mediaRecorder.Reset();
                    mediaRecorder.SetAudioSource(AudioSource.Mic);
                    mediaRecorder.SetVideoSource(VideoSource.Surface);
                    mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
                    mediaRecorder.SetOutputFile(path + "/test.mp4");
                    mediaRecorder.SetVideoEncodingBitRate(10000000);
                    mediaRecorder.SetVideoFrameRate(30);
                    mediaRecorder.SetVideoSize(videoSize.Width, videoSize.Height);
                    mediaRecorder.SetVideoEncoder(VideoEncoder.H264);
                    mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
                    //int rotation = (int)activity.WindowManager.DefaultDisplay.Rotation;
                    //int orientation = ORIENTATIONS.Get(rotation);
                    if (LensFacing == CameraFacing.BACK)
                        mediaRecorder.SetOrientationHint(90);
                    else
                        mediaRecorder.SetOrientationHint(270);
                    mediaRecorder.SetMaxDuration(30000);
                    mediaRecorder.Prepare();
                }
                else
                {
                    mediaRecorder = new MediaRecorder();
                    mediaRecorder.Reset();
                    mediaRecorder.SetAudioSource(AudioSource.Mic);
                    mediaRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
                    mediaRecorder.SetAudioEncoder(AudioEncoder.AmrNb);
                    mediaRecorder.SetOutputFile(path + "/test.mp3");
                    mediaRecorder.SetMaxDuration(30000);
                    mediaRecorder.Prepare();
                }
            }
            catch (System.Exception ex)
            {

            }
        }
        public void StartRecoring()
        {
            try
            {
                if (mediaRecorder != null)
                {
                    activity.imgSwitchCamera.Visibility = ViewStates.Gone;
                    mediaRecorder.Start();
                    IsRecorderPlaying = true;
                    IsRecorderPause = false;
                    timer = new System.Timers.Timer();
                    timer.Elapsed -= Timer_Elapsed;
                    timer.Elapsed += Timer_Elapsed;
                    timer.Interval = 1000;
                    timer.Start();
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                count--;
                if (count >= 0)
                {
                    activity.RunOnUiThread(() =>
                    {
                        activity.txtTime.Text = "00:" + count.ToString("00");
                    });
                }
                else
                {
                    StopRecorder();
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        public void PauseRecording()
        {
            try
            {
                if (mediaRecorder == null)
                    return;
                if (IsRecorderPlaying)
                {
                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    IsRecorderPlaying = false;
                    IsRecorderPause = true;
                    mediaRecorder.Pause();
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        public void ResumeRecording()
        {
            try
            {
                if (mediaRecorder == null)
                    return;
                if (timer != null)
                {
                    timer.Start();
                }
                IsRecorderPlaying = true;
                IsRecorderPause = false;
                mediaRecorder.Resume();
            }
            catch (System.Exception ex)
            {

            }
        }

        public void SwitchCamera()
        {
            try
            {
                if (mediaRecorder != null)
                {
                    cameraDevice?.Close();
                    if (LensFacing == CameraFacing.BACK)
                        openCamera(activity.textureView.Width, activity.textureView.Height, CameraFacing.FRONT);
                    else
                        openCamera(activity.textureView.Width, activity.textureView.Height, CameraFacing.BACK);
                }
            }
            catch (System.Exception ex)
            {

            }
        }
        public void StopRecorder()
        {
            try
            {
                IsRecorderStop = true;
                IsRecorderPlaying = false;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                }
                if (mediaRecorder != null)
                {
                    mediaRecorder.Stop();
                    mediaRecorder.Release();
                    mediaRecorder.Dispose();
                    mediaRecorder = null;
                }
            }
            catch (System.Exception ex)
            {

            }
        }
        public static void GetCameraAndMicCompatPermission(Activity activity, string[] DevicePermissions)
        {
            foreach (var permission in DevicePermissions)
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(activity, permission))
                    ActivityCompat.RequestPermissions(activity, DevicePermissions, 1);
            }
        }
        #region Video Player Size
        public void StartBackgroundThread()
        {
            backgroundThread = new HandlerThread("CameraBackground");
            backgroundThread.Start();
            backgroundHandler = new Handler(backgroundThread.Looper);
        }
        public void openCamera(int width, int height, CameraFacing cameraFacing = CameraFacing.FRONT)
        {
            if (null == this || activity.IsFinishing)
                return;

            manager = (CameraManager)activity.GetSystemService(Context.CameraService);
            try
            {
                if (!cameraOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
                    throw new RuntimeException("Time out waiting to lock camera opening.");
                string cameraId = null;
                CameraCharacteristics characteristics = null;
                var cameraList = manager.GetCameraIdList();
                if (cameraList != null && cameraList.Length >= 0)
                {
                    if (cameraList.Length > 1)
                    {
                        if (cameraFacing == CameraFacing.BACK)
                        {
                            LensFacing = CameraFacing.BACK;
                            cameraId = cameraList[0];
                            characteristics = manager.GetCameraCharacteristics(cameraId);
                        }
                        else if (cameraFacing == CameraFacing.FRONT)
                        {
                            LensFacing = CameraFacing.FRONT;
                            cameraId = cameraList[1];
                            characteristics = manager.GetCameraCharacteristics(cameraId);
                        }
                    }
                    else if (cameraList.Length == 1)
                    {
                        cameraFacing = CameraFacing.BACK;
                        activity.imgSwitchCamera.Visibility = ViewStates.Gone;
                        cameraId = cameraList[0];
                        characteristics = manager.GetCameraCharacteristics(cameraId);
                    }
                }
                else
                {
                    activity.imgSwitchCamera.Visibility = ViewStates.Gone;
                    Toast.MakeText(activity, "Camera not avaiable", ToastLength.Short);
                    activity.Finish();
                    return;
                }
                configureTransform(width, height);
                if (!string.IsNullOrEmpty(cameraId))
                    manager.OpenCamera(cameraId, stateListener, null);
                SetUpCameraOutputs(cameraId);
            }
            catch (CameraAccessException ex)
            {
                Toast.MakeText(activity, "Cannot access the camera.", ToastLength.Short).Show();
                activity.Finish();
            }
            catch (NullPointerException)
            {
            }
            catch (InterruptedException)
            {
                throw new RuntimeException("Interrupted while trying to lock camera opening.");
            }
        }

        private void SetUpCameraOutputs(string selectedCameraId)
        {
            CameraCharacteristics characteristics = manager.GetCameraCharacteristics(cameraId);
            var map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
            if (map == null)
            {
                return;
            }
            // For still image captures, we use the largest available size.
            Size largest = (Size)Collections.Max(Arrays.AsList(map.GetOutputSizes((int)ImageFormatType.Jpeg)),
                new CompareSizesByArea());
            // coordinate.
            var displayRotation = activity.WindowManager.DefaultDisplay.Rotation;
            var sensorOrientation = (int)characteristics.Get(CameraCharacteristics.SensorOrientation);
            bool swappedDimensions = false;
            switch (displayRotation)
            {
                case SurfaceOrientation.Rotation0:
                case SurfaceOrientation.Rotation180:
                    if (sensorOrientation == 90 || sensorOrientation == 270)
                    {
                        swappedDimensions = true;
                    }
                    break;
                case SurfaceOrientation.Rotation90:
                case SurfaceOrientation.Rotation270:
                    if (sensorOrientation == 0 || sensorOrientation == 180)
                    {
                        swappedDimensions = true;
                    }
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"Display rotation is invalid: {displayRotation}");
                    break;
            }

            Point displaySize = new Point();
            activity.WindowManager.DefaultDisplay.GetSize(displaySize);
            var rotatedPreviewWidth = activity.textureView.Width;
            var rotatedPreviewHeight = activity.textureView.Height;
            var maxPreviewWidth = displaySize.X;
            var maxPreviewHeight = displaySize.Y;

            if (swappedDimensions)
            {
                rotatedPreviewWidth = activity.textureView.Height;
                rotatedPreviewHeight = activity.textureView.Width;
                maxPreviewWidth = displaySize.Y;
                maxPreviewHeight = displaySize.X;
            }

            // Danger, W.R.! Attempting to use too large a preview size could  exceed the camera
            // bus' bandwidth limitation, resulting in gorgeous previews but the storage of
            // garbage capture data.
            videoSize = ChooseVideoSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))));
            previewSize = ChooseOptimalSize(map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture))),
                rotatedPreviewWidth, rotatedPreviewHeight, maxPreviewWidth,
                maxPreviewHeight, largest);
            activity.textureView.SetAspectRatio(previewSize.Height, previewSize.Width);
            return;
        }

        public byte[] GetVideoData(string localPath = null)
        {
            System.Byte[] bytes = null;
            if (string.IsNullOrEmpty(localPath))
            {
                string fileName;
                if (userResponse == UserResponse.Video)
                    fileName = "test.mp4";
                else
                    fileName = "test.mp3";
                bytes = File.ReadAllBytes(System.IO.Path.Combine(path, fileName));
            }
            else
            {
                bytes = File.ReadAllBytes(localPath);
            }
            return bytes;
        }

        public string GetFilePath()
        {
            Context context = Android.App.Application.Context;
            var filePath = context.GetExternalFilesDir(null);
            return filePath.Path;// + "/PlayTube/Video";
        }

        public void configureTransform(int viewWidth, int viewHeight)
        {
            if (null == this || null == previewSize || null == activity.textureView)
                return;

            int rotation = (int)activity.WindowManager.DefaultDisplay.Rotation;
            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, previewSize.Height, previewSize.Width);
            float centerX = viewRect.CenterX();
            float centerY = viewRect.CenterY();
            if ((int)SurfaceOrientation.Rotation90 == rotation || (int)SurfaceOrientation.Rotation270 == rotation)
            {
                bufferRect.Offset((centerX - bufferRect.CenterX()), (centerY - bufferRect.CenterY()));
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
                float scale = System.Math.Max(
                    (float)viewHeight / previewSize.Height,
                    (float)viewHeight / previewSize.Width);
                matrix.PostScale(scale, scale, centerX, centerY);
                matrix.PostRotate(90 * (rotation - 2), centerX, centerY);
            }
            else if ((int)SurfaceOrientation.Rotation180 == rotation)
            {
                matrix.PostRotate(180, centerX, centerY);
            }
            activity.textureView.SetTransform(matrix);
        }


        private Size ChooseVideoSize(Size[] choices)
        {
            foreach (Size size in choices)
            {
                //Java.Lang.Double aspectRatio = (Java.Lang.Double.ValueOf(activity.WindowManager
                //       .DefaultDisplay.Height / activity.WindowManager
                //       .DefaultDisplay.Width));
                //for (int i = choices.Count() - 1; i > 0; i--)
                //{
                //    Java.Lang.Double tmpRatio = Java.Lang.Double.ValueOf(choices[i].Height
                //            / choices[i].Width);
                //    var d = (double)aspectRatio - (double)tmpRatio;
                //    if (Java.Lang.Math.Abs(d) < 0.5)
                //    {
                //        return choices[i];
                //    }
                //}

                if (size.Width == size.Height * 4 / 3 && size.Width <= 1080)
                    return size;
            }
            return choices[choices.Length - 1];
        }

        private static Size ChooseOptimalSize(Size[] choices, int textureViewWidth,
            int textureViewHeight, int maxWidth, int maxHeight, Size aspectRatio)
        {
            // Collect the supported resolutions that are at least as big as the preview Surface
            var bigEnough = new List<Size>();
            // Collect the supported resolutions that are smaller than the preview Surface
            var notBigEnough = new List<Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;

            for (var i = 0; i < choices.Length; i++)
            {
                Size option = choices[i];
                if (option.Height == option.Width * h / w)
                {
                    if (option.Width >= textureViewWidth &&
                        option.Height >= textureViewHeight)
                    {
                        bigEnough.Add(option);
                    }
                    else if ((option.Width <= maxWidth) && (option.Height <= maxHeight))
                    {
                        notBigEnough.Add(option);
                    }
                }
            }

            // Pick the smallest of those big enough. If there is no one big enough, pick the
            // largest of those not big enough.
            if (bigEnough.Count > 0)
            {
                return (Size)Collections.Max(bigEnough, new CompareSizesByArea());
            }
            else if (notBigEnough.Count > 0)
            {
                return (Size)Collections.Max(notBigEnough, new CompareSizesByArea());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Couldn't find any suitable preview size");
                return choices[0];
            }
        }
        private class CompareSizesByArea : Java.Lang.Object, Java.Util.IComparator
        {
            public int Compare(Java.Lang.Object lhs, Java.Lang.Object rhs)
            {
                // We cast here to ensure the multiplications won't overflow
                if (lhs is Size && rhs is Size)
                {
                    var right = (Size)rhs;
                    var left = (Size)lhs;
                    return Long.Signum((long)left.Width * left.Height -
                        (long)right.Width * right.Height);
                }
                else
                    return 0;

            }
        }

        public void startPreview()
        {
            if (null == cameraDevice || !activity.textureView.IsAvailable || null == previewSize)
                return;

            try
            {
                SetUpRecoder();
                SurfaceTexture texture = activity.textureView.SurfaceTexture;
                texture.SetDefaultBufferSize(previewSize.Width, previewSize.Height);
                previewBuilder = cameraDevice.CreateCaptureRequest(CameraTemplate.Record);
                var surfaces = new List<Surface>();
                var previewSurface = new Surface(texture);
                surfaces.Add(previewSurface);
                previewBuilder.AddTarget(previewSurface);

                var recorderSurface = mediaRecorder.Surface;
                surfaces.Add(recorderSurface);
                previewBuilder.AddTarget(recorderSurface);

                cameraDevice.CreateCaptureSession(surfaces, new PreviewCaptureStateCallback(activity, this), backgroundHandler);

            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
            catch (Java.IO.IOException e)
            {
                e.PrintStackTrace();
            }
        }
        public void updatePreview()
        {
            if (null == cameraDevice)
                return;

            try
            {
                setUpCaptureRequestBuilder(previewBuilder);
                HandlerThread thread = new HandlerThread("CameraPreview");
                thread.Start();
                previewSession.SetRepeatingRequest(previewBuilder.Build(), null, backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }
        private void setUpCaptureRequestBuilder(CaptureRequest.Builder builder)
        {
            builder.Set(CaptureRequest.ControlMode, new Java.Lang.Integer((int)ControlMode.Auto));
        }
        #endregion
    }
}