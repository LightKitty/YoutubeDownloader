using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YoutubeDownloader.Core;
using YoutubeDownloader.Web.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace YoutubeDownloader.Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(HomeModel homeModel)
        {
            DownloadQueue.Add(homeModel.Url);
            return RedirectToAction("DownloadList");
        }

        [HttpGet]
        public IActionResult GetProgress(string url)
        {
            double process = -1;
            var downloadTasks = DownloadQueue.DownloadTasks.FirstOrDefault(x => x.Url == url);
            if (downloadTasks != null)
            {
                process = downloadTasks.VideoProgress;
            }
            return Content(process.ToString());
        }

        public IActionResult DownloadList()
        {
            DownloadListModel downloadListModel = new DownloadListModel
            {
                DownloadTasks = DownloadQueue.DownloadTasks,
                //AutoRefresh = DownloadQueue.DownloadTasks.Any(x => !x.IsStop())
            };

            return View(downloadListModel);
        }
    }
}
