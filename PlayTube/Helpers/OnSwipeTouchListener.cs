using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Java.Interop;
using static Android.Views.GestureDetector;
using static Android.Views.View;

namespace PlayTube.Helpers
{
    public enum SwipeEnum
    {
        Left,
        Right,
        Top,
        Bottom,
        SingleTab
    }
    public class OnSwipeTouchListener : Java.Lang.Object, IOnTouchListener
    {
        private GestureDetectorCompat gestureDetector;

        public delegate void SwipeGesture(SwipeEnum swipe);
        public event SwipeGesture SwipeEvent;

        public OnSwipeTouchListener(Context ctx)
        {
            //GestureListener gestureListener = new GestureListener();
            GestureDetectorListener gestureListener = new GestureDetectorListener();
            gestureListener.SwipeEvent -= GestureListener_SwipeEvent;
            gestureListener.SwipeEvent += GestureListener_SwipeEvent;
            gestureDetector = new GestureDetectorCompat(ctx, gestureListener);
        }

        private void GestureListener_SwipeEvent(SwipeEnum swipe)
        {
            if (SwipeEvent != null)
                SwipeEvent(swipe);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return gestureDetector.OnTouchEvent(e);
        }
    }

    public class GestureDetectorListener : Java.Lang.Object, GestureDetector.IOnGestureListener
    {
        private static int SWIPE_THRESHOLD = 100;
        private static int SWIPE_VELOCITY_THRESHOLD = 100;

        public delegate void SwipeGesture(SwipeEnum swipe);
        public event SwipeGesture SwipeEvent;

        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            bool result = false;
            try
            {
                float diffY = e2.GetY() - e1.GetY();
                float diffX = e2.GetX() - e1.GetX();
                if (Math.Abs(diffX) > Math.Abs(diffY))
                {
                    if (Math.Abs(diffX) > SWIPE_THRESHOLD && Math.Abs(velocityX) > SWIPE_VELOCITY_THRESHOLD)
                    {
                        if (diffX > 0)
                        {
                            OnSwipeRight();
                        }
                        else
                        {
                            OnSwipeLeft();
                        }
                        result = true;
                    }
                }
                else if (Math.Abs(diffY) > SWIPE_THRESHOLD && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
                {
                    if (diffY > 0)
                    {
                        OnSwipeBottom();
                    }
                    else
                    {
                        OnSwipeTop();
                    }
                    result = true;
                }
            }
            catch (Exception exception)
            {

            }
            return result;
        }
        public void OnSwipeRight()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Right);
        }

        public void OnSwipeLeft()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Left);
        }

        public void OnSwipeTop()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Top);
        }

        public void OnSwipeBottom()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Bottom);
        }

        public void OnLongPress(MotionEvent e)
        {
            //throw new NotImplementedException();
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.SingleTab);
            return true;
            //throw new NotImplementedException();
        }
    }

    public class GestureListener : SimpleOnGestureListener
    {
        private static int SWIPE_THRESHOLD = 100;
        private static int SWIPE_VELOCITY_THRESHOLD = 100;

        public delegate void SwipeGesture(SwipeEnum swipe);
        public event SwipeGesture SwipeEvent;

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            bool result = false;
            try
            {
                float diffY = e2.GetY() - e1.GetY();
                float diffX = e2.GetX() - e1.GetX();
                if (Math.Abs(diffX) > Math.Abs(diffY))
                {
                    if (Math.Abs(diffX) > SWIPE_THRESHOLD && Math.Abs(velocityX) > SWIPE_VELOCITY_THRESHOLD)
                    {
                        if (diffX > 0)
                        {
                            OnSwipeRight();
                        }
                        else
                        {
                            OnSwipeLeft();
                        }
                        result = true;
                    }
                }
                else if (Math.Abs(diffY) > SWIPE_THRESHOLD && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
                {
                    if (diffY > 0)
                    {
                        OnSwipeBottom();
                    }
                    else
                    {
                        OnSwipeTop();
                    }
                    result = true;
                }
            }
            catch (Exception exception)
            {

            }
            return result;
        }
        public void OnSwipeRight()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Right);
        }

        public void OnSwipeLeft()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Left);
        }

        public void OnSwipeTop()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Top);
        }

        public void OnSwipeBottom()
        {
            if (SwipeEvent != null)
                SwipeEvent(SwipeEnum.Bottom);
        }
    }
}