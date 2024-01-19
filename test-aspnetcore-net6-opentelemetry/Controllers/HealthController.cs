using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace test_aspnetcore_net6_opentelemetry.Controllers
{
    public class HealthController : Controller
    {
        private static readonly StatusCodeResult StatusCodeOk = new((int)HttpStatusCode.OK);

        private static ActivitySource _source = new ActivitySource("test-app", "1.0");

        [HttpGet]
        public ContentResult Live()
        {
            using var activity = _source.StartActivity("test activity");
            activity?.SetTag("test", "test-value");

            return new ContentResult() { Content = activity?.RootId.ToString() };
        }
    }
}