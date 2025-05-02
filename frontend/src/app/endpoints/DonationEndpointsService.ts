import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MyConfig } from '../my-config';

export interface DonationCreateRequest {
  userId: number;
  shelterId: number;
  amount: number;
}

@Injectable({
  providedIn: 'root',
})

export class DonationEndpointsService {
  private apiUrl = `${MyConfig.api_address}`;

  constructor(private http: HttpClient) {}

  createDonation(request: DonationCreateRequest) {
    return this.http.post(`${this.apiUrl}/donations`, request);
  }
}
