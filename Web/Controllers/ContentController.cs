using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Diov.Data;
using Microsoft.AspNetCore.Mvc;

namespace Diov.Web
{
    public class ContentController : Controller
    {
        public ContentController(
            IContentRepository contentRepository)
        {
            ContentRepository = contentRepository ??
                throw new ArgumentNullException(nameof(contentRepository));
        }

        public IContentRepository ContentRepository { get; }

        [HttpGet("content/{path}")]
        public async Task<IActionResult> Detail(string path)
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

        [HttpGet("")]
        [HttpGet("content")]
        public async Task<IActionResult> Index(
            int page = 0)
        {
            var searchResponse = await ContentRepository
                .SearchContentsAsync(
                    new ContentSearchRequest
                    {
                        IsIndexed = true,
                    },
                    page);

            return View(searchResponse);
        }
    }
}