using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace LightQuery.IntegrationTestsServer.Controllers
{
    [Route("AsyncPaginatedLightQuery")]
    public class AsyncPaginatedLightQueryController : Controller
    {

        public AsyncPaginatedLightQueryController(LightQueryContext context)
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
