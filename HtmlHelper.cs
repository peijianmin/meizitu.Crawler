using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace meizitu.Crawler
{
    public class HtmlHelper
    {

        /// <summary>
        /// 创建文档对象
        /// </summary>
        /// <param name="url"></param>
        public async static Task<HtmlDocument> CreateDocument(string url)
        {
            try
            {
                HttpMessageHandler handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };//开启压缩
                using (HttpClient httpClient = new HttpClient(handler))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(60);

                    var html = await httpClient.GetStreamAsync(url);
                    var doc = new HtmlDocument();
                    doc.Load(html);
                    return doc;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"地址：{url}获取文档对象失败:" + ex.Message);
                File.AppendAllLines($@"d:/meizi/log.txt", new List<string> { $"{DateTime.Now.ToString()}  地址：{url}获取文档对象失败:" + ex.Message });
                return null;
            }

        }

    }
}
