using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Xunit;

namespace LightQuery.Client.Tests
{
    public class PaginationBaseServiceTests
    {
        [Fact]
        public void AcceptsNullBaseUrl()
        {
            var service = new PaginationBaseService<MockPayload>(null, x => Task.FromResult(new HttpResponseMessage()));
            Assert.NotNull(service);
        }
        
        [Fact]
        public void SetsBaseUrlToStringEmptyForEmptyBaseUrl()
        {
            var service = new PaginationBaseService<MockPayload>(null, x => Task.FromResult(new HttpResponseMessage()));
            Assert.Equal(string.Empty, service.BaseUrl);
        }

        [Fact]
        public void AcceptsEmptyBaseUrl()
        {
            var service = new PaginationBaseService<MockPayload>(string.Empty, x => Task.FromResult(new HttpResponseMessage()));
            Assert.NotNull(service);
        }

        [Fact]
        public void AcceptsAbsoluteBaseUrl()
        {
            var baseUrl = "https://example.com";
            var service = new PaginationBaseService<MockPayload>(baseUrl, x => Task.FromResult(new HttpResponseMessage()));
            Assert.Equal(baseUrl, service.BaseUrl);
        }

        [Fact]
        public void AcceptsRelativeBaseUrl()
        {
            var baseUrl = "/api/values";
            var service = new PaginationBaseService<MockPayload>(baseUrl, x => Task.FromResult(new HttpResponseMessage()));
            Assert.Equal(baseUrl, service.BaseUrl);
        }

        [Fact]
        public void ArgumentNullExceptionForNullGetHttpAsyncAction()
        {
            Assert.Throws<ArgumentNullException>("getHttpAsync", () => new PaginationBaseService<MockPayload>(string.Empty, (Func<string, Task<HttpResponseMessage>>)null));
        }

        [Fact]
        public void ArgumentExceptionForInvalidSortPropertyInDefaultOptions()
        {
            Assert.Throws<ArgumentException>("propertyName", () => new PaginationBaseService<MockPayload>(string.Empty, x => Task.FromResult(new HttpResponseMessage()), new DefaultPaginationOptions {SortProperty = "InvalidProp"}));
        }

        [Fact]
        public void SetsPropertiesFromDefaultOptions()
        {
            var service = new PaginationBaseService<MockPayload>(string.Empty, x => Task.FromResult(new HttpResponseMessage()), new DefaultPaginationOptions
            {
                Page = 12,
                PageSize = 4,
                SortDescending = true,
                SortProperty = "Name"
            });
            Assert.Equal(12, service.Page);
            Assert.Equal(4, service.PageSize);
            Assert.True(service.SortDescending);
            Assert.Equal("Name", service.SortProperty);
        }

        public class Cancellation
        {
            [Fact]
            public async Task CancelsFirstRequestWhenSecondOneIsSent_SortPropertyChanged()
            {
                var results = 0;
                var requestsSent = 0;
                Func<string, Task<HttpResponseMessage>> getHttpAsync = async _ =>
                {
                    requestsSent++;
                    return (await GetResponseWithDelay(CancellationToken.None)).Item1;
                };
                var paginationService = new PaginationBaseService<User>("https://example.com", getHttpAsync);
                paginationService.PaginationResult.Subscribe(_ => results++);

                await Task.WhenAny(paginationService.PaginationResult.FirstAsync().ToTask(),
                     Task.Delay(10_000)); // Timeout

                Assert.Equal(1, results);
                Assert.Equal(1, requestsSent);

                paginationService.SetSortProperty("Email");
                paginationService.SetSortProperty("UserName");

                await Task.WhenAny(paginationService.PaginationResult.Skip(1).FirstAsync().ToTask(),
                    Task.Delay(10_000)); // Timeout

                Assert.Equal(2, results);
                Assert.Equal(3, requestsSent);
            }

            [Fact]
            public async Task CancelsFirstRequestWhenSecondOneIsSent_PageChanged()
            {
                var results = 0;
                var requestsSent = 0;
                Func<string, Task<HttpResponseMessage>> getHttpAsync = async _ =>
                {
                    requestsSent++;
                    return (await GetResponseWithDelay(CancellationToken.None)).Item1;
                };
                var paginationService = new PaginationBaseService<User>("https://example.com", getHttpAsync);
                paginationService.PaginationResult.Subscribe(_ => results++);

                await Task.WhenAny(paginationService.PaginationResult.FirstAsync().ToTask(),
                     Task.Delay(10_000)); // Timeout

                Assert.Equal(1, results);
                Assert.Equal(1, requestsSent);

                paginationService.Page++;
                paginationService.Page++;

                await Task.WhenAny(paginationService.PaginationResult.Skip(1).FirstAsync().ToTask(),
                    Task.Delay(10_000)); // Timeout

                Assert.Equal(2, results);
                Assert.Equal(3, requestsSent);
            }

            [Fact]
            public async Task CancelsFirstRequestWhenSecondOneIsSent_UsesCancellationToken()
            {
                var results = 0;
                var requestsSent = 0;
                var requestsCancelled = new bool[3];
                var responseTasks = new List<Task>();
                var currentRequests = 0;
                Func<string, CancellationToken, Task<HttpResponseMessage>> getHttpAsync = async (_, token) =>
                {
                    requestsSent++;
                    var currentRequest = currentRequests++;
                    var responseTask = GetResponseWithDelay(token);
                    responseTasks.Add(responseTask);
                    var response = await responseTask;
                    requestsCancelled[currentRequest] = response.Item2;
                    return response.Item1;
                };
                var paginationService = new PaginationBaseService<User>("https://example.com", getHttpAsync);
                paginationService.PaginationResult.Subscribe(_ => results++);

                await Task.WhenAny(paginationService.PaginationResult.FirstAsync().ToTask(),
                     Task.Delay(10_000)); // Timeout

                Assert.Equal(1, results);
                Assert.Equal(1, requestsSent);

                paginationService.Page++;
                paginationService.Page++;

                await Task.WhenAny(paginationService.PaginationResult.Skip(1).FirstAsync().ToTask(),
                    Task.Delay(10_000)); // Timeout

                Assert.Equal(2, results);
                Assert.Equal(3, requestsSent);

                Assert.False(requestsCancelled[0]);
                Assert.True(requestsCancelled[1]);
                Assert.False(requestsCancelled[2]);
            }

            private class User
            {
                public string Email { get; set; }
                public string UserName { get; set; }
            }

            private async Task<(HttpResponseMessage, bool)> GetResponseWithDelay(CancellationToken cancellationToken)
            {
                // Adding a short delay to simulate a long running operation, e.g. a web request
                await Task.Delay(100);

                var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                var memStream = new MemoryStream();
                var usersPaginated = new PaginationResult<User>
                {
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    Data = new List<User>
                {
                    new User
                    {
                        Email = "george@example.com",
                        UserName = "George"
                    }
                }
                };
                var usersJson = JsonConvert.SerializeObject(usersPaginated);
                using (var sw = new StreamWriter(memStream, Encoding.UTF8, 2048, true))
                {
                    await sw.WriteAsync(usersJson);
                }

                memStream.Position = 0;
                httpResponse.Content = new StreamContent(memStream);

                return (httpResponse, cancellationToken.IsCancellationRequested);
            }
        }

        public class ForceRefresh
        {
            private readonly PaginationBaseService<MockPayload> _service;
            private bool _hasCalled = false;

            public ForceRefresh()
            {
                _service = new PaginationBaseService<MockPayload>(string.Empty, x =>
                {
                    _hasCalled = true;
                    return Task.FromResult(new HttpResponseMessage());
                });
            }

            [Fact]
            public void CallsAgainOnForceRefresh()
            {
                _hasCalled = false;
                _service.ForceRefresh();
                Assert.True(_hasCalled);
            }
        }

        public class RequestRunningNotifications
        {
            private readonly PaginationBaseService<MockPayload> _service;
            private readonly List<bool> _isRunningEvents = new List<bool>();

            public RequestRunningNotifications()
            {
                _service = new PaginationBaseService<MockPayload>(string.Empty, x => Task.FromResult(new HttpResponseMessage()));
                _service.RequestRunning.Subscribe(isRunning => _isRunningEvents.Add(isRunning));
            }

            [Fact]
            public void HasOneInitialEventWhichIsFalse()
            {
                Assert.Single(_isRunningEvents);
                Assert.False(_isRunningEvents.First());
            }

            [Fact]
            public void RaisesIsRunningAndReleasesOnForceRefresh()
            {
                _isRunningEvents.Clear();
                Assert.Empty(_isRunningEvents);
                _service.ForceRefresh();
                Assert.Equal(2, _isRunningEvents.Count);
                Assert.True(_isRunningEvents[0]); // First notification about is running
                Assert.False(_isRunningEvents[1]); // Second to indicate request is finished
            }

            [Fact]
            public void RaisesIsRunningAndReleasesOnUrlChange()
            {
                _isRunningEvents.Clear();
                Assert.Empty(_isRunningEvents);
                _service.Page++;
                Assert.Equal(2, _isRunningEvents.Count);
                Assert.True(_isRunningEvents[0]); // First notification about is running
                Assert.False(_isRunningEvents[1]); // Second to indicate request is finished
            }
        }

        public class Disposable
        {
            private readonly PaginationBaseService<MockPayload> _service;
            private bool _hasCalled = false;

            public Disposable()
            {
                _service = new PaginationBaseService<MockPayload>(string.Empty, x =>
                {
                    _hasCalled = true;
                    return Task.FromResult(new HttpResponseMessage());
                });
            }

            [Fact]
            public void DoesNotCallAfterForceRefreshWhenDisposed()
            {
                _hasCalled = false;
                _service.ForceRefresh();
                Assert.True(_hasCalled);
                _hasCalled = false;
                _service.Dispose();
                _service.ForceRefresh();
                Assert.False(_hasCalled);
            }

            [Fact]
            public void DoesNotCallOnUrlChangeWhenDisposed()
            {
                _hasCalled = false;
                _service.Page++;
                Assert.True(_hasCalled);
                _hasCalled = false;
                _service.Dispose();
                _service.Page++;
                Assert.False(_hasCalled);
            }
        }

        public class Refreshing
        {
            private PaginationBaseService<MockPayload> _service;
            private HttpResponseMessage _response;
            private PaginationResult<MockPayload> _receivedResult;
            private bool _hasRaisedNewPaginationResult;

            private void InitializeServiceAndResetObservedValues(object payload = null)
            {
                _response = GetResponse(payload);
                _service = new PaginationBaseService<MockPayload>(string.Empty, x => Task.FromResult(_response));
                _service.PaginationResult.Subscribe(r =>
                {
                    _hasRaisedNewPaginationResult = true;
                    _receivedResult = r;
                });
                _receivedResult = null;
                _hasRaisedNewPaginationResult = false;
            }

            private HttpResponseMessage GetResponse(object payload = null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = GetJsonContent(payload)
                };
                return response;
            }

            private StringContent GetJsonContent(object payload = null)
            {
                if (payload == null)
                {
                    var payloadContent = Enumerable.Range(0, 5).Select(x => new MockPayload { Name = "PayloadName" });
                    payload = new PaginationResult<MockPayload>
                    {
                        Data = payloadContent.ToList()
                    };
                }
                var json = JsonConvert.SerializeObject(payload);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                return jsonContent;
            }

            [Fact]
            public void RefreshesOnPageChange()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _service.Page++;
                Assert.True(_hasRaisedNewPaginationResult);
            }

            [Fact]
            public void RefreshesOnPageSizeChange()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _service.PageSize++;
                Assert.True(_hasRaisedNewPaginationResult);
            }

            [Fact]
            public void RefreshesOnSortPropertyChange()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _service.SetSortProperty("Name");
                Assert.True(_hasRaisedNewPaginationResult);
            }
            [Fact]
            public void RefreshesOnSortDirectionChange()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _service.SetSortProperty("Name");
                _receivedResult = null;
                _hasRaisedNewPaginationResult = false;
                Assert.False(_hasRaisedNewPaginationResult);
                _service.SortDescending = !_service.SortDescending;
                Assert.True(_hasRaisedNewPaginationResult);
            }

            [Fact]
            public void RefreshesOnAddCustomParameter()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _service.SetQueryParameter("includeDetails");
                Assert.True(_hasRaisedNewPaginationResult);
            }

            [Fact]
            public void RefreshesOnRemoveCustomParameter()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _service.SetQueryParameter("includeDetails");
                _receivedResult = null;
                _hasRaisedNewPaginationResult = false;
                Assert.False(_hasRaisedNewPaginationResult);
                _service.RemoveQueryParameter("includeDetails");
                Assert.True(_hasRaisedNewPaginationResult);
            }

            [Fact]
            public void DoesNotRefreshWhenPageIsSetToSame()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _service.Page = _service.Page;
                Assert.False(_hasRaisedNewPaginationResult);
            }

            [Fact]
            public void DoesNotBreakObservableChainForErrorenousResponse()
            {
                InitializeServiceAndResetObservedValues();
                Assert.False(_hasRaisedNewPaginationResult);
                _response = GetResponse(new {InvalidData = "Here be pirates"});
                _service.ForceRefresh();
                Assert.False(_hasRaisedNewPaginationResult);
                _response = GetResponse();
                _service.ForceRefresh();
                Assert.True(_hasRaisedNewPaginationResult);
            }
        }

        public class ResponseDeserialization
        {
            private PaginationBaseService<MockPayload> _service;
            private HttpResponseMessage _response;
            private PaginationResult<MockPayload> _receivedResult;
            private bool _hasRaisedNewPaginationResult = false;

            private void InitializeService()
            {
                _service = new PaginationBaseService<MockPayload>(string.Empty, x => Task.FromResult(_response));
                _service.PaginationResult.Subscribe(r =>
                {
                    _hasRaisedNewPaginationResult = true;
                    _receivedResult = r;
                });
            }

            private StringContent GetJsonContent(object payload)
            {
                var json = JsonConvert.SerializeObject(payload);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                return jsonContent;
            }

            [Fact]
            public void CanDeserializeResponse()
            {
                var payloadContent = Enumerable.Range(0, 5).Select(x => new MockPayload {Name = "PayloadName"});
                var payload = new PaginationResult<MockPayload>
                {
                    Data = payloadContent.ToList()
                };
                _response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = GetJsonContent(payload)
                };
                InitializeService();
                Assert.True(_hasRaisedNewPaginationResult);
                Assert.NotNull(_receivedResult);
                Assert.All(_receivedResult.Data, m => Assert.True(m.Name == "PayloadName"));
            }

            [Fact]
            public void DoesNothingForErrorResponse()
            {
                _response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                InitializeService();
                Assert.False(_hasRaisedNewPaginationResult);
            }

            [Fact]
            public void DoesNothingForSuccessResponseWithInvalidBody()
            {
                _response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new MemoryStream(new byte[] {12, 13, 14, 15, 16, 17, 18, 19}))
                };
                InitializeService();
                Assert.False(_hasRaisedNewPaginationResult);
            }
        }

        public class PageRangeCheck
        {
            private PaginationBaseService<MockPayload> _service;
            private int _initialResponsePage = 1;
            private int _lastCalledResponsePage;
            private int _firstCalledResponsePage;
            private int _responsePageSize = 20;
            private int _responseTotalCount = 20;

            private void InitializeService()
            {
                _service = new PaginationBaseService<MockPayload>(string.Empty, x =>
                {
                    var response = GetResponse();
                    var parsedQuery = QueryHelpers.ParseQuery(x);
                    _lastCalledResponsePage = Convert.ToInt32(parsedQuery["page"]);
                    if (_firstCalledResponsePage == default)
                    {
                        _firstCalledResponsePage = _lastCalledResponsePage;
                    }
                    return Task.FromResult(response);
                }, new DefaultPaginationOptions{ Page = _initialResponsePage });
            }

            private HttpResponseMessage GetResponse()
            {
                var content = new PaginationResult<MockPayload>
                {
                    Data = new List<MockPayload>(),
                    Page = _lastCalledResponsePage == 0 ? _initialResponsePage : _lastCalledResponsePage,
                    PageSize = _responsePageSize,
                    TotalCount = _responseTotalCount
                };
                var json = JsonConvert.SerializeObject(content);
                var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = jsonContent
                };
            }

            [Fact]
            public void CallsAgainWithLastPageContainingItemsIfPageOutOfRange_01()
            {
                _initialResponsePage = 2;
                InitializeService();
                Assert.Equal(1, _lastCalledResponsePage);
                Assert.Equal(2, _firstCalledResponsePage);
            }

            [Fact]
            public void CallsAgainWithLastPageContainingItemsIfPageOutOfRange_02()
            {
                _responseTotalCount = 19;
                _initialResponsePage = 2;
                InitializeService();
                Assert.Equal(1, _lastCalledResponsePage);
                Assert.Equal(2, _firstCalledResponsePage);
            }

            [Fact]
            public void CallsAgainWithLastPageContainingItemsIfPageOutOfRange_03()
            {
                _responseTotalCount = 21;
                _initialResponsePage = 3;
                InitializeService();
                Assert.Equal(2, _lastCalledResponsePage);
                Assert.Equal(3, _firstCalledResponsePage);
            }
        }

        public class UrlBuilder
        {
            private readonly string _baseUrl = "https://www.example.com/api/v1";
            private PaginationBaseService<MockPayload> _service;
            private string _lastRequestedUrl;

            public UrlBuilder()
            {
                _service = new PaginationBaseService<MockPayload>(_baseUrl, x =>
                {
                    _lastRequestedUrl = x;
                    return Task.FromResult(new HttpResponseMessage());
                });
            }

            [Fact]
            public void CallsOnInitialization()
            {
                var expectedUrl = $"{_baseUrl}?page=1&pageSize=20";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnInitializationWithCustomOptions()
            {
                _service = new PaginationBaseService<MockPayload>(_baseUrl, x =>
                {
                    _lastRequestedUrl = x;
                    return Task.FromResult(new HttpResponseMessage());
                }, new DefaultPaginationOptions
                {
                    Page = 3,
                    PageSize = 17,
                    SortDescending = true,
                    SortProperty = "Name"
                });
                var expectedUrl = $"{_baseUrl}?page=3&pageSize=17&sort=Name%20desc";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnPageChange()
            {
                _lastRequestedUrl = null;
                _service.Page = 3;
                var expectedUrl = $"{_baseUrl}?page=3&pageSize=20";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnPageSizeChange()
            {
                _lastRequestedUrl = null;
                _service.PageSize = 3;
                var expectedUrl = $"{_baseUrl}?page=1&pageSize=3";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnSortPropertyChange()
            {
                _lastRequestedUrl = null;
                _service.SetSortProperty("Name");
                var expectedUrl = $"{_baseUrl}?page=1&pageSize=20&sort=Name%20asc";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnSortDescendingChange()
            {
                _service.SetSortProperty("Name");
                _lastRequestedUrl = null;
                _service.SortDescending = true;
                var expectedUrl = $"{_baseUrl}?page=1&pageSize=20&sort=Name%20desc";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnQueryParameterAdded()
            {
                _service.SetQueryParameter("customParam", "Bob");
                var expectedUrl = $"{_baseUrl}?page=1&pageSize=20&customParam=Bob";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnQueryParameterChanged()
            {
                _service.SetQueryParameter("customParam", "Bob");
                _lastRequestedUrl = null;
                _service.SortDescending = true;
                _service.SetQueryParameter("customParam", "Jack");
                var expectedUrl = $"{_baseUrl}?page=1&pageSize=20&customParam=Jack";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }

            [Fact]
            public void CallsOnQueryParameterRemoved()
            {
                _service.SetQueryParameter("customParam", "Bob");
                _lastRequestedUrl = null;
                _service.SortDescending = true;
                _service.RemoveQueryParameter("customParam");
                var expectedUrl = $"{_baseUrl}?page=1&pageSize=20";
                Assert.Equal(expectedUrl, _lastRequestedUrl);
            }
        }

        public class PropertyChanged
        {
            private string _lastChangedProperty;

            public PropertyChanged()
            {
                _service.PropertyChanged += (s, e) =>
                {
                    _lastChangedProperty = e.PropertyName;
                };
            }

            private readonly PaginationBaseService<MockPayload> _service = new PaginationBaseService<MockPayload>(string.Empty, x => Task.FromResult(new HttpResponseMessage()));

            [Fact]
            public void UpdatesPage()
            {
                _service.Page = 13;
                Assert.Equal(nameof(_service.Page), _lastChangedProperty);
                Assert.Equal(13, _service.Page);
            }

            [Fact]
            public void DoesntUpdatePageForSameValue()
            {
                _service.Page = _service.Page;
                Assert.Null(_lastChangedProperty);
            }

            [Fact]
            public void SetsToOneForNegativePage()
            {
                _service.Page = -14;
                Assert.Equal(1, _service.Page);
            }

            [Fact]
            public void SetsToOneForZeroPage()
            {
                _service.Page = 0;
                Assert.Equal(1, _service.Page);
            }

            [Fact]
            public void UpdatesPageSize()
            {
                _service.PageSize = 13;
                Assert.Equal(nameof(_service.PageSize), _lastChangedProperty);
                Assert.Equal(13, _service.PageSize);
            }

            [Fact]
            public void DoesntUpdatePageSizeForSameValue()
            {
                _service.PageSize = _service.PageSize;
                Assert.Null(_lastChangedProperty);
            }

            [Fact]
            public void SetsToOneForNegativePageSize()
            {
                _service.PageSize = -14;
                Assert.Equal(1, _service.PageSize);
            }

            [Fact]
            public void SetsToOneForZeroPageSize()
            {
                _service.PageSize = 0;
                Assert.Equal(1, _service.PageSize);
            }

            [Fact]
            public void UpdatesSortPropertyViaString()
            {
                _service.SetSortProperty(nameof(MockPayload.Name));
                Assert.Equal(nameof(_service.SortProperty), _lastChangedProperty);
                Assert.Equal(nameof(MockPayload.Name), _service.SortProperty);
            }

            [Fact]
            public void UpdatesSortPropertyViaExpression()
            {
                _service.SetSortProperty(x => x.Name);
                Assert.Equal(nameof(_service.SortProperty), _lastChangedProperty);
                Assert.Equal(nameof(MockPayload.Name), _service.SortProperty);
            }

            [Fact]
            public void DoesntUpdateSortPropertyForSameValueViaString()
            {
                _service.SetSortProperty(nameof(MockPayload.Name));
                _lastChangedProperty = null;
                _service.SetSortProperty(nameof(MockPayload.Name));
                Assert.Null(_lastChangedProperty);
            }

            [Fact]
            public void DoesntUpdateSortPropertyForSameValueViaExpression()
            {
                _service.SetSortProperty(x => x.Name);
                _lastChangedProperty = null;
                _service.SetSortProperty(x => x.Name);
                Assert.Null(_lastChangedProperty);
            }

            [Fact]
            public void ArgumentExceptionForIllegalSortPropertyViaString()
            {
                Assert.Throws<ArgumentException>("propertyName", () => _service.SetSortProperty("NonProperty"));
            }

            [Fact]
            public void ArgumentExceptionForIllegalSortPropertyViaExpression()
            {
                Assert.Throws<ArgumentException>("propertyName", () => _service.SetSortProperty(x => x.Name.Length));
            }

            [Fact]
            public void ArgumentExceptionForNonPropertyAccessorForSortPropertyViaExpression()
            {
                Assert.Throws<ArgumentException>("property", () => _service.SetSortProperty(x => x.GetType()));
            }

            [Fact]
            public void UpdatesSortDescending()
            {
                _service.SortDescending = !_service.SortDescending;
                Assert.Equal(nameof(_service.SortDescending), _lastChangedProperty);
            }

            [Fact]
            public void DoesntUpdateSortDescendingForSameValue()
            {
                _service.SortDescending = _service.SortDescending;
                Assert.Null(_lastChangedProperty);
            }
        }
    }
}
