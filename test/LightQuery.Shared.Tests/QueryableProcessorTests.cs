using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LightQuery.Shared.Tests
{
    public class QueryableProcessorTests
    {
        private class User
        {
            public string UserName { get; set; }
            public string Email { get; set; }
        }

        #region Relational Data Sorting

        private class Order
        {
            public string Title { get; set; }
            public Product Product { get; set; }
        }

        private class Product
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Amount { get; set; }
            public ProductDetail Detail { get; set; }
        }

        private class ProductDetail
        {
            public string Barcode { get; set; }
            public string ManufacturerCode { get; set; }
        }

        // Relational Data Sorting Sample Data
        private IQueryable<Order> GetOrders()
        {
            return (new[]
                {
                    new Order
                    {
                        Title = "Order 1",
                        Product =new Product()
                        {
                            Amount = 100,
                            Quantity = 3,
                            Name = "Apple IPhone",
                            Detail = new ProductDetail()
                            {
                                Barcode = "A123456",
                                ManufacturerCode =null
                            }
                        }
                    },
                    new Order
                    {
                        Title = "Order 2",
                        Product =new Product()
                        {
                            Amount = 150,
                            Quantity = 2,
                            Name = "Pear",
                            Detail = new ProductDetail()
                            {
                                Barcode = "C123456",
                                ManufacturerCode = "A1234"
                            }
                        }
                    },
                    new Order
                    {
                        Title = "Order 3",
                        Product =new Product()
                        {
                            Amount = 200,
                            Quantity = 10,
                            Name = "Zipper",
                            Detail = new ProductDetail()
                            {
                                Barcode = "B123456",
                                ManufacturerCode =null
                            }
                        }
                    }
                }).OrderBy(x => Guid.NewGuid())
                .ToList()
                .AsQueryable();
        }

        #endregion Relational Data Sorting

        private IQueryable<User> GetUsers()
        {
            return new[]
                {
                    new User {UserName = "Joe", Email = "Joe@example.com"},
                    new User {UserName = "Alice", Email = "BettyAlice@example.com"},
                    new User {UserName = "Hank", Email = "Arthur.Hank@example.com"},
                    new User {UserName = "Zack", Email = "Zack@example.com"},
                    new User {UserName = "Dan", Email = "Hammerdan@example.com"}
                }
                .ToList()
                .AsQueryable();
        }

        [Fact]
        public void ArgumentNullExceptionOnNullQueryable()
        {
            var queryOptions = new QueryOptions();
            Assert.Throws<ArgumentNullException>("queryable", () => QueryableProcessor.ApplySorting(null, queryOptions));
        }

        [Fact]
        public void ArgumentNullExceptionOnNullQueryOptions()
        {
            var queryable = new[] { string.Empty }.AsQueryable();
            Assert.Throws<ArgumentNullException>("queryOptions", () => queryable.ApplySorting(null));
        }

        [Fact]
        public void ApplySortByUsernameAscending()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "UserName",
                IsDescending = false
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Alice", actual.First().UserName);
            Assert.Equal("Zack", actual.Last().UserName);
        }

        [Fact]
        public void ApplySortByUsernameDescending()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "UserName",
                IsDescending = true
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Zack", actual.First().UserName);
            Assert.Equal("Alice", actual.Last().UserName);
        }

        [Fact]
        public void ApplySortByEmailAscending()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Email",
                IsDescending = false
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Arthur.Hank@example.com", actual.First().Email);
            Assert.Equal("Zack@example.com", actual.Last().Email);
        }

        [Fact]
        public void ApplySortByEmailDescending()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Email",
                IsDescending = true
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Zack@example.com", actual.First().Email);
            Assert.Equal("Arthur.Hank@example.com", actual.Last().Email);
        }

        [Fact]
        public void ApplySortByUsernameAscendingWithCamelCase()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "userName",
                IsDescending = false
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Alice", actual.First().UserName);
            Assert.Equal("Zack", actual.Last().UserName);
        }

        [Fact]
        public void ApplySortByUsernameDescendingWithCamelCase()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "userName",
                IsDescending = true
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Zack", actual.First().UserName);
            Assert.Equal("Alice", actual.Last().UserName);
        }

        [Fact]
        public void ApplySortByEmailAscendingWithCamelCase()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "email",
                IsDescending = false
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Arthur.Hank@example.com", actual.First().Email);
            Assert.Equal("Zack@example.com", actual.Last().Email);
        }

        [Fact]
        public void ApplySortByEmailDescendingWithCamelCase()
        {
            var users = GetUsers();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "email",
                IsDescending = true
            };
            var actual = users.ApplySorting(queryOptions).Cast<User>();
            Assert.Equal("Zack@example.com", actual.First().Email);
            Assert.Equal("Arthur.Hank@example.com", actual.Last().Email);
        }

        [Fact]
        public void DoNothingByInvalidPropertyName()
        {
            var originalUsers = GetUsers();
            Func<IEnumerable<User>, string> aggregateEmails = users =>
                users
                    .Select(u => u.Email)
                    .Aggregate((current, next) => current + next);
            var originalEmails = aggregateEmails(originalUsers);
            var invalidQueryOptions = new QueryOptions { SortPropertyName = "Age" };
            var sortedUsers = originalUsers.ApplySorting(invalidQueryOptions).Cast<User>();
            var sortedEmails = aggregateEmails(sortedUsers);
            Assert.Equal(originalEmails, sortedEmails);
        }

        [Fact]
        public void DoNothingByNullPropertyName()
        {
            var originalUsers = GetUsers();
            Func<IEnumerable<User>, string> aggregateEmails = users =>
                users
                    .Select(u => u.Email)
                    .Aggregate((current, next) => current + next);
            var originalEmails = aggregateEmails(originalUsers);
            var invalidQueryOptions = new QueryOptions { SortPropertyName = null };
            var sortedUsers = originalUsers.ApplySorting(invalidQueryOptions).Cast<User>();
            var sortedEmails = aggregateEmails(sortedUsers);
            Assert.Equal(originalEmails, sortedEmails);
        }

        #region Relational data sorting tests

        [Fact]
        public void ApplySortByTitleAscending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Title",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();
            Assert.Equal("Order 1", actual.First().Title);
            Assert.Equal("Order 3", actual.Last().Title);
        }

        [Fact]
        public void ApplyRelationalSortByProductNameAscending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Name",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();
            Assert.Equal("Apple IPhone", actual.First().Product.Name);
            Assert.Equal("Zipper", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByQuantityAscending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Quantity",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();

            Assert.Equal(2, actual.First().Product.Quantity);
            Assert.Equal("Pear", actual.First().Product.Name);

            Assert.Equal(10, actual.Last().Product.Quantity);
            Assert.Equal("Zipper", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByAmountAscending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Amount",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();
            Assert.Equal(100, actual.First().Product.Amount);
            Assert.Equal("Apple IPhone", actual.First().Product.Name);

            Assert.Equal(200, actual.Last().Product.Amount);
            Assert.Equal("Zipper", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByProductNameDescending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Name",
                IsDescending = true
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();
            Assert.Equal("Zipper", actual.First().Product.Name);
            Assert.Equal("Apple IPhone", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByQuantityDescending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Quantity",
                IsDescending = true
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();

            Assert.Equal(10, actual.First().Product.Quantity);
            Assert.Equal("Zipper", actual.First().Product.Name);

            Assert.Equal(2, actual.Last().Product.Quantity);
            Assert.Equal("Pear", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByAmountDescending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Amount",
                IsDescending = true
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();

            Assert.Equal(200, actual.First().Product.Amount);
            Assert.Equal("Zipper", actual.First().Product.Name);

            Assert.Equal(100, actual.Last().Product.Amount);
            Assert.Equal("Apple IPhone", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyThirdLevelRelationalSortByBarcodeAscending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Detail.Barcode",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();

            Assert.Equal("A123456", actual.First().Product.Detail.Barcode);
            Assert.Equal("C123456", actual.Last().Product.Detail.Barcode);
        }

        [Fact]
        public void ApplyThirdLevelRelationalSortByBarcodeDescending()
        {
            var orders = GetOrders();
            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Detail.Barcode",
                IsDescending = true
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>();

            Assert.Equal("C123456", actual.First().Product.Detail.Barcode);
            Assert.Equal("A123456", actual.Last().Product.Detail.Barcode);
        }

        [Fact]
        public void ReturnsNoneForSecondLevelQueryWhenAllSecondLevelItemsAreNull()
        {
            var orders = new[]
            {
                new Order{Title = "#1" },
                new Order{Title = "#2" },
                new Order{Title = "#3" },
                new Order{Title = "#4" },
                new Order{Title = "#5" },
            }
                .OrderBy(x => Guid.NewGuid())
                .AsQueryable();

            Assert.All(orders, o => Assert.Null(o.Product));

            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Detail",
                IsDescending = true
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>().ToList();

            Assert.Empty(actual);
        }

        [Fact]
        public void RemovesNullSecondLevelProperties()
        {
            var orders = new[]
                {
                    new Order{Title = "#1" },
                    new Order{Title = "#2", Product = new Product{ Name = "Zippers" } },
                    new Order{Title = "#3", Product = new Product{ Name = "Apples" }  },
                    new Order{Title = "#4", Product = new Product{ Name = "Oranges" }  },
                    new Order{Title = "#5" },
                }
               .OrderBy(x => Guid.NewGuid())
               .AsQueryable();

            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Name",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>().ToList();

            Assert.Equal("#3", actual[0].Title);
            Assert.Equal("#4", actual[1].Title);
            Assert.Equal("#2", actual[2].Title);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void RemovesNullThirdLevelProperties()
        {
            var orders = new[]
                {
                    new Order{Title = "#1" },
                    new Order{Title = "#2", Product = new Product{ Detail = new ProductDetail{Barcode = "#456" } } },
                    new Order{Title = "#3", Product = new Product{ Detail = new ProductDetail{Barcode = "#123" } } },
                    new Order{Title = "#4", Product = new Product{ Detail = new ProductDetail{Barcode = "#789" } } },
                    new Order{Title = "#5" },
                }
               .OrderBy(x => Guid.NewGuid())
               .AsQueryable();

            var queryOptions = new QueryOptions
            {
                SortPropertyName = "Product.Detail.Barcode",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>().ToList();

            Assert.Equal("#3", actual[0].Title);
            Assert.Equal("#2", actual[1].Title);
            Assert.Equal("#4", actual[2].Title);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void RemovesNullThirdLevelPropertiesWithCamelCasedQuery()
        {
            var orders = new[]
                {
                    new Order{Title = "#1" },
                    new Order{Title = "#2", Product = new Product{ Detail = new ProductDetail{Barcode = "#456" } } },
                    new Order{Title = "#3", Product = new Product{ Detail = new ProductDetail{Barcode = "#123" } } },
                    new Order{Title = "#4", Product = new Product{ Detail = new ProductDetail{Barcode = "#789" } } },
                    new Order{Title = "#5" },
                }
               .OrderBy(x => Guid.NewGuid())
               .AsQueryable();

            var queryOptions = new QueryOptions
            {
                SortPropertyName = "product.detail.barcode",
                IsDescending = false
            };
            var actual = orders.ApplySorting(queryOptions).Cast<Order>().ToList();

            Assert.Equal("#3", actual[0].Title);
            Assert.Equal("#2", actual[1].Title);
            Assert.Equal("#4", actual[2].Title);
            Assert.Equal(3, actual.Count);
        }

        #endregion Relational data sorting tests
    }
}
