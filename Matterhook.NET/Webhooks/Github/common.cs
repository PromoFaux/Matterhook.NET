using System;
using System.Collections.Generic;
using Matterhook.NET.Code;
using Newtonsoft.Json;

namespace Matterhook.NET.Webhooks.Github
{
    public class Tree
    {
        public string sha { get; set; }
        public string url { get; set; }
    }

    public class Branch
    {
        public string name { get; set; }
        public Commit commit { get; set; }
    }


    public class Release
    {
        public string url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string html_url { get; set; }
        public int? id { get; set; }
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public object name { get; set; }
        public bool draft { get; set; }
        public Author author { get; set; }
        public bool prerelease { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime published_at { get; set; }
        public List<object> assets { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public object body { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string email { get; set; }
        public string username { get; set; }
    }

    public class Committer
    {
        public string name { get; set; }
        public string email { get; set; }
        public string username { get; set; }
    }

    public class Commit
    {
        public string html_url;
        public string id { get; set; }
        public string tree_id { get; set; }
        public bool distinct { get; set; }
        public string message { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime timestamp { get; set; }
        public string url { get; set; }
        public Author author { get; set; }
        public Committer committer { get; set; }
        public List<object> added { get; set; }
        public List<object> removed { get; set; }
        public List<string> modified { get; set; }
    }


    public class Pull_Request
    {
        public string url { get; set; }
        public int? id { get; set; }
        public string html_url { get; set; }
        public string diff_url { get; set; }
        public string patch_url { get; set; }
        public string issue_url { get; set; }
        public int? number { get; set; }
        public string state { get; set; }
        public bool locked { get; set; }
        public string title { get; set; }
        public User user { get; set; }
        public string body { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        public object closed_at { get; set; }
        public object merged_at { get; set; }
        public object merge_commit_sha { get; set; }
        public object assignee { get; set; }
        public object milestone { get; set; }
        public string commits_url { get; set; }
        public string review_comments_url { get; set; }
        public string review_comment_url { get; set; }
        public string comments_url { get; set; }
        public string statuses_url { get; set; }
        public Head head { get; set; }
        [JsonProperty(PropertyName = "base")]
        public Base _base { get; set; }
        public _Links _links { get; set; }
        public bool merged { get; set; }
        public object mergeable { get; set; }
        public string mergeable_state { get; set; }
        public object merged_by { get; set; }
        public int? comments { get; set; }
        public int? review_comments { get; set; }
        public int? commits { get; set; }
        public int? additions { get; set; }
        public int? deletions { get; set; }
        public int? changed_files { get; set; }
        public User asignee { get; set; }
    }


    public class Head
    {
        public string label { get; set; }
        [JsonProperty(PropertyName = "ref")]
        public string _ref { get; set; }
        public string sha { get; set; }
        public User user { get; set; }
        public Repository repo { get; set; }
    }

    public class Base
    {
        public string label { get; set; }
        [JsonProperty(PropertyName = "ref")]
        public string _ref { get; set; }
        public string sha { get; set; }
        public User user { get; set; }
        public Repository repo { get; set; }
    }

    public class Review
    {
        public int id { get; set; }
        public User user { get; set; }
        public string body { get; set; }
        public DateTime submitted_at { get; set; }
        public string state { get; set; }
        public string html_url { get; set; }
        public string pull_request_url { get; set; }
        public _Links _links { get; set; }
    }

    public class _Links
    {
        public Self self { get; set; }
        public Html html { get; set; }
        public Issue issue { get; set; }
        public Comments comments { get; set; }
        public Review_Comments review_comments { get; set; }
        public Review_Comment review_comment { get; set; }
        public Commits commits { get; set; }
        public Statuses statuses { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Html
    {
        public string href { get; set; }
    }

    public class Comments
    {
        public string href { get; set; }
    }

    public class Review_Comments
    {
        public string href { get; set; }
    }

    public class Review_Comment
    {
        public string href { get; set; }
    }

    public class Commits
    {
        public string href { get; set; }
    }

    public class Statuses
    {
        public string href { get; set; }
    }


    public class Project
    {
        public string owner_url { get; set; }
        public string url { get; set; }
        public string columns_url { get; set; }
        public int? id { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public int? number { get; set; }
        public string state { get; set; }
        public Creator creator { get; set; }
        public int? created_at { get; set; }
        public int? updated_at { get; set; }
    }


    public class Project_Column
    {
        public string url { get; set; }
        public string project_url { get; set; }
        public string cards_url { get; set; }
        public int? id { get; set; }
        public string name { get; set; }
        public int? created_at { get; set; }
        public int? updated_at { get; set; }
    }


    public class Build
    {
        public string url { get; set; }
        public string status { get; set; }
        public Error error { get; set; }
        public User pusher { get; set; }
        public string commit { get; set; }
        public int? duration { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
    }

    public class Error
    {
        public object message { get; set; }
    }

    public class Invitation
    {
        public int? id { get; set; }
        public string login { get; set; }
        public object email { get; set; }
        public string role { get; set; }
    }

    public class Membership
    {
        public string url { get; set; }
        public string state { get; set; }
        public string role { get; set; }
        public string organization_url { get; set; }
        public User user { get; set; }
    }


    public class Milestone
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string labels_url { get; set; }
        public int? id { get; set; }
        public int? number { get; set; }
        public string title { get; set; }
        public object description { get; set; }
        public Creator creator { get; set; }
        public int? open_issues { get; set; }
        public int? closed_issues { get; set; }
        public string state { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        public object due_on { get; set; }
        public object closed_at { get; set; }
    }


    public class Team
    {
        public string name { get; set; }
        public int? id { get; set; }
        public string slug { get; set; }
        public string permission { get; set; }
        public string url { get; set; }
        public string members_url { get; set; }
        public string repositories_url { get; set; }
    }

    public class Marketplace_Purchase
    {
        public User account { get; set; }
        public string billing_cycle { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime next_billing_date { get; set; }
        public int? unit_count { get; set; }
        public Plan plan { get; set; }
    }


    public class Plan
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int? monthly_price_in_cents { get; set; }
        public int? yearly_price_in_cents { get; set; }
        public string price_model { get; set; }
        public object unit_name { get; set; }
        public List<string> bullets { get; set; }
    }

    public class Previous_Marketplace_Purchase
    {
        public User account { get; set; }
        public string billing_cycle { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime next_billing_date { get; set; }
        public int? unit_count { get; set; }
        public Plan plan { get; set; }
    }


    public class Organization
    {
        public string login { get; set; }
        public int? id { get; set; }
        public string url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string hooks_url { get; set; }
        public string issues_url { get; set; }
        public string members_url { get; set; }
        public string public_members_url { get; set; }
        public string avatar_url { get; set; }
        public string description { get; set; }
    }


    public class Issue
    {
        public string url { get; set; }
        public string labels_url { get; set; }
        public string comments_url { get; set; }
        public string events_url { get; set; }
        public string html_url { get; set; }
        public int? id { get; set; }
        public int? number { get; set; }
        public string title { get; set; }
        public User user { get; set; }
        public List<Label> labels { get; set; }
        public string state { get; set; }
        public bool locked { get; set; }
        public User assignee { get; set; }
        public object milestone { get; set; }
        public int? comments { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        public object closed_at { get; set; }
        public string body { get; set; }
        public string href { get; set; }
    }


    public class Label
    {
        public string url { get; set; }
        public string name { get; set; }
        public string color { get; set; }
    }


    public class Repositories_Removed
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
    }


    public class Installation
    {
        public int? id { get; set; }
        public User account { get; set; }
        public string repository_selection { get; set; }
        public string access_tokens_url { get; set; }
        public string repositories_url { get; set; }
    }

    public class Page
    {
        public string page_name { get; set; }
        public string title { get; set; }
        public object summary { get; set; }
        public string action { get; set; }
        public string sha { get; set; }
        public string html_url { get; set; }
    }


    public class Forkee
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
        public User owner { get; set; }
        [JsonProperty(PropertyName = "private")]
        public bool _private { get; set; }
        public string html_url { get; set; }
        public string description { get; set; }
        public bool fork { get; set; }
        public string url { get; set; }
        public string forks_url { get; set; }
        public string keys_url { get; set; }
        public string collaborators_url { get; set; }
        public string teams_url { get; set; }
        public string hooks_url { get; set; }
        public string issue_events_url { get; set; }
        public string events_url { get; set; }
        public string assignees_url { get; set; }
        public string branches_url { get; set; }
        public string tags_url { get; set; }
        public string blobs_url { get; set; }
        public string git_tags_url { get; set; }
        public string git_refs_url { get; set; }
        public string trees_url { get; set; }
        public string statuses_url { get; set; }
        public string languages_url { get; set; }
        public string stargazers_url { get; set; }
        public string contributors_url { get; set; }
        public string subscribers_url { get; set; }
        public string subscription_url { get; set; }
        public string commits_url { get; set; }
        public string git_commits_url { get; set; }
        public string comments_url { get; set; }
        public string issue_comment_url { get; set; }
        public string contents_url { get; set; }
        public string compare_url { get; set; }
        public string merges_url { get; set; }
        public string archive_url { get; set; }
        public string downloads_url { get; set; }
        public string issues_url { get; set; }
        public string pulls_url { get; set; }
        public string milestones_url { get; set; }
        public string notifications_url { get; set; }
        public string labels_url { get; set; }
        public string releases_url { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime pushed_at { get; set; }
        public string git_url { get; set; }
        public string ssh_url { get; set; }
        public string clone_url { get; set; }
        public string svn_url { get; set; }
        public object homepage { get; set; }
        public int? size { get; set; }
        public int? stargazers_count { get; set; }
        public int? watchers_count { get; set; }
        public object language { get; set; }
        public bool has_issues { get; set; }
        public bool has_downloads { get; set; }
        public bool has_wiki { get; set; }
        public bool has_pages { get; set; }
        public int? forks_count { get; set; }
        public object mirror_url { get; set; }
        public int? open_issues_count { get; set; }
        public int? forks { get; set; }
        public int? open_issues { get; set; }
        public int? watchers { get; set; }
        public string default_branch { get; set; }
        [JsonProperty(PropertyName = "public")]
        public bool _public { get; set; }
    }


    public class Deployment_Status
    {
        public string url { get; set; }
        public int? id { get; set; }
        public string state { get; set; }
        public Creator creator { get; set; }
        public object description { get; set; }
        public object target_url { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        public string deployment_url { get; set; }
        public string repository_url { get; set; }
    }


    public class Deployment
    {
        public string url { get; set; }
        public int? id { get; set; }
        public string sha { get; set; }
        [JsonProperty(PropertyName = "ref")]
        public string _ref { get; set; }
        public string task { get; set; }
        public Payload payload { get; set; }
        public string environment { get; set; }
        public object description { get; set; }
        public Creator creator { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        public string statuses_url { get; set; }
        public string repository_url { get; set; }
    }

    public class Payload
    {
    }

    public class Creator
    {
        public string login { get; set; }
        public int? id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
    }


    public class Comment
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public int? id { get; set; }
        public User user { get; set; }
        public object position { get; set; }
        public object line { get; set; }
        public object path { get; set; }
        public string commit_id { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        public string body { get; set; }
    }

    public class User
    {
        public string login { get; set; }
        public int? id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public bool site_admin { get; set; }
        public string organization_billing_email { get; set; }

    }

    public class Repository
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
        public User owner { get; set; }
        [JsonProperty(PropertyName = "private")]
        public bool _private { get; set; }
        public string html_url { get; set; }
        public string description { get; set; }
        public bool fork { get; set; }
        public string url { get; set; }
        public string forks_url { get; set; }
        public string keys_url { get; set; }
        public string collaborators_url { get; set; }
        public string teams_url { get; set; }
        public string hooks_url { get; set; }
        public string issue_events_url { get; set; }
        public string events_url { get; set; }
        public string assignees_url { get; set; }
        public string branches_url { get; set; }
        public string tags_url { get; set; }
        public string blobs_url { get; set; }
        public string git_tags_url { get; set; }
        public string git_refs_url { get; set; }
        public string trees_url { get; set; }
        public string statuses_url { get; set; }
        public string languages_url { get; set; }
        public string stargazers_url { get; set; }
        public string contributors_url { get; set; }
        public string subscribers_url { get; set; }
        public string subscription_url { get; set; }
        public string commits_url { get; set; }
        public string git_commits_url { get; set; }
        public string comments_url { get; set; }
        public string issue_comment_url { get; set; }
        public string contents_url { get; set; }
        public string compare_url { get; set; }
        public string merges_url { get; set; }
        public string archive_url { get; set; }
        public string downloads_url { get; set; }
        public string issues_url { get; set; }
        public string pulls_url { get; set; }
        public string milestones_url { get; set; }
        public string notifications_url { get; set; }
        public string labels_url { get; set; }
        public string releases_url { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime created_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updated_at { get; set; }
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime pushed_at { get; set; }
        public string git_url { get; set; }
        public string ssh_url { get; set; }
        public string clone_url { get; set; }
        public string svn_url { get; set; }
        public object homepage { get; set; }
        public int? size { get; set; }
        public int? stargazers_count { get; set; }
        public int? watchers_count { get; set; }
        public object language { get; set; }
        public bool has_issues { get; set; }
        public bool has_downloads { get; set; }
        public bool has_wiki { get; set; }
        public bool has_pages { get; set; }
        public int? forks_count { get; set; }
        public object mirror_url { get; set; }
        public int? open_issues_count { get; set; }
        public int? forks { get; set; }
        public int? open_issues { get; set; }
        public int? watchers { get; set; }
        public string default_branch { get; set; }
        [JsonProperty(PropertyName = "public")]
        public bool _public { get; set; }

    }

}