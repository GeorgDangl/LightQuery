
import { TestBed, inject, async } from '@angular/core/testing';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PaginationBaseService } from './pagination-base.service';
import { PaginationResult } from './pagination-result';
import { take, skip } from 'rxjs/operators';

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
            data: [{ id: 1, userName: 'George' }],
            page: 1,
            pageSize: 20,
            totalCount: 1
        };
        request.flush(paginationResponse);
        const serviceResult = await service.paginationResult
        .pipe(take(1))
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

    it('updates cached last pagination result before anouncing new result with forceRefresh', async(() => {
        asyncStyleCacheCheck(s => s.forceRefresh());
    }));

    it('updates cached last pagination result before anouncing new result with requery', async(() => {
        asyncStyleCacheCheck(s => s.publicRequery());
    }));

    let asyncStyleCacheCheck = (requeryAction: (UserService) => void) => {
        // This is a regression for the following bug:
        // A derived service did implement a getItemById() function which did, before making a
        // network call to retrieve the resource from the server, check if the item with the given
        // id is present in the 'lastPaginationResult' local cache.
        // Now after a force refresh and a subsequent retrieval, the following sequence of events happened:
        // 1. Component subscribes to the pagination result to be notified when an update was finished.
        // 2. The same component did react to the subscribe event by getting an item by its id
        // 3. The service refreshed the items and raised the events
        // 4. The component reacted by getting the item by id, which retrived it from the (stale) local cache of the service
        // 5. The service finished it's refresh action and updated the local cache
        // This was happening because after emitting 'next()' on an observable, subscribed actions were executed before
        // continuing with the current code path
        // Something learned today!
        let service = getService();
        service.baseUrl = '/users';
        
        let httpMock = getHttpMock();

        const initialUsers = [{ userName: 'George', id: 1 }, { userName: 'Dave', id: 2 }];
        const updatedUsers = [{ userName: 'Giorgio', id: 1 }, { userName: 'Dave', id: 2 }]

        const initialRequest = httpMock.expectOne('/users?page=1&pageSize=20');
        const initialPaginationResponse: PaginationResult<User> = {
            data: initialUsers,
            page: 1,
            pageSize: 20,
            totalCount: 1
        };
        initialRequest.flush(initialPaginationResponse);

        service.paginationResult
        .pipe(skip(1)) // To not get the initial value from the first request
        .subscribe(usersPaginated => {
            var hasNewUser = usersPaginated.data.find(u => u.id === 1 && u.userName === 'Giorgio');
            expect(hasNewUser).toBeTruthy();
            var newUserById = service.getUserById(1)
            .then(user => {
                expect(user.userName).toBe('Giorgio'); // Initially, this returned the user 'George' from the stale cache
            });
        });

        requeryAction(service); // This is currently either calling 'forceRefresh()' or 'requery()'

        const secondRequest = httpMock.expectOne('/users?page=1&pageSize=20');
        const secondPaginationResponse: PaginationResult<User> = {
            data: updatedUsers,
            page: 1,
            pageSize: 20,
            totalCount: 1
        };
        secondRequest.flush(secondPaginationResponse);
    }

});

@Injectable()
class UserService extends PaginationBaseService<User> {

    constructor(protected http: HttpClient) {
        super(http);
    }

    public publicRequery = () => this.requery();

    public getUserById(userId: number): Promise<User> {
        if (this.lastPaginationResult && this.lastPaginationResult.data) {
            const cachedUser = this.lastPaginationResult.data.find(u => u.id === userId);
            if (cachedUser) {
                return Promise.resolve(cachedUser);
            }
        }
        throw Error('Not implemented');
    }
}

interface User {
    id: number,
    userName: string;
}
