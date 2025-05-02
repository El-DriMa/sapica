import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MyConfig } from '../my-config';

export interface AdoptionRequestReadResponse {
  id: number;
  date: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  reason: string;
  livingSpace: string;
  backyard: string;
  backyardSize?:string;
  familyMembers: number;
  anyKids : string;
  numberOfKids?: number;
  anyAnimalsBefore: string;
  animalsBefore?:string;
  experience?:string;
  timeCommitment: number;
  preferredCharacteristic:string;
  age: number;
  adoptionPostId: number;
  cityId: number;
  userId?: number;
}
export interface AdoptionRequestByUsernameReadResponse

{
  id:number;
  date?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phoneNumber?: string;
  reason?: string;
  livingSpace?: string;
  backyard?: string;
  familyMembers?: number;
  anyKids? : string;
  anyAnimalsBefore?: string;
  timeCommitment?: number;
  preferredCharacteristic?:string;
  age?: number;
  adoptionPostId?: number;
  cityId?: number;
  city?:string;
  animalImage?:string;
  animalName?:string;
  animalType?:string;
  isAccepted?:boolean;

}

export interface AdoptionRequestCreateRequest {
  date?: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  reason: string;
  livingSpace: string;
  backyard: string;
  backyardSize?:string;
  familyMembers: number;
  anyKids : string;
  numberOfKids?: number;
  anyAnimalsBefore: string;
  animalsBefore?:string;
  experience?:string;
  timeCommitment: number;
  preferredCharacteristic: string;
  age: number;
  adoptionPostId: number;
  cityId: number;
  userId?: number;
}

export interface AdoptionRequestUpdateRequest {
  date?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phoneNumber?: string;
  reason?: string;
  livingSpace?: string;
  backyard?: string;
  backyardSize?:string;
  familyMembers?: number;
  anyKids? : string;
  numberOfKids?: number;
  anyAnimalsBefore?: string;
  animalsBefore?:string;
  experience?:string;
  timeCommitment?: number;
  preferredCharacteristic?:string;
  age?: number;
  adoptionPostId?: number;
  cityId?: number;
  userId?: number;
  city?:string;
}

@Injectable({
  providedIn: 'root'
})
export class AdoptionRequestEndpointsService {
  private apiUrl = `${MyConfig.api_address}`;

  constructor(private http: HttpClient) {}

  getAllAdoptionRequests() {
    return this.http.get<AdoptionRequestReadResponse[]>(`${this.apiUrl}/adoption-requests`);
  }

  getAdoptionRequestById(id: number) {
    return this.http.get<AdoptionRequestReadResponse>(`${this.apiUrl}/adoption-requests/${id}`);
  }

  createAdoptionRequest(request: AdoptionRequestCreateRequest) {
    return this.http.post<AdoptionRequestReadResponse>(`${this.apiUrl}/adoption-requests`, request);
  }
  getAdoptionRequestsByUsername(username:string)
  {
    return this.http.get<AdoptionRequestByUsernameReadResponse[]>(`${this.apiUrl}/requests/${username}`)
  }

  updateAdoptionRequest(id: number, request: AdoptionRequestUpdateRequest) {
    return this.http.put<AdoptionRequestReadResponse>(`${this.apiUrl}/adoption-requests/${id}`, request);
  }

  deleteAdoptionRequest(id: number) {
    return this.http.delete(`${this.apiUrl}/adoption-requests/${id}`);
  }
  acceptAdoptionRequest(id:number){
    return this.http.patch(`${this.apiUrl}/request/${id}`,id);
  }
  getCities() {
    return this.http.get<{ id: number, name: string, countryId: number, latitude: number, longitude: number }[]>(`${this.apiUrl}/cities`);
  }
}
