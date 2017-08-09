using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightQuery.IntegrationTestsServer;
using Xunit;

namespace LightQuery.Tests.Integration.ControllerTests
{
    public class ValuesControllerTests : ControllerTestBase
    {
        [Fact]
        public async Task GetAllValues()
        {
            var url = "/Values";
            var response = await GetResponse<List<User>>(url);
            var expectedUsers = DatabaseInitializer.GetInitialUsers();

            foreach (var expectedUser in expectedUsers)
            {
                Assert.True(response.Any(u => u.UserName == expectedUser.UserName && u.Email == expectedUser.Email));
            }
        }

        [Fact]
        public async Task ValuesAreUnsorted()
        {
            var url = "/Values";
            var response1 = await GetResponse<List<User>>(url);
            var response2 = await GetResponse<List<User>>(url);
            var response3 = await GetResponse<List<User>>(url);
            var response4 = await GetResponse<List<User>>(url);
            Func<IEnumerable<User>, string> aggregateEmails = users =>
                users
                    .Select(u => u.Email)
                    .Aggregate((current, next) => current + next);
            var firstAggregate = aggregateEmails(response1) + aggregateEmails(response2);
            var secondAggregate = aggregateEmails(response3) + aggregateEmails(response4);
            Assert.NotEqual(firstAggregate, secondAggregate);
        }
    }
}
