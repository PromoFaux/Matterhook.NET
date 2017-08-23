using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matterhook.NET
{
    public class Config
    {
        public string DiscourseUrl { get; set; }
        public string DiscourseWebhookSecret { get; set; }

        public string MattermostWebhookUrl { get; set; }
        public string MattermostBotChannel { get; set; }
        public string MatthermostBotName { get; set; }
        public string MattermostBotImage { get; set; }
        public string[] MattermostIgnoredTopicTitles { get; set; }
        public bool MattermostIgnorePrivateMessages { get; set; }
    }
}
