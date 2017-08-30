using Newtonsoft.Json;

namespace Matterhook.NET.Webhooks.DockerHub
{
    public class DockerHubHook
    {
        public DockerHubHook(string payloadString)
        {
            payload = JsonConvert.DeserializeObject<Payload>(payloadString);
        }

        public Payload payload { get; }
    }
}
