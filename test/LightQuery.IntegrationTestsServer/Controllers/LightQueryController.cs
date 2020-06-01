using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        [LightQuery]
        public IActionResult GetValues()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }

        [HttpGet("/LightQueryWithDefaultSort")]
        [LightQuery(defaultSort: nameof(LightQuery.IntegrationTestsServer.User.Email) + " asc")]
        public IActionResult GetValuesSortedByDefault()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }
    }
}
