using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("/api/MdExport/{*url}")]
    public class MdExportController
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            return new ContentResult
            {
                ContentType = "text/html",
                Content = ""
            };
        }

    }
}
