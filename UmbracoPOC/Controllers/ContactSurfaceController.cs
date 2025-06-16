using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Website.Controllers;
using UmbracoPOC.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;

namespace UmbracoPOC.Controllers
{
    public class ContactSurfaceController : SurfaceController
    {
        private readonly AzureFunctionSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public ContactSurfaceController(
            IOptions<AzureFunctionSettings> settings,
            IHttpClientFactory httpClientFactory,
            IUmbracoContextAccessor contextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider urlProvider)
            : base(contextAccessor, databaseFactory, services, appCaches, profilingLogger, urlProvider)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitContact(string name, string email, string message)
        {
            var client = _httpClientFactory.CreateClient();
            var payload = new { name, email, message };
            var json = JsonSerializer.Serialize(payload);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_settings.ContactHandlerUrl, content);

            TempData["Message"] = response.IsSuccessStatusCode
                ? "Thank you! Your message has been sent."
                : "Oops! Something went wrong.";

            return RedirectToCurrentUmbracoPage();
        }
    }
}
