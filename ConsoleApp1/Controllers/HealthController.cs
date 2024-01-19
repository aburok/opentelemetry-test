using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleApp1.Controllers
{
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private static ActivitySource _source = new("aspnetcore-controller-api", "1.0");

        [Route("live")]
        [HttpGet]
        public ContentResult Live()
        {
            using var activity = _source.StartActivity("test activity");
            activity?.SetTag("test", "test-value");

            return new ContentResult() { Content = activity?.RootId.ToString() };
        }

        [Route("reflect")]
        [HttpPost]
        public async Task<ContentResult> ReflectBody()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine(body);
            Console.WriteLine("--------------------------------------------------------------------------------");
            return Content(body, "application/json");
        }
    }
}