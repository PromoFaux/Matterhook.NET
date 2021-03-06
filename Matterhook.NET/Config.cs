﻿using System.Collections.Generic;

namespace Matterhook.NET
{
    public class Config
    {
        public DiscourseConfig DiscourseConfig { get; set; }
        public GithubConfig GithubConfig { get; set; }
        public DockerHubConfig DockerHubConfig { get; set; }
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
    }

    public class DockerHubConfig
    {
        public bool LogOnlyErrors { get; set; } = true;
        public MattermostConfig DefaultMattermostConfig { get; set; }
        public List<RepoConfig> RepoList { get; set; }
    }
}