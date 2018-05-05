import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subject, ReplaySubject, Observable, empty } from 'rxjs';
import { PaginationResult } from './pagination-result';
import { distinctUntilChanged, merge, switchMap, catchError, filter } from 'rxjs/operators';

@Injectable()
export abstract class PaginationBaseService<T> {

    protected paginationResultSource = new ReplaySubject<PaginationResult<T>>(1);
    protected lastPaginationResult: PaginationResult<T>;
    paginationResult = this.paginationResultSource.asObservable();
    private requestUrl = new Subject<string>();
    private forceRefreshUrl = new Subject<string>();

    constructor(protected http: HttpClient) {
        this.requestUrl.pipe(
            distinctUntilChanged(),
            merge(this.forceRefreshUrl),
            switchMap((url: string) => {
                return this.http.get<PaginationResult<T>>(url)
                .pipe(catchError(er => empty()));
            }),
            filter(r => r != null))
            .subscribe(result => {
                this.lastPaginationResult = result;
                this.paginationResultSource.next(result);
            }, error => { /* Does nothing on error */ });
    }

    public forceRefresh() {
        const url = this.buildUrl();
        this.forceRefreshUrl.next(url);
    }

    protected requery() {
        var url = this.buildUrl();
        this.http.get<PaginationResult<T>>(url)
            .subscribe(result => {
                this.lastPaginationResult = result;
                this.paginationResultSource.next(result);
            });
    }

    private _baseUrl: string = null;
    get baseUrl(): string { return this._baseUrl; }
    set baseUrl(value: string) {
        if (value !== this._baseUrl) {
            this._baseUrl = value;
            this.refresh();
        }
    }

    private _page: number = 1;
    get page(): number { return this._page; }
    set page(value: number) {
        if (value !== this._page) {
            this._page = value;
            this.refresh();
        }
    }

    private _pageSize: number = 20;
    get pageSize(): number { return this._pageSize; }
    set pageSize(value: number) {
        if (value !== this._pageSize) {
            this._pageSize = value;
            this.refresh();
        }
    }

    private _sort?: { propertyName: string, isDescending: boolean };
    get sort(): { propertyName: string, isDescending: boolean } | null {
        return this._sort;
    }
    set sort(value: { propertyName: string, isDescending: boolean }) {
        this._sort = value;
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
        var url = this.buildUrl();
        this.requestUrl.next(url);
    }

    private buildUrl(): string {
        var url = `${this.baseUrl}?page=${this.page}&pageSize=${this.pageSize}`;
        if (this.sort) {
            var sortParam = `sort=${this.sort.propertyName} ${this.sort.isDescending ? 'desc' : 'asc'}`;
            url += `&${sortParam}`;
        }
        for (var name in this._additionalQueryParams) {
            if (this._additionalQueryParams.hasOwnProperty(name)) {
                var value = this._additionalQueryParams[name];
                if (value) {
                    url += `&${name}=${value}`;
                }
            }
        }
        return url;
    }
}
