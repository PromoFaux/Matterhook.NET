using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Matterhook.NET.Webhooks.Discourse;
using Matterhook.NET.Webhooks.Github;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
            //Generate GithubHook Object
            var githubHook = new GithubHook(Request,_config.Secret);
            return Ok();
        }
    }
}
