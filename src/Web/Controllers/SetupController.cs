using Diov.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Diov.Web;

[Authorize]
[Route("[controller]")]
public class SetupController(
    IContentAccessor contentAccessor,
    IOptions<JsonSerializerOptions> jsonSerializerOptionsAccessor) :
    Controller
{
    public IContentAccessor ContentAccessor { get; } =
        contentAccessor;
    public JsonSerializerOptions JsonSerializerOptions { get; } =
        jsonSerializerOptionsAccessor.Value;

    [HttpGet("[action]")]
    public async Task<IActionResult> Export(
        CancellationToken cancellationToken = default)
    {
        var content = new List<Content>();

        var pagedContent = new SearchResponse<Content>
        {
            Skip = -100,
            Take = 100,
        };
        do
        {
            pagedContent.Skip += pagedContent.Take;

            pagedContent = await ContentAccessor
                .SearchContentAsync(
                    new ContentSearchRequest(),
                    pagedContent.Skip,
                    pagedContent.Take,
                    cancellationToken)
                .ConfigureAwait(false);

            content.AddRange(pagedContent.Items);
        }
        while (pagedContent.TotalCount >=
            pagedContent.Skip + pagedContent.Take);

        return Ok(content);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Import(
        [FromForm] SetupModel setupModel,
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
                cancellationToken)
            .ConfigureAwait(false);
        if (contents != null)
        {
            foreach (var content in contents)
            {
                var existingContent = await ContentAccessor
                    .GetContentAsync(
                        content.Path,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (existingContent != null)
                {
                    content.Id = existingContent.Id;
                    await ContentAccessor
                        .UpdateContentAsync(
                            content,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await ContentAccessor
                        .AddContentAsync(
                            content,
                            cancellationToken)
                        .ConfigureAwait(false);
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