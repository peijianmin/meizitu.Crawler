using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace meizitu.Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //目标网站：https://www.tuiimg.com/meinv/
            /*
             1.先爬取所有封面地址：获取总页数以及每个页面上的封面地址链接
             2.获取封面中图片页数
             3.下载封面中的所有图片
             */
            //每页的外层的图片地址

            //控制线程的个数
          // var r = ThreadPool.SetMaxThreads(5, 4);

            var doc =  HtmlHelper.CreateDocument("https://www.tuiimg.com/meinv/").Result;
            //爬取目录总页数    
            int pageCount = int.Parse(doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div/a[8]").InnerText);
            for (int i = 0; i < pageCount; i++)
            {
                var firstImageLists =  GetAllFirstImagLink(i + 1).Result;

                //foreach (var item in firstImageLists)
                //{
                //    string url = item.Split(",")[0];
                //    string title = item.Split(",")[1];
                  
                //    DownAllImage(url, $"{title}").Wait();
                //}
                //并发执行
                Parallel.ForEach(firstImageLists,(a)=> {

                    string url = a.Split(",")[0];
                    string title = a.Split(",")[1];

                    DownAllImage(url, $"{title}").Wait();
                });


            }
            Console.WriteLine("恭喜您所有妹子下载完成!!!");
            Console.Read();
        }

        /// <summary>
        /// 获取页面上所有封面链接
        /// </summary>
        /// <param name="page"></param>
        public async static Task<List<string>> GetAllFirstImagLink(int page)
        {
            List<string> firstUrls = new List<string>();
            //每个页面上所有的目录链接，标题
            var doc = await HtmlHelper.CreateDocument($"https://www.tuiimg.com/meinv/list_{page}.html");
            if (doc == null) return new List<string>();
            var ul = doc.DocumentNode.SelectNodes("/html/body/div[3]/ul")[0];
            foreach (var node in ul.SelectNodes("li"))
            {
                string href = node.SelectSingleNode("a").GetAttributeValue("href", "");
                string title = node.SelectSingleNode("a/img").GetAttributeValue("alt", "");
                Console.WriteLine(href);
                Console.WriteLine(title);
                firstUrls.Add($"{href},{title}");
            }
            return firstUrls;
        }


        /// <summary>
        /// 根据链接下载外层链接下的所有图片 默认存放 d:/meizi/
        /// </summary>
        /// <param name="url"></param>s
        /// <param name="title"></param>
        public async static Task DownAllImage(string url, string title)
        {

            //下载每一个链接中的所有图片
            //foreach (var item in firstUrls)
            //{
            //    string url = item.Split(",")[0];
            //    string title = item.Split(",")[1];
            //创建目录
            var doc = await HtmlHelper.CreateDocument(url);
            if (doc == null) { return; }
            var iamgeCount = int.Parse(doc.DocumentNode.SelectSingleNode("/html/body/div[2]/div[2]/span[2]/i").InnerText.Replace("展开全图(1/", "").Replace(")", ""));


            //doc = HtmlHelper.CreateDocument($"{url}/{i + 1}");
            //if (doc == null) { return; }
            string iamgeUrl = doc.DocumentNode.SelectSingleNode($"/html/body/div[2]/div[1]/img").GetAttributeValue("src", "");
            iamgeUrl = iamgeUrl.Substring(0, iamgeUrl.LastIndexOf("/")); ;
            for (int i = 0; i < iamgeCount; i++)
            {
                int j = i + 1;
                //下载图片
                //Task.Run(() =>
                //{
                string _iamgeUrl = $"{iamgeUrl}/{j}.jpg";
                await DownLoad(_iamgeUrl, title, j);
                //});
            }
        }

        /// <summary>s
        /// 图片下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="title"></param>
        /// <param name="num"></param>
        /// <param name="iamgeUrl"></param>
        public async static Task DownLoad(string url, string title, int num)
        {
            try
            {
                Console.WriteLine($"成功下载:{title}{num }:{url}");
                if (!Directory.Exists(@$"d:\meizi\{title}")) //如果不存在就创建 dir 文件夹  
                    Directory.CreateDirectory(@$"d:\meizi\{title}");
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.122 Safari/537.36");
                    webClient.Headers.Add("Upgrade-Insecure-Requests", "1");
                    // webClient.Headers.Add("cookie", "Hm_lvt_cb7f29be3c304cd3bb0c65a4faa96c30=1594394450,1594402822; Hm_lpvt_cb7f29be3c304cd3bb0c65a4faa96c30=1594403065; views=43");
                    // webClient.Headers.Add("Referer", "https://www.tuiimg.com"); //防盗链用


                    await webClient.DownloadFileTaskAsync(new Uri(url), $@"d:/meizi/{title}/{num}.jpg");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"图片:{url}下载失败:" + ex.Message);
            }
        }

    }
}
