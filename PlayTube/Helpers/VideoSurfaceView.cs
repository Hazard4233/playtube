using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace PlayTube.Helpers
{
    class VideoSurfaceView : SurfaceView, Android.Media.MediaPlayer.IOnVideoSizeChangedListener
    {
        private int mVideoWidth;
        private int mVideoHeight;

        public VideoSurfaceView(Context context) : base(context)
        {
        }

        public VideoSurfaceView(Context context, IAttributeSet attrs) : base(context, attrs)
        {

        }

        public VideoSurfaceView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {

        }

        /**
         * Set video size.
         *
         * @see MediaPlayer#getVideoWidth()
         * @see MediaPlayer#getVideoHeight()
         */
        public void setVideoSize(int videoWidth, int videoHeight)
        {
            mVideoWidth = videoWidth;
            mVideoHeight = videoHeight;
        }


        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            //Log.i("@@@@", "onMeasure(" + MeasureSpec.toString(widthMeasureSpec) + ", "
            //        + MeasureSpec.toString(heightMeasureSpec) + ")");

            int width = GetDefaultSize(mVideoWidth, widthMeasureSpec);
            int height = GetDefaultSize(mVideoHeight, heightMeasureSpec);
            if (mVideoWidth > 0 && mVideoHeight > 0)
            {

                MeasureSpecMode widthSpecMode = MeasureSpec.GetMode(widthMeasureSpec);
                int widthSpecSize = MeasureSpec.GetSize(widthMeasureSpec);
                MeasureSpecMode heightSpecMode = MeasureSpec.GetMode(heightMeasureSpec);
                int heightSpecSize = MeasureSpec.GetSize(heightMeasureSpec);

                if (widthSpecMode == MeasureSpecMode.Exactly && heightSpecMode == MeasureSpecMode.Exactly)
                {
                    // the size is fixed
                    width = widthSpecSize;
                    height = heightSpecSize;

                    // for compatibility, we adjust size based on aspect ratio
                    if (mVideoWidth * height < width * mVideoHeight)
                    {
                        //Log.i("@@@", "image too wide, correcting");
                        width = height * mVideoWidth / mVideoHeight;
                    }
                    else if (mVideoWidth * height > width * mVideoHeight)
                    {
                        //Log.i("@@@", "image too tall, correcting");
                        height = width * mVideoHeight / mVideoWidth;
                    }
                }
                else if (widthSpecMode == MeasureSpecMode.Exactly)
                {
                    // only the width is fixed, adjust the height to match aspect ratio if possible
                    width = widthSpecSize;
                    height = width * mVideoHeight / mVideoWidth;
                    if (heightSpecMode == MeasureSpecMode.AtMost && height > heightSpecSize)
                    {
                        // couldn't match aspect ratio within the constraints
                        height = heightSpecSize;
                    }
                }
                else if (heightSpecMode == MeasureSpecMode.Exactly)
                {
                    // only the height is fixed, adjust the width to match aspect ratio if possible
                    height = heightSpecSize;
                    width = height * mVideoWidth / mVideoHeight;
                    if (widthSpecMode == MeasureSpecMode.AtMost && width > widthSpecSize)
                    {
                        // couldn't match aspect ratio within the constraints
                        width = widthSpecSize;
                    }
                }
                else
                {
                    // neither the width nor the height are fixed, try to use actual video size
                    width = mVideoWidth;
                    height = mVideoHeight;
                    if (heightSpecMode == MeasureSpecMode.AtMost && height > heightSpecSize)
                    {
                        // too tall, decrease both width and height
                        height = heightSpecSize;
                        width = height * mVideoWidth / mVideoHeight;
                    }
                    if (widthSpecMode == MeasureSpecMode.AtMost && width > widthSpecSize)
                    {
                        // too wide, decrease both width and height
                        width = widthSpecSize;
                        height = width * mVideoHeight / mVideoWidth;
                    }
                }
            }
            else
            {
                // no size yet, just adopt the given spec sizes
            }
            SetMeasuredDimension(width, height);
        }


        public void OnVideoSizeChanged(Android.Media.MediaPlayer mp, int width, int height)
        {
            mVideoWidth = mp.VideoWidth;
            mVideoHeight = mp.VideoHeight;
            if (mVideoWidth != 0 && mVideoHeight != 0)
            {
                Holder.SetFixedSize(mVideoWidth, mVideoHeight);
                RequestLayout();
            }
        }
    }
}