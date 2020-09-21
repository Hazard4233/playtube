using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PlayTube.Activities.UserReponse
{
	public class MySurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
	{
		RecordResponseManager recordManager;
		public MySurfaceTextureListener(RecordResponseManager _recordManager)
		{
			recordManager = _recordManager;
		}

		public void OnSurfaceTextureAvailable(SurfaceTexture surface_texture, int width, int height)
		{
			recordManager.openCamera(width, height, recordManager.LensFacing);
		}

		public void OnSurfaceTextureSizeChanged(SurfaceTexture surface_texture, int width, int height)
		{
			recordManager.configureTransform(width, height);
		}

		public bool OnSurfaceTextureDestroyed(SurfaceTexture surface_texture)
		{
			return true;
		}

		public void OnSurfaceTextureUpdated(SurfaceTexture surface_texture)
		{
		}

	}
}