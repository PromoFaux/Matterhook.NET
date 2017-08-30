using System;
using System.Collections.Generic;
using Matterhook.NET.Code;
using Newtonsoft.Json;

namespace Matterhook.NET.Webhooks.DockerHub
{
    public class Payload
    {
        [JsonProperty(PropertyName = "push_data")]
        public PushData PushData { get; set; }

        [JsonProperty(PropertyName = "callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty(PropertyName = "repository")]
        public Repository Repository { get; set; }
    }

    public class PushData
    {
        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "pushed_at")]
        public DateTime PushedAt { get; set; }

        [JsonProperty(PropertyName = "images")]
        public List<string> Images { get; set; }

        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }

        [JsonProperty(PropertyName = "pusher")]
        public string Pusher { get; set; }
    }

    public class Repository
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "is_trusted")]
        public bool IsTrusted { get; set; }

        [JsonProperty(PropertyName = "full_description")]
        public string FullDescription { get; set; }

        [JsonProperty(PropertyName = "repo_url")]
        public string RepoUrl { get; set; }

        [JsonProperty(PropertyName = "owner")]
        public string Owner { get; set; }

        [JsonProperty(PropertyName = "is_official")]
        public bool IsOfficial { get; set; }

        [JsonProperty(PropertyName = "is_private")]
        public bool IsPrivate { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "namespace")]
        public string Namespace { get; set; }

        [JsonProperty(PropertyName = "star_count")]
        public int StarCount { get; set; }

        [JsonProperty(PropertyName = "comment_count")]
        public int CommentCount { get; set; }

        [JsonConverter(typeof(UnixDateTimeConverter))]
        [JsonProperty(PropertyName = "date_created")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "docker_file")]
        public string Dockerfile { get; set; }

        [JsonProperty(PropertyName = "repo_name")]
        public string RepoName { get; set; }
    }
}