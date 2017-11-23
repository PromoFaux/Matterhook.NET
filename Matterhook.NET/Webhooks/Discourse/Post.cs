using System;

namespace Matterhook.NET.Webhooks.Discourse
{
    public class Post
    {
        public long id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string avatar_template { get; set; }
        public DateTime created_at { get; set; }
        public string cooked { get; set; }
        public long post_number { get; set; }
        public long post_type { get; set; }
        public DateTime updated_at { get; set; }
        public long reply_count { get; set; }
        public long? reply_to_post_number { get; set; }
        public long quote_count { get; set; }
        public object avg_time { get; set; }
        public long incoming_link_count { get; set; }
        public long reads { get; set; }
        public float score { get; set; }
        public bool yours { get; set; }
        public long topic_id { get; set; }
        public string topic_slug { get; set; }
        public string topic_title { get; set; }
        public string display_username { get; set; }
        public object primary_group_name { get; set; }
        public object primary_group_flair_url { get; set; }
        public object primary_group_flair_bg_color { get; set; }
        public object primary_group_flair_color { get; set; }
        public long version { get; set; }
        public object user_title { get; set; }
        public Reply_To_User reply_to_user { get; set; }
        public Actions_Summary[] actions_summary { get; set; }
        public bool moderator { get; set; }
        public bool admin { get; set; }
        public bool staff { get; set; }
        public long user_id { get; set; }
        public bool hidden { get; set; }
        public object hidden_reason_id { get; set; }
        public long trust_level { get; set; }
        public object deleted_at { get; set; }
        public bool user_deleted { get; set; }
        public object edit_reason { get; set; }
        public bool can_view_edit_history { get; set; }
        public bool wiki { get; set; }
        public bool can_accept_answer { get; set; }
        public bool can_unaccept_answer { get; set; }
        public bool accepted_answer { get; set; }
        public bool can_translate { get; set; }
    }

    public class Reply_To_User
    {
        public string username { get; set; }
        public string avatar_template { get; set; }
    }
}