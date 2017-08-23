﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matterhook.NET.Webhooks.Discourse
{
    public class Topic
    {
        public int id { get; set; }
        public string title { get; set; }
        public string fancy_title { get; set; }
        public int posts_count { get; set; }
        public DateTime created_at { get; set; }
        public int views { get; set; }
        public int reply_count { get; set; }
        public int participant_count { get; set; }
        public int like_count { get; set; }
        public DateTime last_posted_at { get; set; }
        public bool visible { get; set; }
        public bool closed { get; set; }
        public bool archived { get; set; }
        public bool has_summary { get; set; }
        public string archetype { get; set; }
        public string slug { get; set; }
        public object category_id { get; set; }
        public int word_count { get; set; }
        public object deleted_at { get; set; }
        public int pending_posts_count { get; set; }
        public int user_id { get; set; }
        public bool pm_with_non_human_user { get; set; }
        public object draft { get; set; }
        public string draft_key { get; set; }
        public int draft_sequence { get; set; }
        public object unpinned { get; set; }
        public bool pinned_globally { get; set; }
        public bool pinned { get; set; }
        public object pinned_at { get; set; }
        public object pinned_until { get; set; }
        public Details details { get; set; }
        public int highest_post_number { get; set; }
        public object deleted_by { get; set; }
        public bool has_deleted { get; set; }
        public Actions_Summary[] actions_summary { get; set; }
        public int chunk_size { get; set; }
        public object bookmarked { get; set; }
        public bool message_archived { get; set; }
        public object[] tags { get; set; }
        public object featured_link { get; set; }
        public object topic_timer { get; set; }
        public string unicode_title { get; set; }
        public int message_bus_last_id { get; set; }
        public bool can_vote { get; set; }
        public object vote_count { get; set; }
        public bool user_voted { get; set; }
    }

    public class Details
    {
        public Created_By created_by { get; set; }
        public Last_Poster last_poster { get; set; }
        public object[] allowed_groups { get; set; }
        public Allowed_Users[] allowed_users { get; set; }
        public Participant[] participants { get; set; }
        public Suggested_Topics[] suggested_topics { get; set; }
        public Link[] links { get; set; }
        public int notification_level { get; set; }
        public bool can_move_posts { get; set; }
        public bool can_edit { get; set; }
        public bool can_delete { get; set; }
        public bool can_remove_allowed_users { get; set; }
        public bool can_invite_to { get; set; }
        public bool can_invite_via_email { get; set; }
        public bool can_create_post { get; set; }
        public bool can_reply_as_new_topic { get; set; }
        public bool can_flag_topic { get; set; }
    }

    public class Created_By
    {
        public int id { get; set; }
        public string username { get; set; }
        public string avatar_template { get; set; }
    }

    public class Last_Poster
    {
        public int id { get; set; }
        public string username { get; set; }
        public string avatar_template { get; set; }
    }

    public class Allowed_Users
    {
        public int id { get; set; }
        public string username { get; set; }
        public string avatar_template { get; set; }
    }

    public class Participant
    {
        public int id { get; set; }
        public string username { get; set; }
        public string avatar_template { get; set; }
        public int post_count { get; set; }
        public object primary_group_name { get; set; }
        public object primary_group_flair_url { get; set; }
        public object primary_group_flair_color { get; set; }
        public object primary_group_flair_bg_color { get; set; }
    }

    public class Suggested_Topics
    {
        public int id { get; set; }
        public string title { get; set; }
        public string fancy_title { get; set; }
        public string slug { get; set; }
        public int posts_count { get; set; }
        public int reply_count { get; set; }
        public int highest_post_number { get; set; }
        public object image_url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime last_posted_at { get; set; }
        public bool bumped { get; set; }
        public DateTime bumped_at { get; set; }
        public bool unseen { get; set; }
        public int last_read_post_number { get; set; }
        public int unread { get; set; }
        public int new_posts { get; set; }
        public bool pinned { get; set; }
        public object unpinned { get; set; }
        public bool visible { get; set; }
        public bool closed { get; set; }
        public bool archived { get; set; }
        public int notification_level { get; set; }
        public bool bookmarked { get; set; }
        public bool liked { get; set; }
        public string archetype { get; set; }
        public int like_count { get; set; }
        public int views { get; set; }
        public object category_id { get; set; }
        public object[] tags { get; set; }
        public object featured_link { get; set; }
        public Poster[] posters { get; set; }
    }

    public class Poster
    {
        public string extras { get; set; }
        public string description { get; set; }
        public User user { get; set; }
    }

    public class Link
    {
        public string url { get; set; }
        public object title { get; set; }
        public object fancy_title { get; set; }
        public bool _internal { get; set; }
        public bool attachment { get; set; }
        public bool reflection { get; set; }
        public int clicks { get; set; }
        public int user_id { get; set; }
        public string domain { get; set; }
    }

}
