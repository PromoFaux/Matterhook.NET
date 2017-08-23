using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Matterhook.NET.Code;
using Matterhook.NET.Webhooks.Discourse;
using Matterhook.NET.Webhooks.Github;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using ReverseMarkdown;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Matterhook.NET.Controllers
{
    [Route("[Controller]")]
    public class GithubHookController : Controller
    {

        private readonly GithubConfig _config;
        private MatterhookClient matterHook;

        public GithubHookController(IOptions<Config> config)
        {
            var c = config ?? throw new ArgumentNullException(nameof(config));
            _config = c.Value.GithubConfig;

        }

        [HttpPost("")]
        public async Task<IActionResult> Receive()
        {
            try
            {
                string payloadText;

                //Generate GithubHook Object
                //Generate DiscourseHook object for easier reading
                Console.WriteLine($"Github Hook received: {DateTime.Now}");

                Request.Headers.TryGetValue("X-GitHub-Event", out StringValues strEvent);
                Request.Headers.TryGetValue("X-Hub-Signature", out StringValues signature);
                Request.Headers.TryGetValue("X-GitHub-Delivery", out StringValues delivery);

                Console.WriteLine($"Hook Id: {delivery}");

                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadText = await reader.ReadToEndAsync();
                }

                var calcSig = Util.CalculateSignature(payloadText, signature, _config.Secret, "sha1=");


                if (signature == calcSig)
                {
                    var githubHook = new GithubHook(strEvent, signature, delivery, payloadText);

                    switch (githubHook.Event)
                    {
                        case "pull_request":
                            var toSend = GetMessagePullRequest((PullRequestEvent)githubHook.Payload);
                            var test = await matterHook.PostAsync(toSend);
                            return Ok();
                        default:
                            break;

                    }

                    return Ok();
                }
                else
                {
                    Console.WriteLine("Invalid Signature!");
                    Console.WriteLine($"Expected: {signature}");
                    Console.WriteLine($"Calculated: {calcSig}");
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Content(e.Message);
            }
        }

        private MattermostMessage GetMessagePullRequest(PullRequestEvent payload)
        {
            var mmc = GetMattermostDetails(payload.repository.full_name);
            matterHook = new MatterhookClient(mmc.WebhookUrl);

            var retVal = new MattermostMessage
            {
                Channel = mmc.Channel,
                Username = mmc.Username,
                IconUrl = new Uri(mmc.IconUrl),
                
            };

            switch (payload.action)
            {
                case "opened":
                    retVal.Attachments = new List<MattermostAttachment>
                    {
                        new MattermostAttachment
                        {
                            Title = payload.pull_request.title,
                            TitleLink = new Uri(payload.pull_request.html_url),
                            Text = payload.pull_request.body,
                            AuthorName = payload.pull_request.user.login,
                            AuthorIcon = new Uri(payload.pull_request.user.avatar_url),
                            AuthorLink = new Uri(payload.pull_request.user.url)

                        }
                    };

                    retVal.Text =
                        $"#New-Pull-Request in [{payload.repository.full_name}]({payload.repository.html_url}) ([#{payload.pull_request.number}]({payload.pull_request.html_url}))";
                    
                        retVal.Attachments[0].Fields = new List<MattermostField>
                        {
                            new MattermostField
                            {
                                Short = true,
                                Title = payload.pull_request.changed_files.ToString(),
                                Value = payload.pull_request.created_at.ToString()
                                
                            }
                        };
                    

                    break;
            }




            return retVal;
        }

        /// <summary>
        /// Verifies mattermost config on a per-repo basis. If it's not found, then it's posted to the default settings.
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private MattermostConfig GetMattermostDetails(string fullName)
        {

            var repo = _config.RepoList.FirstOrDefault(x => string.Equals(x.RepoName, fullName, StringComparison.CurrentCultureIgnoreCase));

            if (repo != null)
            {
                return new MattermostConfig
                {
                    Channel = string.IsNullOrWhiteSpace(repo.MattermostConfig.Channel)
                        ? _config.DefaultMattermostChannel
                        : repo.MattermostConfig.Channel,
                    IconUrl = string.IsNullOrWhiteSpace(repo.MattermostConfig.IconUrl)
                        ? _config.DefaultMattermostIcon
                        : repo.MattermostConfig.IconUrl,
                    Username = string.IsNullOrWhiteSpace(repo.MattermostConfig.Username)
                        ? _config.DefaultMattermostUsername
                        : repo.MattermostConfig.Username,
                    WebhookUrl = string.IsNullOrWhiteSpace(repo.MattermostConfig.WebhookUrl)
                        ? _config.DefaultMattermostWebhookUrl
                        : repo.MattermostConfig.WebhookUrl
                };
            }


            return new MattermostConfig
            {
                Channel = _config.DefaultMattermostChannel,
                IconUrl = _config.DefaultMattermostIcon,
                Username = _config.DefaultMattermostUsername,
                WebhookUrl = _config.DefaultMattermostWebhookUrl
            };
        }
    }
}
