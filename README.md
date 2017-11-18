# LightQuery
[![Build Status](https://jenkins.dangl.me/buildStatus/icon?job=LightQuery.Tests)](https://jenkins.dangl.me/job/LightQuery.Tests)

[Online Documentation](https://docs.dangl-it.com/Projects/LightQuery)

This project aims to provide a lightweight `ActionFilterAttribute`
that takes care of sorting and paginating Asp.Net Core API results.

This project is for you if you're still waiting for OData support in Asp.Net Core, even though you
only need the most basic of operations. It's also for everyone tired of writing like the 17th
`string sort = "Username"` parameter and lines over lines of switch statements in their controller actions.

It supports EntityFrameworkCores async query materialization with the optional `LightQuery.EntityFrameworkCore` package.

[![npm](https://img.shields.io/npm/v/ng-lightquery.svg)](https://www.npmjs.com/package/ng-lightquery)

In addition to the C# client, there's also a client for Angular 5+ on npm: `ng-lightquery`

## Installation
[![NuGet](https://img.shields.io/nuget/v/LightQuery.svg)](https://www.nuget.org/packages/LightQuery)
[![MyGet](https://img.shields.io/myget/dangl/v/LightQuery.svg)](https://www.myget.org/feed/dangl/package/nuget/LightQuery)

[The package is available on nuget](https://www.nuget.org/packages/LightQuery).
[Daily builds are on myget](https://www.myget.org/feed/dangl/package/nuget/LightQuery).

MyGet feed: `https://www.myget.org/F/dangl/api/v3/index.json`

>PM> **Install-Package LightQuery**

Includes the core functionality to sort and paginate Asp.Net Core controller results

>PM> **Install-Package LightQuery.EntityFrameworkCore**

Includes support for EntityFramework.Core async query materialization

>PM> **Install-Package LightQuery.Client**

Includes LightQuery models and the QueryBuilder utility

Both **NETStandard 1.6** and **.Net 4.6.1** are supported.

## Documentation - Server

See below how to apply sorting & filtering to your API controllers. At a glance:
    
* Return an `ObjectResult` from your controller with an `IQueryable` value
* Use `sort` to sort, `page` & `pageSize` for pagination in your requests

You can find a demo in the integration test projects for an example of using this in an Asp.Net Core MVC application
for sorting and filtering.

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

This will sort the result by its `Email` property (it is title-cased if no `email` property is found) in `descending` order.

### Pagination & Sorting

Paging is **active when the request includes pagination query parameters** or via explicitly setting the `forcePagination`
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

### Async Materialization

The `LightQuery.EntityFrameworkCore` package provides an `AsyncLightQueryAttribute`. This can be used
for data sources that support async materialization of queries, e.g. `var result = await context.Users.ToListAsync()`.

## Documentation - C# Client

The `LightQuery.Client` package contains the `PaginationResult<T>` base class as well as a `QueryBuilder` utlity class to construct queries.

**Example**
```csharp
using LightQuery.Client;

var url = QueryBuilder.Build(page: 3, pageSize: 25, sortParam: "email");
var response = await _client.GetAsync(url);
var responseContent = await response.Content.ReadAsStringAsync();
var deserializedResponse = JsonConvert.DeserializeObject<PaginationResult<User>>(responseContent);
```

### PaginationBaseService

The `LightQuery.Client` package contains an abstract class `PaginationBaseService<T>` that can be used in reactive clients. It is similar in functionality
to the TypeScript client.

## Documentation - TypeScript & Angular

There is [an Angular example](AngularExample.ts) that can be used standalone or
in combination with a [@angular/material2](https://github.com/angular/material2) DataTable and its pagination and sort functionality.

**Example with Material 2 DataTable**
```html
<md-table [dataSource]="dataSource"
          mdSort
          [mdSortActive]="usersService.sort?.propertyName"
          [mdSortDirection]="usersService.sort?.isDescending ? 'desc' : 'asc'"
          (mdSortChange)="onSort($event)">
    <ng-container cdkColumnDef="email">
        <md-header-cell md-sort-header *cdkHeaderCellDef> Email </md-header-cell>
        <md-cell *cdkCellDef="let row">
            {{row.email}}
        </md-cell>
    </ng-container>
    <md-header-row *cdkHeaderRowDef="['email']"></md-header-row>
    <md-row *cdkRowDef="let row; columns: ['email'];"></md-row>
</md-table>
<md-paginator [length]="usersPaginated.totalCount"
              [pageSize]="usersPaginated.pageSize"
              [pageIndex]="usersPaginated.page - 1"
              (page)="onPage($event)">
</md-paginator>
```
```typescript
export class UsersComponent implements OnInit, OnDestroy {

    constructor(public userService: UserService) { }

    private usersPaginatedSubscription: Subscription;
    usersPaginated: PaginationResult<User>;
    dataSource: DataSource<User>;

    onPage(pageEvent: PageEvent) {
        this.userService.page = pageEvent.pageIndex + 1;
        this.userService.pageSize = pageEvent.pageSize;
    }

    onSort(event: { active: string, direction: string }) {
        if (!event.direction) {
            this.userService.sort = null;
        } else {
            this.userService.sort = { propertyName: event.active, isDescending: event.direction === 'desc' };
        }
    }

    ngOnInit() {
        this.dataSource = this.userService;
        this.usersPaginatedSubscription = this.userService.paginationResult.subscribe(r => this.usersPaginated = r);
    }

    ngOnDestroy() {
        this.usersPaginatedSubscription.unsubscribe();
    }
}
```


---

[MIT License](LICENSE.md)