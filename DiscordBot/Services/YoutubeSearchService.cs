using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DiscordBot.Common.Entities;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Services;

public class YoutubeSearchService
{
    private string ApiKey { get; }
    private HttpClient Http { get; }
    
    public YoutubeSearchService()
    {
        ApiKey = MusicService.Config.YoutubeAPIKey;
        Http = new HttpClient()
        {
            BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/search")
        };
        Http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Companion-Cube");
    }
    
    public async Task<List<YouTubeSearchResult>> SearchAsync(string term)
    {
        var uri = new Uri($"https://www.googleapis.com/youtube/v3/search?part=snippet&maxResults=5&type=video&fields=items(id(videoId),snippet(title))&key={ApiKey}&q={WebUtility.UrlEncode(term)}");

        var json = "{}";
        using (var req = await Http.GetAsync(uri))
        using (var res = await req.Content.ReadAsStreamAsync())
        using (var sr = new StreamReader(res, new UTF8Encoding(false)))
            json = await sr.ReadToEndAsync();

        var jsonData = JObject.Parse(json);
        var data = jsonData["items"]?.ToObject<IEnumerable<YouTubeApiResponseItem>>()?.Select(x => new YouTubeSearchResult(x.Snippet.Title, x.Id.VideoId)).ToList();
        
        uri = new Uri($"https://youtube.googleapis.com/youtube/v3/videos?part=contentDetails&id={string.Join(',',data?.Select(x=>x.Id))}&key={ApiKey}");
        using (var req = await Http.GetAsync(uri))
        using (var res = await req.Content.ReadAsStreamAsync())
        using (var sr = new StreamReader(res, new UTF8Encoding(false)))
            json = await sr.ReadToEndAsync();

        jsonData = JObject.Parse(json);
        var contentData = jsonData["items"]?.ToObject<IEnumerable<YouTubeApiResponseItemContent>>();
        
        for (int i = 0; i < data.Count; i++)
        {
            data[i].Duration = XmlConvert.ToTimeSpan(contentData.First(x=> x.Id == data[i].Id).ContentDetails.Duration);
        }

        return data;
    }
}