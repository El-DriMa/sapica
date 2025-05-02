import { TestBed } from '@angular/core/testing';

import { MyAuthInterceptorService } from './my-auth-interceptor.service';

describe('MyAuthInterceptorService', () => {
  let service: MyAuthInterceptorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MyAuthInterceptorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
