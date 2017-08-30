using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Matterhook.NET.MatterhookClient;
using Matterhook.NET.Webhooks.DockerHub;

namespace Matterhook.NET.Controllers
{
    [Route("[Controller]")]
    public class DockerHubHookController : Controller
    {
        private readonly DockerHubConfig _config;

        public DockerHubHookController(IOptions<Config> config)
        {
            var c = config ?? throw new ArgumentNullException(nameof(config));
            _config = c.Value.DockerHubConfig;
        }

        [HttpPost("")]
        public async Task<IActionResult> Receive()
        {
            try
            {
                string payloadText;
                //Generate DiscourseHook object for easier reading
                Console.WriteLine($"DockerHub Hook received: {DateTime.Now}");

                Request.Headers.TryGetValue("X-Request-Id", out StringValues requestId);
                Console.WriteLine($"Hook Id: {requestId}");
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadText = await reader.ReadToEndAsync();
                }

                //No fancy checksumming on this hook. I'll keep an eye on it in future...
                var dockerhubHook = new DockerHubHook(payloadText);

                var mm = GetMattermostDetails(dockerhubHook.payload.Repository.RepoName);

                var matterHook = new MatterhookClient.MatterhookClient(mm.WebhookUrl);

                var reponame = dockerhubHook.payload.Repository.RepoName;
                var repoMd = $"[{reponame}]({dockerhubHook.payload.Repository.RepoUrl})";
                
                var msg = new MattermostMessage
                {
                    Channel = mm.Channel,
                    Username = mm.Username,
                    IconUrl = mm.IconUrl != null ? new Uri(mm.IconUrl) : null,
                    Text = $"#### New image built and pushed to {repoMd}:{dockerhubHook.payload.PushData.Tag}"
                };

                var response = await matterHook.PostAsync(msg);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok();
                }
                return Content("Unable to post to Mattermost");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Content(e.Message);
            }
        }


        //TODO:Break this out to the Util Class to avoid duplication
        /// <summary>
        ///     Verifies mattermost config on a per-repo basis. If it's not found, then it's posted to the default settings.
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private MattermostConfig GetMattermostDetails(string fullName)
        {
            var repo = _config.RepoList.FirstOrDefault(
                x => string.Equals(x.RepoName, fullName, StringComparison.CurrentCultureIgnoreCase));

            if (repo != null)
                return new MattermostConfig
                {
                    Channel = string.IsNullOrWhiteSpace(repo.MattermostConfig.Channel)
                        ? _config.DefaultMattermostConfig.Channel
                        : repo.MattermostConfig.Channel,
                    IconUrl = string.IsNullOrWhiteSpace(repo.MattermostConfig.IconUrl)
                        ? _config.DefaultMattermostConfig.IconUrl
                        : repo.MattermostConfig.IconUrl,
                    Username = string.IsNullOrWhiteSpace(repo.MattermostConfig.Username)
                        ? _config.DefaultMattermostConfig.Username
                        : repo.MattermostConfig.Username,
                    WebhookUrl = string.IsNullOrWhiteSpace(repo.MattermostConfig.WebhookUrl)
                        ? _config.DefaultMattermostConfig.WebhookUrl
                        : repo.MattermostConfig.WebhookUrl
                };


            return new MattermostConfig
            {
                Channel = _config.DefaultMattermostConfig.Channel,
                IconUrl = _config.DefaultMattermostConfig.IconUrl,
                Username = _config.DefaultMattermostConfig.Username,
                WebhookUrl = _config.DefaultMattermostConfig.WebhookUrl
            };
        }
    }


}