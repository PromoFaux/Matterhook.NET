namespace Matterhook.NET.Webhooks.Discourse
{
    public class Payload
    {
       
    }

    public class UserPayload : Payload
    {
        public User user { get; set; }
    }

    public class TopicPayload : Payload
    {
        public Topic topic { get; set; }
    }

    public class PostPayload : Payload
    {
        public Post post { get; set; }
    }

}
