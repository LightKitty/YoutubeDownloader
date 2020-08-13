using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core
{
    public class DownloadTask
    {
        #region public property

        public string Id { get; set; }

        public string Url { get; set; }

        public string Path { get; set; }

        public string VideoPath { get; set; }

        public string AudioPath { get; set; }

        public string Title { get; set; }

        public double Progress { get; set; }

        public double VideoProgress { get; set; }

        public double AudioProgress { get; set; }

        public DateTime DownloadTime { get; set; }

        public string VideoQualityLable { get; set; }

        public string AudioBitrateLable { get; set; }

        public string VideoSizeLable { get; set; }

        public string AudioSizeLable { get; set; }

        public bool IsError { get; set; }

        public string ErrorMessage { get; set; }

        private static readonly char[] illegalChars = new string("[ \\[ \\] \\^ \\-_*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]").ToArray();

        #endregion

        #region private property

        private static YoutubeClient youtubeClient = new YoutubeClient();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        #endregion

        /// <summary>
        /// 开始下载
        /// </summary>
        internal async void StartAsync()
        {
            try
            {
                var video = await youtubeClient.Videos.GetAsync(Url);
                Title = video.Title;
                Id = video.Id;

                var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(Url);

                var streamInfoVideo = streamManifest
                    //.GetVideoOnly()
                    .GetVideo()
                    .Where(s => (s.VideoQuality == VideoQuality.High1080 || s.VideoQuality == VideoQuality.High720) && s.Container == Container.Mp4)
                    .WithHighestVideoQuality();

                if (streamInfoVideo != null) DownloadVideoAsync(streamInfoVideo);
                else
                {
                    IsError = true;
                    ErrorMessage = "No suitable video was found";
                    return;
                }

                var streamInfoAudio = streamManifest
                    .GetAudioOnly()
                    .Where(s => s.Container == Container.Mp4)
                    .WithHighestBitrate();

                if (streamInfoAudio != null) DownloadAudioAsync(streamInfoAudio);
                else
                {
                    IsError = true;
                    ErrorMessage = "No suitable audio was found";
                    return;
                }
            }
            catch(Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// 停止下载
        /// </summary>
        internal void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// 下载视频
        /// </summary>
        /// <param name="streamInfoVideo"></param>
        async void DownloadVideoAsync(IVideoStreamInfo streamInfoVideo)
        {
            try
            {
                VideoQualityLable = streamInfoVideo.VideoQualityLabel;
                // Get the actual stream
                var stream = await youtubeClient.Videos.Streams.GetAsync(streamInfoVideo);

                VideoSizeLable = (stream.Length / 1024.0 / 1024.0).ToString("f2") + "MB";

                string path = $"files/{MakeValidFileName(Title)}_{Id}_video.{streamInfoVideo.Container}";

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += new EventHandler<double>(VideoProgressEvent);

                // Download the stream to file
                await youtubeClient.Videos.Streams.DownloadAsync(streamInfoVideo, "wwwroot/" + path, progress, cancellationTokenSource.Token);
                VideoPath = path;

                stream.Close();
                stream.Dispose();
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// 下载音频
        /// </summary>
        /// <param name="streamInfoAudio"></param>
        async void DownloadAudioAsync(IStreamInfo streamInfoAudio)
        {
            try
            {
                AudioBitrateLable = streamInfoAudio.Bitrate.ToString().Replace(" ","");
                // Get the actual stream
                var stream = await youtubeClient.Videos.Streams.GetAsync(streamInfoAudio);

                AudioSizeLable = (stream.Length / 1024.0 / 1024.0).ToString("f2") + "MB";

                string path = $"files/{MakeValidFileName(Title)}_{Id}_audio.{streamInfoAudio.Container}";

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += new EventHandler<double>(AudioProgressEvent);

                // Download the stream to file
                await youtubeClient.Videos.Streams.DownloadAsync(streamInfoAudio, "wwwroot/" + path, progress, cancellationTokenSource.Token);
                AudioPath = path;

                stream.Close();
                stream.Dispose();
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// 视频进度事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        void VideoProgressEvent(object sender, double value)
        {
            VideoProgress = value;
        }

        /// <summary>
        /// 音频进度事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="value"></param>
        void AudioProgressEvent(object sender, double value)
        {
            AudioProgress = value;
        }

        /// <summary>
        /// 删除下载文件
        /// </summary>
        internal void DeleteFiles()
        {
            string vPath = $"wwwroot/" + VideoPath;
            if (File.Exists(vPath)) File.Delete(vPath);
            string aPath = $"wwwroot/" + AudioPath;
            if (File.Exists(aPath)) File.Delete(aPath);
        }

        /// <summary>
        /// 是否下载完成
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            return (Progress >= 1.0 || (VideoProgress >= 1.0 && AudioProgress >= 1.0));
        }

        /// <summary>
        /// 是否停止
        /// </summary>
        /// <returns></returns>
        public bool IsStop()
        {
            return IsComplete() || IsError;
        }

        /// <summary>
        /// 去除无效字符
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string MakeValidFileName(string text, char replacement = '_')
        {
            StringBuilder str = new StringBuilder(text.Length);
            //var invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (var c in text)
            {
                if ((illegalChars.Contains(c)))
                { //非法字符替换
                    if(str[str.Length - 1] != replacement)
                    {
                        str.Append(replacement);
                    }
                }
                else
                {
                    str.Append(c);
                }
            }

            return str.ToString();
        }
    }
}
