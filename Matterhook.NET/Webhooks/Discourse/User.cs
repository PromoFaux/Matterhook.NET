using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matterhook.NET.Webhooks.Discourse
{

    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string avatar_template { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public object last_posted_at { get; set; }
        public object last_seen_at { get; set; }
        public DateTime created_at { get; set; }
        public bool can_edit { get; set; }
        public bool can_edit_username { get; set; }
        public bool can_edit_email { get; set; }
        public bool can_edit_name { get; set; }
        public object[] stats { get; set; }
        public bool can_send_private_messages { get; set; }
        public bool can_send_private_message_to_user { get; set; }
        public int trust_level { get; set; }
        public bool moderator { get; set; }
        public bool admin { get; set; }
        public object title { get; set; }
        public object uploaded_avatar_id { get; set; }
        public int badge_count { get; set; }
        public bool has_title_badges { get; set; }
        public object custom_fields { get; set; }
        public int pending_count { get; set; }
        public int profile_view_count { get; set; }
        public object primary_group_name { get; set; }
        public object primary_group_flair_url { get; set; }
        public object primary_group_flair_bg_color { get; set; }
        public object primary_group_flair_color { get; set; }
        public int post_count { get; set; }
        public bool can_be_deleted { get; set; }
        public bool can_delete_all_posts { get; set; }
        public string locale { get; set; }
        public object[] muted_category_ids { get; set; }
        public object[] watched_tags { get; set; }
        public object[] watching_first_post_tags { get; set; }
        public object[] tracked_tags { get; set; }
        public object[] muted_tags { get; set; }
        public object[] tracked_category_ids { get; set; }
        public object[] watched_category_ids { get; set; }
        public object[] watched_first_post_category_ids { get; set; }
        public Private_Messages_Stats private_messages_stats { get; set; }
        public object system_avatar_upload_id { get; set; }
        public string system_avatar_template { get; set; }
        public object gravatar_avatar_upload_id { get; set; }
        public object gravatar_avatar_template { get; set; }
        public object custom_avatar_upload_id { get; set; }
        public object custom_avatar_template { get; set; }
        public object[] muted_usernames { get; set; }
        public int mailing_list_posts_per_day { get; set; }
        public bool can_change_bio { get; set; }
        public object user_api_keys { get; set; }
        public object invited_by { get; set; }
        public Group[] groups { get; set; }
        public Group_Users[] group_users { get; set; }
        public object[] featured_user_badge_ids { get; set; }
        public object card_badge { get; set; }
        public User_Option user_option { get; set; }
    }

    public class Private_Messages_Stats
    {
        public int all { get; set; }
        public int mine { get; set; }
        public int unread { get; set; }
    }

    public class User_Option
    {
        public int user_id { get; set; }
        public bool email_always { get; set; }
        public bool mailing_list_mode { get; set; }
        public int mailing_list_mode_frequency { get; set; }
        public bool email_digests { get; set; }
        public bool email_private_messages { get; set; }
        public bool email_direct { get; set; }
        public bool external_links_in_new_tab { get; set; }
        public bool dynamic_favicon { get; set; }
        public bool enable_quoting { get; set; }
        public bool disable_jump_reply { get; set; }
        public object digest_after_minutes { get; set; }
        public bool automatically_unpin_topics { get; set; }
        public int auto_track_topics_after_msecs { get; set; }
        public int notification_level_when_replying { get; set; }
        public int new_topic_duration_minutes { get; set; }
        public int email_previous_replies { get; set; }
        public bool email_in_reply_to { get; set; }
        public int like_notification_frequency { get; set; }
        public bool include_tl0_in_digests { get; set; }
        public string theme_key { get; set; }
        public int theme_key_seq { get; set; }
    }

    public class Group
    {
        public int id { get; set; }
        public bool automatic { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public int user_count { get; set; }
        public int alias_level { get; set; }
        public int visibility_level { get; set; }
        public object automatic_membership_email_domains { get; set; }
        public bool automatic_membership_retroactive { get; set; }
        public bool primary_group { get; set; }
        public object title { get; set; }
        public object grant_trust_level { get; set; }
        public object incoming_email { get; set; }
        public bool has_messages { get; set; }
        public object flair_url { get; set; }
        public object flair_bg_color { get; set; }
        public object flair_color { get; set; }
        public object bio_raw { get; set; }
        public object bio_cooked { get; set; }
        public bool public_admission { get; set; }
        public bool public_exit { get; set; }
        public bool allow_membership_requests { get; set; }
        public object full_name { get; set; }
        public int default_notification_level { get; set; }
        public object membership_request_template { get; set; }
    }

    public class Group_Users
    {
        public int group_id { get; set; }
        public int user_id { get; set; }
        public int notification_level { get; set; }
    }

}
