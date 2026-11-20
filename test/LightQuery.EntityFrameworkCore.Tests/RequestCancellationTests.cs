using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LightQuery.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace LightQuery.EntityFrameworkCore.Tests
{
    public class RequestCancellationTests
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task PassesRequestTokenToQueries(bool forcePagination, bool requestPagination)
        {
            using var cancellation = new CancellationTokenSource();
            var state = new QueryState();
            var context = CreateContext(state, cancellation.Token, requestPagination);
            var nextCalled = false;

            await new AsyncLightQueryAttribute(forcePagination).OnResultExecutionAsync(context, () =>
            {
                nextCalled = true;
                return Task.FromResult(new ResultExecutedContext(context, context.Filters, context.Result, context.Controller));
            });

            Assert.True(nextCalled);
            Assert.Equal<CancellationToken?>(cancellation.Token, state.EnumerationToken);
            var result = Assert.IsType<ObjectResult>(context.Result);
            if (forcePagination || requestPagination)
            {
                Assert.Equal<CancellationToken?>(cancellation.Token, state.CountToken);
                var pagination = Assert.IsType<PaginationResult<object>>(result.Value);
                Assert.Equal(3, pagination.TotalCount);
                Assert.Equal(requestPagination ? 2 : 1, pagination.Page);
                Assert.Equal(requestPagination ? new object[] { 2 } : new object[] { 1, 2, 3 }, pagination.Data);
            }
            else
            {
                Assert.Null(state.CountToken);
                Assert.Equal(new object[] { 1, 2, 3 }, Assert.IsType<List<object>>(result.Value));
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task PropagatesCancellationBeforeQueryExecution(bool forcePagination, bool requestPagination)
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var state = new QueryState();
            var context = CreateContext(state, cancellation.Token, requestPagination);
            var nextCalled = false;

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                new AsyncLightQueryAttribute(forcePagination).OnResultExecutionAsync(context, () =>
                {
                    nextCalled = true;
                    return Task.FromResult(new ResultExecutedContext(context, context.Filters, context.Result, context.Controller));
                }));

            Assert.Equal(cancellation.Token, exception.CancellationToken);
            Assert.False(nextCalled);
            if (forcePagination || requestPagination)
            {
                Assert.Equal<CancellationToken?>(cancellation.Token, state.CountToken);
                Assert.Null(state.EnumerationToken);
            }
            else
            {
                Assert.Null(state.CountToken);
                Assert.Equal<CancellationToken?>(cancellation.Token, state.EnumerationToken);
            }
        }

        [Theory]
        [InlineData(false, true)]
        [InlineData(true, false)]
        public async Task PropagatesCancellationBetweenCountAndMaterialization(bool forcePagination, bool requestPagination)
        {
            using var cancellation = new CancellationTokenSource();
            var state = new QueryState { OnCountCompleted = cancellation.Cancel };
            var context = CreateContext(state, cancellation.Token, requestPagination);
            var nextCalled = false;

            var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                new AsyncLightQueryAttribute(forcePagination).OnResultExecutionAsync(context, () =>
                {
                    nextCalled = true;
                    return Task.FromResult(new ResultExecutedContext(context, context.Filters, context.Result, context.Controller));
                }));

            Assert.Equal(cancellation.Token, exception.CancellationToken);
            Assert.Equal<CancellationToken?>(cancellation.Token, state.CountToken);
            Assert.Equal<CancellationToken?>(cancellation.Token, state.EnumerationToken);
            Assert.False(nextCalled);
        }

        private static ResultExecutingContext CreateContext(QueryState state, CancellationToken token, bool requestPagination)
        {
            var httpContext = new DefaultHttpContext { RequestAborted = token };
            if (requestPagination)
            {
                httpContext.Request.QueryString = new QueryString("?page=2&pageSize=1");
            }
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var query = new AsyncQuery<int>(new[] { 1, 2, 3 }, state);
            return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), new ObjectResult(query), new object());
        }

        private class QueryState
        {
            public CancellationToken? CountToken { get; set; }
            public CancellationToken? EnumerationToken { get; set; }
            public Action OnCountCompleted { get; set; }
        }

        private class AsyncQuery<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            private readonly QueryState _state;

            public AsyncQuery(IEnumerable<T> values, QueryState state) : base(values)
            {
                _state = state;
            }

            public AsyncQuery(Expression expression, QueryState state) : base(expression)
            {
                _state = state;
            }

            IQueryProvider IQueryable.Provider => new AsyncQueryProvider(new EnumerableQuery<T>(((IQueryable)this).Expression), _state);

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                _state.EnumerationToken = cancellationToken;
                return new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator(), cancellationToken);
            }
        }

        private class AsyncQueryProvider : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            private readonly QueryState _state;

            public AsyncQueryProvider(IQueryable query, QueryState state)
            {
                _inner = query.Provider;
                _state = state;
            }

            public IQueryable CreateQuery(Expression expression) => throw new NotSupportedException();

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new AsyncQuery<TElement>(expression, _state);

            public object Execute(Expression expression) => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                if (typeof(TResult) != typeof(Task<int>) || !(expression is MethodCallExpression call) || call.Method.Name != nameof(Queryable.Count))
                {
                    throw new NotSupportedException("Only asynchronous Count queries are supported by this test provider.");
                }
                _state.CountToken = cancellationToken;
                cancellationToken.ThrowIfCancellationRequested();
                var count = _inner.Execute<int>(expression);
                _state.OnCountCompleted?.Invoke();
                return (TResult)(object)Task.FromResult(count);
            }
        }

        private class AsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            private readonly CancellationToken _cancellationToken;

            public AsyncEnumerator(IEnumerator<T> inner, CancellationToken cancellationToken)
            {
                _inner = inner;
                _cancellationToken = cancellationToken;
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return default;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                _cancellationToken.ThrowIfCancellationRequested();
                return new ValueTask<bool>(_inner.MoveNext());
            }
        }
    }
}
