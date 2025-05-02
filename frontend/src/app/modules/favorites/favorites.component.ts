import { Component, OnInit } from '@angular/core';
import { FavoriteService,FavoriteReadResponse } from '../../endpoints/FavoriteEndpointsService';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AdoptionPostService, AdoptionPostReadResponse, AnimalReadResponse, AdoptionPostCreateRequest, AnimalCreateRequest, CityReadResponse,ImageReadResponse,ImageCreateRequest,ImageUpdateRequest } from '../../endpoints/AdoptionPostEndpointsService';
import { NgModule } from '@angular/core';
import { from } from 'rxjs';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [RouterModule, CommonModule, HttpClientModule, FormsModule],
  templateUrl: './favorites.component.html',
  styleUrl: './favorites.component.css'
})
export class FavoritesComponent implements OnInit {

  constructor(private favoriteService:FavoriteService,private adoptionPostService:AdoptionPostService) {}

  favorites:FavoriteReadResponse[]=[];
  animals: AnimalReadResponse[] = [];
  adoptionPosts: AdoptionPostReadResponse[] = [];
  animalImages: ImageReadResponse[] = [];
 

  ngOnInit(): void {
    this.getFavorites();
  }

  getFavorites(){
    this.favoriteService.getFavorites().subscribe((response) => {
      this.favorites = response;
      console.log(response);
    });
  }

  removeFavorite(id:number){
    console.log("id:",id);
    this.favoriteService.removeFavorite(id).subscribe(x=>{
      console.log("Favorit uspjesno izbrisan");
      this.getFavorites();
    });
  }
  
}
