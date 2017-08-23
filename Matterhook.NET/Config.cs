using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matterhook.NET
{
    public class Config
    {
        public DiscourseConfig DiscourseConfig { get; set; }
        
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
}
