import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {Router, RouterLink} from "@angular/router";
import {DatePipe, NgForOf, NgIf} from "@angular/common";
import {
  UserEndpointsService, UserPatchRequest,
  UserReadResponse,
} from "../../../endpoints/UserEndpointsService";
import {MyAuthService} from "../../../services/auth-services/my-auth.service";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {UniqueUsernameValidatorDirective} from "../../../shared/unique-username-validator.directive";
import {UniqueEmailValidatorDirective} from "../../../shared/unique-email-validator.directive";
import Swal from "sweetalert2";
import {ImageCroppedEvent, ImageCropperComponent} from "ngx-image-cropper";
import {ImageCompressionService} from "../image-compression.service";
import { AdoptionPostComponent } from '../../adoption-post/adoption-post.component';
import {
  AdoptionRequestByUsernameReadResponse,
  AdoptionRequestEndpointsService
} from "../../../endpoints/AdoptionRequestEndpointsService";
import { FavoritesComponent } from "../../favorites/favorites.component";


@Component({
  selector: 'app-my-profile',
  standalone: true,
    imports: [
    RouterLink,
    NgIf,
    ReactiveFormsModule,
    FormsModule,
    NgForOf,
    UniqueUsernameValidatorDirective,
    UniqueEmailValidatorDirective,
    ImageCropperComponent,
    AdoptionPostComponent,
    DatePipe,
    FavoritesComponent
],
  templateUrl: './my-profile.component.html',
  styleUrl: './my-profile.component.css'
})
export class MyProfileComponent implements OnInit {
  updateUser: UserPatchRequest = {
    firstName: '',
    lastName: '',
    yearBorn: 1930,
    username: '',
    email: '',
    phoneNumber: '',
    cityId: 0,
    imageUrl: '',
  };
  backgroundUrl: string = 'assets/background.jpg';
  activeTab: 'posts' | 'favorites' | 'requests' = 'posts';
  user: UserReadResponse | null = null;
  isLoggedIn: boolean = false;
  isImageModalOpen: boolean = false;
  zoomLevel: number = 1;
  isEditProfileModalOpen: boolean = false;
  isChangePasswordModalOpen: boolean = false;
  passwordData = {
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
  };
  passwordErrors: { [key: string]: string } = {};
  cities: { id: number, name: string, countryId: number, latitude: number, longitude: number }[] = [];
  errors: { [key: string]: string } = {};
  isSubmitting = false;
  isUsernameOrEmailUpdated: boolean = false;
  isChangeImageModalOpen = false;
  imageChangedEvent: Event | null = null;
  croppedImage: Blob | null | undefined = null;

  username = '';
  requests: AdoptionRequestByUsernameReadResponse[] = [];

  @ViewChild('tableBody', { static: true }) tableBody!: ElementRef<HTMLTableSectionElement>;
  constructor(private http: HttpClient, private userService: UserEndpointsService,
              private authService: MyAuthService, private router: Router,
              private imageCompressionService: ImageCompressionService,private adoptionRequestService: AdoptionRequestEndpointsService,
             ) {}

  ngOnInit(): void {
    this.userService.loadUserProfile().subscribe({
      next: (data: UserReadResponse) => {
        this.user = data;
        this.username=data.username;
        this.fetchAndRenderAdoptionRequests(data.username);
      },
      error: (error) => {
        console.error('Greška pri preuzimanju korisničkih podataka:', error);
      }
    });
    this.isLoggedIn = this.authService.isLoggedIn();
    this.loadCities();
  }
  fetchAndRenderAdoptionRequests(username:string): void {
    this.adoptionRequestService.getAdoptionRequestsByUsername(username).subscribe({
      next: (data) => {
        this.requests = data;

      },
      error: (error) => {
        if (error.status === 404) {
          console.warn('No requests found for the user.'); // Obavesti korisnika u konzoli
          this.requests = []; // Postavi prazan niz za tabelu
        } else {
          console.error('An error occurred:', error);
        }}

    });
  }


  viewRequest(id:number) {
    this.router.navigate(['request-details',id]);
  }

  async rejectRequest(id: number) {
    const result = await Swal.fire({
      title: 'Da li ste sigurni?',
      text: 'Ova akcija će obrisati ovaj zahtjev!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Da, obriši zahtjev!',
      cancelButtonText: 'Otkaži'
    });

    if (result.isConfirmed) {
      try {
        await this.adoptionRequestService.deleteAdoptionRequest(id).toPromise();
        await Swal.fire({
          title: 'Obrisan zahtjev!',
          text: 'Ovaj zahtjev je uspješno obrisan.',
          icon: 'success',
          confirmButtonColor: '#3085d6',
          confirmButtonText: 'U redu'
        });
        this.fetchAndRenderAdoptionRequests(this.username);


      } catch (error) {
        await Swal.fire({
          title: 'Greška',
          text: 'Došlo je do greške prilikom brisanja zahtjeva. Molimo pokušajte ponovo.',
          icon: 'error',
          confirmButtonColor: '#d33',
          confirmButtonText: 'U redu'
        });
        console.error("Error deleting request", error);
      }
    }
  }

  async acceptRequest(id:number) {
    const result = await Swal.fire({
      title: 'Da li ste sigurni?',
      text: 'Ovime ćete prihvatiti zahtjev za udomljavanje!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Da, prihvati!',
      cancelButtonText: 'Otkaži'
    });

    if (result.isConfirmed) {
      try {
        await this.adoptionRequestService.acceptAdoptionRequest(id).subscribe();
        await Swal.fire({
          title: 'Prihvaćen zahtjev!',
          text: 'Budući udomitelj će dobiti mail obavještenje o odobrenju.',
          icon: 'success',
          confirmButtonColor: '#3085d6',
          confirmButtonText: 'U redu'
        });
        this.fetchAndRenderAdoptionRequests(this.username);


      } catch (error) {
        await Swal.fire({
          title: 'Greška',
          text: 'Došlo je do greške prilikom prihvatanja zahtjeva. Molimo pokušajte ponovo.',
          icon: 'error',
          confirmButtonColor: '#d33',
          confirmButtonText: 'U redu'
        });
        console.error("Error accepting request", error);
      }
    }
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

  switchTab(tab: 'posts' | 'favorites' | 'requests'): void {
    this.activeTab = tab;
  }

  openImageModal(): void {
    this.isImageModalOpen = true;
  }

  closeImageModal(): void {
    this.isImageModalOpen = false;
  }

  zoomIn(): void {
    this.zoomLevel += 0.2;
  }

  zoomOut(): void {
    if (this.zoomLevel > 0.4) {
      this.zoomLevel -= 0.2;
    }
  }

  resetZoom(): void {
    this.zoomLevel = 1;
  }

  openEditProfileModal(): void {
    this.isEditProfileModalOpen = true;
    if (this.user?.id) {
      this.updateUser = { ...this.user };
    }
  }

  closeEditProfileModal(): void {
    this.isEditProfileModalOpen = false;
  }

  onUpdate() {
    this.isSubmitting = true;

    if (!this.user?.id) {
      console.error('User ID is missing');
      return;
    }

    if (this.user.username != this.updateUser.username || this.user.email != this.updateUser.email) {
      this.isUsernameOrEmailUpdated = true;
    }

    this.userService.patchUser(this.user.id, this.updateUser).subscribe(
      (response) => {
        if (this.isUsernameOrEmailUpdated) {
          Swal.fire({
            title: 'Uspješno ažuriranje!',
            text: 'Podaci su uspješno ažurirani! Molimo Vas da se ponovo prijavite.',
            icon: 'success',
            confirmButtonText: 'OK',
            background: '#f9f9f9',
            confirmButtonColor: '#007bff',
            customClass: {
              popup: 'swal-text',
            },
          }).then(() => {
            this.router.navigate(['/login']);
          });
          return;
        }

        Swal.fire({
          title: 'Uspješno ažuriranje!',
          text: 'Podaci su uspješno ažurirani.',
          icon: 'success',
          confirmButtonText: 'OK',
          background: '#f9f9f9',
          confirmButtonColor: '#007bff',
          customClass: {
            popup: 'swal-text',
          },
        }).then(() => {
          window.location.reload();
        });
      },
      (error) => {
        this.isSubmitting = false;
        if (error.status === 400) {
          if (typeof error.error === 'string') {
            Swal.fire({
              title: 'Greška',
              text: error.error,
              icon: 'error',
              confirmButtonText: 'OK',
              background: '#f9f9f9',
              confirmButtonColor: '#007bff',
            });
          } else {
            console.error('Unhandled backend error:', error.error);
          }
        } else {
          console.error('Error updating user:', error);
        }
      }
    );
  }

  openChangePasswordModal(): void {
    this.isChangePasswordModalOpen = true;
    this.passwordData = { currentPassword: '', newPassword: '', confirmNewPassword: '' };
    this.passwordErrors = {};
  }

  validatePasswordInput(field: string): void {
    if (field === 'newPassword' && this.passwordData.newPassword.length < 8) {
      this.passwordErrors['newPassword'] = 'Nova lozinka mora imati najmanje 8 karaktera.';
    } else if (
      field === 'newPassword' &&
      !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_]{8,}$/.test(this.passwordData.newPassword)
    ) {
      this.passwordErrors['newPassword'] = 'Lozinka mora sadržavati barem jedno veliko slovo, jedno malo slovo i jedan broj.';
    } else if (field === 'confirmNewPassword' && this.passwordData.newPassword !== this.passwordData.confirmNewPassword) {
      this.passwordErrors['confirmNewPassword'] = 'Lozinke se ne poklapaju.';
    } else {
      delete this.passwordErrors[field];
    }
  }

  onChangePassword() {
    const { currentPassword, newPassword, confirmNewPassword } = this.passwordData;

    if (!currentPassword || !newPassword || !confirmNewPassword) {
      this.passwordErrors['general'] = 'Sva polja su obavezna.';
      return;
    }

    if (newPassword !== confirmNewPassword) {
      this.passwordErrors['confirmNewPassword'] = 'Nova lozinka i potvrda moraju biti iste.';
      return;
    }

    this.userService.changePassword({ currentPassword, newPassword }).subscribe({
      next: (response) => {
        Swal.fire({
          title: 'Uspješno!',
          text: 'Vaša lozinka je uspješno promijenjena.',
          icon: 'success',
          confirmButtonText: 'OK',
          background: '#f9f9f9',
          confirmButtonColor: '#007bff',
          customClass: {
            popup: 'swal-text',
          },
        }).then(() => {
          this.closeChangePasswordModal();
        });
      },
      error: (err: any) => {
        if (err.error?.detail === "Current password is incorrect.") {
          this.passwordErrors['currentPassword'] = "Trenutna lozinka je netačna.";
        } else if (err.error?.detail) {
          this.passwordErrors['general'] = err.error.detail;
        } else {
          this.passwordErrors['general'] = 'Došlo je do greške. Pokušajte ponovo.';
        }
      },
    });
  }

  closeChangePasswordModal(): void {
    this.isChangePasswordModalOpen = false;
  }

  validateInput(field: keyof UserPatchRequest) {
    const value = this.updateUser[field];
    let message = '';
    if (field === 'firstName' && !(value as string)) {
      message = 'Ime je obavezno.';
    } else if (field === 'firstName' && (value as string).length > 20) {
      message = 'Ime ne smije imati preko 20 karaktera.';
    }
    if (field === 'lastName' && !(value as string)) {
      message = 'Prezime je obavezno.';
    } else if (field === 'lastName' && (value as string).length > 30) {
      message = 'Prezime ne smije imati preko 30 karaktera.';
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

  openChangeImageModal() {
    this.isChangeImageModalOpen = true;
  }

  closeChangeImageModal() {
    this.isChangeImageModalOpen = false;
    this.imageChangedEvent = null;
    this.croppedImage = null;
  }

  fileChangeEvent(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    if (inputElement?.files?.length) {
      this.imageChangedEvent = event;
    }
  }

  imageCropped(event: ImageCroppedEvent) {
    if (event.blob) {
      this.croppedImage = event.blob;
    } else {
      console.error("Greška: Nema izrezane slike!");
    }
  }

  async onChangeImage() {
    console.log('Pozvana onChangeImage');

    if (this.croppedImage) {
      const file = new File([this.croppedImage], 'profile-image.jpg', { type: 'image/jpeg' });

      try {
        const compressedFile = await this.imageCompressionService.compressImage(file, 800, 800, 0.8);
        this.updateUser.imageUrl = await this.userService.updateUserImage(compressedFile);

        Swal.fire({
          title: 'Uspješno!',
          text: 'Profilna slika uspješno promijenjena!',
          icon: 'success',
          confirmButtonText: 'OK',
          background: '#f9f9f9',
          confirmButtonColor: '#007bff',
          customClass: {
            popup: 'swal-text',
          },
        }).then(() => {
          this.closeChangeImageModal();
          window.location.reload();
        });
      } catch (error) {
        console.error(error);
        Swal.fire({
          title: 'Greška',
          text: 'Greška prilikom učitavanja slike!',
          icon: 'error',
          confirmButtonText: 'OK',
          background: '#f9f9f9',
          confirmButtonColor: '#007bff',
        });
      }
    } else {
      Swal.fire({
        title: 'Greška',
        text: 'Molimo Vas da postavite sliku.',
        icon: 'error',
        confirmButtonText: 'OK',
        background: '#f9f9f9',
        confirmButtonColor: '#007bff',
      });
    }
  }

  confirmDeleteProfile() {
    const userId = this.user?.id;
    if (!userId) {
      Swal.fire({
        title: 'Greška',
        text: 'Došlo je do greške prilikom brisanja korisnika.',
        icon: 'error',
        confirmButtonColor: '#d33',
        confirmButtonText: 'U redu'
      });
      return;
    }

    Swal.fire({
      title: 'Da li ste sigurni?',
      text: 'Ova akcija će trajno obrisati vaš profil i ne može se poništiti!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Da, obriši profil!',
      cancelButtonText: 'Otkaži'
    }).then((result) => {
      if (result.isConfirmed) {
        this.userService.deleteUser(userId).subscribe(
          () => {
            Swal.fire({
              title: 'Profil obrisan!',
              text: 'Vaš profil je uspješno obrisan.',
              icon: 'success',
              confirmButtonColor: '#3085d6',
              confirmButtonText: 'U redu'
            }).then(() => {
              this.router.navigate(['/logout']);
              console.log("User deleted with ID:", userId);
            });
          },
          (error) => {
            Swal.fire({
              title: 'Greška',
              text: 'Došlo je do greške prilikom brisanja vašeg profila. Molimo pokušajte ponovo.',
              icon: 'error',
              confirmButtonColor: '#d33',
              confirmButtonText: 'U redu'
            });
            console.error("Error deleting user", error);
          }
        );
      }
    });
  }

}
