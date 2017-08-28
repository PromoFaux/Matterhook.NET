using System;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Matterhook.NET.Webhooks.Discourse
{
    public class DiscourseHook
    {
        

        public DiscourseHook(StringValues eventId, StringValues eventType, StringValues eventName, StringValues signature, string payloadText)
        {
            EventId = eventId;
            EventType = eventType;
            EventName = eventName;
            Signature = signature;
            PayloadString = payloadText;

            switch (EventType)
            {
                case "post":
                    Payload = JsonConvert.DeserializeObject<PostPayload>(PayloadString);
                    break;
                case "topic":
                    Payload = JsonConvert.DeserializeObject<TopicPayload>(PayloadString);
                    break;
                case "user":
                    Payload = JsonConvert.DeserializeObject<UserPayload>(PayloadString);
                    break;
                case "ping":
                    Payload = null;
                    Console.WriteLine("Ping from Discourse!");
                    break;
                default:
                    throw new Exception($"Uknown Event Type: {EventType}");

            }
        }

      

        public string ContentType { get; set; }
        public string EventId { get; set; }
        public string EventType { get; set; }
        public string EventName { get; set; }
        public string ContentLength { get; set; }
        
        public string Signature { get; set; }

        private string PayloadString { get; }
        public Payload Payload { get; }

       

    }
}
