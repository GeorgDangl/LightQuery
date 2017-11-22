
import { TestBed, inject, async } from '@angular/core/testing';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PaginationBaseService } from './pagination-base.service';
import { PaginationResult } from './pagination-result';
import "rxjs/add/operator/take";
import { Observable } from 'rxjs/Observable';
import { setTimeout } from 'core-js/library/web/timers';

describe('PaginationBaseService', () => {

    let getHttpMock: () => HttpTestingController = () => TestBed.get(HttpTestingController);
    let getService: () => UserService = () => TestBed.get(UserService);

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule
            ],
            providers: [
                UserService
            ]
        });
    });

    it('can provide HttpClient', inject([HttpClient], (httpClient: HttpClient) => {
        expect(httpClient).toBeTruthy();
        expect(httpClient instanceof HttpClient).toBeTruthy();
    }));

    it('can be provided', () => {
        let service = getService();
        expect(service).toBeTruthy();
        expect(service instanceof UserService).toBeTruthy();
    });

    it('returns the correct result', async(async () => {
        let service = getService();
        service.baseUrl = '/users';
        let httpMock = getHttpMock();
        const request = httpMock.expectOne('/users?page=1&pageSize=20');
        const paginationResponse: PaginationResult<User> = {
            data: [{ userName: 'George' }],
            page: 1,
            pageSize: 20,
            totalCount: 1
        };
        request.flush(paginationResponse);
        const serviceResult = await service.paginationResult
            .take(1)
            .toPromise();
        expect(serviceResult.totalCount).toBe(1);
        httpMock.verify();
    }));

    it('requeries again on page change', () => {
        let service = getService();
        service.baseUrl = '/users';
        let httpMock = getHttpMock();
        httpMock.expectOne('/users?page=1&pageSize=20');
        httpMock.verify();
        service.page++;
        httpMock.expectOne('/users?page=2&pageSize=20');
        httpMock.verify();
    });

    it('requeries again on pageSize change', () => {
        let service = getService();
        service.baseUrl = '/users';
        let httpMock = getHttpMock();
        httpMock.expectOne('/users?page=1&pageSize=20');
        httpMock.verify();
        service.pageSize++;
        httpMock.expectOne('/users?page=1&pageSize=21');
        httpMock.verify();
    });

    it('requeries again on parameter addition', () => {
        let service = getService();
        service.baseUrl = '/users';
        let httpMock = getHttpMock();
        httpMock.expectOne('/users?page=1&pageSize=20');
        httpMock.verify();
        service.setQueryParameter('filter', 'byName')
        httpMock.expectOne('/users?page=1&pageSize=20&filter=byName');
        httpMock.verify();
    });
    
    it('requeries again on parameter change', () => {
        let service = getService();
        service.baseUrl = '/users';
        let httpMock = getHttpMock();
        httpMock.expectOne('/users?page=1&pageSize=20');
        httpMock.verify();
        service.setQueryParameter('filter', 'byName')
        httpMock.expectOne('/users?page=1&pageSize=20&filter=byName');
        httpMock.verify();
        service.setQueryParameter('filter', 'byAge')
        httpMock.expectOne('/users?page=1&pageSize=20&filter=byAge');
        httpMock.verify();
    });

    it('requeries again on force refresh', () => {
        let service = getService();
        service.baseUrl = '/users';
        let httpMock = getHttpMock();
        httpMock.expectOne('/users?page=1&pageSize=20');
        httpMock.verify();
        service.forceRefresh();
        httpMock.expectOne('/users?page=1&pageSize=20');
        httpMock.verify();
    });

    it('calls the correct url', () => {
        let service = getService();
        service.baseUrl = '/users';
        service.pageSize = 13;
        service.page = 2;
        service.setQueryParameter('filter', 'byName');
        let httpMock = getHttpMock();
        httpMock.expectOne('/users?page=2&pageSize=13&filter=byName');
    });

    it('does not emit result on error response', () => {
        let hasReceivedResponse = false;
        let service = getService();
        service.baseUrl = '/users';
        service.paginationResult.subscribe(r => hasReceivedResponse = true);
        let httpMock = getHttpMock();
        const request = httpMock.expectOne('/users?page=1&pageSize=20');
        request.error(new ErrorEvent('error'));
        expect(hasReceivedResponse).toBeFalsy();
    });

    it('cancels first request when second request send', () => {
        let service = getService();
        service.baseUrl = '/users';
        let httpMock = getHttpMock();
        const firstRequest = httpMock.expectOne('/users?page=1&pageSize=20');
        service.forceRefresh();
        const secondRequest = httpMock.expectOne('/users?page=1&pageSize=20');
        expect(firstRequest.cancelled).toBeTruthy();
        expect(secondRequest.cancelled).toBeFalsy();
    })

    it('does not break observable chain when receiving an error', () => {
        let hasReceivedResponse = false;
        let service = getService();
        service.baseUrl = '/users';
        service.paginationResult.subscribe(r => hasReceivedResponse = true);
        let httpMock = getHttpMock();
        const request = httpMock.expectOne('/users?page=1&pageSize=20');
        request.error(new ErrorEvent('error'));

        expect(hasReceivedResponse).toBeFalsy();
        httpMock.verify();
        service.forceRefresh();
        const secondRequest = httpMock.expectOne('/users?page=1&pageSize=20');
        secondRequest.flush({});
        expect(hasReceivedResponse).toBeTruthy();
    });

});

@Injectable()
class UserService extends PaginationBaseService<User> {

    constructor(protected http: HttpClient) {
        super(http);
    }

}

interface User {
    userName: string;
}
