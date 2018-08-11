//Based on: https://github.com/MichaCo/WebHookExample

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.IdentityModel.Tokens;


namespace Matterhook.NET.Controllers
{
    [Route("[Controller]")]
    public class DiscourseHookController : Controller
    {
        private readonly DiscourseConfig _config;
        private string _discourseUrl;


        public DiscourseHookController(IOptions<Config> config)
        {
            try
            {
                _config = config.Value.DiscourseConfig;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> Receive()
        {
            var stuffToLog = new List<string>();
            try
            {
                string payloadText;
                //Generate DiscourseHook object for easier reading
                stuffToLog.Add($"Discourse Hook received: {DateTime.Now}");

                Request.Headers.TryGetValue("X-Discourse-Event-Id", out StringValues eventId);
                Request.Headers.TryGetValue("X-Discourse-Event-Type", out StringValues eventType);
                Request.Headers.TryGetValue("X-Discourse-Event", out StringValues eventName);
                Request.Headers.TryGetValue("X-Discourse-Event-Signature", out StringValues signature);
                Request.Headers.TryGetValue("X-Discourse-Instance", out StringValues discourseUrl);
                Request.Headers.TryGetValue("Content-type", out var content);

                _discourseUrl = discourseUrl;

                stuffToLog.Add($"Hook Id: {eventId}");

                if (content != "application/json")
                {
                    const string error = "Invalid content type. Expected application/json";
                    stuffToLog.Add(error);
                    Util.LogList(stuffToLog);
                    return StatusCode(400, error);

                }

                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadText = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                var calcSig = Util.CalculateSignature(payloadText, signature, _config.Secret, "sha256=");


                if (signature == calcSig)
                {
                    var discourseHook = new DiscourseHook(eventId, eventType, eventName, signature, payloadText);
                    var matterHook = new MatterhookClient.MatterhookClient(_config.MattermostConfig.WebhookUrl);
                    HttpResponseMessage response = null;
                    MattermostMessage message = null;
                    if (discourseHook.EventName == "post_created")
                    {
                        message = PostCreated((PostPayload)discourseHook.Payload);
                        response = await matterHook.PostAsync(message);
                    }

                    if (response == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        stuffToLog.Add(response != null
                            ? $"Unable to post to Mattermost {response.StatusCode}"
                            : "Unable to post to Mattermost");

                        return StatusCode(500, response != null
                            ? $"Unable to post to Mattermost: {response.StatusCode}"
                            : "Unable to post to Mattermost");
                    }

                    if (!_config.LogOnlyErrors)
                    {
                        if (message != null) stuffToLog.Add(message.Text);
                        stuffToLog.Add("Succesfully posted to Mattermost");
                        Util.LogList(stuffToLog);
                    }
                    
                    return StatusCode(200, "Succesfully posted to Mattermost");
                }

                stuffToLog.Add("Invalid Signature!");
                stuffToLog.Add($"Expected: {signature}");
                stuffToLog.Add($"Calculated: {calcSig}");
                Util.LogList(stuffToLog);
                return StatusCode(401,"Invalid signature. Please check your secret values in the config and Discourse");

            }
            catch (Exception e)
            {
                stuffToLog.Add(e.Message);
                Util.LogList(stuffToLog);
                return StatusCode(e is NotImplementedException ? 501 : e is WarningException? 202: 500, e.Message);
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

            if (_config.IgnoredTopicTitles.Contains(p.topic_title))
            {
                throw new WarningException("Post title matches an ignored title");
                
            }

            if (_config.IgnorePrivateMessages)
            {
                //We're not really doing anything here yet
                //TODO: Revisit in the future
                try
                {
                    JObject.Parse(
                        new WebClient().DownloadString($"{_discourseUrl}/t/{p.topic_id}.json"));
                }
                catch
                {
                    throw new WarningException("Unable to retrieve topic, possibly a PM so we should ignore this.");
                }
            }

            if (p.cooked == "")
            {
                throw new WarningException("Body empty, nothing to post.");
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
                        TitleLink = new Uri($"{_discourseUrl}/t/{p.topic_id}/{p.post_number}"),
                        Text = new Converter().Convert(ExpandDiscourseUrls(p.cooked,_discourseUrl)),
                        AuthorName = p.username,
                        AuthorLink = new Uri($"{_discourseUrl}/u/{p.username}"),
                        AuthorIcon = new Uri($"{_discourseUrl}{p.avatar_template.Replace("{size}","16")}")
                    }
                }

            };

            if (p.post_number.ToString() == "1")
            {
                retVal.Text = "#NewTopic\n";
            }

            retVal.Text += $"#{p.topic_slug}";

            return retVal;
        }


    }
}