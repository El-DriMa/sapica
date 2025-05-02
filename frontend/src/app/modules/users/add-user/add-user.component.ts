import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserEndpointsService, UserCreateRequest } from '../../../endpoints/UserEndpointsService';
import { FormsModule } from '@angular/forms';
import {NgForOf, NgIf} from "@angular/common";
import {UniqueUsernameValidatorDirective} from "../../../shared/unique-username-validator.directive";
import {UniqueEmailValidatorDirective} from "../../../shared/unique-email-validator.directive";
import {DndDirective} from "./dnd.directive";
import {ImageCompressionService} from "../image-compression.service";
import Swal from "sweetalert2";
@Component({
  selector: 'app-add-user',
  templateUrl: './add-user.component.html',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf,
    NgIf,
    UniqueUsernameValidatorDirective,
    UniqueEmailValidatorDirective,
    DndDirective
  ],
  styleUrls: ['./add-user.component.css']
})
export class AddUserComponent implements OnInit {
  newUser: UserCreateRequest = {
    firstName: '',
    lastName: '',
    yearBorn: 1930,
    username: '',
    password: '',
    email: '',
    phoneNumber: '',
    cityId: 0,
    imageUrl: '',
  };

  backgroundUrl: string = 'assets/background.jpg';
  errors: { [key: string]: string } = {};
  cities: { id: number, name: string, countryId: number, latitude: number, longitude: number }[] = [];
  isSubmitting = false;
  file: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;
  passwordStrength = {
    score: 0, // 0 to 4
    label: 'Slaba',
    color: 'red'
  };

  constructor(private userService: UserEndpointsService, private router: Router,
              private imageCompressionService: ImageCompressionService) {}
  ngOnInit(): void {
    this.loadCities();
  }

  loadCities() {
    this.userService.getCities().subscribe(
      (data) => {
        console.log("Fetched cities:", data);
        this.cities = data;
      },
      (error) => {
        console.error("Error fetching cities", error);
      }
    );
  }

  triggerFileInput() {
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    fileInput?.click();
  }

  onFileDropped(fileList: FileList) {
    this.processFile(fileList[0]);
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target?.files && target.files.length > 0) {
      this.processFile(target.files[0]);
    }
  }

  async processFile(file: File) {
    if (!file.type.startsWith('image/')) {
      alert('Samo slike su dozvoljene!');
      return;
    }

    try {
      console.log('Original File Size:', file.size / 1024, 'KB');

      const compressedFile = await this.imageCompressionService.compressImage(file, 800, 800, 0.8);
      this.file = compressedFile;

      console.log('Compressed File Size:', compressedFile.size / 1024, 'KB');

      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result;
      };
      reader.readAsDataURL(compressedFile);
    } catch (error) {
      console.error('Error compressing image:', error);
      alert('Greška pri obradi slike.');
    }
  }

  removeFile() {
    this.file = null;
    this.previewUrl = null;
  }

  formatBytes(bytes: any, decimals = 2) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }

  validateInput(field: keyof UserCreateRequest) {
    const value = this.newUser[field];
    let message = '';
    if (field === 'firstName' && !(value as string)) {
      message = 'Ime je obavezno.';
    } else if (field === 'firstName' && (value as string).length > 20 && (value as string).length < 2) {
      message = 'Ime ne smije imati ispod 2 i preko 20 karaktera.';
    }
    if (field === 'lastName' && !(value as string)) {
      message = 'Prezime je obavezno.';
    } else if (field === 'lastName' && (value as string).length > 30 && (value as string).length < 2) {
      message = 'Prezime ne smije imati ispod 2 i preko 30 karaktera.';
    }
    if (field === 'yearBorn' && ((value as number) < 1930 || (value as number) > 2010)) {
      message = 'Godina rođenja mora biti između 1930 i 2010.';
    }
    if (field === 'username' && !(value as string)) {
      message = 'Korisničko ime je obavezno.';
    } else if (field === 'username' && (value as string).length < 1) {
      message = 'Korisničko ime mora imati barem 1 karakter.';
    } else if (field === 'username' && (value as string).length > 20) {
      message = 'Korisničko ime ne smije imati preko 20 karaktera.';
    } else if (field === 'username' && !/^[a-zA-Z0-9_]*$/.test(value as string)) {
      message = 'Korisničko ime može sadržavati samo slova, brojeve i donje crte.';
    }
    if (field === 'password' && !(value as string)) {
      message = 'Lozinka je obavezna.';
    } else if (field === 'password' && (value as string).length < 8) {
      message = 'Lozinka mora imati najmanje osam karaktera.';
    } else if (field === 'password' && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_]{8,}$/.test(value as string)) {
      message = 'Lozinka mora sadržavati barem jedno veliko slovo, jedno malo slovo i jedan broj.';
    }
    if (field === 'email' && !(value as string)) {
      message = 'Email je obavezan.';
    } else if (field === 'email' && !/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(value as string)) {
      message = 'Format nije valjan.';
    }
    if (field === 'phoneNumber' && !(value as string)) {
      message = 'Broj telefona je obavezan.';
    } else if (field === 'phoneNumber' && !/^(\+387|0)6[0-7][0-9][0-9][0-9][0-9][0-9][0-9]$/.test(value as string)) {
      message = 'Broj telefona mora biti validan za Bosnu i Hercegovinu.';
    }
    if (message) {
      this.errors[field] = message;
    } else {
      delete this.errors[field];
    }
  }

  onlyAllowNumbers(event: KeyboardEvent) {
    const allowedKeys = ['Backspace', 'ArrowLeft', 'ArrowRight', 'Delete', 'Tab', 'Enter'];

    if (allowedKeys.indexOf(event.key) !== -1) {
      return;
    }

    if (!/\d/.test(event.key)) {
      event.preventDefault();
      return;
    }
  }

  calculatePasswordStrength(password: string) {
    let score = 0;

    if (password.length >= 8) score++;
    if (password.length >= 12) score++;

    if (/\d/.test(password)) score++;

    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) score++;

    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;

    this.passwordStrength.score = Math.min(score, 4);
    switch (score) {
      case 0:
      case 1:
        this.passwordStrength.label = 'Slaba';
        this.passwordStrength.color = 'red';
        break;
      case 2:
        this.passwordStrength.label = 'Srednje jaka';
        this.passwordStrength.color = 'orange';
        break;
      case 3:
        this.passwordStrength.label = 'Jaka';
        this.passwordStrength.color = 'blue';
        break;
      case 4:
        this.passwordStrength.label = 'Vrlo jaka';
        this.passwordStrength.color = 'green';
        break;
    }
  }

  isValidForm() {
    const { imageUrl, ...requiredFields } = this.newUser;

    const areFieldsValid = Object.values(requiredFields).every(
      (value) => value !== null && value !== undefined && value !== ''
    );

    return (
      areFieldsValid &&
      //!this.errors.cityId &&
      this.newUser.cityId > 0 &&
      this.file !== null &&
      Object.values(this.errors).every((error) => !error)
    );
  }

  async uploadUserImage(file: File): Promise<void> {
    try {
      const imageUrl = await this.userService.uploadImage(file);
      console.log('Uploaded image URL:', imageUrl);
    } catch (error) {
      console.error('Failed to upload image:', error);
    }
  }

  async onAddUser() {
    if (!this.file) {
      await Swal.fire({
        title: 'Greška',
        text: 'Molimo Vas da postavite sliku.',
        icon: 'error',
        confirmButtonText: 'OK',
        background: '#f9f9f9',
        confirmButtonColor: '#007bff',
      });
      return;
    }

    this.isSubmitting = true;

    try {
      this.newUser.imageUrl = await this.userService.uploadImage(this.file);

      this.userService.createUser(this.newUser).subscribe(
        (response) => {
          Swal.fire({
            title: 'Uspješno!',
            text: 'Profil uspješno kreiran! Provjerite Vaš mail kako biste aktivirali profil!',
            icon: 'success',
            confirmButtonText: 'OK',
            background: '#f9f9f9',
            confirmButtonColor: '#007bff',
          }).then(() => {
            this.router.navigate(['/login']);
          });
        },
        (error) => {
          console.error('Error adding user:', error);
          const errorMessage =
            error.status === 400 && typeof error.error === 'string'
              ? error.error
              : 'Došlo je do greške prilikom dodavanja korisnika.';

          Swal.fire({
            title: 'Greška',
            text: errorMessage,
            icon: 'error',
            confirmButtonText: 'OK',
            background: '#f9f9f9',
            confirmButtonColor: '#007bff',
          });
          this.isSubmitting = false;
        }
      );
    } catch (error) {
      console.error('Failed to upload image:', error);

      await Swal.fire({
        title: 'Greška',
        text: 'Greška pri slanju slike. Molimo pokušajte ponovo.',
        icon: 'error',
        confirmButtonText: 'OK',
        background: '#f9f9f9',
        confirmButtonColor: '#007bff',
      });
      this.isSubmitting = false;
    }
  }
}
