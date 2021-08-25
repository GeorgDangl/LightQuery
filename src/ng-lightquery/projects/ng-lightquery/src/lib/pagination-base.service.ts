import { EMPTY, Observable, ReplaySubject, Subject, merge, of } from 'rxjs';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  filter,
  switchMap,
  take,
} from 'rxjs/operators';

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationResult } from './pagination-result';

@Injectable()
export abstract class PaginationBaseService<T> {
  protected paginationResultSource = new ReplaySubject<PaginationResult<T>>(1);
  protected lastPaginationResult: PaginationResult<T>;
  paginationResult = this.paginationResultSource.asObservable();
  private requestUrl = new Subject<string>();
  private forceRefreshUrl = new Subject<string>();

  constructor(protected http: HttpClient) {
    merge(this.requestUrl.pipe(distinctUntilChanged()), this.forceRefreshUrl)
      .pipe(
        // We're putting a 0 (zero) debounce time here, mostly to
        // ensure that when multiple changes are applied via code, e.g.
        // setting a page number and a query parameter, we don't send two requests
        // and cancel the first but wait until the code has executed
        // to just send a single request
        debounceTime(0),
        switchMap((url: string) => {
          return this.http
            .get<PaginationResult<T>>(url)
            .pipe(catchError((_) => EMPTY));
        }),
        filter((r) => r != null)
      )
      .subscribe(
        (result) => {
          this.lastPaginationResult = result;
          this.paginationResultSource.next(result);
        },
        (error) => {
          /* Does nothing on error */
        }
      );
  }

  public forceRefresh() {
    const url = this.buildUrl();
    this.forceRefreshUrl.next(url);
  }

  protected requery() {
    const url = this.buildUrl();
    this.http.get<PaginationResult<T>>(url).subscribe((result) => {
      this.lastPaginationResult = result;
      this.paginationResultSource.next(result);
    });
  }

  private _baseUrl: string = null;
  get baseUrl(): string {
    return this._baseUrl;
  }
  set baseUrl(value: string) {
    if (value !== this._baseUrl) {
      this._baseUrl = value;
      this.refresh();
    }
  }

  private _page: number = 1;
  get page(): number {
    return this._page;
  }
  set page(value: number) {
    if (value !== this._page) {
      this._page = value;
      this.refresh();
    }
  }

  private _pageSize: number = 20;
  get pageSize(): number {
    return this._pageSize;
  }
  set pageSize(value: number) {
    if (value !== this._pageSize) {
      this._pageSize = value;
      this.refresh();
    }
  }

  private _sort?: { propertyName: string; isDescending: boolean } | null;
  get sort(): { propertyName: string; isDescending: boolean } | null {
    return this._sort;
  }
  set sort(value: { propertyName: string; isDescending: boolean } | null) {
    this._sort = value;
    this.refresh();
  }

  private _thenSort?: { propertyName: string; isDescending: boolean } | null;
  get thenSort(): { propertyName: string; isDescending: boolean } | null {
    return this._thenSort;
  }
  set thenSort(value: { propertyName: string; isDescending: boolean } | null) {
    this._thenSort = value;
    this.refresh();
  }

  private _additionalQueryParams: { [parameter: string]: string } = {};
  setQueryParameter(name: string, value: string) {
    this._additionalQueryParams[name] = value;
    this.refresh();
  }
  getQueryParameter(parameterName: string): string | null {
    return this._additionalQueryParams[parameterName] || null;
  }

  private refresh() {
    if (!this.baseUrl) {
      return;
    }
    const url = this.buildUrl();
    this.requestUrl.next(url);
  }

  private buildUrl(): string {
    let url = `${this.baseUrl}?page=${this.page}&pageSize=${this.pageSize}`;
    if (this.sort) {
      const sortParam = `sort=${this.sort.propertyName} ${
        this.sort.isDescending ? 'desc' : 'asc'
      }`;
      url += `&${sortParam}`;
    }

    if (this.thenSort && this.thenSort.propertyName) {
      const thenSortParam = `thenSort=${this.thenSort.propertyName} ${
        this.thenSort.isDescending ? 'desc' : 'asc'
      }`;
      url += `&${thenSortParam}`;
    }

    for (const name in this._additionalQueryParams) {
      if (this._additionalQueryParams.hasOwnProperty(name)) {
        const value = this._additionalQueryParams[name];
        if (value) {
          url += `&${name}=${value}`;
        }
      }
    }
    return url;
  }

  private buildUrlAll(page: number): string {
    const url = `${this.baseUrl}?page=${page}&pageSize=500`;
    return url;
  }

  getAll(): Observable<T[]> {
    if (!this.baseUrl) {
      return of(null);
    }
    let hasMore = true;
    let currentPage = 1;
    const listResultAll: T[] = [];
    const listResultAllSource = new Subject<T[]>();

    const getData = () => {
      if (!hasMore) {
        listResultAllSource.next(listResultAll);
        listResultAllSource.complete();
        return;
      }

      const url = this.buildUrlAll(currentPage++);
      this.http.get<PaginationResult<T>>(url).subscribe((c) => {
        if (c.page !== currentPage - 1) {
          hasMore = false;
        } else if (c.data.length) {
          listResultAll.push(...c.data);
          hasMore = true;
        }
        getData();
      });
    };
    getData();

    return listResultAllSource.asObservable().pipe(take(1));
  }
}
