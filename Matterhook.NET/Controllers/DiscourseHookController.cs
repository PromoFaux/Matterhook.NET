//Based on: https://github.com/MichaCo/WebHookExample

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Matterhook.NET.Webhooks.Discourse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using ReverseMarkdown;


namespace Matterhook.NET.Controllers
{
    [Route("[Controller]")]
    public class DiscourseHookController : Controller
    {
        private const string Sha256Prefix = "sha256=";
        private readonly IOptions<Config> _config;

        public DiscourseHookController(IOptions<Config> config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [HttpPost("")]
        public async Task<IActionResult> Receive()
        {

            try
            {
                //Generate DiscourseHook object for easier reading
                Console.WriteLine($"Hook received: {DateTime.Now}");
                var discourseHook = new DiscourseHook(Request, _config.Value.DiscourseWebhookSecret);
                Console.WriteLine($"Processing Incoming Hook Id: {discourseHook.EventId}");

                //Did the checksums match?
                if (discourseHook.SignatureValid)
                {

                    MatterhookClient matterHook = new MatterhookClient(_config.Value.MattermostWebhookUrl);

                    switch (discourseHook.EventName)
                    {
                        case "post_created":
                            //todo: check status of posting to mattermost
                            var test = await matterHook.PostAsync(PostCreated((PostPayload)discourseHook.Payload));
                            return Ok();
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Signature!");
                    Console.WriteLine($"Expected: {discourseHook.Signature}");
                    Console.WriteLine($"Calculated: {discourseHook.CalcSignature}");
                    return Unauthorized();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Content(e.Message);
            }

            return Ok();

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
            var dUrl = _config.Value.DiscourseUrl;

            if (_config.Value.MattermostIgnoredTopicTitles.Contains(p.topic_title))
                throw new Exception("Post title matches ignored titles");

            if (_config.Value.MattermostIgnorePrivateMessages)
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
                Channel = _config.Value.MattermostBotChannel,
                IconUrl = new Uri(_config.Value.MattermostBotImage),
                Username = _config.Value.MatthermostBotName,

                Attachments = new List<MattermostAttachment>
                {
                    new MattermostAttachment
                    {
                        Fallback = "New Post in Discourse Topic",
                        Title = p.topic_title,
                        TitleLink = new Uri($"{_config.Value.DiscourseUrl}/t/{p.topic_id}/{p.post_number}"),
                        Text = new Converter().Convert(ExpandDiscourseUrls(p.cooked,dUrl)),
                        AuthorName = p.username,
                        AuthorLink = new Uri($"{dUrl}/u/{p.username}"),
                        AuthorIcon = new Uri($"{dUrl}{p.avatar_template.Replace("{size}","16")}")
                    }
                }
                
            };
            return retVal;
        }

        
    }
}