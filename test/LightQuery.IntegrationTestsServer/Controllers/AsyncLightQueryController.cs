using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LightQuery.EntityFrameworkCore;

namespace LightQuery.IntegrationTestsServer.Controllers
{
    [Route("AsyncLightQuery")]
    public class AsyncLightQueryController : Controller
    {
        public AsyncLightQueryController(LightQueryContext context)
        {
            _context = context;
        }

        private readonly LightQueryContext _context;

        [AsyncLightQuery]
        public IActionResult GetValues()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }
    }
}