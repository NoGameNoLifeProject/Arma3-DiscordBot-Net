using System;
using Newtonsoft.Json;

namespace DiscordBot.Common.Entities;

public class YouTubeSearchResult
{
    public string Title { get; }

    public string Id { get; }
    
    public TimeSpan Duration { get; set; }

    public YouTubeSearchResult(string title, string id)
    {
        Title = title;
        Id = id;
    }

}

internal class YouTubeApiResponseItemContent
{
    [JsonProperty("id")]
    public string Id { get; private set; }
    
    [JsonProperty("contentDetails")]
    public ResponseContentDetails ContentDetails { get; private set; }
    
    public struct ResponseContentDetails
    {
        [JsonProperty("duration")]
        public string Duration { get; private set; }
    }
}

internal class YouTubeApiResponseItem
{
    [JsonProperty("id")]
    public ResponseId Id { get; private set; }

    [JsonProperty("snippet")]
    public ResponseSnippet Snippet { get; private set; }


    public struct ResponseId
    {
        [JsonProperty("videoId")]
        public string VideoId { get; private set; }
    }

    public struct ResponseSnippet
    {
        [JsonProperty("title")]
        public string Title { get; private set; }
    }
}