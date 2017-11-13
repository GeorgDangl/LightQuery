using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightQuery.Client
{
    public class PaginationBaseService<T> : INotifyPropertyChanged
    {
        private readonly string _baseUrl;
        private readonly Func<string, Task<HttpResponseMessage>> _getHttpAsync;
        
        public PaginationBaseService(string baseUrl, Func<string, Task<HttpResponseMessage>> getHttpAsync, DefaultPaginationOptions options = null)
        {
            _baseUrl = baseUrl ?? string.Empty;
            _getHttpAsync = getHttpAsync ?? throw new ArgumentNullException(nameof(getHttpAsync));
            
            PaginationResult = _paginationResultSource.AsObservable();
            RequestRunning = _requestRunningSource.AsObservable();

            if (options != null)
            {
                Page = options.Page;
                PageSize = options.PageSize;
                SetSortProperty(options.SortProperty);
                SortDescending = options.SortDescending;
            }

            _querySubscription = _forceRefreshSource
                .Merge(_requestUrl.DistinctUntilChanged())
                .SelectMany(async url =>
                {
                    try
                    {
                        _requestRunningSource.OnNext(true);
                        url = "https://budgeteria.dangl.me" + url;
                        return await _getHttpAsync(url);
                    }
                    catch
                    {
                        return await Task.FromResult<HttpResponseMessage>(null);
                    }
                })
                .SelectMany(async httpResponse => await DeserializeResponse(httpResponse))
                .Select(result =>
                {
                    _requestRunningSource.OnNext(false);
                    return result;
                })
                .Subscribe(paginationResult =>
                {
                    if (paginationResult != null)
                    {
                        CheckPaginationResponseIfPageOutOfRange(paginationResult);
                        _paginationResultSource.OnNext(paginationResult);
                    }
                });
            BuildUrl();
        }
        private int _page = 1;
        private int _pageSize = 20;
        private string _sortProperty;
        private bool _sortDescending;
        private readonly Dictionary<string, string> _additionalQueryParameters = new Dictionary<string, string>();
        private readonly Subject<string> _requestUrl = new Subject<string>();
        private readonly ReplaySubject<PaginationResult<T>> _paginationResultSource = new ReplaySubject<PaginationResult<T>>(1);
        public IObservable<PaginationResult<T>> PaginationResult { get; }
        private BehaviorSubject<bool> _requestRunningSource = new BehaviorSubject<bool>(false);
        public IObservable<bool> RequestRunning { get; }
        private Subject<string> _forceRefreshSource = new Subject<string>();

        private IDisposable _querySubscription;

        public string BaseUrl => _baseUrl;
        
        private async Task<PaginationResult<T>> DeserializeResponse(HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<PaginationResult<T>>(responseContent);
            return deserializedResponse;
        }

        private void CheckPaginationResponseIfPageOutOfRange(PaginationResult<T> paginationResult)
        {
            if (paginationResult.Data.Count > 0)
            {
                return;
            }
            if (paginationResult.Page == 1)
            {
                return;
            }
            if (paginationResult.TotalCount == 0)
            {
                return;
            }
            if (PageSize <= 0)
            {
                return;
            }
            var lastPageWithItems = paginationResult.TotalCount / paginationResult.PageSize + 1;
            Page = lastPageWithItems;
        }

        public int Page
        {
            get
            {
                return _page;
            }
            set
            {
                if (_page != value)
                {
                    _page = value;
                    OnPropertyChanged();
                    BuildUrl();
                }
            }
        }

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    OnPropertyChanged();
                    BuildUrl();
                }
            }
        }

        public string SortProperty
        {
            get
            {
                return _sortProperty;
            }
        }

        public void SetSortProperty(string propertyName)
        {
            CheckSortPropertyValidity(propertyName);
            var previousSortProperty = _sortProperty;
            if (propertyName == null)
            {
                _sortProperty = null;
            }
            else
            {
                _sortProperty = propertyName;
            }
            if (_sortProperty != previousSortProperty)
            {
                OnPropertyChanged(nameof(SortProperty));
                BuildUrl();
            }
        }

        public void SetSortProperty<TKey>(Expression<Func<T, TKey>> sortProperty)
        {
            if (sortProperty == null)
            {
                SetSortProperty(null);
            }
            else
            {
                var parameterName = string.Empty;
                var expressionBody = sortProperty.Body;
                if (expressionBody is MemberExpression memberExpression)
                {
                    parameterName = memberExpression.Member.Name;
                }
                if (expressionBody is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
                {
                    parameterName = unaryMemberExpression.Member.Name;
                }
                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    var typeName = typeof(T).GetTypeInfo().Name;
                    throw new ArgumentException($"The expression must be a property accessor on {typeName}, e.g. \"t => t.Property\"", nameof(sortProperty));
                }
                SetSortProperty(parameterName);
            }
        }

        private void CheckSortPropertyValidity(string propertyName)
        {
            var propertyExistsOnType = PropertyExistsOnType(propertyName);
            if (!propertyExistsOnType)
            {
                var typeName = typeof(T).GetTypeInfo().Name;
                throw new ArgumentException($"The property \"{propertyName}\" does not exist on type {typeName}", nameof(propertyName));
            }
        }

        private static bool PropertyExistsOnType(string parameterName)
        {
            var propertyExists = typeof(T).GetTypeInfo().DeclaredProperties.Any(p => p.Name == parameterName);
            return propertyExists;
        }

        public bool SortDescending
        {
            get
            {
                return _sortDescending;
            }
            set
            {
                if (_sortDescending != value)
                {
                    _sortDescending = value;
                    OnPropertyChanged();
                    BuildUrl();
                }
            }
        }

        public void SetQueryParameter(string name, string value = null)
        {
            _additionalQueryParameters[name] = value;
            BuildUrl();
        }

        public string GetQueryParameter(string name)
        {
            if (_additionalQueryParameters.ContainsKey(name))
            {
                return _additionalQueryParameters[name];
            }
            return null;
        }

        public void RemoveQueryParameter(string name)
        {
            if (_additionalQueryParameters.ContainsKey(name))
            {
                _additionalQueryParameters.Remove(name);
                BuildUrl();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BuildUrl()
        {
            var query = QueryBuilder.Build(Page, PageSize, SortProperty, SortDescending, _additionalQueryParameters);
            var url = $"{_baseUrl}{query}";
            _requestUrl.OnNext(url);
        }

        public void ForceRefresh()
        {
            var query = QueryBuilder.Build(Page, PageSize, SortProperty, SortDescending, _additionalQueryParameters);
            var url = $"{_baseUrl}{query}";
            _forceRefreshSource.OnNext(url);
        }
    }
}
