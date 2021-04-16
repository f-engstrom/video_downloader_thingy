using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ConsoleApp1
{
    class Program
    {
        static HttpClient client = new HttpClient();


        static async Task Main(string[] args)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "yourtoken");


            var startUrl = "anotheruri";
            var firstLap = true;
            string newUrl = "";

            int iteration = 0;
            var allVideoUrlWithTitles = new List<TitleAndUrl>();
            do
            {
                var idsAndUrl = await GetVideoIds(firstLap ? startUrl : newUrl);
                Console.WriteLine($"lap {iteration} new url is {idsAndUrl.Url} ");
                newUrl = idsAndUrl.Url;

                allVideoUrlWithTitles.AddRange(await GetVideoUrls(idsAndUrl.IdAndTitles));
                iteration++;

                firstLap = false;
            } while (newUrl != null);


            //var urlsString = String.Join(",", allVideoUrls);

            var josnString = JsonConvert.SerializeObject(allVideoUrlWithTitles);

            System.IO.File.WriteAllText("videoUrlTitleObjectsList.txt", josnString);
        }


        private static async Task<IdsAndUrl> GetVideoIds(string url)
        {
            IdsAndUrl idsAndUrl = new IdsAndUrl();


            var response =
                await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                Root1 videoObjectsResponse = new Root1();

                videoObjectsResponse = JsonConvert.DeserializeObject<Root1>(jsonString, new JsonSerializerSettings
                {
                    Error = HandleDeserializationError
                });


                if (videoObjectsResponse != null)
                {
                    idsAndUrl.Url = videoObjectsResponse.next;

                    idsAndUrl.IdAndTitles = videoObjectsResponse.results.Where(y => y.asset.asset_type == "Video")
                        .Select(video => new IdAndTitle
                        {
                           Id = video.id.ToString(),
                           Title = video.title
                            
                        } ).ToList();
                }
            }
            else
            {
                Console.WriteLine("oh no no video ids!");
            }

            return idsAndUrl;
        }


        private static async Task<List<TitleAndUrl>> GetVideoUrls(List<IdAndTitle> videoIdAndTitles)
        {
            List<TitleAndUrl> titleAndUrls = new List<TitleAndUrl>();


            foreach (var videoIdTitle in videoIdAndTitles)
            {
                Console.WriteLine($"id {videoIdTitle.Id} {videoIdTitle.Title}");
                var videoResponse = await client.GetAsync("urigoeshere");
                    
                if (videoResponse.IsSuccessStatusCode)
                {
                    var jsonStringVideo = await videoResponse.Content.ReadAsStringAsync();
                    var videoObject = JsonConvert.DeserializeObject<Root>(jsonStringVideo, new JsonSerializerSettings
                    {
                        Error = HandleDeserializationError
                    });


                    try
                    {
                        TitleAndUrl titleAndUrl = new TitleAndUrl();
                        titleAndUrl.Url = videoObject.asset.media_sources[0].src;
                        titleAndUrl.Title = videoIdTitle.Title;


                        using (var client = new WebClient())
                        {
                            client.DownloadFile(titleAndUrl.Url, $"{titleAndUrl.Title}.mp4");
                        }    
                        
                        titleAndUrls.Add(titleAndUrl);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else
                {
                    Console.WriteLine("oh no, get no urls!");
                }
            }


            return titleAndUrls;
        }


        public static void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }
    }


    public class IdsAndUrl
    {
        public List<IdAndTitle> IdAndTitles { get; set; } = new List<IdAndTitle>();
        public string Url { get; set; }
    }


    public class IdAndTitle
    {
        public string Title { get; set; }
        public string Id { get; set; }
        
    }

    public class TitleAndUrl
    {
        public string Title { get; set; }
        public string Url { get; set; }
        
    }


    public class Asset1
    {
        public string _class { get; set; }
        public int? id { get; set; }
        public string asset_type { get; set; }
        public string title { get; set; }
        public DateTime created { get; set; }
    }

    public class Result1
    {
        public string _class { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public DateTime created { get; set; }
        public string description { get; set; }
        public string title_cleaned { get; set; }
        public bool is_published { get; set; }
        public string transcript { get; set; }
        public bool is_downloadable { get; set; }
        public bool is_free { get; set; }
        public Asset1 asset { get; set; }
        public int sort_order { get; set; }
    }

    public class Root1
    {
        public int count { get; set; }
        public string next { get; set; }
        public object previous { get; set; }
        public List<Result1> results { get; set; }
    }

    public class MediaSource
    {
        public string type { get; set; }
        public string src { get; set; }
        public string label { get; set; }
    }

    public class Video
    {
        public string type { get; set; }
        public string label { get; set; }
        public string file { get; set; }
    }

    public class DownloadUrls
    {
        public List<Video> Video { get; set; }
    }

    public class Asset
    {
        public string _class { get; set; }
        public int id { get; set; }
        public List<MediaSource> media_sources { get; set; }
        public DownloadUrls download_urls { get; set; }
    }

    public class Root
    {
        public string _class { get; set; }
        public int id { get; set; }
        public string description { get; set; }
        public bool is_free { get; set; }
        public Asset asset { get; set; }
        public object last_watched_second { get; set; }
        public string download_url { get; set; }
    }
}