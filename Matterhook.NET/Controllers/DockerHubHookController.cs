using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Matterhook.NET.Code;
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
            try
            {
                _config = config.Value.DockerHubConfig;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Receive()
        {

            var stuffToLog = new List<string>();

            try
            {
                string payloadText;
                //Generate DiscourseHook object for easier reading
                stuffToLog.Add($"DockerHub Hook received: {DateTime.Now}");

                Request.Headers.TryGetValue("X-Request-Id", out StringValues requestId);
                stuffToLog.Add($"Hook Id: {requestId}");
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadText = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                //No fancy checksumming on this hook. I'll keep an eye on it in future...
                var dockerhubHook = new DockerHubHook(payloadText);
                
                var mm = Util.GetMattermostDetails(_config.DefaultMattermostConfig,
                    _config.RepoList, dockerhubHook.payload.Repository.RepoName);

                var matterHook = new MatterhookClient.MatterhookClient(mm.WebhookUrl);

                var reponame = dockerhubHook.payload.Repository.RepoName;
                var repoMd = $"[{reponame}]({dockerhubHook.payload.Repository.RepoUrl})";
                
                var msg = new MattermostMessage
                {
                    Channel = mm.Channel,
                    Username = mm.Username,
                    IconUrl = mm.IconUrl != null ? new Uri(mm.IconUrl) : null,
                    Text = $"New image built and pushed to {repoMd} with tag `{dockerhubHook.payload.PushData.Tag}`"
                };

                stuffToLog.Add(msg.Text);

                var response = await matterHook.PostAsync(msg);

                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    stuffToLog.Add(response != null
                        ? $"Unable to post to Mattermost {response.StatusCode}"
                        : "Unable to post to Mattermost");

                    return StatusCode(500, response != null
                        ? $"Unable to post to Mattermost: {response.StatusCode}"
                        : "Unable to post to Mattermost");
                }

                if (!_config.LogOnlyErrors)
                {
                    stuffToLog.Add(msg.Text);
                    stuffToLog.Add("Succesfully posted to Mattermost");
                    Util.LogList(stuffToLog);
                }
                
                return StatusCode(200, "Succesfully posted to Mattermost");
            }
            catch (Exception e)
            {
                stuffToLog.Add(e.Message);
                Util.LogList(stuffToLog);
                return StatusCode(500,e.Message);
            }
        }
    }


}