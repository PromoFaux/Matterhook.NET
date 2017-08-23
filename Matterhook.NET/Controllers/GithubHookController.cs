using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using Matterhook.NET.Code;
using Matterhook.NET.Webhooks.Discourse;
using Matterhook.NET.Webhooks.Github;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using ReverseMarkdown;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Matterhook.NET.Controllers
{
    [Route("[Controller]")]
    public class GithubHookController : Controller
    {

        private readonly GithubConfig _config;

        public GithubHookController(IOptions<Config> config)
        {
            var c = config ?? throw new ArgumentNullException(nameof(config));
            _config = c.Value.GithubConfig;

        }

        [HttpPost("")]
        public async Task<IActionResult> Receive()
        {
            try
            {
                string payloadText;

                //Generate GithubHook Object
                //Generate DiscourseHook object for easier reading
                Console.WriteLine($"Github Hook received: {DateTime.Now}");

                Request.Headers.TryGetValue("X-GitHub-Event", out StringValues strEvent);
                Request.Headers.TryGetValue("X-Hub-Signature", out StringValues signature);
                Request.Headers.TryGetValue("X-GitHub-Delivery", out StringValues delivery);

                Console.WriteLine($"Hook Id: {delivery}");

                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadText = await reader.ReadToEndAsync();
                }

                var calcSig = Util.CalculateSignature(payloadText, signature, _config.Secret, "sha1=");

                //var githubHook =  new GithubHook(Request,_config.Secret);


                if (signature == calcSig)
                {
                    var githubHook = new GithubHook(strEvent, signature, delivery, payloadText);
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
                Console.WriteLine(e);
                throw;
            }
            
            
        }
    }
}
