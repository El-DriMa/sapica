import { Component, OnInit,Inject,ChangeDetectorRef,AfterViewChecked, ElementRef, ViewChild  } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { AdoptionPostService, AdoptionPostReadResponse,CityReadResponse } from '../../endpoints/AdoptionPostEndpointsService';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SponsorsService,SponsorReadResponse } from '../../endpoints/SponsorsEndpointsService';
import {MyAuthService} from "../../services/auth-services/my-auth.service";
import {UserEndpointsService, UserReadResponse} from "../../endpoints/UserEndpointsService";
import * as L from 'leaflet';
import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';

import { jsPDF } from 'jspdf';
import html2canvas from "html2canvas";
import { DatePipe } from '@angular/common';
import { FavoriteService,FavoriteReadResponse } from '../../endpoints/FavoriteEndpointsService';

@Component({
  selector: 'app-homepage',
  standalone: true,
  imports: [RouterModule,HttpClientModule,FormsModule,CommonModule],
  templateUrl: './homepage.component.html',
  styleUrl: './homepage.component.css'
})
export class HomepageComponent implements OnInit {

  @ViewChild('mapContainer') mapContainerRef: ElementRef | undefined;

  adoptionPosts: AdoptionPostReadResponse[] = [];
  selectedPostId: number | null = null;
  selectedPost: AdoptionPostReadResponse | null = null;
  isMoreInfoVisible: boolean = false;
  city: CityReadResponse | null = null;
  currentPage : number = 1;
  itemsPerPage: number = 10;
  totalPages: number = 1;
  cities: CityReadResponse[] = [];
  favorites:FavoriteReadResponse[]=[];
  filters = {
    urgent: false,
    animalType: '',
    gender: '',
    cityId: 0,
    size:'',
  };
  sponsors: SponsorReadResponse[] = [];
  isLoggedIn: boolean = false;
  isShelter:boolean=false;
  isUser:boolean=false;

  private cachedPosts: { [key: string]: any[] } = {};
  private marker: L.Marker | null = null;

  renderer: any;

  constructor(private AdoptionPostService: AdoptionPostService,private SponsorService:SponsorsService,
              private authService: MyAuthService, private userService: UserEndpointsService,
              @Inject(PLATFORM_ID) private platformId: Object,
              private cdr: ChangeDetectorRef,private favoriteService:FavoriteService){}


  ngOnInit(): void {
    this.getAdoptionPosts();
    this.getCities();
    this.getSponsors();
    this.isLoggedIn = this.authService.isLoggedIn();
    const role=this.authService.getCurrentUser()?.Role;
    if(role=='Shelter')
    {
      this.isShelter=true;
    }
    else {
      this.isUser=true;
    }
    if(this.isLoggedIn) {
      this.getFavorites();
      this.userService.loadUserProfile().subscribe({
        next: (data: UserReadResponse) => {
          this.user = data;
        },
        error: (error) => {
          console.error('Greška pri preuzimanju korisniĝkih podataka:', error);
        }
      });
    }
  }

  ngAfterViewInit(): void {
    // Ensure selectedPost exists and has valid coordinates before initializing the map
    if (this.selectedPost && this.selectedPost.city?.latitude && this.selectedPost.city?.longitude) {
      // Initialize map only if it hasn't been initialized yet
      if (!this.map) {
        this.loadLeafletAndInitializeMap(this.selectedPost.city.latitude, this.selectedPost.city.longitude);
      }
    }
  }

  getFavorites() {
    this.favoriteService.getFavorites().subscribe({
      next: (favorites) => {
        this.favorites = favorites || [];
      },
      error: () => {

      }
    });
  }

  isFavorite(postId: number): boolean {
    return this.favorites.some(fav => fav.adoptionPostId === postId);
  }

  toggleFavorite(postId: number) {
    if (this.isFavorite(postId)) return;
    console.log(postId);
    this.favoriteService.createFavorite(postId).subscribe({
      next: () => {
        this.getFavorites();
      },
      error: (error) => {
        console.error('Greška pri dodavanju u favorite:', error);
      }
    });
  }



  mapInitialized: { [key: number]: boolean } = {};

  private map: L.Map | null = null;  // Keep the map variable for the current map instance


loadLeafletAndInitializeMap(latitude: number, longitude: number): void {
  if (!this.selectedPost) {
    console.log("selectedPost nije definisan.");
    return; // If selectedPost is not defined, exit
  }

  // Validate the coordinates before using them
  if (latitude == null || longitude == null) {
    console.log("Invalid coordinates: Latitude or Longitude is null or undefined.");
    return; // Exit if coordinates are invalid
  }

  console.log("Loading map with coordinates:", latitude, longitude);

  // If the map already exists, remove it
  if (this.map) {
    this.map.remove();  // Remove the old map if it exists
    this.map = null; // Reset the map object
  }

  import('leaflet').then((L) => {
    const bounds = L.latLngBounds(
      L.latLng(latitude - 0.5, longitude - 0.5),
      L.latLng(latitude + 0.5, longitude + 0.5)
    );

    const mapContainer = this.mapContainerRef?.nativeElement;

    // Check if map container exists
    if (mapContainer) {
      mapContainer.style.height = '500px'; // Set container height

      // Initialize the map
      this.map = L.map(mapContainer, {
        maxBounds: bounds,
        minZoom: 10,
        maxZoom: 18,
        zoomSnap: 0.5,
      });

      // Set map view
      this.map.setView([latitude, longitude], 13);

      // Add tile layer
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(this.map);
      const marker = L.marker([latitude, longitude]).addTo(this.map);
      const cityName = this.city?.name || 'Unknown City';
      marker.bindPopup(`<b>${cityName}</b>`);
    } else {
      console.error("Map container is missing.");
    }
  }).catch((error) => {
    console.error("Error loading Leaflet:", error);
  });
}

  openFullMap(postId: string): void {
    console.log("Open full map");
    if (isPlatformBrowser(this.platformId)) {
      setTimeout(() => {
        const post = this.adoptionPosts.find(p => p.id === Number(postId));
        if (post?.city?.latitude && post?.city?.longitude) {
          const mapUrl = `https://www.openstreetmap.org/?mlat=${post.city.latitude}&mlon=${post.city.longitude}#map=12/${post.city.latitude}/${post.city.longitude}`;
          window.open(mapUrl, '_blank');
        }
      }, 0);
    }
  }

  getSponsors(){
    this.SponsorService.getAllSponsors().subscribe((response) => {
      this.sponsors=response;
    });
  }

  openSponsorPage(sponsor:any): void {
    console.log('Sponsor link:', sponsor.websiteUrl);
    window.open(sponsor.websiteUrl, '_blank');
  }

  getCities(): void {
    this.AdoptionPostService.getCities().subscribe((response) => {
      this.cities = response;
    });
  }

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

  getCityById(id: number) {
    return this.AdoptionPostService.getCityById(id);
  }

  openMoreInfo(id: number): void {
    console.log("MORE INFO POZVANA");
    this.closeForm();
    this.AdoptionPostService.getAdoptionPostById(id).subscribe((response) => {
      this.selectedPost = response;
      console.log("Selected post:", this.selectedPost);
      if (this.selectedPost.cityId) {
        this.getCityById(this.selectedPost.cityId).subscribe((city) => {
          this.city = city;
          console.log("City fetched:", city);
          const latitude = this.city?.latitude;
          const longitude = this.city?.longitude;

          if (latitude != null && longitude != null) {
            // Remove map initialization check for the moment
            setTimeout(() => {
              this.loadLeafletAndInitializeMap(latitude, longitude);
            }, 0);
          } else {
            console.log("Invalid coordinates: Latitude or Longitude is null or undefined.");
          }

          this.cdr.detectChanges();
        });
      }
      this.isMoreInfoVisible = true;
    });
  }



  closeForm(): void {
    this.isMoreInfoVisible = false;
    this.selectedPostId = null;
  }
  closeMoreInfo(): void {
    this.isMoreInfoVisible = false;
    this.selectedPost = null;
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

  currentImageIndex: number = 0;
  user: UserReadResponse | null = null;

  previousImage() {
    if (this.selectedPost?.animal?.images?.length) {
      this.currentImageIndex = (this.currentImageIndex - 1 + this.selectedPost.animal.images.length) % this.selectedPost.animal.images.length;
    }
  }

  nextImage() {
    if (this.selectedPost?.animal?.images?.length) {
      this.currentImageIndex = (this.currentImageIndex + 1) % this.selectedPost.animal.images.length;
    }
  }

  zoomScale: number = 1;
  minZoomScale: number = 1;
  maxZoomScale: number = 3;

  setZoomLevel(scale: number): void {
    this.zoomScale = Math.min(this.maxZoomScale, Math.max(this.minZoomScale, scale));
    const image = document.querySelectorAll('.carousel-image')[this.currentImageIndex] as HTMLImageElement;
    if (image) {
      image.style.transform = `scale(${this.zoomScale})`;
    }
  }

  onWheelZoom(event: WheelEvent): void {
    const zoomFactor = event.deltaY < 0 ? 0.1 : -0.1;
    this.setZoomLevel(this.zoomScale + zoomFactor);
    event.preventDefault();
  }


  generatePDF() {
    const doc = new jsPDF();

    try {
      doc.addFont('assets/Amiko-Regular.ttf', 'Amiko', 'normal');
      doc.setFont("Amiko");
    } catch (e) {
      console.error("Greška pri dodavanju fonta:", e);
      doc.setFont("helvetica");
    }

    doc.setFontSize(12);
    doc.text("Informacije o životinji", 20, 20);

    let yPosition = 30;

    const animalName = this.selectedPost?.animal.name ?? '';
    console.log('Original name:', animalName);
    doc.text(`Ime: ${animalName}`, 20, yPosition);
    yPosition += 10;

    const dateOfCreation = this.selectedPost ? new Date(this.selectedPost.dateOfCreation).toLocaleDateString('bs-BA') : 'N/A';
    doc.text(`Datum objave: ${dateOfCreation}`, 20, yPosition);
    yPosition += 10;

    const cityName = this.city?.name ?? 'N/A';
    doc.text(`Grad: ${cityName}`, 20, yPosition);
    yPosition += 10;

    const animalType = this.selectedPost?.animal.animalType ?? 'N/A';
    doc.text(`Vrsta životinje: ${animalType}`, 20, yPosition);
    yPosition += 10;

    const animalSize = this.selectedPost?.animal.size ?? 'N/A';
    doc.text(`Velicina: ${animalSize}`, 20, yPosition);
    yPosition += 10;

    const animalWeight = this.selectedPost?.animal.weight ?? 'N/A';
    doc.text(`Težina: ${animalWeight.toString()} kg`, 20, yPosition);
    yPosition += 10;

    const animalColor = this.selectedPost?.animal.color ?? 'N/A';
    doc.text(`Boja: ${animalColor}`, 20, yPosition);
    yPosition += 10;

    const vaccinated = this.selectedPost?.animal.vaccinated ? 'Da' : 'Ne';
    doc.text(`Vakcinisan: ${vaccinated}`, 20, yPosition);
    yPosition += 10;

    const sterilized = this.selectedPost?.animal.sterilized ? 'Da' : 'Ne';
    doc.text(`Sterilisan: ${sterilized}`, 20, yPosition);
    yPosition += 10;

    const parasiteFree = this.selectedPost?.animal.parasiteFree ? 'Da' : 'Ne';
    doc.text(`Oĝišćen od parazita: ${parasiteFree}`, 20, yPosition);
    yPosition += 10;

    const hasPassport = this.selectedPost?.animal.hasPassport ? 'Da' : 'Ne';
    doc.text(`Ima pasoš: ${hasPassport}`, 20, yPosition);
    yPosition += 10;

    const ownerName = this.selectedPost?.username ?? 'N/A';
    doc.text(`Vlasnik: ${ownerName}`, 20, yPosition);
    yPosition += 10;

    if (this.selectedPost?.animal.images[0].image) {
      doc.text("Slika životinje:", 20, yPosition);
      yPosition += 10;
      doc.addImage(this.selectedPost?.animal.images[0].image, 'JPEG', 20, yPosition, 85, 85);
    }


    const date = new Date();
    const formattedDate = date.toLocaleString('bs-BA', { dateStyle: 'short', timeStyle: 'short' });
    doc.setFontSize(10);
    doc.text(`Datum: ${formattedDate}`, 150, doc.internal.pageSize.height - 10);


    doc.save("info_o_zivotinji.pdf");
  }


}
