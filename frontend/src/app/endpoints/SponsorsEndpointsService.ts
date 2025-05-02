import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MyConfig } from '../my-config';
import { catchError, Observable, of, tap, throwError } from 'rxjs';

export interface SponsorReadResponse {
    name:string;
    logoUrl:string;
    websiteUrl:string;
    description:string;
    contactEmail:string;
    address:string;
}

@Injectable({
    providedIn:'root'
})

export class SponsorsService{
    private apiUrl=`${MyConfig.api_address}`;

    constructor(private http: HttpClient){}

    getAllSponsors():Observable<SponsorReadResponse[]>{
        return this.http.get<SponsorReadResponse[]>(`${this.apiUrl}/sponsors/all`)
    }
}