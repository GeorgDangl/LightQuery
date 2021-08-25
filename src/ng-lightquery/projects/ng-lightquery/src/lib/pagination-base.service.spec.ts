import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { TestBed, async, inject } from '@angular/core/testing';
import { skip, take } from 'rxjs/operators';

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaginationBaseService } from './pagination-base.service';
import { PaginationResult } from './pagination-result';

describe('PaginationBaseService', () => {
  let getHttpMock: () => HttpTestingController = () =>
    TestBed.get(HttpTestingController);
  let getService: () => UserService = () => TestBed.get(UserService);

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserService],
    });
  });

  it('can provide HttpClient', inject(
    [HttpClient],
    (httpClient: HttpClient) => {
      expect(httpClient).toBeTruthy();
      expect(httpClient instanceof HttpClient).toBeTruthy();
    }
  ));

  it('can be provided', () => {
    let service = getService();
    expect(service).toBeTruthy();
    expect(service instanceof UserService).toBeTruthy();
  });

  it('returns the correct result', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const request = httpMock.expectOne('/users?page=1&pageSize=20');
    const paginationResponse: PaginationResult<User> = {
      data: [{ id: 1, userName: 'George' }],
      page: 1,
      pageSize: 20,
      totalCount: 1,
    };
    request.flush(paginationResponse);
    const serviceResult = await service.paginationResult
      .pipe(take(1))
      .toPromise();
    expect(serviceResult.totalCount).toBe(1);
    httpMock.verify();
  }));

  it('requeries again on page change', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const req = httpMock.expectOne('/users?page=1&pageSize=20');
    expect(req.request.method).toBe('GET');
    httpMock.verify();
    service.page++;
    await delay(1);
    httpMock.expectOne('/users?page=2&pageSize=20');
    httpMock.verify();
  }));

  it('requeries again on pageSize change', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const req = httpMock.expectOne('/users?page=1&pageSize=20');
    expect(req.request.method).toBe('GET');
    httpMock.verify();
    service.pageSize++;
    await delay(1);
    httpMock.expectOne('/users?page=1&pageSize=21');
    httpMock.verify();
  }));

  it('requeries again on parameter addition', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const req = httpMock.expectOne('/users?page=1&pageSize=20');
    expect(req.request.method).toBe('GET');
    httpMock.verify();
    service.setQueryParameter('filter', 'byName');
    await delay(1);
    httpMock.expectOne('/users?page=1&pageSize=20&filter=byName');
    httpMock.verify();
  }));

  it('requeries again on parameter change', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const req = httpMock.expectOne('/users?page=1&pageSize=20');
    expect(req.request.method).toBe('GET');
    httpMock.verify();
    service.setQueryParameter('filter', 'byName');
    await delay(1);
    httpMock.expectOne('/users?page=1&pageSize=20&filter=byName');
    httpMock.verify();
    service.setQueryParameter('filter', 'byAge');
    await delay(1);
    httpMock.expectOne('/users?page=1&pageSize=20&filter=byAge');
    httpMock.verify();
  }));

  it('requeries again on force refresh', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const req = httpMock.expectOne('/users?page=1&pageSize=20');
    expect(req.request.method).toBe('GET');
    httpMock.verify();
    service.forceRefresh();
    await delay(1);
    httpMock.expectOne('/users?page=1&pageSize=20');
    httpMock.verify();
  }));

  it('calls the correct url', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    service.pageSize = 13;
    service.page = 2;
    service.setQueryParameter('filter', 'byName');
    await delay(1);
    let httpMock = getHttpMock();
    const req = httpMock.expectOne('/users?page=2&pageSize=13&filter=byName');
    expect(req.request.method).toBe('GET');
  }));

  it('calls url to get All items', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    service.getAll()
      .toPromise();
    await delay(1);
    let httpMock = getHttpMock();
    const req = httpMock.expectOne('/users?page=1&pageSize=500');
    expect(req.request.method).toBe('GET');
  }));

  it('does not emit result on error response', async(async () => {
    let hasReceivedResponse = false;
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    service.paginationResult.subscribe((r) => (hasReceivedResponse = true));
    let httpMock = getHttpMock();
    const request = httpMock.expectOne('/users?page=1&pageSize=20');
    request.error(new ErrorEvent('error'));
    expect(hasReceivedResponse).toBeFalsy();
  }));

  it('cancels first request when second request sent', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const firstRequest = httpMock.expectOne('/users?page=1&pageSize=20');
    service.forceRefresh();
    await delay(1);
    const secondRequest = httpMock.expectOne('/users?page=1&pageSize=20');
    expect(firstRequest.cancelled).toBeTruthy();
    expect(secondRequest.cancelled).toBeFalsy();
  }));

  it('does not send two requests when calling forceRefresh twice subsequently', async(async () => {
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    let httpMock = getHttpMock();
    const firstRequest = httpMock.expectOne('/users?page=1&pageSize=20');
    service.forceRefresh();
    service.forceRefresh();
    await delay(1);
    const secondRequest = httpMock.expectOne('/users?page=1&pageSize=20');
    expect(firstRequest.cancelled).toBeTruthy();
    expect(secondRequest.cancelled).toBeFalsy();
  }));

  it('does not break observable chain when receiving an error', async(async () => {
    let hasReceivedResponse = false;
    let service = getService();
    service.baseUrl = '/users';
    await delay(1);
    service.paginationResult.subscribe((r) => (hasReceivedResponse = true));
    let httpMock = getHttpMock();
    const request = httpMock.expectOne('/users?page=1&pageSize=20');
    request.error(new ErrorEvent('error'));

    expect(hasReceivedResponse).toBeFalsy();
    httpMock.verify();
    service.forceRefresh();
    await delay(1);
    const secondRequest = httpMock.expectOne('/users?page=1&pageSize=20');
    secondRequest.flush({});
    expect(hasReceivedResponse).toBeTruthy();
  }));

  it('updates cached last pagination result before anouncing new result with forceRefresh', async(async () => {
    await asyncStyleCacheCheck((s) => s.forceRefresh());
  }));

  it('updates cached last pagination result before anouncing new result with requery', async(async () => {
    await asyncStyleCacheCheck((s) => s.publicRequery());
  }));

  let asyncStyleCacheCheck = async (requeryAction: (UserService) => void) => {
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
    await delay(1);

    let httpMock = getHttpMock();

    const initialUsers = [
      { userName: 'George', id: 1 },
      { userName: 'Dave', id: 2 },
    ];
    const updatedUsers = [
      { userName: 'Giorgio', id: 1 },
      { userName: 'Dave', id: 2 },
    ];

    const initialRequest = httpMock.expectOne('/users?page=1&pageSize=20');
    const initialPaginationResponse: PaginationResult<User> = {
      data: initialUsers,
      page: 1,
      pageSize: 20,
      totalCount: 1,
    };
    initialRequest.flush(initialPaginationResponse);

    service.paginationResult
      .pipe(skip(1)) // To not get the initial value from the first request
      .subscribe((usersPaginated) => {
        var hasNewUser = usersPaginated.data.find(
          (u) => u.id === 1 && u.userName === 'Giorgio'
        );
        expect(hasNewUser).toBeTruthy();
        var newUserById = service.getUserById(1).then((user) => {
          expect(user.userName).toBe('Giorgio'); // Initially, this returned the user 'George' from the stale cache
        });
      });

    requeryAction(service); // This is currently either calling 'forceRefresh()' or 'requery()'
    await delay(1);

    const secondRequest = httpMock.expectOne('/users?page=1&pageSize=20');
    const secondPaginationResponse: PaginationResult<User> = {
      data: updatedUsers,
      page: 1,
      pageSize: 20,
      totalCount: 1,
    };
    secondRequest.flush(secondPaginationResponse);
  };

  function delay(ms: number) {
    return new Promise((resolve) => setTimeout(resolve, ms));
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
      const cachedUser = this.lastPaginationResult.data.find(
        (u) => u.id === userId
      );
      if (cachedUser) {
        return Promise.resolve(cachedUser);
      }
    }
    throw Error('Not implemented');
  }
}

interface User {
  id: number;
  userName: string;
}
