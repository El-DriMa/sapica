import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ShelterEndpointsService, ShelterReadResponse } from '../../endpoints/ShelterEndpointsService';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import {FormsModule} from "@angular/forms";
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';



@Component({
  selector: 'app-shelters',
  standalone: true,
  imports: [RouterModule, CommonModule, HttpClientModule, FormsModule],
  templateUrl: './shelters.component.html',
  styleUrl: './shelters.component.css'
})
export class SheltersComponent implements OnInit {
  filteredShelters: ShelterReadResponse[] = [];
  cities: { id: number; name: string }[] = [];
  showPopup = false;
  selectedShelterId: number | null = null;
  loading: boolean = false;
  sortColumn: string = '';
  sortOrder: 'asc' | 'desc' = 'asc';



  filter = {
    name: '',
    owner: '',
    username: '',
    cityId: '',
    yearFounded: 0
  };

  pagination = {
    page: 1,
    pageSize: 10,
    totalItems: 0
  };


  constructor(private ShelterService: ShelterEndpointsService, private router: Router) {}

  ngOnInit(): void {
    this.LoadAllShelters();
    this.loadCities();
  }

  /*LoadAllShelters() {
    this.ShelterService.getAllShelters().subscribe(
      (data: ShelterReadResponse[]) => {
        this.Shelters = data;
        this.filteredShelters = data; // Initially show all shelters
      },
      (error) => {
        console.error('Error fetching shelters', error);
      }
    );
  }*/
  LoadAllShelters() {
    this.loading = true;
    const params: any = {};

    if (this.filter.name) params.name = this.filter.name;
    if (this.filter.owner) params.owner = this.filter.owner;
    if (this.filter.username) params.username = this.filter.username;
    if (this.filter.cityId) params.cityId = parseInt(this.filter.cityId, 10);
    if (this.filter.yearFounded) params.yearFounded = this.filter.yearFounded;

    params.page = this.pagination.page;
    params.pageSize = this.pagination.pageSize;

    this.ShelterService.getFilteredShelters(params).subscribe(
      (response: any) => {
        this.filteredShelters = response.shelters;
        this.pagination.totalItems = response.totalItems;
        this.loading = false;
      },
      (error) => {
        console.error('Error fetching shelters with filters and pagination', error);
        this.loading = false;
      }
    );
  }

  onPageChange(newPage: number): void {
    this.pagination.page = newPage;
    this.LoadAllShelters();
  }
  get totalPages(): number {
    return Math.ceil(this.pagination.totalItems / this.pagination.pageSize);
  }



  getCityName(cityId: number): string {
    const city = this.cities.find((c) => c.id === cityId);
    return city ? city.name : 'Nepoznato';
  }


  validateYear(): void {
    if (this.filter.yearFounded < 1800 || this.filter.yearFounded > 2023) {
      alert('Molimo unesite godinu između 1800 i 2023.');
      this.filter.yearFounded = 0;
    }
  }


  loadCities() {

    this.ShelterService.getCities().subscribe(
      (data: { id: number; name: string }[]) => {
        this.cities = data;
      },
      (error) => {
        console.error('Error fetching cities', error);
      }
    );
  }


  applyFilter() {
    if (this.filter.cityId && isNaN(parseInt(this.filter.cityId, 10))) {
      alert('Molimo odaberite validan grad.');
      return;
    }
    this.pagination.page = 1;
    this.LoadAllShelters();
  }


  openPopup(id: number): void {
    this.selectedShelterId = id;
    this.showPopup = true;
  }

  closePopup(): void {
    this.showPopup = false;
    this.selectedShelterId = null;
  }

  confirmDelete(): void {
    if (this.selectedShelterId !== null) {
      this.ShelterService.deleteShelter(this.selectedShelterId).subscribe(
        () => {
          //alert('Shelter obrisan uspješno!');
          this.closePopup();
          this.LoadAllShelters();
        },
        (error) => {
          console.error('Greška pri brisanju azila:', error);
          this.closePopup();
        }
      );
    }
  }
  exportToPDF(): void {
    const doc = new jsPDF();


    doc.setFontSize(18);
    let title = 'Lista azila';



    const appliedFilters = [];

    if (this.filter.name) {
      appliedFilters.push(`Ime: ${this.filter.name}`);
    }
    if (this.filter.owner) {
      appliedFilters.push(`Vlasnik: ${this.filter.owner}`);
    }
    if (this.filter.yearFounded) {
      appliedFilters.push(`Godina osnivanja: ${this.filter.yearFounded}`);
    }
    if (this.filter.cityId) {
      const city = this.cities.find(city => city.id === parseInt(this.filter.cityId, 10));
      appliedFilters.push(`Grad: ${city ? city.name : 'Nepoznato'}`);
    }

    if (appliedFilters.length > 0) {
      title += ` (${appliedFilters.join(', ')})`;
    }
    doc.text(title,14,20);

    console.log(title);
    const data = this.filteredShelters.map(shelter => [
      shelter.id,
      shelter.name,
      shelter.owner,
      shelter.yearFounded,
      shelter.address,
      this.getCityName(shelter.cityId),
      shelter.email,
      shelter.phoneNumber,
    ]);

    console.log(data);


    autoTable(doc, {
      head: [['ID', 'Naziv', 'Vlasnik', 'Godina', 'Adresa', 'Grad', 'Email', 'Telefon']],
      body: data,
      startY: 30,
      styles: {
        fontSize: 10,
        cellPadding: 3,
        lineColor: [189, 154, 122],
        lineWidth: 0.5
      },
      headStyles: {
        fillColor: [141, 110, 99]
      }
    });


    const date = new Date();
    const formattedDate = date.toLocaleString('bs-BA', { dateStyle: 'short', timeStyle: 'short' });
    doc.setFontSize(10);
    doc.text(`Datum: ${formattedDate}`, 150, doc.internal.pageSize.height - 10);


    doc.save('Lista_azila.pdf');
  }
  sortBy(column: string): void {

    if (this.sortColumn === column) {
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {

      this.sortColumn = column;
      this.sortOrder = 'asc';
    }

    this.applySort();
  }
  applySort(): void {
    this.filteredShelters.sort((a, b) => {
      if (this.sortColumn === 'name') {
        const nameA = a.name.toLowerCase();
        const nameB = b.name.toLowerCase();
        if (nameA < nameB) {
          return this.sortOrder === 'asc' ? -1 : 1;
        }
        if (nameA > nameB) {
          return this.sortOrder === 'asc' ? 1 : -1;
        }
        return 0;
      }
      if (this.sortColumn === 'yearFounded') {
        const yearA = a.yearFounded;
        const yearB = b.yearFounded;
        if (yearA < yearB) {
          return this.sortOrder === 'asc' ? -1 : 1;
        }
        if (yearA > yearB) {
          return this.sortOrder === 'asc' ? 1 : -1;
        }
        return 0;
      }


      return 0;
    });
  }


  deleteShelter(id: number) {
    if (confirm('Are you sure you want to delete this user?')) {
      this.ShelterService.deleteShelter(id).subscribe(
        () => {
          alert('Shelter deleted successfully!');
          this.LoadAllShelters(); // Reload the list after deletion
        },
        (error) => {
          console.error('Error deleting shelter:', error);
        }
      );
    }
  }
}
