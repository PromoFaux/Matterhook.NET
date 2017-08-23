using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matterhook.NET
{
    public class Config
    {
        public DiscourseConfig DiscourseConfig { get; set; }
        public GithubConfig GithubConfig { get; set; }
        
    }

    public class DiscourseConfig
    {
        public string Url { get; set; }
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

    public class GithubConfig
    {
        public string Secret { get; set; }
        public string DefaultMattermostChannel { get; set; }
        public string DefaultMattermostUsername { get; set; }
        public string DefaultMattermostIcon { get; set; }
        public string DefaultMattermostWebhookUrl { get; set; }

        //TODO Look at this
        //public string DefaultSubscribedEvents { get; set; }

        public List<RepoConfig> RepoList { get; set; }
    }

    public class RepoConfig
    {
        public string RepoName { get; set; }
        //Todo: Look at this
        //public string SubscribedEvents { get; set; }
        public MattermostConfig MattermostConfig { get; set; }
        
    }
}
