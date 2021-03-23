using Diov.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Diov.Web
{
    public class ContentController : Controller
    {
        public ContentController(
            IContentRepository contentRepository)
        {
            ContentRepository = contentRepository ??
                throw new ArgumentNullException(
                    nameof(contentRepository));
        }

        public IContentRepository ContentRepository { get; }

        [Authorize]
        [HttpGet("[controller]/[action]")]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize]
        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> Add(
            [FromForm]Content content)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes
                    .Status400BadRequest;
                return View(content);
            }

            await ContentRepository
                .AddContentAsync(content);

            return RedirectToAction(
                "Detail",
                "Content",
                new { path = content.Path, });
        }

        [Authorize]
        [HttpGet("[controller]/{path}/[action]")]
        public async Task<IActionResult> Delete(
            [FromRoute]string path)
        {
            var content = (await ContentRepository
                .SearchContentsAsync(
                    new ContentSearchRequest
                    {
                        Path = path,
                    },
                    0,
                    1))
                .Items
                .FirstOrDefault();

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
            [FromForm]Content content)
        {
            var contentId = (await ContentRepository
                .SearchContentsAsync(
                    new ContentSearchRequest
                    {
                        Path = path,
                    },
                    0,
                    1))
                .Items
                .FirstOrDefault()?
                .Id ?? 0;

            if (contentId != 0)
            {
                await ContentRepository
                    .DeleteContentAsync(contentId);
            }

            return RedirectToAction(
                "Index",
                "Content");
        }

        [HttpGet("[controller]/{path}")]
        public async Task<IActionResult> Detail(
            [FromRoute]string path)
        {
            var content = (await ContentRepository
                .SearchContentsAsync(
                    new ContentSearchRequest
                    {
                        Path = path,
                    },
                    0,
                    1))
                .Items
                .FirstOrDefault();

            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        [Authorize]
        [HttpGet("[controller]/{path}/[action]")]
        public async Task<IActionResult> Edit(
            [FromRoute]string path)
        {
            var content = (await ContentRepository
                .SearchContentsAsync(
                    new ContentSearchRequest
                    {
                        Path = path,
                    },
                    0,
                    1))
                .Items
                .FirstOrDefault();

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
            [FromForm]Content content)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = StatusCodes
                    .Status400BadRequest;
                return View(content);
            }

            content.Id = (await ContentRepository
                .SearchContentsAsync(
                    new ContentSearchRequest
                    {
                        Path = path,
                    },
                    0,
                    1))
                .Items
                .FirstOrDefault()?
                .Id ?? 0;

            if (content.Id == 0)
            {
                return NotFound();
            }

            await ContentRepository
                .UpdateContentAsync(content);

            return RedirectToAction(
                "Detail",
                "Content",
                new { path = content.Path, });
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            [FromQuery]int page = 0,
            [FromQuery]bool indexed = true)
        {
            if (!User.Identity.IsAuthenticated)
            {
                indexed = true;
            }

            var searchResponse = await ContentRepository
                .SearchContentsAsync(
                    new ContentSearchRequest
                    {
                        IsIndexed = indexed,
                    },
                    page);

            return View(new ContentIndexModel
            {
                IsIndexed = indexed,
                SearchResponse = searchResponse,
            });
        }
    }
}