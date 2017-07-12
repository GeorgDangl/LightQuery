# LightQuery

This project aims to provide a lightweight `ActionFilterAttribute`
that takes care of sorting and paginating Asp.Net Core API results.

This project is for you if you're still waiting for OData support in Asp.Net Core, even though you
only need the most basic of operations. It's also for everyone tired of writing like the 17th
`string sort = "Username"` parameter and lines over lines of switch statements in their controller actions.

This project was just started and is a WIP, expect a first release within the next days.

## Installation

_Coming soon to NuGet_

Both **NETStandard 1.6** and **.Net 4.5.1** and above are supported.

## Example

```csharp
using LightQuery;

public class ApiController : Conttoller
{
    [LightQuery]
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

---

[MIT License](LICENSE.md)