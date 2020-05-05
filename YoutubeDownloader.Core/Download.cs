using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core
{
    public static class Download
    {
        private static int taksId;
        private static List<DownloadTask> downloadTasks = new List<DownloadTask>();
        public static List<DownloadTask> DownloadTasks { get { return downloadTasks; } private set { downloadTasks = value; } }
        public static void Start(string url)
        {
            if (downloadTasks.Any(x => x.Url == url)) return; //已存在

            while (downloadTasks.Count >= 5)
            { //任务过多
                downloadTasks.First().Stop();
                downloadTasks.First().DeleteFiles();
                downloadTasks.RemoveAt(0);
            }

            DownloadTask downloadTask = new DownloadTask
            {
                Id = ++taksId,
                Url = url,
                DownloadTime = DateTime.UtcNow
            };

            downloadTasks.Add(downloadTask);
            downloadTask.Start();
        }
    }
}
