using Diov.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Diov.Web
{
    [Authorize]
    [Route("[controller]")]
    public class SetupController : Controller
    {
        public SetupController(
            IContentRepository contentRepository,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            ContentRepository = contentRepository ??
                throw new ArgumentNullException(
                    nameof(contentRepository));
            JsonSerializerOptions = jsonSerializerOptions?.Value ??
                throw new ArgumentNullException(
                    nameof(contentRepository));
        }

        public IContentRepository ContentRepository { get; }
        public JsonSerializerOptions JsonSerializerOptions { get; }

        [HttpGet("[action]")]
        public async Task<IActionResult> Export()
        {
            var contents = new List<Content>();

            var pagedContents = new SearchResponse<Content>
            {
                PageIndex = -1,
            };
            do
            {
                pagedContents = await ContentRepository
                    .SearchContentsAsync(
                        new ContentSearchRequest(),
                        ++pagedContents.PageIndex,
                        100);

                contents.AddRange(pagedContents.Items);
            }
            while (pagedContents.TotalCount >=
                pagedContents.PageIndex *
                (pagedContents.PageSize + 1));

            return Ok(contents);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Import(
            SetupModel setupModel)
        {
            var contents = await JsonSerializer
                .DeserializeAsync<IEnumerable<Content>>(
                    setupModel.ImportFile.OpenReadStream(),
                    JsonSerializerOptions);
            foreach (var content in contents)
            {
                var existingContent = (await ContentRepository
                    .SearchContentsAsync(
                        new ContentSearchRequest
                        {
                            Path = content.Path,
                        },
                        0,
                        1))
                    .Items
                    .FirstOrDefault();
                if (existingContent != null)
                {
                    content.Id = existingContent.Id;
                    await ContentRepository
                        .UpdateContentAsync(content);
                }
                else
                {
                    await ContentRepository
                        .AddContentAsync(content);
                }
            }

            return RedirectToAction(
                "Index",
                "Content");
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}