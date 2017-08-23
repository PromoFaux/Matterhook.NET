using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
