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

namespace TaiwanGoodsTradeData
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

            HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//button[@class='btn g-recaptcha']");
            string dataSiteKey = htmlNode.GetAttributeValue("data-sitekey", "");

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
            outgoingQueryString.Add("pStartyear", "108"); // import: 3/export: 6
            outgoingQueryString.Add("pEndyer", "110"); // by year: 1/by month: 0
            outgoingQueryString.Add("pStartmonth", "1");
            outgoingQueryString.Add("pEndmonth", "3");
            outgoingQueryString.Add("pGtNote", "");
            outgoingQueryString.Add("pColSeq", "進出口別 / 日期 / 貨品別");
            outgoingQueryString.Add("pCnyList", "");
            outgoingQueryString.Add("minYear", "92");
            outgoingQueryString.Add("maxYear", "110");
            outgoingQueryString.Add("maxMonth", "3");
            outgoingQueryString.Add("minMonth", "1");
            outgoingQueryString.Add("maxYearByYear", "109");

            outgoingQueryString.Add("searchInfo.TypePort", "6"); // import: 3/export: 6
            outgoingQueryString.Add("searchInfo.TypeTime", "0"); // by year: 1/by month: 0
            outgoingQueryString.Add("searchInfo.StartYear", "108");
            outgoingQueryString.Add("searchInfo.StartMonth", "1");
            outgoingQueryString.Add("searchInfo.EndYear", "110");
            outgoingQueryString.Add("searchInfo.EndMonth", "3");
            outgoingQueryString.Add("searchInfo.goodsName", "21");
            outgoingQueryString.Add("searchInfo.goodsType", "2");
            outgoingQueryString.Add("searchInfo.goodsCodeGroup", "8486");
            outgoingQueryString.Add("searchInfo.groupType", "0");
            outgoingQueryString.Add("searchInfo.CountryName", "");
            outgoingQueryString.Add("searchInfo.Type", "1");
            outgoingQueryString.Add("searchInfo.OrderType", "進出口別 / 日期 / 貨品別");
            outgoingQueryString.Add("g-recaptcha-response", "03AGdBq268Dq5OPGwAjdKBZuBCXJG6bmJ74Zj2jHCgZR0_EyTzYO_GMUe0imC_vHDI6YT16yJkFkAGjnRFmMkpfvJmBwkheaNZ1LywZVoRGCNYHYmgIO5_Fn70iCsC3JWCOURsFw1U4JtdRpq8KVDtA_RFihV3spjp9fT10_eC1_FM6vAkiCMl8qDBKbATLrwP3gCe8gWVrLiGBSzK5YHe5viJ7GHBhBAmTQN8fTohtoVxoj6zOVWKpVxkwZ-J6dSoXu5VsPUV_bgM5lb_6WahLVDUxyX8v0gMp5YkL1eciT-HXohr3_cgZbzRrmQMkvs_2PDNsV5juRygZIeOkhVAte-YdKomHs66QVXVBbEg1GWzCG_wqL5MPVovNGSCg3YJhO1NCkXLkJUrYe1-KEC5W49Q7LBpnVEEiuBLKj9bZmOigmNAhVH8WpujUqh3kVW5GbWFeZsXB5ONUjJw_7yc9CyiNfg-c2ErJA");
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
