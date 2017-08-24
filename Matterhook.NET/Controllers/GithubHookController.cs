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
        private MatterhookClient _matterHook;

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
                    MattermostMessage message = null;
                    switch (githubHook.Event)
                    {
                        case "pull_request":
                            message = GetMessagePullRequest((PullRequestEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "issues":
                            message = GetMessageIssues((IssuesEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "issue_comment":
                            message = GetMessageIssueComment((IssueCommentEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "repository":
                            message = GetMessageRepository((RepositoryEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "create":
                            message = GetMessageCreate((CreateEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "delete":
                            message = GetMessageDelete((DeleteEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "pull_request_review_comment":
                            message = GetMessagePullRequestReviewComment((PullRequestReviewCommentEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "push":
                            message = GetMessagePush((PushEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case "commit_comment":
                            message = GetMessageCommitComment((CommitCommentEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message);
                            break;
                        default:
                            break;
                    }

                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                        return Ok();

                    return Content(
                        response != null ? $"Problem posting to Mattermost: {response.StatusCode}" : "Error!");
                }
                Console.WriteLine("Invalid Signature!");
                Console.WriteLine($"Expected: {signature}");
                Console.WriteLine($"Calculated: {calcSig}");
                return Unauthorized();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Content(e.Message);
            }
        }

        private MattermostMessage GetMessageCommitComment(CommitCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);
            MattermostAttachment att = null;

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var commitMd = $"[`{payload.comment.commit_id.Substring(0,7)}`]({payload.comment.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            switch (payload.action)
            {
                case "created":
                    retVal.Text = $"{userMd} commented on {commitMd} in {repoMd}";
                    att = new MattermostAttachment
                    {
                        Title = payload.comment.commit_id.Substring(0,7),
                        TitleLink = new Uri(payload.comment.html_url),
                        AuthorName = payload.sender.login,
                        AuthorLink = new Uri(payload.sender.html_url),
                        AuthorIcon = new Uri(payload.sender.avatar_url),
                        Text = payload.comment.body

                        
                    };
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            retVal.Attachments = new List<MattermostAttachment>
            {
                att
            };

            return retVal;
        }

        private MattermostMessage GetMessagePush(PushEvent payload)
        {

            var retVal = BaseMessageForRepo(payload.repository.full_name);
            MattermostAttachment att = null;

            if (!payload.deleted && !payload.forced)
                if (!payload._ref.StartsWith("refs/tags/"))
                {
                    var multi = payload.commits.Count > 1 ? "s" : "";
                    var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
                    var branch = payload._ref.Replace("refs/heads/", "");
                    var branchMd = $"[{branch}]({payload.repository.html_url}/tree/{branch})";
                    var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
                    retVal.Text =
                        $"{userMd} pushed {payload.commits.Count} commit{multi} to {branchMd} on {repoMd}";


                    if (payload.commits.Count > 0)
                    {
                        att = new MattermostAttachment
                        {
                            Fields = new List<MattermostField>()
                        };

                        var tmpAdded = new MattermostField
                        {
                            Short = true,
                            Title = "Added"
                        };
                        var tmpRemoved = new MattermostField
                        {
                            Short = true,
                            Title = "Removed"
                        };
                        var tmpModified = new MattermostField
                        {
                            Short = true,
                            Title = "Modified"
                        };

                        foreach (var commit in payload.commits)
                        {
                            att.Text += $"- [`{commit.id.Substring(0, 8)}`]({commit.url}) - {commit.message}\n";
                            if (commit.added.Any())
                                tmpAdded.Value += commit.added.Aggregate("", (current, added) => current + $"`{added}`\n");
                            if (commit.removed.Any())
                                tmpRemoved.Value +=
                                    commit.removed.Aggregate("", (current, removed) => current + $"`{removed}`\n");
                            if (commit.modified.Any())
                                tmpModified.Value +=
                                    commit.modified.Aggregate("", (current, modified) => current + $"`{modified}`\n");
                        }

                        att.Fields.Add(tmpAdded);
                        att.Fields.Add(tmpRemoved);
                        att.Fields.Add(tmpModified);
                    }


                }

            if (att != null)
            {
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };
            }

            return retVal;
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

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";

            switch (payload.ref_type)
            {
                case "branch":
                    retVal.Text = $"{userMd} deleted branch `{payload._ref}` from {repoMd}";
                    break;
                case "tag":
                    retVal.Text = $"{userMd} deleted tag `{payload._ref}` from {repoMd}";
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.ref_type}");
            }

            return retVal;
        }


        private MattermostMessage GetMessageCreate(CreateEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";

            switch (payload.ref_type)
            {
                case "branch":
                    retVal.Text = $"{userMd} added branch `{payload._ref}` to {repoMd}";
                    break;
                case "tag":
                    retVal.Text = $"{userMd} added tag `{payload._ref}` to {repoMd}";
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
                    throw new Exception($"Not implemented Event action: {payload.action}");
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private MattermostMessage GetMessageIssueComment(IssueCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);
            MattermostAttachment att = null;
            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var titleMd = $"[#{payload.issue.number} {payload.issue.title}]({payload.issue.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            switch (payload.action)
            {
                case "created":
                    retVal.Text = $"{userMd} commented on issue {titleMd} in {repoMd}";
                    if (!string.IsNullOrEmpty(payload.issue.body))
                    {
                        att = new MattermostAttachment
                        {
                            Title = $"#{payload.issue.number} {payload.issue.title}",
                            TitleLink = new Uri(payload.comment.html_url),
                            AuthorName = payload.sender.login,
                            AuthorLink = new Uri(payload.sender.html_url),
                            AuthorIcon = new Uri(payload.sender.avatar_url),
                            Text = payload.comment.body
                        };
                    }
                    break;
                //case "edited": // This gets annoying
                //    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            if (att != null)
            {
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };
            }

            return retVal;
        }

        private MattermostMessage GetMessageIssues(IssuesEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            MattermostAttachment att = null;

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var titleMd = $"[#{payload.issue.number} {payload.issue.title}]({payload.issue.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            switch (payload.action)
            {
                case "opened":
                    retVal.Text = $"{userMd} opened issue {titleMd} in {repoMd}";
                    if (!string.IsNullOrEmpty(payload.issue.body))
                    {
                        att = new MattermostAttachment
                        {
                            Title = $"#{payload.issue.number} {payload.issue.title}",
                            TitleLink = new Uri(payload.issue.html_url),
                            AuthorName = payload.sender.login,
                            AuthorLink = new Uri(payload.sender.html_url),
                            AuthorIcon = new Uri(payload.sender.avatar_url),
                            Text = payload.issue.body
                        };
                    }
                    break;
                case "closed":
                    retVal.Text = $"{userMd} closed issue {titleMd} in {repoMd}";
                    break;
                case "reopened":
                    retVal.Text = $"{userMd} reopened issue {titleMd} in {repoMd}";
                    break;
                case "labeled":
                    retVal.Text = $"{userMd} added label: `{payload.label.name}` to {titleMd} in {repoMd}";
                    break;
                case "unlabeled":
                    retVal.Text = $"{userMd} removed label: `{payload.label.name}` from {titleMd} in {repoMd}";
                    break;
                case "assigned":
                    var asignMd = $"[{payload.issue.assignee.login}]({payload.issue.assignee.html_url})";
                    retVal.Text = $"{userMd} assigned {asignMd} to {titleMd} in {repoMd}";
                    break;
                case "unassigned":
                    var unasignMd = $"[{payload.assignee.login}]({payload.assignee.html_url})";
                    retVal.Text = $"{userMd} unassigned {unasignMd} from {titleMd} in {repoMd}";
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }
            if (att != null)
            {
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };
            }

            return retVal;
        }

        private MattermostMessage GetMessagePullRequest(PullRequestEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);
            MattermostAttachment att = null;

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var titleMd = $"[#{payload.pull_request.number} {payload.pull_request.title}]({payload.pull_request.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            switch (payload.action)
            {
                case "opened":
                    //IDE complains : Cannot convert source type 'System.Collections.Generic.List<Matterhook.Net.MattermostAttachement>' to target type 'System.Collections.Generic.List`1'
                    //But it builds. Google reveals nothing, but it compiles and runs so....
                    att = new MattermostAttachment
                    {
                        Title = payload.pull_request.title,
                        TitleLink = new Uri(payload.pull_request.html_url),
                        Text = payload.pull_request.body,
                        AuthorName = payload.pull_request.user.login,
                        AuthorIcon = new Uri(payload.pull_request.user.avatar_url),
                        AuthorLink = new Uri(payload.pull_request.user.html_url)
                    };
                    retVal.Text =
                        $"#New-Pull-Request in [{payload.repository.full_name}]({payload.repository.html_url}) ([#{payload.pull_request.number}]({payload.pull_request.html_url}))";
                    break;
                case "labeled":
                    retVal.Text = $"{userMd} added label: `{payload.label.name}` to {titleMd} in {repoMd}";
                    break;
                case "unlabeled":
                    retVal.Text = $"{userMd} removed label: `{payload.label.name}` from {titleMd} in {repoMd}";
                    break;
                case "closed":
                    break;
                case "assigned":
                    var asignMd = $"[{payload.pull_request.asignee.login}]({payload.pull_request.asignee.html_url})";
                    retVal.Text = $"{userMd} assigned {asignMd} to {titleMd} in {repoMd}";
                    break;
                case "unassigned":
                    var unasignMd = $"[{payload.assignee.login}]({payload.assignee.html_url})";
                    retVal.Text = $"{userMd} unassigned {unasignMd} from {titleMd} in {repoMd}";
                    break;
                default:
                    throw new Exception($"Unhandled Event action: {payload.action}");
            }

            if (att != null)
            {
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };
            }

            return retVal;
        }


        private MattermostMessage BaseMessageForRepo(string repoName)
        {
            var mmc = GetMattermostDetails(repoName);
            //set matterHook Client to correct webhook.
            _matterHook = new MatterhookClient(mmc.WebhookUrl);

            var retVal = new MattermostMessage
            {
                Channel = mmc.Channel,
                Username = mmc.Username,
                IconUrl = new Uri(mmc.IconUrl)
            };

            return retVal;
        }


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