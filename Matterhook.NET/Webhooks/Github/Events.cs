using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Matterhook.NET.Webhooks.Github
{
    public class Event
    {
    }

    public class CommitCommentEvent : Event //commit_comment
    {
        public string action { get; set; }
        public Comment comment { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class CreateEvent : Event //create
    {
        [JsonProperty(PropertyName = "ref")]
        public string _ref { get; set; }
        public string ref_type { get; set; }
        public string master_branch { get; set; }
        public string description { get; set; }
        public string pusher_type { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class DeleteEvent : Event //delete
    {
        [JsonProperty(PropertyName = "ref")]
        public string _ref { get; set; }
        public string ref_type { get; set; }
        public string pusher_type { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class DeploymentEvent : Event //deployment
    {
        public Deployment deployment { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class DeploymentStatusEvent : Event //deployment_status
    {
        public Deployment_Status deployment_status { get; set; }
        public Deployment deployment { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class ForkEvent : Event //fork
    {
        public Forkee forkee { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class GollumEvent : Event //gollum (wiki)
    {
        public List<Page> pages { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class InstallationEvent : Event //installation
    {
        public string action { get; set; }
        public Installation installation { get; set; }
        public Sender sender { get; set; }
    }


    public class InstallationRepositoriesEvent : Event //installation_repositories 
    {
        public string action { get; set; }
        public Installation installation { get; set; }
        public string repository_selection { get; set; }
        public List<object> repositories_added { get; set; }
        public List<Repositories_Removed> repositories_removed { get; set; }
        public Sender sender { get; set; }
    }


    public class IssueCommentEvent : Event //issue_comment
    {
        public string action { get; set; }
        public Issue issue { get; set; }
        public Comment comment { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class IssuesEvent : Event //issues
    {
        public string action { get; set; }
        public Issue issue { get; set; }
        public Label label { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
        public Assignee assignee { get; set; }
    }


    public class LabelEvent : Event //label
    {
        public string action { get; set; }
        public Label label { get; set; }
        public Repository repository { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
        
    }

    public class MarketplacePurchaseEvent : Event //marketplace_purchase
    {
        public string action { get; set; }
        public DateTime effective_date { get; set; }
        public Marketplace_Purchase marketplace_purchase { get; set; }
        public Previous_Marketplace_Purchase previous_marketplace_purchase { get; set; }
        public Sender sender { get; set; }
    }



    public class MemberEvent : Event //member
    {
        public string action { get; set; }
        public Member member { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class MembershipEvent : Event //membership
    {
        public string action { get; set; }
        public string scope { get; set; }
        public Member member { get; set; }
        public Sender sender { get; set; }
        public Team team { get; set; }
        public Organization organization { get; set; }
    }




    public class MilestoneEvent : Event //milestone
    {
        public string action { get; set; }
        public Milestone milestone { get; set; }
        public Repository repository { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }

    public class OrganizationEvent : Event //organization
    {
        public string action { get; set; }
        public Invitation invitation { get; set; }
        public Membership membership { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }


    public class OrgBlockEvent : Event //org_block
    {
        public string action { get; set; }
        public Blocked_User blocked_user { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }


    public class PageBuildEvent : Event //page_build
    {
        public int id { get; set; }
        public Build build { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }



    public class ProjectCardEvent : Event //project_card
    {
        public string action { get; set; }
        public Project_Card project_card { get; set; }
        public Repository repository { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }

    public class Project_Card
    {
        public string url { get; set; }
        public string column_url { get; set; }
        public int column_id { get; set; }
        public int id { get; set; }
        public object note { get; set; }
        public Creator creator { get; set; }
        public int created_at { get; set; }
        public int updated_at { get; set; }
        public string content_url { get; set; }
    }


    public class ProjectColumnEvent : Event //project_column
    {
        public string action { get; set; }
        public Project_Column project_column { get; set; }
        public Repository repository { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }


    public class ProjectEvent : Event //project
    {
        public string action { get; set; }
        public Project project { get; set; }
        public Repository repository { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }



    public class PublicEvent : Event //public
    {
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }



    public class PullRequestEvent : Event //pull_request
    {
        public string action { get; set; }
        public int number { get; set; }
        public Pull_Request pull_request { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
        public Installation installation { get; set; }
        public Label label { get; set; }
        public string requested_reviewers { get; set; }
        public Assignee assignee { get; set; }

    }


    public class PullRequestReviewEvent : Event //pull_request_review
    {
        public string action { get; set; }
        public Review review { get; set; }
        public Pull_Request pull_request { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }





    public class PullRequestReviewCommentEvent : Event //pull_request_review_comment
    {
        public string action { get; set; }
        public Comment comment { get; set; }
        public Pull_Request pull_request { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }



    public class PushEvent : Event //push
    {
        [JsonProperty(PropertyName = "ref")]
        public string _ref { get; set; }
        public string before { get; set; }
        public string after { get; set; }
        public bool created { get; set; }
        public bool deleted { get; set; }
        public bool forced { get; set; }
        public object base_ref { get; set; }
        public string compare { get; set; }
        public List<Commit> commits { get; set; }
        public Head_Commit head_commit { get; set; }
        public Repository repository { get; set; }
        public Pusher pusher { get; set; }
        public Sender sender { get; set; }
    }



    public class ReleaseEvent : Event //release
    {
        public string action { get; set; }
        public Release release { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }

    public class RepositoryEvent : Event //repository
    {
        public string action { get; set; }
        public Repository repository { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }



    public class StatusEvent : Event //status
    {
        public int id { get; set; }
        public string sha { get; set; }
        public string name { get; set; }
        public object target_url { get; set; }
        public string context { get; set; }
        public object description { get; set; }
        public string state { get; set; }
        public Commit commit { get; set; }
        public List<Branch> branches { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


    public class TeamEvent : Event //team
    {
        public string action { get; set; }
        public Team team { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }



    public class TeamAddEvent : Event //team_add
    {
        public Team team { get; set; }
        public Repository repository { get; set; }
        public Organization organization { get; set; }
        public Sender sender { get; set; }
    }



    public class WatchEvent : Event //watch
    {
        public string action { get; set; }
        public Repository repository { get; set; }
        public Sender sender { get; set; }
    }


}