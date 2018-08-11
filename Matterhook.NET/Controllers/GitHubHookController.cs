using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Matterhook.NET.Code;
using Matterhook.NET.MatterhookClient;
using Matterhook.NET.Webhooks.Github;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Matterhook.NET.Controllers
{
    [Route("[Controller]")]
    public class GithubHookController : Controller
    {
        private static GithubConfig _config;
        private static MatterhookClient.MatterhookClient _matterHook;

        public GithubHookController(IOptions<Config> config)
        {
            try
            {
                _config = config.Value.GithubConfig;

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

                //Generate GithubHook Object
                //Generate DiscourseHook object for easier reading

                stuffToLog.Add($"Github Hook received: {DateTime.Now}");

                Request.Headers.TryGetValue("X-GitHub-Event", out var strEvent);
                Request.Headers.TryGetValue("X-Hub-Signature", out var signature);
                Request.Headers.TryGetValue("X-GitHub-Delivery", out var delivery);
                Request.Headers.TryGetValue("Content-type", out var content);

                stuffToLog.Add($"Hook Id: {delivery}");
                stuffToLog.Add($"X-Github-Event: {strEvent}");

                if (content != "application/json")
                {
                    const string error = "Invalid content type. Expected application/json";
                    stuffToLog.Add(error);
                    Util.LogList(stuffToLog);
                    return StatusCode(400, error);

                }

                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadText = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                if (_config.DebugSavePayloads)
                {
                    System.IO.File.WriteAllText($"/config/payloads/{delivery}", $"Signature: {signature}\n");
                    System.IO.File.AppendAllText($"/config/payloads/{delivery}", $"Event: {strEvent}\nPayload:\n");
                    System.IO.File.AppendAllText($"/config/payloads/{delivery}", payloadText);
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
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "issues":
                            message = GetMessageIssues((IssuesEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "issue_comment":
                            message = GetMessageIssueComment((IssueCommentEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "repository":
                            message = GetMessageRepository((RepositoryEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "create":
                            message = GetMessageCreate((CreateEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "delete":
                            message = GetMessageDelete((DeleteEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "pull_request_review":
                            message = GetMessagePullRequestReview((PullRequestReviewEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "pull_request_review_comment":
                            message = GetMessagePullRequestReviewComment(
                                (PullRequestReviewCommentEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "push":
                            message = GetMessagePush((PushEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "commit_comment":
                            message = GetMessageCommitComment((CommitCommentEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                        case "status":
                            message = GetMessageStatus((StatusEvent)githubHook.Payload);
                            response = await _matterHook.PostAsync(message, _config.DefaultMattermostConfig.MessageLength);
                            break;
                    }

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
                        if (message != null) stuffToLog.Add(message.Text);
                        stuffToLog.Add("Succesfully posted to Mattermost");
                        Util.LogList(stuffToLog);
                    }

                    return StatusCode(200, "Succesfully posted to Mattermost");
                }

                stuffToLog.Add("Invalid Signature!");
                stuffToLog.Add($"Expected: {signature}");
                stuffToLog.Add($"Calculated: {calcSig}");
                Util.LogList(stuffToLog);
                return StatusCode(401, "Invalid signature. Please check your secret values in the config and on Github.");
            }
            catch (Exception e)
            {
                stuffToLog.Add(e.Message);
                if (!(e is WarningException) && !(e is NotImplementedException))
                {
                    Util.LogList(stuffToLog);
                }
                return StatusCode(e is NotImplementedException ? 501 : e is WarningException ? 202 : 500, e.Message);
            }
        }

        private static MattermostMessage GetMessageStatus(StatusEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            var filter = GetRepoFilters(payload.repository.full_name);

            if (!filter.Status.WebhookEnabled) throw new WarningException("Status Webhooks disabled by Matterhook Config");

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var commitMd = $"[`{payload.sha.Substring(0, 7)}`]({payload.commit.html_url})";
            var contextMd = $"[`{payload.context}`]({payload.target_url})";

            string stateEmoji;

            switch (payload.state)
            {
                case "success":

                    if (!filter.Status.Success.WebhookEnabled) throw new WarningException("Success statuses ignored by Matterhook config");

                    if (filter.Status.Success.IgnoredProviders != null && filter.Status.Success.IgnoredProviders.Contains(payload.context)) throw new WarningException($"Success statuses from {payload.context} ignored by Matterhook Config");

                    stateEmoji = ":white_check_mark:";
                    break;
                case "pending":
                    if (!filter.Status.Pending.WebhookEnabled) throw new WarningException("Pending statuses ignored by Matterhook config");
                    if (filter.Status.Pending.IgnoredProviders != null && filter.Status.Pending.IgnoredProviders.Contains(payload.context)) throw new WarningException($"Pending statuses from {payload.context} ignored by Matterhook Config");

                    stateEmoji = ":question:";
                    break;
                default:
                    if (!filter.Status.Failed.WebhookEnabled) throw new WarningException("Failed statuses ignored by Matterhook config");
                    if (filter.Status.Failed.IgnoredProviders != null && filter.Status.Failed.IgnoredProviders.Contains(payload.context)) throw new WarningException($"Failed statuses from {payload.context} ignored by Matterhook Config");

                    stateEmoji = ":x:";
                    break;
            }

            retVal.Text = $"New Status Message from {contextMd} on commit {commitMd} in {repoMd}\n>{stateEmoji} - {payload.description}";

            return retVal;
        }

        private static MattermostMessage GetMessageCommitComment(CommitCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var commitMd = $"[`{payload.comment.commit_id.Substring(0, 7)}`]({payload.comment.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";

            if (payload.action == "created")
            {
                retVal.Text = $"{userMd} commented on {commitMd} in {repoMd}";
                retVal.Text += $"\n\n{payload.comment.body}";
            }
            else
            {
                throw new NotImplementedException($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private static MattermostMessage GetMessagePush(PushEvent payload)
        {
            if (!payload.deleted && !payload.forced)
                if (!payload._ref.StartsWith("refs/tags/"))
                {
                    var retVal = BaseMessageForRepo(payload.repository.full_name);

                    if (payload.commits.Count > 0)
                    {
                        var multi = payload.commits.Count > 1 ? "s" : "";
                        var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
                        var branch = payload._ref.Replace("refs/heads/", "");
                        var branchMd = $"[{branch}]({payload.repository.html_url}/tree/{branch})";
                        var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
                        retVal.Text =
                            $"{userMd} pushed {payload.commits.Count} commit{multi} to {branchMd} on {repoMd}\n\n";

                        foreach (var commit in payload.commits)
                        {
                            var sanitised = Regex.Replace(commit.message, @"\r\n?|\n", " ");
                            retVal.Text +=
                                $"- [`{commit.id.Substring(0, 8)}`]({commit.url}) - {sanitised}\n";
                        }
                    }
                    else
                    {
                        throw new WarningException("No commits in payload, no need to send message.");
                    }

                    return retVal;
                }

            throw new NotImplementedException($"Unhandled Push Type: {payload._ref}");
        }

        private static MattermostMessage GetMessageDelete(DeleteEvent payload)
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
                    throw new NotImplementedException($"Unhandled Event action: {payload.ref_type}");
            }

            return retVal;
        }


        private static MattermostMessage GetMessageCreate(CreateEvent payload)
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
                    throw new NotImplementedException($"Unhandled Event action: {payload.ref_type}");
            }

            return retVal;
        }

        private static MattermostMessage GetMessageRepository(RepositoryEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            var repoMd = $"[{payload.repository.name}]({payload.repository.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            var orgMd = $"[{payload.organization.login}]({payload.organization.url})";
            switch (payload.action)
            {
                case "created":
                    retVal.Text = $"{userMd} created new repository {repoMd} in {orgMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private static MattermostMessage GetMessageIssueComment(IssueCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);
            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var titleMd = $"[#{payload.issue.number} {payload.issue.title}]({payload.issue.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            if (payload.action == "created")
            {
                retVal.Text = $"{userMd} commented on issue {titleMd} in {repoMd}";
                if (!string.IsNullOrEmpty(payload.comment.body))
                    retVal.Text += $"\n\n{payload.comment.body}";
            }
            else
            {
                throw new NotImplementedException($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private static MattermostMessage GetMessageIssues(IssuesEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var titleMd = $"[#{payload.issue.number} {payload.issue.title}]({payload.issue.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            switch (payload.action)
            {
                case "opened":
                    retVal.Text = $"{userMd} opened a new issue {titleMd} in {repoMd}";
                    if (!string.IsNullOrEmpty(payload.issue.body))
                        retVal.Text += $"\n\n{payload.issue.body}";
                    break;
                case "closed":
                    retVal.Text = $"{userMd} closed issue {titleMd} in {repoMd}";
                    break;
                case "reopened":
                    retVal.Text = $"{userMd} reopened issue {titleMd} in {repoMd}";
                    break;
                //case "labeled":
                //    retVal.Text = $"{userMd} added label: `{payload.label.name}` to {titleMd} in {repoMd}";
                //    break;
                //case "unlabeled":
                //    retVal.Text = $"{userMd} removed label: `{payload.label.name}` from {titleMd} in {repoMd}";
                //    break;
                case "assigned":
                    var asignMd = $"[{payload.assignee.login}]({payload.assignee.html_url})";
                    retVal.Text = $"{userMd} assigned {asignMd} to {titleMd} in {repoMd}";
                    break;
                case "unassigned":
                    var unasignMd = $"[{payload.assignee.login}]({payload.assignee.html_url})";
                    retVal.Text = $"{userMd} unassigned {unasignMd} from {titleMd} in {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.action}");
            }
            return retVal;
        }


        private static MattermostMessage GetMessagePullRequestReview(PullRequestReviewEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);
            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var titleMd =
                $"[#{payload.pull_request.number} {payload.pull_request.title}]({payload.pull_request.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            var reviewerMd = $"[{payload.review.user.login}]({payload.review.user.html_url})";

            switch (payload.action)
            {
                case "submitted":
                    switch (payload.review.state)
                    {
                        //case "commented":
                        //retVal.Text = $"{userMd} created a [review]({payload.review.html_url}) on {titleMd} in {repoMd}";
                        //break;
                        case "approved":
                            retVal.Text = $"{userMd} [approved]({payload.review.html_url}) {titleMd} in {repoMd}";
                            break;
                        case "changes_requested":
                            retVal.Text =
                                $"{userMd} [requested changes]({payload.review.html_url}) on {titleMd} in {repoMd}";
                            break;
                        default:
                            retVal.Text =
                                $"{userMd} submitted a [review]({payload.review.html_url}) on {titleMd} in {repoMd}";
                            break;
                    }

                    if (!string.IsNullOrEmpty(payload.review.body))
                        retVal.Text += $"\n\n{payload.review.body}";

                    break;
                case "dismissed":
                    retVal.Text =
                        $"A [review]({payload.review.html_url}) by {reviewerMd} was dismissed on {titleMd} in {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }


        private static MattermostMessage GetMessagePullRequestReviewComment(PullRequestReviewCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var commitMd = $"[`{payload.comment.commit_id.Substring(0, 7)}`]({payload.comment.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            var titleMd =
                $"[#{payload.pull_request.number} {payload.pull_request.title}]({payload.pull_request.html_url})";

            switch (payload.action)
            {
                case "created":
                    retVal.Text = $"{userMd} commented on {titleMd}:{commitMd}  in {repoMd}";
                    retVal.Text = $"\n\n{payload.comment.body}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }

        private static MattermostMessage GetMessagePullRequest(PullRequestEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.repository.full_name);

            var repoMd = $"[{payload.repository.full_name}]({payload.repository.html_url})";
            var titleMd =
                $"[#{payload.pull_request.number} {payload.pull_request.title}]({payload.pull_request.html_url})";
            var userMd = $"[{payload.sender.login}]({payload.sender.html_url})";
            switch (payload.action)
            {
                case "opened":
                    retVal.Text = $"{userMd} opened pull request {titleMd} in {repoMd}";
                    retVal.Text += $"\n\n{payload.pull_request.body}";
                    break;
                //case "labeled":
                //    retVal.Text = $"{userMd} added label: `{payload.label.name}` to {titleMd} in {repoMd}";
                //    break;
                //case "unlabeled":
                //    retVal.Text = $"{userMd} removed label: `{payload.label.name}` from {titleMd} in {repoMd}";
                //    break;
                case "closed":
                    retVal.Text = $"{userMd} closed pull request {titleMd} in {repoMd}";
                    break;
                case "assigned":
                    var asignMd = $"[{payload.assignee.login}]({payload.assignee.html_url})";
                    retVal.Text = $"{userMd} assigned {asignMd} to {titleMd} in {repoMd}";
                    break;
                case "unassigned":
                    var unasignMd = $"[{payload.assignee.login}]({payload.assignee.html_url})";
                    retVal.Text = $"{userMd} unassigned {unasignMd} from {titleMd} in {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.action}");
            }

            return retVal;
        }


        private static MattermostMessage BaseMessageForRepo(string repoName)
        {
            var mmc = Util.GetMattermostDetails(_config.DefaultMattermostConfig, _config.RepoList, repoName);
            //set matterHook Client to correct webhook.
            _matterHook = new MatterhookClient.MatterhookClient(mmc.WebhookUrl);

            var retVal = new MattermostMessage
            {
                Channel = mmc.Channel,
                Username = mmc.Username,
                IconUrl = mmc.IconUrl != null ? new Uri(mmc.IconUrl) : null
            };

            return retVal;
        }


        private static Filters GetRepoFilters(string repoName)
        {
            var repo = _config.RepoList.FirstOrDefault(x => x.RepoName == repoName);

            return repo.Filters == null ? new Filters() : repo.Filters;
        }
    }
}
