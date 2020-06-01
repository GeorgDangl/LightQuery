using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace LightQuery.IntegrationTestsServer.Controllers
{
    [Route("PaginatedLightQuery")]
    public class PaginatedLightQueryController : Controller
    {

        public PaginatedLightQueryController(LightQueryContext context)
        {
            _context = context;
        }

        private readonly LightQueryContext _context;

        [HttpGet]
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

        [HttpGet("/PaginatedLightQueryWithDefaultSort")]
        [LightQuery(forcePagination: true, defaultPageSize: 3, defaultSort: nameof(LightQuery.IntegrationTestsServer.User.Email) + " asc")]
        public IActionResult GetValuesSortedByDefault()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }

        [HttpGet("/PaginatedLightQueryWithBadRequestResponse")]
        [LightQuery(forcePagination: true, defaultPageSize: 3, defaultSort: nameof(LightQuery.IntegrationTestsServer.User.Email) + " asc")]
        public IActionResult GetBadRequestResult()
        {
            return BadRequest();
        }
    }
}
