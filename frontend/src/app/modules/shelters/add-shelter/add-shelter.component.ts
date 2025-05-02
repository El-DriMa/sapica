import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ShelterEndpointsService, ShelterCreateRequest } from '../../../endpoints/ShelterEndpointsService';
import { FormsModule, NgModel } from '@angular/forms';
import {NgForOf, NgIf} from "@angular/common";
import imageCompression from 'browser-image-compression';


@Component({
  selector: 'app-add-shelter',
  templateUrl: './add-shelter.component.html',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf,
    NgIf
  ],
  styleUrls: ['./add-shelter.component.css']
})
export class AddShelterComponent implements OnInit {
  showPopup = false;
  passwordStrength: number = 0;
  passwordStrengthColor: string = 'red';
  passwordMessage: string = '';
  compressedFilee:string='';
  newShelter: ShelterCreateRequest = {
    name: '',
    owner: '',
    yearFounded: 0,
    address:'',
    username: '',
    password: '',
    email: '',
    phoneNumber: '',
    cityId: 0,
    imageUrl: '',
  };


  errors: { [key: string]: string } = {};
  cities: { id: number, name: string, countryId: number, latitude: number, longitude: number }[] = [];
  isSubmitting = false;

  constructor(private shelterService: ShelterEndpointsService, private router: Router) {}

  ngOnInit(): void {
    this.loadCities();
  }

  loadCities() {
    this.shelterService.getCities().subscribe(
      (data) => {
        console.log("Fetched cities:", data);
        this.cities = data;
      },
      (error) => {
        console.error("Error fetching cities", error);
      }
    );
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const container = event.target as HTMLElement;
    container.classList.add('drag-over');
  }

  onDragLeave(): void {
    const container = document.querySelector('.drag-drop-container');
    container?.classList.remove('drag-over');
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const container = document.querySelector('.drag-drop-container');
    container?.classList.remove('drag-over');

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
  }

/*
  handleFile(file: File): void {
    const input = document.querySelector('#imageUrl') as HTMLInputElement;

    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = () => {
        this.newShelter.imageUrl = reader.result as string;
        delete this.errors['imageUrl'];
      };
      reader.readAsDataURL(file);
    } else {
      // Resetuje stanje ako fajl nije validan
      this.newShelter.imageUrl = '';
      this.errors['imageUrl'] = 'Samo slike su dozvoljene!';
      if (input) {
        input.value = '';
      }
    }
  }*/
/*
  async handleFile(file: File): Promise<void> {
    if (file.type.startsWith('image/')) {
      try {
        this.isSubmitting = true;
        const imageUrl = await this.uploadImageToBlob(file); // Otpremanje slike
        this.newShelter.imageUrl = imageUrl; // Dodavanje URL-a slike u podatke azila
        delete this.errors['imageUrl'];
      } catch (error) {
        this.errors['imageUrl'] = 'Greška prilikom otpremanja slike. Pokušajte ponovo.';
      } finally {
        this.isSubmitting = false;
      }
    } else {
      this.errors['imageUrl'] = 'Samo slike su dozvoljene!';
    }
  }
  async uploadImageToBlob(file: File): Promise<string> {
    try {
      const formData = new FormData();
      formData.append('file', file);


      const response = await fetch('https://localhost:7291/api/PicturesEndpoints/upload-image', {
        method: 'POST',
        body: formData,
      });

      if (!response.ok) {
        throw new Error('Greška prilikom otpremanja slike');
      }

      const data = await response.json();
      return data.imageUrl;
    } catch (error) {
      console.error('Greška prilikom otpremanja slike:', error);
      throw error;
    }
  }*/


  async handleFile(file: File): Promise<void> {
    if (file.type.startsWith('image/')) {
      try {
        this.isSubmitting = true;


        const compressedFile = await this.compressImage(file);


        const imageUrl = await this.uploadImageToBlob(compressedFile);
        this.newShelter.imageUrl = imageUrl;
        delete this.errors['imageUrl'];
      } catch (error) {
        this.errors['imageUrl'] = 'Greška prilikom obrade slike. Pokušajte ponovo.';
      } finally {
        this.isSubmitting = false;
      }
    } else {
      this.errors['imageUrl'] = 'Samo slike su dozvoljene!';
    }
  }

  async compressImage(file: File): Promise<File> {
    const options = {
      maxSizeMB: 1,
      maxWidthOrHeight: 1024,
      useWebWorker: true,
    };

    try {
      const compressedFile = await imageCompression(file, options);
      console.log('Original file size:', file.size / 1024, 'KB');
      console.log('Compressed file size:', compressedFile.size / 1024, 'KB');
      this.compressedFilee=`${compressedFile.size / 1024}, KB`;
      console.log(this.compressedFilee);
      return compressedFile;
    } catch (error) {
      console.error('Greška prilikom kompresije slike:', error);
      throw error;
    }
  }

  async uploadImageToBlob(file: File): Promise<string> {
    try {
      const formData = new FormData();
      const blob = new Blob([file], { type: file.type });
      formData.append('file',blob, file.name);

      const response = await fetch('https://localhost:7291/api/PicturesEndpoints/upload-image', {
        method: 'POST',
        body: formData,
      });

      if (!response.ok) {
        throw new Error('Greška prilikom otpremanja slike');
      }

      const data = await response.json();
      return data.imageUrl;
    } catch (error) {
      console.error('Greška prilikom otpremanja slike:', error);
      throw error;
    }
  }

  removeImage(): void {
    this.newShelter.imageUrl = null;
    this.compressedFilee='';
  }
  validateInput(field: keyof ShelterCreateRequest) {
    const value = this.newShelter[field];
    let message = '';

    if (field === 'name' && !(value as string)) {
      message = 'Naziv je obavezan.';
    } else if (field === 'name' && (value as string).length > 50) {
      message = 'Naziv mora imati manje od 50 karaktera.';
    }

    if (field === 'owner' && !(value as string)) {
      message = 'Vlasnik je obavezan.';
    } else if (field === 'owner' && (value as string).length > 50) {
      message = 'Vlasnik mora imati manje od 50 karaktera.';
    }

    if (field === 'yearFounded' && ((value as number) < 1800 || (value as number) > 2023)) {
      message = 'Godina mora biti između 1800. i 2023.';
    }

    if (field === 'address' && !(value as string)) {
      message = 'Adresa je obavezna.';
    } else if (field === 'address' && (value as string).length > 100) {
      message = 'Adresa mora imati manje od 100 karaktera.';
    }


    if (field === 'username' && !(value as string)) {
      message = 'Korisničko ime je obavezno.';
    } else if (field === 'username' && (value as string).length < 1) {
      message = 'Korisničko ime mora imati barem jedan karakter.';
    } else if (field === 'username' && (value as string).length > 20) {
      message = 'Korisičko ime ne može imati više od 20 karaktera.';
    } else if (field === 'username' && !/^[a-zA-Z0-9_]*$/.test(value as string)) {
      message = 'Korisničko ime može imati samo slova, brojeve i crtice.';
    }

    if (field === 'password' && !(value as string)) {
      message = 'Lozinka je obavezna.';
    } else if (field === 'password' && (value as string).length < 8) {
      message = 'Lozinka mora imati barem 8 karaktera.';
      // } else if (field === 'password' && !/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$/
      //   .test(value as string)) {
      //   message = 'Password must contain at least one lowercase letter, one uppercase letter, and one number. You cant use dot.';
    }



    if (field === 'email' && !(value as string)) {
      message = 'Email je obavezan.';
    } else if (field === 'email' && !/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(value as string)) {
      message = 'Format nije valjan.';
    }

    if (field === 'phoneNumber' && !(value as string)) {
      message = 'Broj telefona je obavezan.';
    } else if (field === 'phoneNumber' && !/^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$/.test(value as string)) {
      message = 'Broj telefona mora biti validan za Bosnu i Hercegovinu.';
    }

    if (message) {
      this.errors[field] = message;
    } else {
      delete this.errors[field];
    }
  }


  isValidForm() {
    return Object.values(this.errors).every((error) => !error) &&
      Object.values(this.newShelter).every((value) => value);

  }

  closePopup() {
    this.showPopup = false;
    this.router.navigate(['/']);
  }

  async submitShelterData() {
    // Simulacija uspješnog zahtjeva
    return new Promise((resolve) => setTimeout(resolve, 1000));
  }
  checkPasswordStrength(password: string): void {
    let strength = 0;

    if (password.length >= 8) strength += 25; // Minimalna dužina
    if (/[A-Z]/.test(password)) strength += 25; // Velika slova
    if (/[a-z]/.test(password)) strength += 25; // Mala slova
    if (/[0-9]/.test(password)) strength += 15; // Brojevi
    if (/[^A-Za-z0-9]/.test(password)) strength += 10; // Specijalni znakovi

    this.passwordStrength = Math.min(strength, 100); // Maksimalna vrijednost je 100

    // Određivanje boje i poruke na osnovu jačine lozinke
    if (this.passwordStrength <= 40) {
      this.passwordStrengthColor = 'red';
      this.passwordMessage = 'Slaba lozinka';
    } else if (this.passwordStrength <= 75) {
      this.passwordStrengthColor = 'orange';
      this.passwordMessage = 'Srednja lozinka';
    } else {
      this.passwordStrengthColor = 'green';
      this.passwordMessage = 'Sigurna lozinka';
    }
  }

  onAddShelter() {

    console.log("Payload:", this.newShelter);
    this.isSubmitting = true;

    this.shelterService.createShelter(this.newShelter).subscribe(
      (response) => {
        //alert('Registracija azila uspješna!');

        this.showPopup = true;
        //this.router.navigate(['/']);
      },
      (error) => {


        if (error.status === 400) {
          if (typeof error.error === "string") {
            alert(error.error);
          } else {
            console.error('Unheandled backend error: ', error.error);
          }
        } else {
          console.error('Error adding shelter:', error);
        }
        this.isSubmitting = false;
      }
    );
  }
}
