using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Matterhook.NET
{
    public class Config
    {
        public DiscourseConfig DiscourseConfig { get; set; }
        public GithubConfig GithubConfig { get; set; }
        public DockerHubConfig DockerHubConfig { get; set; }

        public void Save(string path)
        {
            // serialize JSON directly to a file
            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                serializer.Serialize(file, this);
            }
        }

        internal void Save()
        {
            throw new NotImplementedException();
        }
    }

    public class DiscourseConfig
    {
        public bool LogOnlyErrors { get; set; } = true;
        public string Secret { get; set; }
        public string[] IgnoredTopicTitles { get; set; }
        public bool IgnorePrivateMessages { get; set; }
        public MattermostConfig MattermostConfig { get; set; }

    }

    public class MattermostConfig
    {
        public string WebhookUrl { get; set; }
        public string Channel { get; set; }
        public string Username { get; set; }
        public string IconUrl { get; set; }
    }

    //TODO: Look at configuring subscribed events
    public class GithubConfig
    {
        public bool LogOnlyErrors { get; set; } = true;
        public string Secret { get; set; }
        public MattermostConfig DefaultMattermostConfig { get; set; }
        public List<RepoConfig> RepoList { get; set; }
        public bool DebugSavePayloads { get; set; } = false;
    }

    public class RepoConfig
    {
        public string RepoName { get; set; }
        public MattermostConfig MattermostConfig { get; set; }
        public Filters Filters { get; set; }
    }

    public class Filters
    {
        public StatusFilter Status { get; set; } = new StatusFilter();
    }

    public class Filter
    {
        public bool WebhookEnabled { get; set; } = true;
        public string[] IgnoredProviders { get; set; }
    }

    public class StatusFilter : Filter
    {
        public Filter Success { get; set; }
        public Filter Pending { get; set; } = new Filter { WebhookEnabled = false };
        public Filter Failed { get; set; }
    }

    public class DockerHubConfig
    {
        public bool LogOnlyErrors { get; set; } = true;
        public MattermostConfig DefaultMattermostConfig { get; set; }
        public List<RepoConfig> RepoList { get; set; }
    }
}