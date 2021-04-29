using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Taiwan_Import_Export
{
    class Program
    {
        static void Main(string[] args)
        {
            StartCPTCrawlerAsync().Wait();
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
        }
    }
}
