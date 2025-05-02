import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MyConfig } from '../my-config';
import { AdoptionPostReadResponse } from './AdoptionPostEndpointsService';

export interface FavoriteReadResponse{
    id:number;
    userId:number;
    adoptionPostId:number;
    adoptionPost:AdoptionPostReadResponse;
}


@Injectable({
    providedIn:'root'
})


export class FavoriteService{
    private apiUrl=`${MyConfig.api_address}`;

    constructor(private http: HttpClient){}

    getFavorites(){
        return this.http.get<FavoriteReadResponse[]>(`${this.apiUrl}/favorites/byUsername`);
    }

    createFavorite(id:number){
        return this.http.post<number>(`${this.apiUrl}/favorites/create`,{}, { params: { adoptionPostId: id } });
    }

    removeFavorite(id:number){
        return this.http.delete<number>(`${this.apiUrl}/favorites/delete`,{ params: { favoriteId: id } });
    }
}