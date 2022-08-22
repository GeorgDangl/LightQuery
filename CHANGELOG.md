# Changelog

All notable changes to **LightQuery** are documented here.

## v2.2.2:
- The `PaginationBaseService` in the Angular library now destroys it's subscription internally in the `ngOnDestroy` lifecycle hook

## v2.2.1:
- Added an optional parameter to the LightQuery client for Angular to supply custom query parameters when calling `getAll()`

## v2.2.0:
- Added a dedicated target for .NET 6 for the Entity Framework Core projects
- Added tests for .NET 6
- Dropped tests for `netcoreapp2.1`
- The Angular client was updated to Angular v14

## v2.1.0:
- The Angular client was updated to Angular v12

## v2.0.0
- Added a compilation target for `net5.0` and added tests for `net5.0`
- Dropped support for `netcoreapp3.0` and changed target to `netcoreapp3.1`
- Added a new property `wrapNestedSortInNullChecks` to the ASP.NET Core controller attributes. This defaults to `false` for regular `[LightQuery]` and to `true` for `[AsyncLightQuery]`. It controls whether nested sorting / relational sorting will introduce null checks, e.g. sorting by `x.SubProperty.SubId` is either translated as `.Where(x => x.SubProperty != null).OrderBy(x => x.SubProperty.SubId)` or directly as `.OrderBy(x => x.SubProperty.SubId)`
- Added a `debounceTime(0)` pipe to the Angular `PaginationBaseService<T>`, to ensure that changing multiple options of the service in code doesn't result in sending a separate request (which is then cancelled) for every change

## v1.9.1:
- Drop tests for `netcoreapp2.0` and `netcoreapp3.0` and add tests for `netcoreapp3.1`
- The Angular library was updated to v10

## v1.9.0:
- Addition of the **LightQuery.Swashbuckle** (thanks to GitHub user @berkayakcay) and **LightQuery.NSwag** packages to support Swagger & OpenAPI generation

## v1.8.1:
- The Angular library was updated to be compatible with Angular v9.1

## v1.8.0:
- Add a `thenSort` parameter to specify a second sort option. This translates to something like `queryable.OrderBy(sort).ThenBy(thenSort)`
- Fix C# client not cancelling previous requests when query parameters in the `PaginationBaseService` were changed. If a new request is started due to parameter changes while another request is still en-route, the previous request is discarded and no event is emitted for when the previous request completes
- The C# client has now an additional constructor overload for the `PaginationBaseService` to be able to pass a `CancellationToken`

## v1.7.2:
- Fix possible `NullReferenceException` in case of relational sorting where an invalid property name is passed via the query. Thanks to GitHub user @smitpatel for discovering it!

## v1.7.1:
- Fixed a bug that caused Entity Framework Core to throw an `InvalidOperationException` when sorting was applied to projections to a class that inherited from the base query type. The error was an incorrectly used reflection invocation to determine the type the query applies to

## v1.7.0:
- Add support for ASP.NET Core 3.0

## v1.6.2:
- The .NET `PaginationBaseService` no longer makes requests when the url is null

## v1.6.1:
- Fix issue where `BadRequest` results in `AsyncLightQuery` decorated controllers with `forcePagination:true` were returning an empty `OkResult` with status code `200` instead of the original `404 - Bad Request`

## v1.6.0:
- The generated assemblies now have a strong name. This is a breaking change of the binary API and will require recompilation on all systems that consume this package. The strong name of the generated assembly allows compatibility with other, signed tools. Please note that this does not increase security or provide tamper-proof binaries, as the key is available in the source code per [Microsoft guidelines](https://msdn.microsoft.com/en-us/library/wd40t7ad(v=vs.110).aspx)

## v1.5.2:
- Bugfix: Empty results now report the `page` as `1` instead of `0`. Thanks to GitHub user @erdembas for the pull request!

## v1.5.1:
- When a `page` is requested that is higher than the last available one, the last available one will be returned instead. Thanks to GitHub user @erdembas for the pull request!

## v1.5.0:
- It's now possible to do relational sorting, meaning that nested properties can be used for sorting. For example, it is now possible to sort by `user.bankAccount.balance`. Thanks to GitHub user @erdembas for the pull request!

## v1.4.0:
- The `defaultSort` parameter was introduced for the server side controller attributes. Thanks to GitHub user @erdembas for the pull request!

## v1.3.0:
- Raise minimum supported .NET Standard version to `netstandard2.0`
- Bump version to align .NET and npm package versions

## v1.1.0:
- Publish ng-lightquery npm package for angular and include PaginationBaseService in the client libraries

## v1.0.2:
- Update version to align with new releases of LightQuery.EntityFrameworkCore and LightQuery.Client. Added support for EntityFrameworkCore async materialization of queries and a client side package for easier consuming of APIs

## v1.0.1:
- Forced pagination was not applied when no query string at all was present in the Http request

## v1.0.0:
- Initial release
