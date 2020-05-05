using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace YoutubeDownloader.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ClearFiles();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:5000")
                .UseStartup<Startup>();

        private static void ClearFiles()
        {
            DirectoryInfo dir = new DirectoryInfo("wwwroot/Files");
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
            if (fileinfo == null) return;
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)            //判断是否文件夹
                {
                    DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                    subdir.Delete(true);          //删除子目录和文件
                }
                else
                {
                    //如果 使用了 streamreader 在删除前 必须先关闭流 ，否则无法删除 sr.close();
                    File.Delete(i.FullName);      //删除指定文件
                }
            }
        }
    }
}
