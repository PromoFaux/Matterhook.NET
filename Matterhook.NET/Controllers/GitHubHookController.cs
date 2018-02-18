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
using GithubWebhook;
using Matterhook.NET.Code;
using Matterhook.NET.MatterhookClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using GithubWebhook.Events;



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

                var hook = new GhWebhook(Request);

                if (hook.SignatureValid(_config.Secret))
                {
                    HttpResponseMessage response = null;
                    MattermostMessage message = null;
                    switch (hook.Event)
                    {
                        case PullRequestEvent.EventString:
                            message = GetMessagePullRequest((PullRequestEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case IssuesEvent.EventString:
                            message = GetMessageIssues((IssuesEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case IssueCommentEvent.EventString:
                            message = GetMessageIssueComment((IssueCommentEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case RepositoryEvent.EventString:
                            message = GetMessageRepository((RepositoryEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case CreateEvent.EventString:
                            message = GetMessageCreate((CreateEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case DeleteEvent.EventString:
                            message = GetMessageDelete((DeleteEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case PullRequestReviewEvent.EventString:
                            message = GetMessagePullRequestReview((PullRequestReviewEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case PullRequestReviewCommentEvent.EventString:
                            message = GetMessagePullRequestReviewComment(
                                (PullRequestReviewCommentEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case PushEvent.EventString:
                            message = GetMessagePush((PushEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case CommitCommentEvent.EventString:
                            message = GetMessageCommitComment((CommitCommentEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case StatusEvent.EventString:
                            message = GetMessageStatus((StatusEvent)hook.PayloadObject);
                            response = await _matterHook.PostAsync(message);
                            break;
                        case PingEvent.EventString:
                            return StatusCode(200, "Pong!");
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
                stuffToLog.Add($"Calculated: {hook.GetExpectedSignature(_config.Secret)}");
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
            var retVal = BaseMessageForRepo(payload.Repository.FullName);

            var filter = GetRepoFilters(payload.Repository.FullName);

            if (!filter.Status.WebhookEnabled) throw new WarningException("Status Webhooks disabled by Matterhook Config");

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var commitMd = $"[`{payload.Sha.Substring(0, 7)}`]({payload.Commit.HtmlUrl})";
            var contextMd = $"[`{payload.Context}`]({payload.TargetUrl})";

            string stateEmoji;

            switch (payload.State)
            {
                case "success":


                    if (!filter.Status.Success.WebhookEnabled) throw new WarningException("Success statuses ignored by Matterhook config");

                    if (filter.Status.Success.IgnoredProviders != null && filter.Status.Success.IgnoredProviders.Contains(payload.Context)) throw new WarningException($"Success statuses from {payload.Context} ignored by Matterhook Config");

                    stateEmoji = ":white_check_mark:";
                    break;
                case "pending":
                    if (!filter.Status.Pending.WebhookEnabled) throw new WarningException("Pending statuses ignored by Matterhook config");
                    if (filter.Status.Pending.IgnoredProviders != null && filter.Status.Pending.IgnoredProviders.Contains(payload.Context)) throw new WarningException($"Pending statuses from {payload.Context} ignored by Matterhook Config");

                    stateEmoji = ":question:";
                    break;
                default:
                    if (!filter.Status.Failed.WebhookEnabled) throw new WarningException("Failed statuses ignored by Matterhook config");
                    if (filter.Status.Failed.IgnoredProviders != null && filter.Status.Failed.IgnoredProviders.Contains(payload.Context)) throw new WarningException($"Failed statuses from {payload.Context} ignored by Matterhook Config");

                    stateEmoji = ":x:";
                    break;
            }

            retVal.Text = $"New Status Message from {contextMd} on commit {commitMd} in {repoMd}\n>{stateEmoji} - {payload.Description}";

            return retVal;
        }

        private static MattermostMessage GetMessageCommitComment(CommitCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);
            MattermostAttachment att = null;

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var commitMd = $"[`{payload.Comment.CommitId.Substring(0, 7)}`]({payload.Comment.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";

            if (payload.Action == "created")
            {
                retVal.Text = $"{userMd} commented on {commitMd} in {repoMd}";
                att = new MattermostAttachment
                {
                    Text = payload.Comment.Body
                };
            }
            else
            {
                throw new NotImplementedException($"Unhandled Event action: {payload.Action}");
            }

            retVal.Attachments = new List<MattermostAttachment>
            {
                att
            };

            return retVal;
        }

        private static MattermostMessage GetMessagePush(PushEvent payload)
        {
            if (payload.Deleted != null && payload.Forced != null && !(bool)payload.Deleted && !(bool)payload.Forced)
                if (!payload.Ref.StartsWith("refs/tags/"))
                {
                    var retVal = BaseMessageForRepo(payload.Repository.FullName);
                    MattermostAttachment att = null;


                    if (payload.Commits.Length > 0)
                    {
                        var multi = payload.Commits.Length > 1 ? "s" : "";
                        var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";
                        var branch = payload.Ref.Replace("refs/heads/", "");
                        var branchMd = $"[{branch}]({payload.Repository.HtmlUrl}/tree/{branch})";
                        var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
                        retVal.Text =
                            $"{userMd} pushed {payload.Commits.Length} commit{multi} to {branchMd} on {repoMd}";

                        att = new MattermostAttachment();

                        foreach (var commit in payload.Commits)
                        {

                            var sanitised = Regex.Replace(commit.Message, @"\r\n?|\n", " ");
                            att.Text +=
                                $"- [`{commit.Id.Substring(0, 8)}`]({commit.Url}) - {sanitised}\n";
                        }
                    }
                    else
                    {
                        throw new WarningException("No commits in payload, no need to send message.");
                    }

                    retVal.Attachments = new List<MattermostAttachment>
                    {
                        att
                    };

                    return retVal;
                }

            throw new NotImplementedException($"Unhandled Push Type: {payload.Ref}");
        }

        private static MattermostMessage GetMessageDelete(DeleteEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";

            switch (payload.RefType)
            {
                case "branch":
                    retVal.Text = $"{userMd} deleted branch `{payload.Ref}` from {repoMd}";
                    break;
                case "tag":
                    retVal.Text = $"{userMd} deleted tag `{payload.Ref}` from {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.RefType}");
            }

            return retVal;
        }


        private static MattermostMessage GetMessageCreate(CreateEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";

            switch (payload.RefType)
            {
                case "branch":
                    retVal.Text = $"{userMd} added branch `{payload.Ref}` to {repoMd}";
                    break;
                case "tag":
                    retVal.Text = $"{userMd} added tag `{payload.Ref}` to {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.RefType}");
            }

            return retVal;
        }

        private static MattermostMessage GetMessageRepository(RepositoryEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);

            var repoMd = $"[{payload.Repository.Name}]({payload.Repository.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";
            var orgMd = $"[{payload.Organization.Login}]({payload.Organization.Url})";
            switch (payload.Action)
            {
                case "created":
                    retVal.Text = $"{userMd} created new repository {repoMd} in {orgMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.Action}");
            }

            return retVal;
        }

        private static MattermostMessage GetMessageIssueComment(IssueCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);
            MattermostAttachment att = null;
            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var titleMd = $"[#{payload.Issue.Number} {payload.Issue.Title}]({payload.Issue.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";
            if (payload.Action == "created")
            {
                retVal.Text = $"{userMd} commented on issue {titleMd} in {repoMd}";
                if (!string.IsNullOrEmpty(payload.Comment.Body))
                    att = new MattermostAttachment
                    {
                        Text = payload.Comment.Body
                    };
            }
            else
            {
                throw new NotImplementedException($"Unhandled Event action: {payload.Action}");
            }

            if (att != null)
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };

            return retVal;
        }

        private static MattermostMessage GetMessageIssues(IssuesEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);

            MattermostAttachment att = null;

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var titleMd = $"[#{payload.Issue.Number} {payload.Issue.Title}]({payload.Issue.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";
            switch (payload.Action)
            {
                case "opened":
                    retVal.Text = $"{userMd} opened a [new issue]({payload.Issue.HtmlUrl}) in {repoMd}";
                    if (!string.IsNullOrEmpty(payload.Issue.Body))
                        att = new MattermostAttachment
                        {
                            Title = $"#{payload.Issue.Number} {payload.Issue.Title}",
                            TitleLink = new Uri(payload.Issue.HtmlUrl),
                            Text = payload.Issue.Body
                        };
                    break;
                case "closed":
                    retVal.Text = $"{userMd} closed issue {titleMd} in {repoMd}";
                    break;
                case "reopened":
                    retVal.Text = $"{userMd} reopened issue {titleMd} in {repoMd}";
                    break;
                case "labeled":
                    //retVal.Text = $"{userMd} added label: `{payload.Issue.Labels[0].Name}` to {titleMd} in {repoMd}";
                    break;
                case "unlabeled":
                    //retVal.Text = $"{userMd} removed label: `{payload.Label.Name}` from {titleMd} in {repoMd}";
                    break;
                case "assigned":
                    var asignMd = $"[{payload.Issue.Assignee.Login}]({payload.Issue.Assignee.HtmlUrl})";
                    retVal.Text = $"{userMd} assigned {asignMd} to {titleMd} in {repoMd}";
                    break;
                case "unassigned":
                    var unasignMd = $"[{payload.Issue.Assignee.Login}]({payload.Issue.Assignee.HtmlUrl})";
                    retVal.Text = $"{userMd} unassigned {unasignMd} from {titleMd} in {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.Action}");
            }
            if (att != null)
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };

            return retVal;
        }


        private static MattermostMessage GetMessagePullRequestReview(PullRequestReviewEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);
            MattermostAttachment att = null;

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var titleMd =
                $"[#{payload.PullRequest.Number} {payload.PullRequest.Title}]({payload.PullRequest.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";
            var reviewerMd = $"[{payload.Review.User.Login}]({payload.Review.User.HtmlUrl})";

            switch (payload.Action)
            {
                case "submitted":
                    switch (payload.Review.State)
                    {
                        //case "commented":
                        //retVal.Text = $"{userMd} created a [review]({payload.Review.HtmlUrl}) on {titleMd} in {repoMd}";
                        //break;
                        case "approved":
                            retVal.Text = $"{userMd} [approved]({payload.Review.HtmlUrl}) {titleMd} in {repoMd}";
                            break;
                        case "changes_requested":
                            retVal.Text =
                                $"{userMd} [requested changes]({payload.Review.HtmlUrl}) on {titleMd} in {repoMd}";
                            break;
                        default:
                            retVal.Text =
                                $"{userMd} submitted a [review]({payload.Review.HtmlUrl}) on {titleMd} in {repoMd}";
                            break;
                    }

                    if (!string.IsNullOrEmpty(payload.Review.Body))
                        att = new MattermostAttachment
                        {
                            Text = payload.Review.Body
                        };
                    break;
                case "dismissed":
                    retVal.Text =
                        $"A [review]({payload.Review.HtmlUrl}) by {reviewerMd} was dismissed on {titleMd} in {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.Action}");
            }

            if (att != null)
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };

            return retVal;
        }


        private static MattermostMessage GetMessagePullRequestReviewComment(PullRequestReviewCommentEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);
            MattermostAttachment att = null;

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var commitMd = $"[`{payload.Comment.CommitId.Substring(0, 7)}`]({payload.Comment.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";
            var titleMd =
                $"[#{payload.PullRequest.Number} {payload.PullRequest.Title}]({payload.PullRequest.HtmlUrl})";

            switch (payload.Action)
            {
                case "created":
                    retVal.Text = $"{userMd} commented on {titleMd}:{commitMd}  in {repoMd}";
                    att = new MattermostAttachment
                    {
                        Text = payload.Comment.Body
                    };

                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.Action}");
            }

            retVal.Attachments = new List<MattermostAttachment>
            {
                att
            };

            return retVal;
        }

        private static MattermostMessage GetMessagePullRequest(PullRequestEvent payload)
        {
            var retVal = BaseMessageForRepo(payload.Repository.FullName);
            MattermostAttachment att = null;

            var repoMd = $"[{payload.Repository.FullName}]({payload.Repository.HtmlUrl})";
            var titleMd =
                $"[#{payload.PullRequest.Number} {payload.PullRequest.Title}]({payload.PullRequest.HtmlUrl})";
            var userMd = $"[{payload.Sender.Login}]({payload.Sender.HtmlUrl})";
            switch (payload.Action)
            {
                case "opened":
                    retVal.Text = $"{userMd} opened a [new pull request]({payload.PullRequest.HtmlUrl}) in {repoMd}";

                    if (!string.IsNullOrEmpty(payload.PullRequest.Body))
                        att = new MattermostAttachment
                        {
                            Title = $"#{payload.PullRequest.Number} {payload.PullRequest.Title}",
                            TitleLink = new Uri(payload.PullRequest.HtmlUrl),
                            Text = payload.PullRequest.Body
                        };
                    break;
                case "labeled":
                    //retVal.Text = $"{userMd} added label: `{payload.Label.Name}` to {titleMd} in {repoMd}";
                    break;
                case "unlabeled":
                    //retVal.Text = $"{userMd} removed label: `{payload.Label.Name}` from {titleMd} in {repoMd}";
                    break;
                case "closed":
                    retVal.Text = $"{userMd} closed pull request {titleMd} in {repoMd}";
                    break;
                case "assigned":
                    var asignMd = $"[{payload.PullRequest.Assignee.Login}]({payload.PullRequest.Assignee.HtmlUrl})";
                    retVal.Text = $"{userMd} assigned {asignMd} to {titleMd} in {repoMd}";
                    break;
                case "unassigned":
                    var unasignMd = $"[{payload.PullRequest.Assignee.Login}]({payload.PullRequest.Assignee.HtmlUrl})";
                    retVal.Text = $"{userMd} unassigned {unasignMd} from {titleMd} in {repoMd}";
                    break;
                default:
                    throw new NotImplementedException($"Unhandled Event action: {payload.Action}");
            }

            if (att != null)
                retVal.Attachments = new List<MattermostAttachment>
                {
                    att
                };

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
