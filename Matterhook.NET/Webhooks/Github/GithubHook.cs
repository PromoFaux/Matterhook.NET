using System;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Matterhook.NET.Webhooks.Github
{
    public class GithubHook
    {
        

      public GithubHook(StringValues strEvent, StringValues signature, StringValues delivery, string payloadText)
        {
            try
            {
                Event = strEvent;
                Signature = signature;
                Delivery = delivery;
                PayloadString = payloadText;

                switch (Event)
                {
                    case "pull_request":
                        Payload = JsonConvert.DeserializeObject<PullRequestEvent>(PayloadString);
                        break;
                    case "issues":
                        Payload = JsonConvert.DeserializeObject<IssuesEvent>(PayloadString);
                        break;
                    case "issue_comment":
                        Payload = JsonConvert.DeserializeObject<IssueCommentEvent>(PayloadString);
                        break;
                    case "repository":
                        Payload = JsonConvert.DeserializeObject<RepositoryEvent>(PayloadString);
                        break;
                    case "create":
                        Payload = JsonConvert.DeserializeObject<CreateEvent>(PayloadString);
                        break;
                    case "delete":
                        Payload = JsonConvert.DeserializeObject<DeleteEvent>(PayloadString);
                        break;
                    case "pull_request_review":
                        Payload = JsonConvert.DeserializeObject<PullRequestReviewEvent>(PayloadString);
                        break;
                    case "pull_request_review_comment":
                        Payload = JsonConvert.DeserializeObject<PullRequestReviewCommentEvent>(PayloadString);
                        break;
                    case "push":
                        Payload = JsonConvert.DeserializeObject<PushEvent>(PayloadString);
                        break;
                    case "commit_comment":
                        Payload = JsonConvert.DeserializeObject<CommitCommentEvent>(PayloadString);
                        break;
                    case "status":
                        Payload = JsonConvert.DeserializeObject<StatusEvent>(PayloadString);
                        break;
                    default:
                        throw new Exception($"Unhandled Event Type: {Event}");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Problem Converting payload: {e.Message}");
            }
           
        }

        public string Event { get; set; }
        public string Signature { get; set; }
        public string Delivery { get; set; }

        public string PayloadString { get; set; }
        public string CalcSignature { get; set; }
        public Event Payload { get; set; }

        

    }
}
