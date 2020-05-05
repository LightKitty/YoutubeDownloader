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
            Download.Start(homeModel.Url);
            return RedirectToAction("DownloadList");
        }

        [HttpGet]
        public IActionResult GetProgress(string url)
        {
            double process = -1;
            var downloadTasks = Download.DownloadTasks.FirstOrDefault(x => x.Url == url);
            if (downloadTasks != null)
            {
                process = downloadTasks.VideoProgress;
            }
            return Content(process.ToString());
        }

        public IActionResult DownloadList()
        {
            //是否定时刷新
            bool htmlRefresh = false;
            if (Download.DownloadTasks.Any(x => !x.IsStop())) htmlRefresh = true;
            ViewData["HtmlRefresh"] = htmlRefresh;

            return View(Download.DownloadTasks);
        }
    }
}
