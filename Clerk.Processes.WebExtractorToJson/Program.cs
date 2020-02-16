using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using Clerk.Processes.WebExtractorToJson.Model.GSMArena;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Clerk.Processes.WebExtractorToJson
{
    class Program
    {
        private static string brandName = string.Empty;
        private static string phoneName = string.Empty;

        public static void Main(string[] args)
        {
            // like this www.gsmarena.com/oneplus-phones-f-95
            ExtractPhones("https://www.gsmarena.com/oneplus-phones-f-95");
            //ExtractPhoneInfo("https://www.gsmarena.com/oneplus_7t-9816.php", 1);
            Console.ReadKey();
        }

        public static void ExtractAllPhones()
        {
            var brandCounter = 1;
            var brandsUrl = $"https://www.gsmarena.com/makers.php3";
            var web = new HtmlWeb();
            var doc = web.Load(Uri.EscapeUriString(brandsUrl));
            var table = doc.DocumentNode?.SelectNodes("//table");
            var phonesListNode = doc.DocumentNode?.SelectNodes("//td").Descendants("a")
                .Select(n => n.Attributes["href"].Value).ToList();

            foreach (var phonesNode in phonesListNode)
            {
                try
                {
                    var phoneKeyWords = phonesNode.Split('-');

                    Console.WriteLine($"-- {brandCounter} -- {phoneKeyWords[0].ToUpper()} ------");
                    brandCounter++;

                    var urlBuilder = $"https://www.gsmarena.com/{phoneKeyWords[0]}-{phoneKeyWords[1]}-f-{phoneKeyWords[2].Split('.')[0]}";
                    ExtractPhones(urlBuilder);
                }
                catch (Exception e)
                {
                    SaveExToFile(e);
                }
            }
        }

        public static void ExtractPhones(string urlLink)
        {
            var pageNo = 1;
            var numberPhone = 0;
            while (true)
            {
                var phonesUrl = $"{urlLink}-0-p{pageNo++}.php";
                var web = new HtmlWeb();
                var doc = web.Load(Uri.EscapeUriString(phonesUrl));
                brandName = doc.DocumentNode?.SelectNodes("//h1[@class='article-info-name']")
                    .Select(n => n.InnerHtml).ToList().First().Split(' ')[0];
                var phonesListNode = doc.DocumentNode?.SelectNodes("//div[@class='makers']/ul").Descendants("li")
                    .Select(n => n.LastChild.Attributes["href"].Value).ToList();

                if (phonesListNode.Count.Equals(0)) break;

                if (!Directory.Exists($"C:\\Licenta\\extracted\\{brandName}"))
                {
                    Directory.CreateDirectory($"C:\\Licenta\\extracted\\{brandName}");
                }

                foreach (var phoneNode in phonesListNode)
                {
                    var phoneUrl = $"https://www.gsmarena.com/{phoneNode}";
                    numberPhone++;
                    ExtractPhoneInfo(phoneUrl, numberPhone);
                }
            }
        }

        public static void ExtractPhoneInfo(string url, int numberPhone)
        {
            var web = new HtmlWeb();
            var doc = web.Load(Uri.EscapeUriString(url));
            phoneName = doc.DocumentNode?.SelectNodes("//h1[@data-spec='modelname']")
                .Select(n => n.InnerHtml).ToList().First().Split('/')[0];
            var filePath = $"C:\\Licenta\\extracted\\{brandName}\\{phoneName}.json";

            var phoneStatus = doc.DocumentNode?.SelectNodes("//td[@data-spec='status']")
                .Select(n => n.InnerHtml).ToList().First().Split(' ')[0];

            if (phoneStatus.Contains("Discontinued"))
            {
                return;
            }

            var bodyNode = doc.DocumentNode?.SelectNodes("//table[@cellspacing]");
            dynamic expando = new ExpandoObject();

            var title = "Name";
            var left = new List<string>() { "Main" };
            var right2 = new List<HtmlNodeCollection>()
                             {
                                 doc.DocumentNode?.SelectNodes("//h1[@data-spec='modelname']")
                             };

            if (doc.DocumentNode?.SelectNodes("//p[@data-spec='comment']") != null)
            {
                left.Add("Others");
                var othersName = doc.DocumentNode?.SelectNodes("//p[@data-spec='comment']")
                    .Select(m => m.LastChild.ParentNode.ChildNodes).ToList();
                right2.AddRange(othersName);
            }

            AddProperty(expando, left, right2, title);

            foreach (var node in bodyNode)
            {
                title = node.Descendants(0).Where(n => n.Name.Equals("th")).Select(m => m.LastChild.InnerHtml)
                    .First();
                left = node.Descendants(0)
                    .Where(n => (n.HasClass("ttl") && n.HasChildNodes.Equals(true)))
                    .Select(m => m.FirstChild.InnerHtml)
                    .ToList();
                var right = node.Descendants(0)
                    .Where(n => (n.HasClass("nfo") && n.HasChildNodes.Equals(true)))
                    .Select(m => m.LastChild.ParentNode.ChildNodes)
                    .ToList();

                AddProperty(expando, left, right, title);
            }

            var test = ConvertToMongo(expando);

            using (var file = File.CreateText(filePath))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, test);
            }

            Console.WriteLine($"{numberPhone}. {phoneName}");
        }

        public static ExpandoObject ConvertToMongo(ExpandoObject expando)
        {
            var outputJson = JsonConvert.SerializeObject(expando);
            var gsmArena = JsonConvert.DeserializeObject<GsmArenaModel>(outputJson);

            return expando;
        }

        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public static void AddProperty(ExpandoObject expando, List<string> propertysName, List<HtmlNodeCollection> propertysValue, string parentName = "")
        {
            var expandoDict = new ExpandoObject() as IDictionary<string, object>;
            var expandoFull = expando as IDictionary<string, object>;

            for (int indexProperty = 0; indexProperty < propertysName.Count; indexProperty++)
            {
                if (parentName.Equals("Tests") && propertysName[indexProperty].Equals("Camera"))
                {
                    continue;
                }
                if (propertysValue.Count > indexProperty)
                {
                    foreach (var value in propertysValue[indexProperty])
                    {
                        var removeTinyWeirdSpace = value.InnerHtml.Replace("&thinsp;", String.Empty);
                        var valueInnerHtmlDecoded = WebUtility.HtmlDecode(removeTinyWeirdSpace);
                        if (valueInnerHtmlDecoded.Replace("\r\n", string.Empty).Length > 0)
                        {
                            if (expandoDict.ContainsKey(propertysName[indexProperty]) || (propertysName[indexProperty].Equals("&nbsp;") && expandoDict.ContainsKey("Orphans")))
                            {
                                if (propertysValue.Count > indexProperty)
                                {
                                    if (value.Name.Contains("text") && valueInnerHtmlDecoded.Length > 1)
                                    {
                                        PopulateList(
                                            propertysName[indexProperty].Equals("&nbsp;") ?
                                                (List<string>)expandoDict["Orphans"] : (List<string>)expandoDict[propertysName[indexProperty]],
                                            valueInnerHtmlDecoded);
                                    }
                                    if (value.Name.Contains("a"))
                                    {
                                        foreach (var v in value.ChildNodes)
                                        {
                                            PopulateList(
                                                propertysName[indexProperty].Equals("&nbsp;") ?
                                                    (List<string>)expandoDict["Orphans"] : (List<string>)expandoDict[propertysName[indexProperty]],
                                                WebUtility.HtmlDecode(value.InnerHtml));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (value.Name.Contains("text") && (valueInnerHtmlDecoded.Length > 0))
                                {
                                    expandoDict.Add(
                                        propertysName[indexProperty].Equals("&nbsp;") ? "Orphans" : propertysName[indexProperty],
                                        new List<string> { valueInnerHtmlDecoded.Replace("\r\n", string.Empty) });
                                }
                                if (value.Name.Contains("h1") && (valueInnerHtmlDecoded.Length > 0))
                                {
                                    expandoDict.Add(
                                        propertysName[indexProperty].Equals("&nbsp;") ? "Orphans" : propertysName[indexProperty],
                                        new List<string> { valueInnerHtmlDecoded.Replace("\r\n", string.Empty) });
                                }
                                if (value.Name.Contains("a") && (valueInnerHtmlDecoded.Length > 0))
                                {
                                    if (value.HasChildNodes)
                                    {
                                        foreach (var v in value.ChildNodes)
                                        {
                                            if (expandoDict.ContainsKey(propertysName[indexProperty]) || (propertysName[indexProperty].Equals("&nbsp;") && expandoDict.ContainsKey("Orphans")))
                                            {
                                                if (propertysValue.Count > indexProperty)
                                                {
                                                    if (v.Name.Contains("text") && WebUtility.HtmlDecode(value.InnerHtml).Length > 1)
                                                    {
                                                        PopulateList(
                                                            propertysName[indexProperty].Equals("&nbsp;") ?
                                                                (List<string>)expandoDict["Orphans"] : (List<string>)expandoDict[propertysName[indexProperty]],
                                                            WebUtility.HtmlDecode(value.InnerHtml));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                expandoDict.Add(propertysName[indexProperty].Equals("&nbsp;") ? "Orphans" : propertysName[indexProperty],
                                                    new List<string>
                                                        {
                                                        WebUtility.HtmlDecode(value.InnerHtml).Replace("\r\n", string.Empty)
                                                        });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        expandoDict.Add(propertysName[indexProperty].Equals("&nbsp;") ? "Orphans" : propertysName[indexProperty],
                                            new List<string>
                                                {
                                                valueInnerHtmlDecoded.Replace("\r\n", string.Empty)
                                                });
                                    }

                                }
                            }
                        }
                    }
                }
            }

            if (parentName.Length > 0)
            {
                expandoFull.Add(parentName, expandoDict);
            }
            else
            {
                expandoFull = expandoDict;
            }
        }

        private static bool PopulateList(List<string> list, string itemPropertie)
        {
            var baseObj = itemPropertie.Replace("\r\n", string.Empty);
            list.Add(baseObj);
            return true;
        }

        private static void SaveExToFile(Exception ex)
        {
            string filePath = @"C:\Licenta\extracted\log.txt";

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine("Date : " + DateTime.Now);
                writer.WriteLine();

                while (ex != null)
                {
                    writer.WriteLine(ex.GetType().FullName);
                    writer.WriteLine("Message : " + ex.Message);
                    writer.WriteLine("StackTrace : " + ex.StackTrace);

                    ex = ex.InnerException;
                }
            }
        }
    }
}
