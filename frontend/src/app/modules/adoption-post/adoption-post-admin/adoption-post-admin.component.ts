import { Component, Input, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import {AdoptionPostService, AdoptionPostReadResponse, CityReadResponse } from '../../../endpoints/AdoptionPostEndpointsService'
import { CommonModule } from '@angular/common';
import { DatePipe } from '@angular/common';
import {FormsModule} from "@angular/forms";
import Swal from 'sweetalert2';


@Component({
  selector: 'app-adoption-post-admin',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, HttpClientModule],
  templateUrl: './adoption-post-admin.component.html',
  styleUrl: './adoption-post-admin.component.css'
})
export class AdoptionPostAdminComponent implements OnInit{
  
  @Input() username?: string;  

  adoptionPosts: AdoptionPostReadResponse[] = [];
  cities: CityReadResponse[] = [];
  filters = {
    urgent: false,
    animalType: '',
    gender: '',
    cityId: 0,
    size:'',
  };
  constructor(private AdoptionPostService: AdoptionPostService){}

  ngOnInit(): void {
    this.getAdoptionPosts();
    this.getCities();
  }
  currentPage : number = 1;
  itemsPerPage: number = 10;
  totalPages: number = 1;

  getAdoptionPosts(): void {
    this.AdoptionPostService.getAllAdoptionPosts(this.currentPage, this.itemsPerPage).subscribe(
      (response: any) => {
        this.adoptionPosts = response.adoptionPosts;
        this.totalPages = response.totalPages;
      },
      (error) => {
        console.error('Error fetching adoption posts:', error);
      }
    );
  }

   searchAdoptionPosts(): void {
      const genderFilter = this.filters.gender || undefined;
      const cityIdFilter = this.filters.cityId !== 0 ? this.filters.cityId : undefined;
      const sizeFilter = this.filters.size || undefined;
  
      console.log(this.filters);
  
      this.AdoptionPostService.getFilteredAdoptionPosts(
        this.filters.urgent,
        this.filters.animalType,
        genderFilter,
        cityIdFilter,
        sizeFilter,
        this.currentPage,
        this.itemsPerPage
      ).subscribe(
        (response: any) => {
          this.adoptionPosts = response.adoptionPosts;
          this.totalPages = response.totalPages;
        },
        (error) => {
          console.error('Error fetching filtered adoption posts:', error);
        }
      );
    }

  getCities(): void {
    this.AdoptionPostService.getCities().subscribe((response) => {
      this.cities = response;
    });
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.getAdoptionPosts();
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.getAdoptionPosts();
    }
  }

   deleteAdoptionPost(id: number): void {
      Swal.fire({
        title: 'Da li ste sigurni?',
        text: 'Ova akcija se ne može poništiti.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Da, obriši!'
      }).then((result) => {
        if (result.isConfirmed) {
          this.AdoptionPostService.deleteAdoptionPost(id).subscribe({
            next: (response) => {
              Swal.fire(
                'Obrisano!',
                'Post je uspješno obrisan.',
                'success'
              );
              this.getAdoptionPosts();
            },
            error: (error) => {
              console.error('Error deleting adoption post', error);
              Swal.fire(
                'Greška!',
                'Došlo je do greške prilikom brisanja.',
                'error'
              );
            }
          });
        }
      });
    }

}
