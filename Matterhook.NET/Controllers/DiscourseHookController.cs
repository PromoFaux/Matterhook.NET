﻿//Based on: https://github.com/MichaCo/WebHookExample

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Matterhook.NET.Code;
using Matterhook.NET.Webhooks.Discourse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using ReverseMarkdown;
using Matterhook.NET.MatterhookClient;


namespace Matterhook.NET.Controllers
{
    [Route("[Controller]")]
    public class DiscourseHookController : Controller
    {
        private const string Sha256Prefix = "sha256=";
        private readonly DiscourseConfig _config;


        public DiscourseHookController(IOptions<Config> config)
        {
            var c = config ?? throw new ArgumentNullException(nameof(config));
            _config = c.Value.DiscourseConfig;

        }

        [HttpPost("")]
        public async Task<IActionResult> Receive()
        {
            try
            {
                string payloadText;
                //Generate DiscourseHook object for easier reading
                Console.WriteLine($"Discourse Hook received: {DateTime.Now}");

                Request.Headers.TryGetValue("X-Discourse-Event-Id", out StringValues eventId);
                Request.Headers.TryGetValue("X-Discourse-Event-Type", out StringValues eventType);
                Request.Headers.TryGetValue("X-Discourse-Event", out StringValues eventName);
                Request.Headers.TryGetValue("X-Discourse-Event-Signature", out StringValues signature);

                Console.WriteLine($"Hook Id: {eventId}");

                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadText = await reader.ReadToEndAsync();
                }

                var calcSig = Util.CalculateSignature(payloadText, signature, _config.Secret, "sha256=");


                if (signature == calcSig)
                {
                    var discourseHook = new DiscourseHook(eventId,eventType,eventName,signature,payloadText);
                    var matterHook = new MatterhookClient.MatterhookClient(_config.MattermostConfig.WebhookUrl);
                    HttpResponseMessage response = null;
                    switch (discourseHook.EventName)
                    {
                        case "post_created":
                            response = await matterHook.PostAsync(PostCreated((PostPayload)discourseHook.Payload));
                            break;
                        default:
                            break;
                    }

                    if (response == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine(response != null
                            ? $"Unable to post to Mattermost {response.StatusCode}"
                            : "Unable to post to Mattermost");

                        return Content(response != null ? $"Problem posting to Mattermost: {response.StatusCode}" : "Problem Posting to Mattermost");
                    }

                    Console.WriteLine("Succesfully posted to Mattermost");
                    return Ok();
                }
                else
                {
                    Console.WriteLine("Invalid Signature!");
                    Console.WriteLine($"Expected: {signature}");
                    Console.WriteLine($"Calculated: {calcSig}");
                    return Unauthorized();
                }
        
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Content(e.Message);
            }
           
        }

        private static string ExpandDiscourseUrls(string input, string discourseUrl)
        {
            input = input.Replace("\"/uploads", $"\"{discourseUrl}/uploads/");
            input = input.Replace("\"/images", $"\"{discourseUrl}/images/");
            return input;
        }

        private MattermostMessage PostCreated(PostPayload payload)
        {
            var p = payload.post;
            var dUrl = _config.Url;

            if (_config.IgnoredTopicTitles.Contains(p.topic_title))
                throw new Exception("Post title matches ignored titles");

            if (_config.IgnorePrivateMessages)
            {
                //We're not really doing anything here yet
                //TODO: Revisit in the future
                try
                {
                    JObject.Parse(
                        new WebClient().DownloadString($"{dUrl}/t/{p.topic_id}.json"));
                }
                catch
                {
                    throw new Exception("Unable to retrieve topic, possibly a PM so we should ignore this.");
                }
            }

           

            var retVal = new MattermostMessage
            {
                Channel = _config.MattermostConfig.Channel,
                IconUrl = new Uri(_config.MattermostConfig.IconUrl),
                Username = _config.MattermostConfig.Username,


                Attachments = new List<MattermostAttachment>
                {
                    new MattermostAttachment
                    {
                        Fallback = "New Post in Discourse Topic",
                        Title = p.topic_title,
                        TitleLink = new Uri($"{dUrl}/t/{p.topic_id}/{p.post_number}"),
                        Text = new Converter().Convert(ExpandDiscourseUrls(p.cooked,dUrl)),
                        AuthorName = p.username,
                        AuthorLink = new Uri($"{dUrl}/u/{p.username}"),
                        AuthorIcon = new Uri($"{dUrl}{p.avatar_template.Replace("{size}","16")}")
                    }
                }

            };

            if (p.post_number.ToString() == "1")
               retVal.Text = "#NewTopic\n";

            retVal.Text += $"#{p.topic_slug}";

            return retVal;
        }


    }
}