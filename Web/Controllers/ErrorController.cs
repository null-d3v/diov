using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Diov.Data;
using Microsoft.AspNetCore.Mvc;

namespace Diov.Web
{
    public class ErrorController : Controller
    {
        [HttpGet("error/{error}")]
        public IActionResult Detail(string error)
        {
            return View(error);
        }
    }
}