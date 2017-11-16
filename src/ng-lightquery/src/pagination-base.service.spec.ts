
import { TestBed, inject } from '@angular/core/testing';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PaginationBaseService } from './pagination-base.service';

describe('PaginationBaseService', () => {

    let httpMock: HttpTestingController;
    let service: UserService;
    
    beforeEach(() => {
    TestBed.configureTestingModule({
        imports: [
            HttpClientTestingModule
        ],
      providers: [
        UserService
    ]
    });
    httpMock = TestBed.get(HttpTestingController);
    service = TestBed.get(UserService);
  });

  it('can be provided', () => expect(service).toBeTruthy());
});

@Injectable()
class UserService extends PaginationBaseService<User> {

    constructor(http: HttpClient){
        super(http);
    }

}

interface User {
    userName: string;
}
