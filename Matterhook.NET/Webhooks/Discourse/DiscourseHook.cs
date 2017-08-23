using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Matterhook.NET.Webhooks.Discourse
{
    public class DiscourseHook
    {
        /// <summary>
        /// Turn incoming Discourse webhook into an object
        /// </summary>
        /// <param name="request">Request containing payload</param>
        /// <param name="secret">Secret if set.</param>
        public DiscourseHook(HttpRequest request, string secret = "")
        {
            request.Headers.TryGetValue("Content-Type", out StringValues contentType);
            ContentType = contentType;

            request.Headers.TryGetValue("X-Discourse-Event-Id", out StringValues eventId);
            EventId = eventId;

            request.Headers.TryGetValue("X-Discourse-Event-Type", out StringValues eventType);
            EventType = eventType;

            request.Headers.TryGetValue("X-Discourse-Event", out StringValues eventName);
            EventName = eventName;

            request.Headers.TryGetValue("X-Discourse-Event-Signature", out StringValues signature);
            Signature = signature;

            request.Headers.TryGetValue("Content-Length", out StringValues contLength);
            ContentLength = contLength;

            GetPayload(request);

            SignatureValid = IsSignatureValid(PayloadString, Signature, secret);

            

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

        private async void GetPayload(HttpRequest request)
        {
            using (var reader = new StreamReader(request.Body, Encoding.UTF8))
            {
                PayloadString = await reader.ReadToEndAsync();
            }
        }

        public string ContentType { get; set; }
        public string EventId { get; set; }
        public string EventType { get; set; }
        public string EventName { get; set; }
        public string ContentLength { get; set; }

        public bool SignatureValid { get; private set; }
        public string Signature { get; set; }
        public string CalcSignature { get; set; }

        private string PayloadString { get; set; }
        public Payload Payload { get; private set; }

        private bool IsSignatureValid(string payload, string signatureWithPrefix, string secret)
        {
            const string sha256Prefix = "sha256=";

            if (!signatureWithPrefix.StartsWith(sha256Prefix, StringComparison.OrdinalIgnoreCase)) return false;

            var signature = signatureWithPrefix.Substring(sha256Prefix.Length);
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (var hmSha256 = new HMACSHA256(secretBytes))
            {
                var hash = hmSha256.ComputeHash(payloadBytes);

                var hashString = ToHexString(hash);
                CalcSignature = hashString;
                if (hashString.Equals(signature))
                {
                    return true;
                }
            }
            return false;

        }

        private static string ToHexString(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }

    }
}
