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
        public IActionResult GetValues(bool returnEmptyList = false)
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            if (returnEmptyList)
            {
                users = users.Where(u => false).AsQueryable().OrderBy(u => Guid.NewGuid());
            }
            return Ok(users);
        }

        [HttpGet("/AsyncPaginatedLightQueryWithDefaultSort")]
        [LightQuery(forcePagination: true, defaultPageSize: 3, defaultSort: nameof(LightQuery.IntegrationTestsServer.User.Email) + " asc")]
        public IActionResult GetValuesSortedByDefault()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }
    }
}
