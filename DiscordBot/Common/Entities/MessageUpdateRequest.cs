using System.Collections.Generic;
using System.Linq;
using Discord;
using Newtonsoft.Json;

namespace DiscordBot.Common.Entities;

public class MessageUpdateRequest
{
    [JsonProperty("content")]
    public string Content { get; set; }
    
    [JsonProperty("embeds")]
    public List<Embed> Embeds { get; set; }

    [JsonProperty("components")]
    public List<ComponentsClass> Components { get; set; }

    public MessageUpdateRequest(string content, Discord.Embed embed, MessageComponent component)
    {
        Content = content;
        Embeds = new List<Embed>() { new Embed()
        {
            Title = embed.Title,
            Color = embed.Color?.RawValue ?? 3092790,
            Fields = new List<Embed.Field>(),
            Image = new Embed.ImageClass()
            {
                Url = embed.Image.HasValue ? embed.Image.Value.Url : "",
                Proxy_url = embed.Image.HasValue ? embed.Image.Value.ProxyUrl : "",
                Width = embed.Image.HasValue ? embed.Image.Value.Width : 0,
                Height = embed.Image.HasValue ? embed.Image.Value.Height : 0
            }
        }};
        foreach (var field in embed.Fields)
        {
            Embeds.FirstOrDefault()?.Fields.Add( new Embed.Field()
            {
                Name = field.Name,
                Value = field.Value,
                Inline = field.Inline
            });
        }

        Components = new List<ComponentsClass>();
        if (component is not null)
        {
            foreach (var comprow in component.Components)
            {
                Components.Add(new ComponentsClass(comprow));
            }
        }
    }
    
    public class Embed
    {
        [JsonProperty("type")]
        public string Type {get; set;} = "rich";
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("color")]
        public uint Color { get; set; }
        
        [JsonProperty("fields")]
        public List<Field> Fields { get; set; }
        
        [JsonProperty("image")]
        public ImageClass Image { get; set; }
        
        public class Field
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("value")]
            public string Value { get; set; }
            
            [JsonProperty("inline")]
            public bool Inline { get; set; }
        }

        public class ImageClass
        {
            [JsonProperty("url")]
            public string Url { get; set; }
            
            [JsonProperty("proxy_url")]
            public string Proxy_url { get; set; }
            
            [JsonProperty("width")]
            public int? Width { get; set; }
            
            [JsonProperty("height")]
            public int? Height { get; set; }
        }
    }

    public class ComponentsClass
    {
        [JsonProperty("type")]
        public int Type { get; set; }
        
        [JsonProperty("components")]
        public List<ComponentsSubClass> Components { get; set; }

        public ComponentsClass(ActionRowComponent components)
        {
            Type = (int)components.Type;
            Components = new List<ComponentsSubClass>();
            foreach (ButtonComponent component in components.Components)
            {
                Components.Add(new ComponentsSubClass
                {
                    Type = (int)component.Type,
                    Style = (int)component.Style,
                    Label = component.Label,
                    Custom_id = component.CustomId,
                    Emoji = component.Emote is not null ? new ComponentsSubClass.EmojiClass()
                    {
                        ID = (component.Emote as GuildEmote).Id.ToString(),
                        Name = (component.Emote as GuildEmote).Name,
                        Animated = (component.Emote as GuildEmote).Animated,
                    } : null
                });
            }
        }

        public class ComponentsSubClass
        {
            [JsonProperty("type")]
            public int Type { get; set; }
            
            [JsonProperty("style")]
            public int Style { get; set; }
            
            [JsonProperty("label")]
            public string Label { get; set; }
            
            [JsonProperty("custom_id")]
            public string Custom_id { get; set; }
            
            [JsonProperty("emoji")]
            public EmojiClass Emoji { get; set; }

            public class EmojiClass
            {
                [JsonProperty("id")]
                public string ID;
                
                [JsonProperty("name")]
                public string Name;
                
                [JsonProperty("animated")]
                public bool Animated;
            }
        }
    }
}