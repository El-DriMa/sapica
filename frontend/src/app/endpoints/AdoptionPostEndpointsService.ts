import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MyConfig } from '../my-config';
import { catchError, Observable, of, tap, throwError } from 'rxjs';

export interface AnimalReadResponse {
    id: number;
    name: string;
    gender: string;
    size: string;
    age: string;
    color: string;
    weight: number;
    animalType: string;
    vaccinated:boolean;
    sterilized:boolean;
    parasiteFree:boolean;
    hasPassport:boolean;
    images: ImageReadResponse[];
  }
  
  export interface AnimalCreateRequest {
    name: string;
    gender: string;
    size: string;
    age: string;
    color: string;
    weight: number;
    animalType: string;
    vaccinated:boolean;
    sterilized:boolean;
    parasiteFree:boolean;
    hasPassport:boolean;
    images?: ImageCreateRequest[];
  }
  
  export interface AnimalUpdateRequest {
    name?: string;
    gender?: string;
    size?: string;
    age?: string;
    color?: string;
    weight?: number;
    animalType?: string;
    vaccinated?:boolean;
    sterilized?:boolean;
    parasiteFree?:boolean;
    hasPassport?:boolean;
    images?: ImageUpdateRequest[];
  }

  export interface AdoptionPostReadResponse {
    id: number;
    dateOfCreation: Date;
    viewCounter: number;
    urgent: boolean;
    shortDescription: string;
    animalId: number;
    username:string;
    animal: AnimalReadResponse;
    cityId?: number;
    city?: CityReadResponse;
  }

export interface AdoptionPostCreateRequest{
    dateOfCreation: Date,
    viewCounter: number
    urgent: boolean,
    shortDescription: string,
    animalId?: number,
    username:string,
    animal?: AnimalCreateRequest;
    cityId?: number;
}

export interface AdoptionPostUpdateRequest {
  dateOfCreation?: Date,
  dateOfModification?: Date,
  viewCounter?: number,
  urgent?: boolean,
  shortDescription?: string,
  username?:string,
  animal?: AnimalUpdateRequest;
  cityId?: number;
}

export interface CityReadResponse {
  id: number;
  name: string;
  latitude:number;
  longitude:number;
}

export interface ImageReadResponse {
  image:string,
  animalId:number
}

export interface ImageCreateRequest {
  image:string,
  animalId:number
}

export interface ImageUpdateRequest {
  image:string,
  animalId:number
}


@Injectable({
    providedIn:'root'
})


export class AdoptionPostService{
    private apiUrl=`${MyConfig.api_address}`;

    constructor(private http: HttpClient){}

    getAllAdoptionPosts(page: number, itemsPerPage: number): Observable<any> {
      return this.http.get<any>(`${this.apiUrl}/adoptionPosts/all?page=${page}&itemsPerPage=${itemsPerPage}`);
    }
    
  

    getAllAdoptionPostsLoggedIn(){
      return this.http.get<AdoptionPostReadResponse[]>(`${this.apiUrl}/adoptionPosts/allLoggedIn`);
    }

    addAdoptionPost(requestPayload: AdoptionPostCreateRequest): Observable<AdoptionPostReadResponse> {
      return this.http.post<AdoptionPostReadResponse>(`${this.apiUrl}/adoptionPosts/add`, requestPayload).pipe(
        catchError((error) => {
          console.error('Error details:', error);
          if (error.status === 400) {
            console.error('Bad Request: ', error.error);
          } else if (error.status === 0) {
            console.error('Network or CORS error');
          } else {
            console.error(`Unexpected Error: ${error.message}`);
          }
          return throwError(() => new Error('Failed to add adoption post'));
        })
      );
    }
    
    updateAdoptionPost(id: number, request: AdoptionPostUpdateRequest): Observable<AdoptionPostReadResponse> {
      return this.http.put<AdoptionPostReadResponse>(`${this.apiUrl}/adoptionPosts/update/${id}`, request).pipe(
        catchError((error) => {
          console.error('Error details:', error);
          if (error.status === 400) {
            console.error('Bad Request: ', error.error);
          }
          return throwError(() => new Error('Failed to update adoption post'));
        })
      );
    }
    
    
    getAdoptionPostById(id: number): Observable<AdoptionPostReadResponse> {
      return this.http.get<AdoptionPostReadResponse>(`${this.apiUrl}/adoptionPosts/adoptionpost/${id}`);
    }
    

    deleteAdoptionPost(id:number):Observable<any>{
      return this.http.delete(`${this.apiUrl}/adoptionPosts/delete/${id}`);
    }

    getCities() {
      return this.http.get<CityReadResponse[]>(`${this.apiUrl}/cities`);
    } 
    
    getCityById(id: number) {
      return this.http.get<CityReadResponse>(`${this.apiUrl}/cities/${id}`);
    }

    getFilteredAdoptionPosts(
      urgent?: boolean,
      animalType?: string,
      gender?: string,
      cityId?: number,
      size?:string,
      page: number = 1,
      itemsPerPage: number = 5
    ): Observable<any> {
      let params: any = { page, itemsPerPage }; 
    
      
      if (urgent !== undefined) params.urgent = urgent;
      if (animalType) params.animalType = animalType;
      if (gender) params.gender = gender;
      if (cityId) params.cityId = cityId;
      if (size) params.size=size;
    
     
      return this.http.get(`${this.apiUrl}/adoptionPosts/search`, { params });
    }
}