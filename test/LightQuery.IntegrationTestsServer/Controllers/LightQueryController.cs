using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LightQuery;

namespace LightQuery.IntegrationTestsServer.Controllers
{
    [Route("LightQuery")]
    public class LightQueryController : Controller
    {
        public LightQueryController(LightQueryContext context)
        {
            _context = context;
        }

        private readonly LightQueryContext _context;

        [LightQuery]
        public IActionResult GetValues()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }
    }
}
