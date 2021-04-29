using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace Taiwan_Import_Export
{
    class Program
    {
        static string sessionId = "";

        static void Main(string[] args)
        {
            StartCPTCrawlerAsync().Wait();
            StartCPTPost();
            Console.ReadLine();
        }

        private static async Task StartCrawlerAsync()
        {
            var url = "https://www.automobile.tn/fr/neuf/bmw";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var cars = new List<Car>();

            var divs = htmlDocument.DocumentNode.Descendants("div").
                Where(node => node.GetAttributeValue("class", "").Equals("versions-item")).ToList();
            foreach (var div in divs)
            {
                var car = new Car
                {
                    Model = div.Descendants("h2").FirstOrDefault().InnerText,
                    Price = div.Descendants("div").FirstOrDefault().InnerText,
                    Link = div.Descendants("a").FirstOrDefault().ChildAttributes("href").FirstOrDefault().Value,
                    ImageUrl = div.Descendants("img").FirstOrDefault().ChildAttributes("src").FirstOrDefault().Value,
                };
                cars.Add(car);
            }
        }

        private static async Task StartCPTCrawlerAsync()
        {
            var url = "https://portal.sw.nat.gov.tw/APGA/GA30";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            //var divss = htmlDocument.DocumentNode.Descendants("div");
            var divs = htmlDocument.DocumentNode.Descendants("div").
                Where(node => node.GetAttributeValue("class", "").Equals("content_info")).ToList();
            string action = divs.First().Descendants("form").First().GetAttributes("action").First().Value;
            int idx = action.IndexOf("APGAJSESSIONID=");
            sessionId = action.Substring(idx + 15);
        }

        private static void StartCPTPost()
        {
            string url = "https://portal.sw.nat.gov.tw/APGA/GA30/APGA/GA30_LIST;APGAJSESSIONID=" + sessionId;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            NameValueCollection outgoingQueryString = HttpUtility.ParseQueryString(String.Empty);
            outgoingQueryString.Add("ExportTotal", "6");
            outgoingQueryString.Add("REPORT_TYPE_0", "0");
            outgoingQueryString.Add("HS_TYPE_2", "2");
            outgoingQueryString.Add("goodsCodeValue", "8486");
            outgoingQueryString.Add("COUNTRY_TYPE", "0");
            outgoingQueryString.Add("Statistics1", "1");
            outgoingQueryString.Add("START_YEAR", "108");
            outgoingQueryString.Add("START_MONTH", "1");
            outgoingQueryString.Add("END_YEAR", "110");
            outgoingQueryString.Add("END_MONTH", "3");
            string postdata = outgoingQueryString.ToString();

            byte[] byteArray = Encoding.UTF8.GetBytes(postdata);
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(byteArray, 0, byteArray.Length);
            }

            string responseStr = "";
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    responseStr = sr.ReadToEnd();
                }
            }
            Console.Write(responseStr);
        }
    }
}
