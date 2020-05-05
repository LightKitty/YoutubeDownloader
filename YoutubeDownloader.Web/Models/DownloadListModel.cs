using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeDownloader.Core;

namespace YoutubeDownloader.Web.Models
{
    public class DownloadListModel
    {
        public List<DownloadTask> DownloadTasks { get; set; }

        public bool AutoRefresh { get; set; }
    }
}
