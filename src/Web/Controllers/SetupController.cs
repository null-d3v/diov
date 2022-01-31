using Diov.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Diov.Web;

[Authorize]
[Route("[controller]")]
public class SetupController : Controller
{
    public SetupController(
        IContentAccessor contentAccessor,
        IOptions<JsonSerializerOptions> jsonSerializerOptionsAccessor)
    {
        ContentAccessor = contentAccessor;
        JsonSerializerOptions = jsonSerializerOptionsAccessor.Value;
    }

    public IContentAccessor ContentAccessor { get; }
    public JsonSerializerOptions JsonSerializerOptions { get; }

    [HttpGet("[action]")]
    public async Task<IActionResult> Export(
        CancellationToken cancellationToken = default)
    {
        var contents = new List<Content>();

        var pagedContents = new SearchResponse<Content>
        {
            Skip = -100,
            Take = 100,
        };
        do
        {
            pagedContents.Skip += pagedContents.Take;

            pagedContents = await ContentAccessor
                .SearchContentAsync(
                    new ContentSearchRequest(),
                    pagedContents.Skip,
                    pagedContents.Take,
                    cancellationToken);

            contents.AddRange(pagedContents.Items);
        }
        while (pagedContents.TotalCount >=
            pagedContents.Skip + pagedContents.Take);

        return Ok(contents);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Import(
        [FromForm]SetupModel setupModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes
                .Status400BadRequest;
            return View(nameof(Index));
        }

        var contents = await JsonSerializer
            .DeserializeAsync<IEnumerable<Content>>(
                setupModel.ImportFile.OpenReadStream(),
                JsonSerializerOptions,
                cancellationToken);
        if (contents != null)
        {
            foreach (var content in contents)
            {
                var existingContent = await ContentAccessor
                    .GetContentAsync(
                        content.Path,
                        cancellationToken);
                if (existingContent != null)
                {
                    content.Id = existingContent.Id;
                    await ContentAccessor.UpdateContentAsync(
                        content, cancellationToken);
                }
                else
                {
                    await ContentAccessor.AddContentAsync(
                        content, cancellationToken);
                }
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