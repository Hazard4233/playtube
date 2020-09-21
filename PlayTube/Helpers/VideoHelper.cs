using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Nio;
using RestSharp.Extensions;

namespace PlayTube.Helpers
{
    public class VideoHelper
    {
        public static string Trim(int startMs, int endMs, string inputPath)
        {
            // Set up MediaExtractor to read from the source.
            MediaExtractor extractor = new MediaExtractor();
            extractor.SetDataSource(inputPath);
            int trackCount = extractor.TrackCount;
            // Set up MediaMuxer for the destination.
            MediaMuxer muxer;
            string outputPath = GetOutputPath(inputPath);
            muxer = new MediaMuxer(outputPath, MuxerOutputType.Mpeg4);
            // Set up the tracks and retrieve the max buffer size for selected
            // tracks.
            Dictionary<int, int> indexDict = new Dictionary<int, int>(trackCount);
            int bufferSize = -1;
            for (int i = 0; i < trackCount; i++)
            {
                MediaFormat format = extractor.GetTrackFormat(i);
                string mime = format.GetString(MediaFormat.KeyMime);
                bool selectCurrentTrack = false;
                if (mime.StartsWith("audio/"))
                {
                    selectCurrentTrack = true;
                }
                else if (mime.StartsWith("video/"))
                {
                    selectCurrentTrack = true;
                }
                if (selectCurrentTrack)
                {
                    extractor.SelectTrack(i);
                    int dstIndex = muxer.AddTrack(format);
                    indexDict.Add(i, dstIndex);
                    if (format.ContainsKey(MediaFormat.KeyMaxInputSize))
                    {
                        int newSize = format.GetInteger(MediaFormat.KeyMaxInputSize);
                        bufferSize = newSize > bufferSize ? newSize : bufferSize;
                    }
                }
            }
            if (bufferSize < 0)
            {
                bufferSize = 1337; //TODO: I don't know what to put here tbh, it will most likely be above 0 at this point anyways :)
            }
            // Set up the orientation and starting time for extractor.
            MediaMetadataRetriever retrieverSrc = new MediaMetadataRetriever();
            retrieverSrc.SetDataSource(inputPath);
            string degreesString = retrieverSrc.ExtractMetadata(MetadataKey.VideoRotation);
            if (degreesString != null)
            {
                int degrees = int.Parse(degreesString);
                if (degrees >= 0)
                {
                    muxer.SetOrientationHint(degrees);
                }
            }
            if (startMs > 0)
            {
                extractor.SeekTo(startMs * 1000, MediaExtractorSeekTo.ClosestSync);
            }
            // Copy the samples from MediaExtractor to MediaMuxer. We will loop
            // for copying each sample and stop when we get to the end of the source
            // file or exceed the end time of the trimming.
            int offset = 0;
            int trackIndex = -1;
            ByteBuffer dstBuf = ByteBuffer.Allocate(bufferSize);
            MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
            try
            {
                muxer.Start();
                while (true)
                {
                    bufferInfo.Offset = offset;
                    bufferInfo.Size = extractor.ReadSampleData(dstBuf, offset);
                    if (bufferInfo.Size < 0)
                    {
                        bufferInfo.Size = 0;
                        break;
                    }
                    else
                    {
                        bufferInfo.PresentationTimeUs = extractor.SampleTime;
                        if (endMs > 0 && bufferInfo.PresentationTimeUs > (endMs * 1000))
                        {
                            Console.WriteLine("The current sample is over the trim end time.");
                            break;
                        }
                        else
                        {
                            bufferInfo.Flags = ConvertMediaExtractorSampleFlagsToMediaCodecBufferFlags(extractor.SampleFlags);
                            trackIndex = extractor.SampleTrackIndex;
                            muxer.WriteSampleData(indexDict[trackIndex], dstBuf, bufferInfo);
                            extractor.Advance();
                        }
                    }
                }
                muxer.Stop();

                //deleting the old file
                //JFile file = new JFile(srcPath);
                //file.Delete();
            }
            catch (IllegalStateException e)
            {
                // Swallow the exception due to malformed source.
                Console.WriteLine("The source video file is malformed");
            }
            finally
            {
                muxer.Release();
            }
            return outputPath;
        }

        //Splits the string at the dot, separating the file name and the extension. then adding the "_trimmed" string between both
        private static string GetOutputPath(string inputPath)
        {
            string[] parts = inputPath.Split('.');
            return $"{parts[0]}_trimmed.{parts[1]}";
        }

        private static MediaCodecBufferFlags ConvertMediaExtractorSampleFlagsToMediaCodecBufferFlags(MediaExtractorSampleFlags mediaExtractorSampleFlag)
        {
            switch (mediaExtractorSampleFlag)
            {
                case MediaExtractorSampleFlags.None:
                    return MediaCodecBufferFlags.None;
                case MediaExtractorSampleFlags.Encrypted:
                    return MediaCodecBufferFlags.KeyFrame;
                case MediaExtractorSampleFlags.Sync:
                    return MediaCodecBufferFlags.SyncFrame;
                default:
                    throw new NotImplementedException("ConvertMediaExtractorSampleFlagsToMediaCodecBufferFlags");
            }
        }


        public static string GetMediaRealPath(Activity activity, Android.Net.Uri uri)
        {
            ContentResolver cr = activity.ContentResolver;
            string type = cr.GetType(uri);
            if (type.Contains("video"))
            {
                string[] projection = { MediaStore.Video.VideoColumns.Data };
                var cursor = activity.ContentResolver.Query(uri, projection, null, null, null);
                if (cursor != null)
                {
                    // HERE YOU WILL GET A NULLPOINTER IF CURSOR IS NULL
                    // THIS CAN BE, IF YOU USED OI FILE MANAGER FOR PICKING THE MEDIA
                    int column_index = cursor.GetColumnIndex(MediaStore.Video.VideoColumns.Data);
                    cursor.MoveToFirst();
                    return cursor.GetString(column_index);
                }
                else
                    return null;
            }
            else if (type.Contains("audio"))
            {
                string[] projection = { MediaStore.Audio.AudioColumns.Data };
                var cursor = activity.ContentResolver.Query(uri, projection, null, null, null);
                if (cursor != null)
                {
                    // HERE YOU WILL GET A NULLPOINTER IF CURSOR IS NULL
                    // THIS CAN BE, IF YOU USED OI FILE MANAGER FOR PICKING THE MEDIA
                    int column_index = cursor.GetColumnIndex(MediaStore.Audio.AudioColumns.Data);
                    cursor.MoveToFirst();
                    return cursor.GetString(column_index);
                }
                else
                    return null;
            }
            return null;
        }
        public static byte[] GetSelectedMediaData(Activity activity, Android.Net.Uri uri)
        {
            try
            {
                string path = string.Empty;
                ContentResolver cr = activity.ContentResolver;
                string type = cr.GetType(uri);
                if (type.Contains("video"))
                {
                    MediaMetadataRetriever retriever = new MediaMetadataRetriever();
                    retriever.SetDataSource(activity, uri);
                    string duration = retriever.ExtractMetadata(MetadataKey.Duration);
                    retriever.Release();
                    if (!string.IsNullOrEmpty(duration))
                    {
                        if (Convert.ToInt32(duration) > 30000)
                        {
                            path = VideoHelper.Trim(1, 30000, VideoHelper.GetMediaRealPath(activity, uri));
                            return System.IO.File.ReadAllBytes(path);
                        }
                    }
                }
                var stream = activity.ContentResolver.OpenInputStream(uri);
                return stream.ReadAsBytes();
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
    }
}