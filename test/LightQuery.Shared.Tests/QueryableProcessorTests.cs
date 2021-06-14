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
            var sortOption = new SortOption();
            Assert.Throws<ArgumentNullException>("queryable", () => QueryableProcessor.ApplySorting(null, sortOption, null));
        }

        [Fact]
        public void ArgumentNullExceptionOnNullSortOption()
        {
            var queryable = new[] { string.Empty }.AsQueryable();
            Assert.Throws<ArgumentNullException>("sortOption", () => queryable.ApplySorting(null, null));
        }

        [Fact]
        public void ApplySortByUsernameAscending()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "UserName",
                IsDescending = false
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
            Assert.Equal("Alice", actual.First().UserName);
            Assert.Equal("Zack", actual.Last().UserName);
        }

        [Fact]
        public void AppliesThenSort()
        {
            var users = new List<User>
            {
                new User
                {
                    UserName = "Alice",
                    Email = "best_alice@example.com"
                },
                new User
                {
                    UserName = "Alice",
                    Email = "awesome_alice@example.com"
                },
                new User
                {
                    UserName = "Bob",
                    Email = "burger_bob@example.com"
                },
                new User
                {
                    UserName = "Bob",
                    Email = "zebras_are_striped_horses@example.com"
                }
            }
            .AsQueryable();
            var sortOption = new SortOption
            {
                PropertyName = "UserName",
                IsDescending = false
            };
            var thenSortOption = new SortOption
            {
                PropertyName = "Email",
                IsDescending = false
            };
            var actual = users.ApplySorting(sortOption, thenSortOption).Cast<User>().ToList();

            Assert.Equal("Alice", actual[0].UserName);
            Assert.Equal("awesome_alice@example.com", actual[0].Email);
            Assert.Equal("Alice", actual[1].UserName);
            Assert.Equal("best_alice@example.com", actual[1].Email);
            Assert.Equal("Bob", actual[2].UserName);
            Assert.Equal("burger_bob@example.com", actual[2].Email);
            Assert.Equal("Bob", actual[3].UserName);
            Assert.Equal("zebras_are_striped_horses@example.com", actual[3].Email);
        }

        [Fact]
        public void AppliesThenSort_Descending()
        {
            var users = new List<User>
            {
                new User
                {
                    UserName = "Alice",
                    Email = "best_alice@example.com"
                },
                new User
                {
                    UserName = "Alice",
                    Email = "awesome_alice@example.com"
                },
                new User
                {
                    UserName = "Bob",
                    Email = "burger_bob@example.com"
                },
                new User
                {
                    UserName = "Bob",
                    Email = "zebras_are_striped_horses@example.com"
                }
            }
            .AsQueryable();
            var sortOption = new SortOption
            {
                PropertyName = "UserName",
                IsDescending = false
            };
            var thenSortOption = new SortOption
            {
                PropertyName = "Email",
                IsDescending = true
            };
            var actual = users.ApplySorting(sortOption, thenSortOption).Cast<User>().ToList();

            Assert.Equal("Alice", actual[0].UserName);
            Assert.Equal("best_alice@example.com", actual[0].Email);
            Assert.Equal("Alice", actual[1].UserName);
            Assert.Equal("awesome_alice@example.com", actual[1].Email);
            Assert.Equal("Bob", actual[2].UserName);
            Assert.Equal("zebras_are_striped_horses@example.com", actual[2].Email);
            Assert.Equal("Bob", actual[3].UserName);
            Assert.Equal("burger_bob@example.com", actual[3].Email);
        }

        [Fact]
        public void ApplySortByUsernameDescending()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "UserName",
                IsDescending = true
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
            Assert.Equal("Zack", actual.First().UserName);
            Assert.Equal("Alice", actual.Last().UserName);
        }

        [Fact]
        public void ApplySortByEmailAscending()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "Email",
                IsDescending = false
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
            Assert.Equal("Arthur.Hank@example.com", actual.First().Email);
            Assert.Equal("Zack@example.com", actual.Last().Email);
        }

        [Fact]
        public void ApplySortByEmailDescending()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "Email",
                IsDescending = true
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
            Assert.Equal("Zack@example.com", actual.First().Email);
            Assert.Equal("Arthur.Hank@example.com", actual.Last().Email);
        }

        [Fact]
        public void ApplySortByUsernameAscendingWithCamelCase()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "userName",
                IsDescending = false
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
            Assert.Equal("Alice", actual.First().UserName);
            Assert.Equal("Zack", actual.Last().UserName);
        }

        [Fact]
        public void ApplySortByUsernameDescendingWithCamelCase()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "userName",
                IsDescending = true
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
            Assert.Equal("Zack", actual.First().UserName);
            Assert.Equal("Alice", actual.Last().UserName);
        }

        [Fact]
        public void ApplySortByEmailAscendingWithCamelCase()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "email",
                IsDescending = false
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
            Assert.Equal("Arthur.Hank@example.com", actual.First().Email);
            Assert.Equal("Zack@example.com", actual.Last().Email);
        }

        [Fact]
        public void ApplySortByEmailDescendingWithCamelCase()
        {
            var users = GetUsers();
            var sortOption = new SortOption
            {
                PropertyName = "email",
                IsDescending = true
            };
            var actual = users.ApplySorting(sortOption, null).Cast<User>();
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
            var invalidSortOption = new SortOption
            {
                PropertyName = "Age"
            };
            var sortedUsers = originalUsers.ApplySorting(invalidSortOption, null).Cast<User>();
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
            var invalidSortOption = new SortOption();
            Assert.Null(invalidSortOption.PropertyName);
            var sortedUsers = originalUsers.ApplySorting(invalidSortOption, null).Cast<User>();
            var sortedEmails = aggregateEmails(sortedUsers);
            Assert.Equal(originalEmails, sortedEmails);
        }

        #region Relational data sorting tests

        [Fact]
        public void ApplySortByTitleAscending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Title",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();
            Assert.Equal("Order 1", actual.First().Title);
            Assert.Equal("Order 3", actual.Last().Title);
        }

        [Fact]
        public void ApplyRelationalSortByProductNameAscending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Name",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();
            Assert.Equal("Apple IPhone", actual.First().Product.Name);
            Assert.Equal("Zipper", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByQuantityAscending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Quantity",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();

            Assert.Equal(2, actual.First().Product.Quantity);
            Assert.Equal("Pear", actual.First().Product.Name);

            Assert.Equal(10, actual.Last().Product.Quantity);
            Assert.Equal("Zipper", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByAmountAscending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Amount",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();
            Assert.Equal(100, actual.First().Product.Amount);
            Assert.Equal("Apple IPhone", actual.First().Product.Name);

            Assert.Equal(200, actual.Last().Product.Amount);
            Assert.Equal("Zipper", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByProductNameDescending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Name",
                IsDescending = true
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();
            Assert.Equal("Zipper", actual.First().Product.Name);
            Assert.Equal("Apple IPhone", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByQuantityDescending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Quantity",
                IsDescending = true
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();

            Assert.Equal(10, actual.First().Product.Quantity);
            Assert.Equal("Zipper", actual.First().Product.Name);

            Assert.Equal(2, actual.Last().Product.Quantity);
            Assert.Equal("Pear", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyRelationalSortByAmountDescending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Amount",
                IsDescending = true
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();

            Assert.Equal(200, actual.First().Product.Amount);
            Assert.Equal("Zipper", actual.First().Product.Name);

            Assert.Equal(100, actual.Last().Product.Amount);
            Assert.Equal("Apple IPhone", actual.Last().Product.Name);
        }

        [Fact]
        public void ApplyThirdLevelRelationalSortByBarcodeAscending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Detail.Barcode",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();

            Assert.Equal("A123456", actual.First().Product.Detail.Barcode);
            Assert.Equal("C123456", actual.Last().Product.Detail.Barcode);
        }

        [Fact]
        public void ApplyThirdLevelRelationalSortByBarcodeDescending()
        {
            var orders = GetOrders();
            var sortOption = new SortOption
            {
                PropertyName = "Product.Detail.Barcode",
                IsDescending = true
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>();

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

            var sortOption = new SortOption
            {
                PropertyName = "Product.Detail",
                IsDescending = true
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>().ToList();

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

            var sortOption = new SortOption
            {
                PropertyName = "Product.Name",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>().ToList();

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

            var sortOption = new SortOption
            {
                PropertyName = "Product.Detail.Barcode",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>().ToList();

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

            var sortOption = new SortOption
            {
                PropertyName = "product.detail.barcode",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>().ToList();

            Assert.Equal("#3", actual[0].Title);
            Assert.Equal("#2", actual[1].Title);
            Assert.Equal("#4", actual[2].Title);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void KeepsNullThirdLevelPropertiesWithCamelCasedQuery()
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

            var sortOption = new SortOption
            {
                PropertyName = "product.detail.barcode",
                IsDescending = false
            };
            Assert.Throws<NullReferenceException>(() => orders.ApplySorting(sortOption, null, false).Cast<Order>().ToList());
        }

        [Fact]
        public void ReturnsUnfilteredForInvalidRelationalSort()
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
               .ToList()
               .AsQueryable();

            var sortOption = new SortOption
            {
                PropertyName = "product.invalid.barcode",
                IsDescending = false
            };
            var actual = orders.ApplySorting(sortOption, null).Cast<Order>().ToList();

            var original = orders.ToList();

            Assert.Equal(original[0].Title, actual[0].Title);
            Assert.Equal(original[1].Title, actual[1].Title);
            Assert.Equal(original[2].Title, actual[2].Title);
            Assert.Equal(original[3].Title, actual[3].Title);
            Assert.Equal(original[4].Title, actual[4].Title);
            Assert.Equal(5, actual.Count);
        }

        #endregion Relational data sorting tests
    }
}
