using System;
using System.Linq;
using LightQuery.IntegrationTestsServer;
using Microsoft.AspNetCore.Mvc;

namespace LightQuery.Tests.Integration.Controllers
{
    [Route("PaginatedLightQuery")]
    public class PaginatedLightQueryController : Controller
    {

        public PaginatedLightQueryController(LightQueryContext context)
        {
            _context = context;
        }

        private readonly LightQueryContext _context;

        [LightQuery(forcePagination: true, defaultPageSize: 3)]
        public IActionResult GetValues()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }
    }
}
