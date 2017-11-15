using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private int _page = 1;
        private int _pageSize = 20;
        private string _sortProperty;
        private bool _sortDescending;
        private readonly Dictionary<string, string> _additionalQueryParameters = new Dictionary<string, string>();
        private readonly BehaviorSubject<string> _requestUrl = new BehaviorSubject<string>(null);
        private readonly ReplaySubject<PaginationResult<T>> _paginationResultSource = new ReplaySubject<PaginationResult<T>>(1);
        private readonly BehaviorSubject<bool> _requestRunningSource = new BehaviorSubject<bool>(false);
        private readonly Subject<string> _forceRefreshSource = new Subject<string>();
        private IDisposable _querySubscription;
        
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
                    if (paginationResult?.Data != null)
                    {
                        CheckPaginationResponseIfPageOutOfRange(paginationResult);
                        _paginationResultSource.OnNext(paginationResult);
                    }
                });
            BuildUrl();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string BaseUrl => _baseUrl;
        public IObservable<PaginationResult<T>> PaginationResult { get; }
        public IObservable<bool> RequestRunning { get; }

        public int Page
        {
            get => _page;
            set => SetProperty(ref _page, value < 1 ? 1 : value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => SetProperty(ref _pageSize, value < 1 ? 1 : value);
        }

        public string SortProperty => _sortProperty;
        
        public bool SortDescending
        {
            get => _sortDescending;
            set => SetProperty(ref _sortDescending, value);
        }

        private void SetProperty<TProperty>(ref TProperty storage, TProperty value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(value, storage))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                BuildUrl();
            }
        }

        private async Task<PaginationResult<T>> DeserializeResponse(HttpResponseMessage response)
        {
            if (response?.Content == null || !response.IsSuccessStatusCode)
            {
                return null;
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                var deserializedResponse = JsonConvert.DeserializeObject<PaginationResult<T>>(responseContent);
                return deserializedResponse;
            }
            catch
            {
                return null;
            }
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
            var lastPageWithItems = paginationResult.TotalCount / paginationResult.PageSize;
            if (paginationResult.TotalCount % paginationResult.PageSize != 0)
            {
                lastPageWithItems++;
            }
            Page = lastPageWithItems;
        }

        public void SetSortProperty(string propertyName)
        {
            if (propertyName == null)
            {
                SetProperty(ref _sortProperty, null, nameof(SortProperty));
            }
            else
            {
                CheckSortPropertyValidity(propertyName);
                SetProperty(ref _sortProperty, propertyName, nameof(SortProperty));
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
                var parameterName = ExpressionPropertyAccessor.GetPropertyNameFromExpression(sortProperty);
                SetSortProperty(parameterName);
            }
        }

        private void CheckSortPropertyValidity(string propertyName)
        {
            var propertyExistsOnType = ExpressionPropertyAccessor.PropertyExistsOnType<T>(propertyName);
            if (!propertyExistsOnType)
            {
                var typeName = typeof(T).GetTypeInfo().Name;
                throw new ArgumentException($"The property \"{propertyName}\" does not exist on type {typeName}", nameof(propertyName));
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

        private void BuildUrl()
        {
            var query = QueryBuilder.Build(Page, PageSize, SortProperty, SortDescending, _additionalQueryParameters);
            var url = $"{_baseUrl}{query}";
            _requestUrl.OnNext(url);
        }

        public void ForceRefresh()
        {
            var lastUrl = _requestUrl.Value;
            _forceRefreshSource.OnNext(lastUrl);
        }
    }
}
