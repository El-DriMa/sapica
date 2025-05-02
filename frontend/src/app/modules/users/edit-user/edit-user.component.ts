import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserEndpointsService, UserUpdateRequest } from '../../../endpoints/UserEndpointsService';
import { FormsModule } from '@angular/forms';
import { NgForOf, NgIf } from "@angular/common";

@Component({
  selector: 'app-edit-user',
  templateUrl: './edit-user.component.html',
  standalone: true,
  imports: [
    FormsModule,
    NgForOf,
    NgIf
  ],
  styleUrls: ['./edit-user.component.css']
})
export class EditUserComponent implements OnInit {
  userId!: number;
  editUser: UserUpdateRequest = {
    firstName: '',
    lastName: '',
    yearBorn: 1900,
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

  constructor(
    private route: ActivatedRoute,
    private userService: UserEndpointsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.userId = +params['id'];
      this.loadUser();
      this.loadCities();
    });
  }

  loadUser() {
    this.userService.getUserById(this.userId).subscribe(
      (data) => {
        console.log('User data fetched:', data);
        this.editUser = data;
      },
      (error) => {
        console.error('Error fetching user data:', error);
        alert('Failed to fetch user details.');
        this.router.navigate(['/users']);
      }
    );
  }

  loadCities() {
    this.userService.getCities().subscribe(
      (data) => {
        console.log('Fetched cities:', data);
        this.cities = data;
      },
      (error) => {
        console.error('Error fetching cities:', error);
      }
    );
  }

  validateInput(field: keyof UserUpdateRequest) {
    const value = this.editUser[field];
    let message = '';

    if (field === 'firstName' && !(value as string)) {
      message = 'First name is required.';
    } else if (field === 'firstName' && (value as string).length > 20) {
      message = 'First name must be under 20 letters.';
    }
    if (field === 'lastName' && !(value as string)) {
      message = 'Last name is required.';
    } else if (field === 'lastName' && (value as string).length > 30) {
      message = 'Last name must be under 30 letters.';
    }
    if (field === 'yearBorn' && ((value as number) < 1900 || (value as number) > 2010)) {
      message = 'Year must be between 1900 and 2010.';
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
    } else if (field === 'password' && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_]{8,}$/.test(value as string)) {
      message = 'Password must contain at least one lowercase letter, one uppercase letter, and one number.';
    }
    if (field === 'email' && !(value as string)) {
      message = 'Email is required.';
    } else if (field === 'email' && !/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(value as string)) {
      message = 'Invalid email format.';
    }
    if (field === 'phoneNumber' && !(value as string)) {
      message = 'Phone number is required.';
    } else if (field === 'phoneNumber' && !/^(\+387|0)6[0-7][0-9][0-9][0-9][0-9][0-9][0-9]$/.test(value as string)) {
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
      'firstName',
      'lastName',
      'yearBorn',
      'username',
      'email',
      'phoneNumber',
      'cityId',
      'password'
    ];

    // Check if all required fields are filled
    const allFieldsFilled = requiredFields.every(
      (field) => this.editUser[field as keyof UserUpdateRequest] !== undefined &&
        this.editUser[field as keyof UserUpdateRequest] !== null &&
        this.editUser[field as keyof UserUpdateRequest] !== ''
    );

    // Ensure there are no errors
    const noErrors = Object.values(this.errors).every((error) => !error);

    return allFieldsFilled && noErrors;
  }


  onUpdateUser() {
    this.isSubmitting = true;

    this.userService.updateUser(this.userId, this.editUser).subscribe(
      (response) => {
        alert('User updated successfully!');
        this.router.navigate(['/users']);
      },
      (error) => {
        this.isSubmitting = false;
        if (error.status === 400) {
          if (typeof error.error === 'string') {
            alert(error.error);
          } else {
            console.error('Unhandled backend error:', error.error);
          }
        } else {
          console.error('Error updating user:', error);
        }
      }
    );
  }
}
