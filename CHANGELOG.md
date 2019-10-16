# Changelog

All notable changes to **LightQuery** are documented here.

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
