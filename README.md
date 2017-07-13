# LightQuery

This project aims to provide a lightweight `ActionFilterAttribute`
that takes care of sorting and paginating Asp.Net Core API results.

This project is for you if you're still waiting for OData support in Asp.Net Core, even though you
only need the most basic of operations. It's also for everyone tired of writing like the 17th
`string sort = "Username"` parameter and lines over lines of switch statements in their controller actions.

This project was just started and is a WIP, expect a first release within the next days.

## Installation

_Coming soon to NuGet_

Both **NETStandard 1.6** and **.Net 4.6.1** and above are supported.

## Documentation

See below how to apply sorting & filtering to your API controllers. At a glance:
    
* Return an `ObjectResult` from your controller with an `IQueryable` value
* Use `sort` to sort, `page` & `pageSize` for pagination in your requests

You can find a demo at _test/LightQuery.Tests.Integration_ for an example of using this in an Asp.Net Core MVC application
for sorting an filtering.

### Sorting

```csharp
using LightQuery;

public class ApiController : Controller
{
    [LightQuery]
    [ProducesResponseType(typeof(IEnumerable<User>), 200)]
    public IActionResult GetValues()
    {
        var values = _repository.GetAllValuesAsQueryable();
        return Ok(values);  
    }
}
```

Annotate your controller actions with the `LightQueryAttribute` and it takes care of
applying url queries to the result. All `ObjectResult`s
([docs](https://docs.microsoft.com/en-us/aspnet/core/api/microsoft.aspnetcore.mvc.objectresult))
that have an `IQueryable` value
will be transformed. You're free to return any other results, too, from the annotated action
and it will simply be ignored.

Example:
`http://your.api.com/values?sort=email desc`

This will sort the result by it's `Email` property (note that is has been title-cased) in `descending` order.

### Pagination & Sorting

Paging is **active when the request includes pagination query parameters** or via explicitly setting the `forcePaginiation`
parameter to true in the attributes' constructor. Sorting works in combination with paging.

```csharp
using LightQuery;

public class ApiController : Controller
{
    [LightQuery(forcePagination: true, defaultPageSize: 3)]
    [ProducesResponseType(typeof(PaginationResult<User>), 200)]
    public IActionResult GetValues()
    {
        var values = _repository.GetAllValuesAsQueryable();
        return Ok(values);  
    }
}
```

Example:
`http://your.api.com/values?sort=email&page=2&pageSize=3`

**Response**
```json
{
    "page": 2,
    "pageSize": 3,
    "totalCount": 20,
    "data": [
        { "userName": "Dave", "email": "dave@example.com" },
        { "userName": "Emilia", "email": "emilia@example.com" },
        { "userName": "Fred", "email": "fred@example.com" }
    ]
}
```

---

[MIT License](LICENSE.md)