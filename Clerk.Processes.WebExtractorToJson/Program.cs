using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Bot.Storage.Elasticsearch;
using Clerk.Processes.WebExtractorToJson.Model.GSMArena;
using Clerk.Processes.WebExtractorToJson.Model.MobilePhone;
using Elasticsearch.Net;
using HtmlAgilityPack;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Clerk.Processes.WebExtractorToJson
{
    internal class Program
    {
        private static string brandName = string.Empty;
        private static string phoneName = string.Empty;
        private static ElasticClient elasticClient;
        private static ElasticsearchStorageOptions elasticOptions;
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer { Formatting = Formatting.Indented };

        public static void Main(string[] args)
        {
            elasticOptions = new ElasticsearchStorageOptions
            {
                ElasticsearchEndpoint = new Uri(@"http://localhost:9200"),
                IndexName = "mobiles",
                IndexMappingDepthLimit = 10000
            };

            var connectionPool = new SingleNodeConnectionPool(elasticOptions.ElasticsearchEndpoint);
            var connectionSettings = new ConnectionSettings(connectionPool, sourceSerializer: JsonNetSerializer.Default);

            if (!string.IsNullOrEmpty(elasticOptions.UserName) && !string.IsNullOrEmpty(elasticOptions.Password))
            {
                connectionSettings = connectionSettings.BasicAuthentication(elasticOptions.UserName, elasticOptions.Password);
            }

            elasticClient = new ElasticClient(connectionSettings);

            //ExtractPhoneInfo("https://www.gsmarena.com/samsung_galaxy_a31-10149.php", 1);
            //ExtractAllPhones();

            //ExtractPhones("samsung", 9);
            //ExtractPhones("htc", 45);
            //ExtractPhones("apple", 48);
            //ExtractPhones("asus", 46);
            //ExtractPhones("blackberry", 36);
            //ExtractPhones("benq", 31);
            //ExtractPhones("blu", 67);
            //ExtractPhones("dell", 61);
            //ExtractPhones("gigabyte", 47);
            //ExtractPhones("hp", 41);
            //ExtractPhones("google", 107);
            //ExtractPhones("honor", 121);
            //ExtractPhones("htc", 45);
            //ExtractPhones("huawei", 58);
            //ExtractPhones("infinix", 119);
            //ExtractPhones("infinix", 119);
            //ExtractPhones("lava", 94);
            //ExtractPhones("lenovo", 73);
            //ExtractPhones("lg", 20);
            //ExtractPhones("microsoft", 64);
            //ExtractPhones("motorola", 4);
            //ExtractPhones("nokia", 1);
            //ExtractPhones("oppo", 82);
            ExtractPhones("oneplus", 95);
            ExtractPhones("orange", 71);
            ExtractPhones("panasonic", 6);
            ExtractPhones("philips", 11);
            ExtractPhones("qmobile", 103);
            ExtractPhones("realme", 118);
            ExtractPhones("xiaomi", 80);
            ExtractPhones("zte", 62);
            ExtractPhones("vivo", 98);
            ExtractPhones("sony", 7);

            Console.WriteLine("Done processing.");
            Console.ReadKey();
        }

        public static void ExtractAllPhones()
        {
            var brandCounter = 1;

            while (brandCounter <= 30)
            {
                var brandsUrl = $"https://www.gsmarena.com/makers.php3";
                var web = new HtmlWeb();
                var doc = web.Load(Uri.EscapeUriString(brandsUrl));
                var table = doc.DocumentNode?.SelectNodes("//table");

                if (table != null)
                {
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
            }
        }

        public static int RandomNumber(int min, int max)
        {
            var random = new Random();
            return random.Next(min, max);
        }

        public static void ExtractPhones(string brand, int phoneNo)
        {
            var urlLink = $"https://www.gsmarena.com/{brand}-phones-f-{phoneNo}";
            ExtractPhones(urlLink);
            Thread.Sleep(TimeSpan.FromSeconds(RandomNumber(30, 120)));
        }

        public static void ExtractPhones(string urlLink)
        {
            var pageNo = 1;
            while (true)
            {
                var numberPhone = 0;
                Console.WriteLine($"----------------- Page no: {pageNo}");
                var phonesUrl = $"{urlLink}-0-p{pageNo++}.php";
                var web = new HtmlWeb();
                var doc = web.Load(Uri.EscapeUriString(phonesUrl));
                brandName = doc.DocumentNode?.SelectNodes("//h1[@class='article-info-name']")
                    .Select(n => n.InnerHtml).ToList().First().Split(' ')[0];

                if (brandName != null && !string.IsNullOrWhiteSpace(brandName))
                {
                    var phonesListNode = doc.DocumentNode?.SelectNodes("//div[@class='makers']/ul").Descendants("li")
                        .Select(n => n.LastChild.Attributes["href"].Value).ToList();

                    if ((phonesListNode != null && phonesListNode.Count.Equals(0)) || pageNo >= 5)
                    {
                        return;
                    }

                    if (!Directory.Exists($"C:\\Licenta\\extracted\\{brandName}"))
                    {
                        Directory.CreateDirectory($"C:\\Licenta\\extracted\\{brandName}");
                    }

                    if (phonesListNode != null)
                    {
                        phonesListNode.RemoveRange(0, numberPhone);
                        foreach (var phoneUrl in phonesListNode.Select(phoneNode => $"https://www.gsmarena.com/{phoneNode}"))
                        {
                            numberPhone++;
                            Thread.Sleep(TimeSpan.FromSeconds(RandomNumber(1,10)));
                            try
                            {
                                ExtractPhoneInfo(phoneUrl, numberPhone);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error - {phoneUrl}");
                                using var writer = new StreamWriter(@"C:\Licenta\extracted\Error.txt", true);
                                writer.WriteLine();
                                writer.WriteLine($"Error - {phoneUrl}");
                                writer.WriteLine("#--------------#");
                                writer.WriteLine(ex.Message);
                                writer.WriteLine();
                            }
                        }
                    }
                }
            }
        }

        public static void ExtractPhoneInfo(string url, int numberPhone)
        {
            var web = new HtmlWeb();
            var doc = web.Load(Uri.EscapeUriString(url));
            phoneName = doc.DocumentNode?.SelectNodes("//h1[@data-spec='modelname']")
                .Select(n => n.InnerHtml).ToList().First().Split('/')[0];

            var phoneImage = doc.DocumentNode?.SelectNodes("//div[contains(@class, 'specs-photo-main')]//img")
                .First().Attributes.First(x => x.Name.Equals("src")).Value;

            if (phoneName != null && !string.IsNullOrWhiteSpace(phoneName))
            {
                var phoneStatus = doc.DocumentNode?.SelectNodes("//td[@data-spec='status']")
                    .Select(n => n.InnerHtml).ToList().First().Split(' ')[0];

                if (phoneStatus.Contains("Discontinued") ||
                    phoneStatus.Contains("Coming") ||
                    phoneStatus.Contains("Cancelled") ||
                    phoneStatus.Contains("Rumored"))
                {
                    return;
                }

                if (phoneName.Contains("Watch"))
                {
                    return;
                }

                var bodyNode = doc.DocumentNode?.SelectNodes("//table[@cellspacing]");
                dynamic expando = new ExpandoObject();

                var expandoFull = expando as IDictionary<string, object>;
                expandoFull.Add("Photo", phoneImage);

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

                var filePath = $"C:\\Licenta\\extracted\\{brandName}\\{phoneName}.json";
                StoreToDbAndFile(expando, filePath, numberPhone);
            }
        }

        public static void StoreToDbAndFile(ExpandoObject expando, string filePath, int numberPhone)
        {
            var outputJson = JsonConvert.SerializeObject(expando);
            var gsmArena = JsonConvert.DeserializeObject<GsmArenaModel>(outputJson);

            if (gsmArena.Network.Technology.First().Contains("no", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            var phoneElastic = new Mobile(gsmArena);

            if (phoneElastic.Display.Size.In < 2 ||
                phoneElastic.Display.Size.In > 8)
            {
                return;
            }

            if (!elasticClient.Indices.Exists(elasticOptions.IndexName).Exists)
            {
                elasticClient.Indices.Create(elasticOptions.IndexName, c => c
                    .Map<Mobile>(p => p.AutoMap()));
            }
            elasticClient.IndexAsync(JObject.FromObject(phoneElastic, JsonSerializer), i => i.Index(elasticOptions.IndexName).Refresh(Refresh.True));

            var phoneData = JsonConvert.SerializeObject(phoneElastic);

            using (var file = File.CreateText(filePath))
            {
                JsonSerializer.Serialize(file, phoneData);
            }

            Console.WriteLine($"{numberPhone}. {phoneName}");
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
