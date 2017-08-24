using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Matterhook.NET.Code;
using Matterhook.NET.Webhooks.Github;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

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
                   
                    HttpResponseMessage response = null;
                    switch (githubHook.Event)
                    {
                        case "pull_request":
                            response = await matterHook.PostAsync(GetMessagePullRequest((PullRequestEvent)githubHook.Payload));
                            break;
                        case "issues":
                            response = await matterHook.PostAsync(GetMessageIssues((IssuesEvent)githubHook.Payload));
                            break;
                        case "issue_comment":
                            response = await matterHook.PostAsync(GetMessageIssueComment((IssueCommentEvent)githubHook.Payload));
                            break;
                        case "repository":
                            response = await matterHook.PostAsync(GetMessageRepository((RepositoryEvent)githubHook.Payload));
                            break;
                        case "create":
                            response = await matterHook.PostAsync(GetMessageCreate((CreateEvent)githubHook.Payload));
                            break;
                        case "delete":
                            response = await matterHook.PostAsync(GetMessageDelete((DeleteEvent)githubHook.Payload));
                            break;
                        case "pull_request_review_comment":
                            response = await matterHook.PostAsync(GetMessagePullRequestReviewComment((PullRequestReviewCommentEvent)githubHook.Payload));
                            break;
                        case "push":
                            break;
                        case "commit_comment":
                            break;
                        default:
                            break;

                    }

                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        return Ok();
                    }

                    return Content(response != null ? $"Problem posting to Mattermost: {response.StatusCode}" : "Error!");
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

        private MattermostMessage GetMessagePullRequestReviewComment(PullRequestReviewCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            switch (payload.action)
            {
                case "created":
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private MattermostMessage GetMessageDelete(DeleteEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            switch (payload.ref_type)
            {
                case "branch":
                    break;
                case "tag":
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.ref_type}");
            }

            return retVal;
        }

      

        private MattermostMessage GetMessageCreate(CreateEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            switch (payload.ref_type)
            {
                case "branch":
                    break;
                case "tag":
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.ref_type}");
            }

            return retVal;
        }

        private MattermostMessage GetMessageRepository(RepositoryEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            switch (payload.action)
            {
                case "created":
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private MattermostMessage GetMessageIssueComment(IssueCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            switch (payload.action)
            {
                case "created":
                    break;
                //case "edited": // This gets annoying
                //    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private MattermostMessage GetMessageIssues(IssuesEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            switch (payload.action)
            {
                case "opened":
                    break;
                case "closed":
                    break;
                case "labeled":
                    break;
                case "assigned":
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private MattermostMessage GetMessagePullRequest(PullRequestEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            switch (payload.action)
            {
                case "opened":
                    //IDE complains : Cannot convert source type 'System.Collections.Generic.List<Matterhook.Net.MattermostAttachement>' to target type 'System.Collections.Generic.List`1'
                    //But it builds. Google reveals nothing, but it compiles and runs so....
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
                    break;
                case "closed":
                    break;
                case "assigned":
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }



        private MattermostMessage BaseMessageForRepo(string repoName)
        {
            var mmc = GetMattermostDetails(repoName);
            //set matterHook Client to correct webhook.
            matterHook = new MatterhookClient(mmc.WebhookUrl);

            var retVal = new MattermostMessage
            {
                Channel = mmc.Channel,
                Username = mmc.Username,
                IconUrl = new Uri(mmc.IconUrl),

            };

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
