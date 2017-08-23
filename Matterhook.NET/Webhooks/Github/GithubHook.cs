using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Matterhook.NET.Webhooks.Github
{
    public class GithubHook
    {

        /// <summary>
        /// Turn incoming Github webhook into an object
        /// </summary>
        /// <param name="request">Request containing payload</param>
        /// <param name="secret">Secret if set.</param>
        public GithubHook(HttpRequest request, string secret = "")
        {

            request.Headers.TryGetValue("X-GitHub-Event", out StringValues strEvent);
            Event = strEvent;

            request.Headers.TryGetValue("X-Hub-Signature", out StringValues singature);
            Signature = singature;

            request.Headers.TryGetValue("X-GitHub-Delivery", out StringValues delivery);
            Delivery = delivery;
        }

        public string Event { get; set; }
        public string Signature { get; set; }
        public string Delivery { get; set; }
        
    }
}
