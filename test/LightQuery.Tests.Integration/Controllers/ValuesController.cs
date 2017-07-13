using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace LightQuery.Tests.Integration.Controllers
{
    [Route("Values")]
    public class ValuesController : Controller
    {
        public ValuesController(LightQueryContext context)
        {
            _context = context;
        }

        private readonly LightQueryContext _context;

        public IActionResult GetValues()
        {
            var users = _context.Users.OrderBy(u => Guid.NewGuid());
            return Ok(users);
        }
    }
}
