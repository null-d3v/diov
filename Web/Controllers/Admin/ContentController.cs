﻿using Diov.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Diov.Web.Admin
{
    [Route("admin")]
    public class ContentController : Controller
    {
        public ContentController(
            IContentRepository contentRepository)
        {
            ContentRepository = contentRepository ??
                throw new ArgumentNullException(nameof(contentRepository));
        }

        public IContentRepository ContentRepository { get; }

        [Authorize]
        [HttpGet("[controller]/{path}")]
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

        [Authorize]
        [HttpGet("", Name = "Admin")]
        [HttpGet("[controller]")]
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