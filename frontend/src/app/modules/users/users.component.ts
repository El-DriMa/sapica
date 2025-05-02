import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { UserEndpointsService, UserReadResponse } from '../../endpoints/UserEndpointsService';
import {FormsModule} from "@angular/forms";
import {debounceTime, distinctUntilChanged, Subject, switchMap} from "rxjs";

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [RouterModule, CommonModule, HttpClientModule, FormsModule],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {
  users: UserReadResponse[] = [];
  totalUsers = 0;
  currentPage = 1;
  pageSize = 10;
  filters: any = {
    firstName: '',
    lastName: '',
    minYearBorn: 1930,
    maxYearBorn: 2010,
    cityId: '',
    sortOrder: 'Descending'
  };
  cities: { id: number, name: string, countryId: number, latitude: number, longitude: number }[] = [];

  searchSubject: Subject<any> = new Subject();

  constructor(private userService: UserEndpointsService) {}

  ngOnInit(): void {
    this.loadAllUsers();
    this.loadCities();

    this.searchSubject.next({ ...this.filters });
    this.searchSubject.pipe(
      debounceTime(100),
      distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
      switchMap((filters) => {
        return this.userService.getAllUsers(this.currentPage, this.pageSize, filters);
      })
    ).subscribe(
      (data) => {
        this.users = data.users.map(user => ({
          ...user,
          postCount: user.postCount || 0
        }));
        this.totalUsers = data.totalUsers;
      },
      (error) => {
        console.error('Error fetching users:', error);
      }
    );
  }

  loadAllUsers() {
    this.userService.getAllUsers(
      this.currentPage,
      this.pageSize,
      this.filters
    ).subscribe(
      (data) => {
        this.users = data.users.map(user => ({
          ...user,
          postCount: user.postCount || 0
        }));
        this.totalUsers = data.totalUsers;
      },
      (error) => {
        console.error("Error fetching users with post counts", error);
      }
    );
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

  isMinYearInvalid(): boolean {
    return this.filters.minYearBorn && (this.filters.minYearBorn < 1930 || this.filters.minYearBorn > 2010);
  }

  isMaxYearInvalid(): boolean {
    return this.filters.maxYearBorn && (this.filters.maxYearBorn < 1930 || this.filters.maxYearBorn > 2010);
  }

  onFilterChange() {
    if (this.isMinYearInvalid() || this.isMaxYearInvalid()) {
      return;
    }
    this.searchSubject.next({ ...this.filters });
  }

  applyFilters() {
    this.currentPage = 1;
    this.loadAllUsers();
  }

  nextPage() {
    if (this.currentPage * this.pageSize < this.totalUsers) {
      this.currentPage++;
      this.loadAllUsers();
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadAllUsers();
    }
  }

  onDeleteUser(id: number) {
    const isConfirmed = window.confirm("Jeste li sigurni da želite obrisati ovog korisnika?");
    if (isConfirmed) {
      this.userService.deleteUser(id).subscribe(
        () => {
          console.log("User deleted with ID:", id);
          this.loadAllUsers();
        },
        (error) => {
          console.error("Error deleting user", error);
        }
      );
    }
  }

  protected readonly Math = Math;
}
