import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import { MyConfig } from '../my-config';
import {Observable} from "rxjs";

export interface UserReadResponse {
  id: number;
  firstName: string;
  lastName: string;
  yearBorn: number;
  username: string;
  email: string;
  imageUrl: string;
  phoneNumber: string;
  cityId: number;
  city: {
    id: number;
    name: string;
    countryId: number;
    latitude: number;
    longitude: number;
  };
  postCount: number;
}

export interface UserCreateRequest {
  firstName: string;
  lastName: string;
  yearBorn: number;
  username: string;
  password: string;
  email: string;
  phoneNumber: string;
  imageUrl?: string | ArrayBuffer | null;
  cityId: number;
}

export interface UserUpdateRequest {
  firstName?: string;
  lastName?: string;
  yearBorn?: number;
  username?: string;
  password?: string;
  email?: string;
  phoneNumber?: string;
  imageUrl?: string | ArrayBuffer | null;
  cityId?: number;
}

export interface UserPatchRequest {
  firstName?: string;
  lastName?: string;
  yearBorn?: number;
  username?: string;
  password?: string;
  email?: string;
  phoneNumber?: string;
  imageUrl?: string | ArrayBuffer | null;
  cityId?: number;
}

@Injectable({
  providedIn: 'root',
})

export class UserEndpointsService {
  private apiUrl = `${MyConfig.api_address}`;

  constructor(private http: HttpClient) {}

  getAllUsers(page: number, pageSize: number, filters: any) {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (filters.firstName) params = params.set('firstName', filters.firstName);
    if (filters.lastName) params = params.set('lastName', filters.lastName);
    if (filters.minYearBorn) params = params.set('minYearBorn', filters.minYearBorn);
    if (filters.maxYearBorn) params = params.set('maxYearBorn', filters.maxYearBorn);
    if (filters.cityId) params = params.set('cityId', filters.cityId);
    if (filters.sortOrder) params = params.set('sortOrder', filters.sortOrder);

    return this.http.get<{ users: UserReadResponse[], totalUsers: number }>(
      `${this.apiUrl}/users`,
      { params }
    );
  }

  getUserById(id: number) {
    return this.http.get<UserReadResponse>(`${this.apiUrl}/users/${id}`);
  }

  loadUserProfile() {
    return this.http.get<UserReadResponse>(`${this.apiUrl}/login/GetCurrentUser`);
  }

  createUser(request: UserCreateRequest) {
    return this.http.post<UserReadResponse>(`${this.apiUrl}/users`, request);
  }

  patchUser(id: number, request: UserPatchRequest) {
    return this.http.patch<UserReadResponse>(
      `${this.apiUrl}/users/${id}`,
      request
    );
  }

  updateUser(id: number, request: UserUpdateRequest) {
    return this.http.put<UserReadResponse>(
      `${this.apiUrl}/users/${id}`,
      request
    );
  }

  deleteUser(id: number) {
    return this.http.delete(`${this.apiUrl}/users/${id}`);
  }

  getCities() {
    return this.http.get<{ id: number, name: string, countryId: number, latitude: number, longitude: number }[]>(`${this.apiUrl}/cities`);
  }

  checkUsernameAvailability(params: { username: string }) {
    const httpParams = new HttpParams()
      .set('username', params.username);

    return this.http.get<{ usernameAvailable: boolean }>(
      `${this.apiUrl}/users/check-username-availability`,
      { params: httpParams }
    );
  }

  checkEmailAvailability(params: { email: string }) {
    const httpParams = new HttpParams()
      .set('email', params.email);

    return this.http.get<{ emailAvailable: boolean }>(
      `${this.apiUrl}/users/check-email-availability`,
      { params: httpParams }
    );
  }

  async uploadImage(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http
      .post<{ imageUrl: string }>(`${this.apiUrl}/api/PicturesEndpoints/upload-image`, formData)
      .toPromise()
      .then(response => {
        if (!response || !response.imageUrl) {
          throw new Error('Response is undefined or does not contain an imageUrl.');
        }
        return response.imageUrl;
      })
      .catch(error => {
        console.error('Image upload failed:', error);
        throw new Error('Failed to upload image.');
      });
  }

  async updateUserImage(file: File): Promise<string> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http
      .post<{ imageUrl: string }>(`${this.apiUrl}/api/PicturesEndpoints/update-user-image`, formData)
      .toPromise()
      .then(response => {
        if (!response || !response.imageUrl) {
          throw new Error('Response is undefined or does not contain an imageUrl.');
        }
        return response.imageUrl;
      })
      .catch(error => {
        console.error('Image upload failed:', error);
        throw new Error('Failed to upload image.');
      });
  }

  changePassword(params: { currentPassword: string; newPassword: string }) {
    const body = {
      currentPassword: params.currentPassword,
      newPassword: params.newPassword
    };

    return this.http.post<{}>(
      `${this.apiUrl}/users/change-password`,
      body
    );
  }
}
