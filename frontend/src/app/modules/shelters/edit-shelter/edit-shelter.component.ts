import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ShelterEndpointsService, ShelterUpdateRequest } from '../../../endpoints/ShelterEndpointsService';
import { FormsModule } from '@angular/forms';
import { NgForOf, NgIf } from "@angular/common";
import imageCompression from "browser-image-compression";
import Swal from "sweetalert2";

@Component({
  selector: 'app-edit-shelter',
  templateUrl: './edit-shelter.component.html',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf,
    NgIf
  ],
  styleUrls: ['./edit-shelter.component.css']
})
export class EditShelterComponent implements OnInit {
  shelterId!: number;
  editShelter: ShelterUpdateRequest = {
    name: '',
    owner: '',
    yearFounded: 1900,
    address:'',
    username: '',
    password: '',
    email: '',
    phoneNumber: '',
    cityId: 0,
    imageUrl: '',
  };
  changePassword = false;
  errors: { [key: string]: string } = {};
  cities: { id: number, name: string, countryId: number, latitude: number, longitude: number }[] = [];
  isSubmitting = false;

  constructor(
    private route: ActivatedRoute,
    private shelterService: ShelterEndpointsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.shelterId = +params['id'];
      this.loadShelter();
      this.loadCities();
    });
  }

  loadShelter() {
    this.shelterService.getShelterById(this.shelterId).subscribe(
      (data) => {
        console.log('Shelter data fetched:', data);
        this.editShelter = data;
      },
      (error) => {
        console.error('Error fetching shelter data:', error);
        alert('Failed to fetch shelter details.');
        this.router.navigate(['/shelters']);
      }
    );
  }

  loadCities() {
    this.shelterService.getCities().subscribe(
      (data) => {
        console.log('Fetched cities:', data);
        this.cities = data;
      },
      (error) => {
        console.error('Error fetching cities:', error);
      }
    );
  }

  validateInput(field: keyof ShelterUpdateRequest) {
    const value = this.editShelter[field];
    let message = '';

    if (field === 'name' && !(value as string)) {
      message = 'Name is required.';
    } else if (field === 'name' && (value as string).length > 50) {
      message = 'Name must be under 50 letters.';
    }

    if (field === 'owner' && !(value as string)) {
      message = 'Owner is required.';
    } else if (field === 'owner' && (value as string).length > 50) {
      message = 'Owner must be under 50 letters.';
    }

    if (field === 'yearFounded' && ((value as number) < 1800 || (value as number) > 2023)) {
      message = 'Year must be between 1800 and 2023.';
    }

    if (field === 'address' && !(value as string)) {
      message = 'Address is required.';
    } else if (field === 'address' && (value as string).length > 100) {
      message = 'Address must be under 100 letters.';
    }


    if (field === 'username' && !(value as string)) {
      message = 'Username is required.';
    } else if (field === 'username' && (value as string).length < 1) {
      message = 'Username must have at least 1 character.';
    } else if (field === 'username' && (value as string).length > 20) {
      message = 'Username cannot be longer than 20 characters.';
    } else if (field === 'username' && !/^[a-zA-Z0-9_]*$/.test(value as string)) {
      message = 'Username can only contain letters, numbers, and underscores.';
    }

    if (field === 'password' && !(value as string)) {
      message = 'Password is required.';
    } else if (field === 'password' && (value as string).length < 8) {
      message = 'Password must be at least 8 characters long.';
      // } else if (field === 'password' && !/(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$/
      //   .test(value as string)) {
      //   message = 'Password must contain at least one lowercase letter, one uppercase letter, and one number. You cant use dot.';
    }

    if (field === 'email' && !(value as string)) {
      message = 'Email is required.';
    } else if (field === 'email' && !/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(value as string)) {
      message = 'Invalid email format.';
    }

    if (field === 'phoneNumber' && !(value as string)) {
      message = 'Phone number is required.';
    } else if (field === 'phoneNumber' && !/^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$/.test(value as string)) {
      message = 'Phone number must be a valid phone number for Bosnia and Herzegovina.';
    }

    if (message) {
      this.errors[field] = message;
    } else {
      delete this.errors[field];
    }
  }

  isValidForm() {
    const requiredFields = [
      'name',
      'owner',
      'yearFounded',
      'address',
      'username',
      'email',
      'phoneNumber',
      'cityId',
      'password'
    ];

    // Check if all required fields are filled
    const allFieldsFilled = requiredFields.every(
      (field) => this.editShelter[field as keyof ShelterUpdateRequest] !== undefined &&
        this.editShelter[field as keyof ShelterUpdateRequest] !== null &&
        this.editShelter[field as keyof ShelterUpdateRequest] !== ''
    );

    // Ensure there are no errors
    const noErrors = Object.values(this.errors).every((error) => !error);

    return allFieldsFilled && noErrors;
  }


  onUpdateShelter() {
    this.isSubmitting = true;

    this.shelterService.updateShelter(this.shelterId, this.editShelter).subscribe(
      (response) => {
        Swal.fire({
          title: 'Podaci o azilu su uređeni!',
          text: 'Uspješno ste spremili podatke o azilu',
          icon: 'success',
          confirmButtonColor: '#3085d6',
          confirmButtonText: 'U redu'
        });

        this.router.navigate(['/shelter/profile']);
      },
      (error) => {
        if (error.status === 400) {
          if (typeof error.error === "string") {
            alert(error.error);
          } else {
            console.error('Unheandled backend error: ', error.error);
          }
        } else {
          console.error('Error updating shelter:', error);
        }
        this.isSubmitting = false;
      }
    );
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
  }
  async handleFile(file: File): Promise<void> {
    if (file.type.startsWith('image/')) {
      try {
        this.isSubmitting = true;





        const imageUrl = await this.uploadImageToBlob(file);
        this.editShelter.imageUrl = imageUrl;
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

  onClose() {
    this.router.navigate(['shelter/profile'])
  }
}
