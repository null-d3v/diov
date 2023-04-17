using Diov.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Diov.Web;

public class ContentController : Controller
{
    public ContentController(
        IContentAccessor contentAccessor)
    {
        ContentAccessor = contentAccessor;
    }

    public IContentAccessor ContentAccessor { get; }

    [Authorize]
    [HttpGet("[controller]/[action]")]
    public IActionResult Add()
    {
        return View();
    }

    [Authorize]
    [HttpPost("[controller]/[action]")]
    public async Task<IActionResult> Add(
        [FromForm]Content content,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes
                .Status400BadRequest;
            return View(content);
        }

        await ContentAccessor
            .AddContentAsync(
                content,
                cancellationToken)
            .ConfigureAwait(false);

        return RedirectToAction(
            "Detail",
            "Content",
            new { path = content.Path, });
    }

    [Authorize]
    [HttpGet("[controller]/{path}/[action]")]
    public async Task<IActionResult> Delete(
        [FromRoute]string path,
        CancellationToken cancellationToken = default)
    {
        var content = await ContentAccessor
            .GetContentAsync(
                path.ToLowerInvariant(),
                cancellationToken)
            .ConfigureAwait(false);

        if (content == null)
        {
            return NotFound();
        }

        return View(content);
    }

    [Authorize]
    [HttpPost("[controller]/{path}/[action]")]
    public async Task<IActionResult> Delete(
        [FromRoute]string path,
        [FromForm]Content _,
        CancellationToken cancellationToken = default)
    {
        await ContentAccessor
            .DeleteContentAsync(
                path.ToLowerInvariant(),
                cancellationToken)
            .ConfigureAwait(false);

        return RedirectToAction(
            "Index",
            "Content");
    }

    [HttpGet("[controller]/{path}")]
    public async Task<IActionResult> Detail(
        [FromRoute]string path,
        CancellationToken cancellationToken = default)
    {
        var content = await ContentAccessor
            .GetContentAsync(
                path.ToLowerInvariant(),
                cancellationToken)
            .ConfigureAwait(false);

        if (content == null)
        {
            return NotFound();
        }

        return View(content);
    }

    [Authorize]
    [HttpGet("[controller]/{path}/[action]")]
    public async Task<IActionResult> Edit(
        [FromRoute]string path,
        CancellationToken cancellationToken = default)
    {
        var content = await ContentAccessor
            .GetContentAsync(
                path.ToLowerInvariant(),
                cancellationToken)
            .ConfigureAwait(false);

        if (content == null)
        {
            return NotFound();
        }

        return View(content);
    }

    [Authorize]
    [HttpPost("[controller]/{path}/[action]")]
    public async Task<IActionResult> Edit(
        [FromRoute]string path,
        [FromForm]Content content,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes
                .Status400BadRequest;
            return View(content);
        }

        content.Id = (await ContentAccessor
            .GetContentAsync(
                path.ToLowerInvariant(),
                cancellationToken)
            .ConfigureAwait(false))?
                .Id ?? 0;

        if (content.Id == 0)
        {
            return NotFound();
        }

        await ContentAccessor
            .UpdateContentAsync(
                content,
                cancellationToken)
            .ConfigureAwait(false);

        return RedirectToAction(
            "Detail",
            "Content",
            new { path = content.Path, });
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery]bool indexed = true,
        [FromQuery]int skip = 0)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            indexed = true;
        }

        var searchResponse = await ContentAccessor
            .SearchContentAsync(
                new ContentSearchRequest
                {
                    IsIndexed = indexed,
                },
                skip)
            .ConfigureAwait(false);

        return View(new ContentIndexModel
        {
            IsIndexed = indexed,
            SearchResponse = searchResponse,
        });
    }
}
