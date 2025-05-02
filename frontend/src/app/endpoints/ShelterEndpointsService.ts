import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MyConfig } from '../my-config';
import {UserReadResponse, UserUpdateRequest} from "./UserEndpointsService";

export interface ShelterReadResponse {
  id: number;
  name: string;
  owner: string;
  yearFounded: number;
  address: string;
  email:string;
  username:string;
  phoneNumber:string;
  imageUrl:string;
  cityId:number;

}

export interface ShelterCreateRequest {
  name: string;
  owner: string;
  yearFounded: number;
  address: string;
  username:string;
  email:string;
  password:string;
  phoneNumber:string;
  imageUrl:string |ArrayBuffer |null;
  cityId:number;
}

export interface ShelterUpdateRequest {
  name?: string;
  owner?: string;
  yearFounded?: number;
  address?: string;
  username?:string;
  email?:string;
  password?:string;
  phoneNumber?:string;
  imageUrl?:string;
  cityId?:number;
}

@Injectable({
  providedIn: 'root'
})
export class ShelterEndpointsService {
  private apiUrl = `${MyConfig.api_address}`;

  constructor(private http: HttpClient) {}

  getAllShelters() {
    return this.http.get<ShelterReadResponse[]>(`${this.apiUrl}/shelters`);
  }

  getShelters() {
    return this.http.get<ShelterReadResponse[]>(`${this.apiUrl}/shelters/cmb`);
  }

  getShelterById(id: number) {
    return this.http.get<ShelterReadResponse>(`${this.apiUrl}/shelters/${id}`);
  }

  // addShelter(request: ShelterCreateRequest) {
  //  return this.http.post<ShelterReadResponse>(`${this.apiUrl}`, request);
  //}

  updateShelter(id: number, request: ShelterUpdateRequest) {
    return this.http.put<ShelterReadResponse>(`${this.apiUrl}/shelters/${id}`, request);
  }
  loadShelterProfile() {
    return this.http.get<ShelterReadResponse>(`${this.apiUrl}/login/GetCurrentUser`);
  }


  deleteShelter(id: number) {
    return this.http.delete(`${this.apiUrl}/shelters/${id}`);
  }
  createShelter(request: ShelterCreateRequest) {
    return this.http.post<ShelterReadResponse>(`${this.apiUrl}/shelters`, request);

  }
  getFilteredShelters(params: any) {
    return this.http.get('https://localhost:7291/shelters', { params });
  }

  getCities() {
    return this.http.get<{ id: number, name: string, countryId: number, latitude: number, longitude: number }[]>(`${this.apiUrl}/cities`);
  }
}
