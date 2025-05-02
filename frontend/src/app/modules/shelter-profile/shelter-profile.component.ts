import {Component, ElementRef, OnInit, ViewChild, PLATFORM_ID, Inject} from '@angular/core';
import {AdoptionPostComponent} from "../adoption-post/adoption-post.component";
import {DatePipe, NgForOf, NgIf, isPlatformBrowser} from "@angular/common";
import {FavoritesComponent} from "../favorites/favorites.component";
import {ImageCroppedEvent, ImageCropperComponent} from "ngx-image-cropper";
import {ReactiveFormsModule} from "@angular/forms";
import {Router, RouterLink} from "@angular/router";
import {UniqueEmailValidatorDirective} from "../../shared/unique-email-validator.directive";
import {UniqueUsernameValidatorDirective} from "../../shared/unique-username-validator.directive";
import {UserEndpointsService, UserPatchRequest, UserReadResponse} from "../../endpoints/UserEndpointsService";
import {
  AdoptionRequestByUsernameReadResponse,
  AdoptionRequestEndpointsService
} from "../../endpoints/AdoptionRequestEndpointsService";
import {HttpClient} from "@angular/common/http";
import {MyAuthService} from "../../services/auth-services/my-auth.service";
import {ImageCompressionService} from "../users/image-compression.service";
import Swal from "sweetalert2";
import {ShelterEndpointsService, ShelterReadResponse} from "../../endpoints/ShelterEndpointsService";
import { AfterViewInit } from '@angular/core';
import * as L from 'leaflet';
@Component({
  selector: 'app-shelter-profile',
  standalone: true,
    imports: [
        AdoptionPostComponent,
        DatePipe,
        FavoritesComponent,
        NgForOf,
        NgIf,
        ReactiveFormsModule,
        RouterLink,
    ],
  templateUrl: './shelter-profile.component.html',
  styleUrl: './shelter-profile.component.css'
})
export class ShelterProfileComponent implements OnInit, AfterViewInit {

  @ViewChild('map', {static: false}) mapContainer!: ElementRef;
  map!: L.Map;

  backgroundUrl: string = 'assets/background.jpg';
  activeTab: 'posts' | 'favorites' | 'requests' = 'posts';
  shelter: ShelterReadResponse | null = null;
  isLoggedIn: boolean = false;
  shelterId: number | undefined = 0;
  isUsernameOrEmailUpdated: boolean = false;
  cities: { id: number, name: string, countryId: number, latitude: number, longitude: number }[] = [];
  city: string | undefined = '';
  username = '';
  requests: AdoptionRequestByUsernameReadResponse[] = [];

  @ViewChild('tableBody', {static: true}) tableBody!: ElementRef<HTMLTableSectionElement>;

  constructor(private http: HttpClient, private shelterService: ShelterEndpointsService,
              private authService: MyAuthService, private router: Router,
              private adoptionRequestService: AdoptionRequestEndpointsService,
              @Inject(PLATFORM_ID) private platformId: Object
  ) {
  }

  // async ngOnInit(): Promise<void> {
  //   this.isLoggedIn = this.authService.isLoggedIn();

  //   // Učitavanje shelter profila
  //   this.shelterService.loadShelterProfile().subscribe({
  //     next: (data: ShelterReadResponse) => {
  //       this.shelter = data;
  //       this.username = data.username;
  //       this.fetchAndRenderAdoptionRequests(data.username);

  //       // Tek kad imamo shelter, učitavamo gradove
  //       this.loadCities().then(() => {
  //         if (this.shelter) {
  //           this.initMapIfReady();
  //         }
  //       });
  //     },
  //     error: (error) => {
  //       console.error('Greška pri preuzimanju korisničkih podataka:', error);
  //     }
  //   });

  //   if (typeof window !== 'undefined') {
  //     await import('leaflet');
  //   }
  // }

  async ngOnInit(): Promise<void> {
    this.isLoggedIn = this.authService.isLoggedIn();

    this.shelterService.loadShelterProfile().subscribe({
      next: (data: ShelterReadResponse) => {
        this.shelter = data;
        this.username = data.username;
        this.fetchAndRenderAdoptionRequests(data.username);

        this.loadCities().then(() => {
          if (this.shelter) {
            const city = this.cities.find(c => c.id === this.shelter?.cityId);
            if (city) {
              this.loadMap(city.latitude, city.longitude);
            }
          }
        });
      },
      error: (error) => {
        console.error('Greška pri preuzimanju korisničkih podataka:', error);
      }
    });

    if (isPlatformBrowser(this.platformId)) {
      await import('leaflet');
    }
  }
  fetchAndRenderAdoptionRequests(username: string): void {
    this.adoptionRequestService.getAdoptionRequestsByUsername(username).subscribe({
      next: (data) => {
        this.requests = data;

      },
      error: (error) => {
        if (error.status === 404) {
          console.warn('No requests found for the user.');
          this.requests = [];
        } else {
          console.error('An error occurred:', error);
        }
      }

    });
  }

  loadCities(): Promise<void> {
    return new Promise((resolve) => {
      this.shelterService.getCities().subscribe({
        next: (data) => {
          console.log("Fetched cities:", data);
          this.cities = data;
          resolve();
        },
        error: (error) => {
          console.error("Error fetching cities", error);
          resolve();
        }
      });
    });
  }

  viewRequest(id: number) {
    this.router.navigate(['request-details', id]);
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

  async acceptRequest(id: number) {
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

  switchTab(tab: 'posts' | 'favorites' | 'requests'): void {
    this.activeTab = tab;
  }


  openEditProfileModal(): void {
    this.shelterId = this.shelter?.id;
    console.log(this.shelterId);
    this.router.navigate(['shelters/edit', this.shelterId]);
  }


  confirmDeleteProfile() {
    const shelterId = this.shelter?.id;
    if (!shelterId) {
      Swal.fire({
        title: 'Greška',
        text: 'Došlo je do greške prilikom brisanja azila.',
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
        this.shelterService.deleteShelter(shelterId).subscribe(
          () => {
            Swal.fire({
              title: 'Profil obrisan!',
              text: 'Vaš profil je uspješno obrisan.',
              icon: 'success',
              confirmButtonColor: '#3085d6',
              confirmButtonText: 'U redu'
            }).then(() => {
              this.router.navigate(['/logout']);
              console.log("Shelter deleted with ID:", shelterId);
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

  getCityName(cityId: number | undefined): string {
    const city = this.cities.find((c) => c.id === cityId);
    return city ? city.name : 'Nepoznato';
  }


  ngAfterViewInit() {
    if (isPlatformBrowser(this.platformId)) {
      setTimeout(() => {
        if (this.shelter?.cityId) {
          const city = this.cities.find(c => c.id === this.shelter?.cityId);
          if (city) {
            this.loadMap(city.latitude, city.longitude);
          }
        }
      }, 500);
    }
  }

  async loadMap(lat: number, lng: number) {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }


    const L = await import('leaflet');

    if (this.map) {
      this.map.remove();
    }

    if (!this.mapContainer || !this.mapContainer.nativeElement) {
      console.error('mapContainer nije dostupan! Mapa se neće učitati.');
      return;
    }

    this.map = L.map(this.mapContainer.nativeElement).setView([lat, lng], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    L.marker([lat, lng]).addTo(this.map)
      .bindPopup('Lokacija azila')
      .openPopup();
  }
}
