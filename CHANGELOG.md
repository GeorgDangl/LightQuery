# Changelog

All notable changes to **LightQuery** are documented here.

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
