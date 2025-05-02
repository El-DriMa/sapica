import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AdoptionPostService, AdoptionPostReadResponse, AnimalReadResponse, AdoptionPostCreateRequest, AnimalCreateRequest, CityReadResponse,ImageReadResponse,ImageCreateRequest,ImageUpdateRequest } from '../../endpoints/AdoptionPostEndpointsService';
import { NgModule } from '@angular/core';
import { ShelterEndpointsService, ShelterReadResponse } from '../../endpoints/ShelterEndpointsService';
import { UserEndpointsService, UserReadResponse } from '../../endpoints/UserEndpointsService';
import { from } from 'rxjs';
import { MyConfig } from '../../my-config';
import { MyAuthService } from '../../services/auth-services/my-auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-adoption-post',
  standalone: true,
  imports: [RouterModule, CommonModule, HttpClientModule, FormsModule],
  templateUrl: './adoption-post.component.html',
  styleUrl: './adoption-post.component.css'
})
export class AdoptionPostComponent implements OnInit {

  currentDate: string = new Date().toISOString().split('T')[0];

  animals: AnimalReadResponse[] = [];
  adoptionPosts: AdoptionPostReadResponse[] = [];
  cities: CityReadResponse[] = [];
  city: CityReadResponse | null = null;
  selectedCityId: number | null = null;
  ageUnits: string[] = ['godine', 'mjeseci'];
  selectedAgeUnit: string = 'godine';
  isFormVisible: boolean = false;
  isMoreInfoVisible: boolean = false;
  selectedPostId: number | null = null;
  selectedVaccinated: boolean = false;
  selectedSterelized: boolean = false;
  selectedParasiteFree: boolean = false;
  selectedHasPassport: boolean = false;
  selectedUrgent: boolean = false;
  selectedPost: AdoptionPostReadResponse | null = null;
  username:string | null = null;
  animalImages: ImageReadResponse[] = [];
  newImages: ImageCreateRequest[] = [];
  user: UserReadResponse | null = null;
  newAnimal: any = {
    name: '',
    gender:'',
    size: '',
    age: '',
    color: '',
    weight: 0.5,
    animalType: '',
    vaccinated: false,
    sterilized: false,
    parasiteFree: false,
    hasPassport: false,
    images: this.newImages,
  }

  newAdoptionPost: any = {
    dateOfCreation: null,
    dateOfModification: null,
    viewCounter: 1,
    urgent: false,
    shortDescription: '',
    username:'',
    animal: this.newAnimal,
    cityId: null
  };
  http: any;

  constructor(private AdoptionPostService: AdoptionPostService,
              private ShelterEndpointsService: ShelterEndpointsService,
              private UserEndpointService: UserEndpointsService,
              private authService: MyAuthService) { }

  ngOnInit(): void {
    this.username = this.authService.getUsername();
    this.getAdoptionPosts();
    this.getCities();
    this.UserEndpointService.loadUserProfile().subscribe({
      next: (data: UserReadResponse) => {
        this.user = data;
      },
      error: (error) => {
        console.error('Greška pri preuzimanju korisniĝkih podataka:', error);
      }
    });
  }

  getAdoptionPosts(): void {
    this.AdoptionPostService.getAllAdoptionPostsLoggedIn().subscribe((response) => {
      this.adoptionPosts = response;
    });
  }

  getCities(): void {
    this.AdoptionPostService.getCities().subscribe((response) => {
      this.cities = response;
    });
  }

  getCityById(id: number) {
    return this.AdoptionPostService.getCityById(id);
  }

  openForm(id: number): void {
    this.isMoreInfoVisible = false;
    this.AdoptionPostService.getAdoptionPostById(id).subscribe(
      (response) => {
        if (response.animal) {
          this.newAnimal = response.animal;
          const ageParts = this.newAnimal.age.split(' ');
          if (ageParts.length > 1) {
            this.selectedAgeUnit = ageParts[1];
            this.newAnimal.age = ageParts[0];
          } else {
            this.selectedAgeUnit = 'godine';
          }


          this.newImages = this.newAnimal.images.map((imageObj: ImageReadResponse) => {
            return { image: 'data:image/jpeg;base64,' + imageObj.image }; // prefix for base64
          });
        }

        this.newAdoptionPost = {
          ...response,
          animal: undefined
        };

        this.selectedCityId = response.cityId ?? null;
        this.isFormVisible = true;
        this.selectedPostId = id;
      },
      (error) => {
        console.error('Error fetching adoption post', error);
      }
    );
  }


  closeForm(): void {
    this.isFormVisible = false;
    this.isMoreInfoVisible = false;
    this.selectedPostId = null;
    this.resetForm();
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
        });
      }
      this.isMoreInfoVisible = true;
    });

  }


  closeMoreInfo(): void {
    this.isMoreInfoVisible = false;
    this.selectedPost = null;
  }

  onSubmit(form:any): void {

    if (!form.valid) {
      Swal.fire(
        'Greška!',
        'Popunite sva obavezna polja.',
        'error'
      );
      return;
    }

    const addRequestPayload: any = {
      dateOfCreation: this.currentDate,
      viewCounter: 1,
      urgent: this.newAdoptionPost.urgent,
      shortDescription: this.newAdoptionPost.shortDescription,
      username:this.username,
      animal: {
        ...this.newAnimal,
        age: `${this.newAnimal.age} ${this.selectedAgeUnit}`,
        images: this.newImages.filter((img: ImageCreateRequest) => img.image !== '') // filtering empty images
      },
      cityId: this.selectedCityId
    };

    const updateRequestPayload: any = {
      dateOfModification: this.currentDate,
      viewCounter: this.newAdoptionPost.viewCounter,
      urgent: this.newAdoptionPost.urgent,
      shortDescription: this.newAdoptionPost.shortDescription,
      username:this.username,
      animal: {
        ...this.newAnimal,
        age: `${this.newAnimal.age} ${this.selectedAgeUnit}`,
        images: this.newImages.filter((img: ImageCreateRequest) => img.image !== '') // filtering empty images
      },
      cityId: this.selectedCityId
    };

    if (this.selectedPostId) {
      this.AdoptionPostService.updateAdoptionPost(this.selectedPostId, updateRequestPayload)
        .subscribe({
          next: (response) => {
            const animalId = response.animal.id;
            this.newImages.forEach(img => img.animalId = animalId);
            console.log('Adoption post updated successfully', response);
            Swal.fire(
              'Uspešno!',
              'Post je uspješno uređen.',
              'success'
            );
            this.getAdoptionPosts();
          },
          error: (error) => {
            console.error('Error updating adoption post', error);
            Swal.fire(
              'Greška!',
              'Došlo je do greške prilikom uređivanja.',
              'error'
            );
          }
        });
    } else {
      this.AdoptionPostService.addAdoptionPost(addRequestPayload)
        .subscribe({
          next: (response) => {
            const animalId = response.animal.id;
            this.newImages.forEach(img => img.animalId = animalId);
            console.log('Adoption post added successfully', response);
            Swal.fire(
              'Uspešno!',
              'Post je uspješno dodat.',
              'success'
            );
            this.getAdoptionPosts();
          },
          error: (error) => {
            console.error('Error adding adoption post', error);
            Swal.fire(
              'Greška!',
              'Došlo je do greške prilikom dodavanja.',
              'error'
            );
          }
        });
    }

    this.resetForm();
    this.newImages = [];
  }


  openNewForm(): void {
    this.resetForm();
    this.isFormVisible = true;
    this.isMoreInfoVisible = false;
  }


  isDragging: boolean = false;

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;

    const files = event.dataTransfer?.files;
    if (files) {
      this.handleFiles(files);
    }
  }

  private handleFiles(files: FileList): void {
    for (const file of Array.from(files)) {
      const reader = new FileReader();
      reader.onload = () => {
        const base64Image = reader.result as string;
        if (!this.newImages.some(img => img.image === base64Image)) {
          this.newImages.push({ image: base64Image, animalId: 0 });
        }
      };
      reader.readAsDataURL(file);
    }
  }

  onFilesSelected(event: Event): void {
    const files = (event.target as HTMLInputElement).files;
    if (files) {
      for (const file of Array.from(files)) {
        const reader = new FileReader();
        reader.onload = () => {
          const base64Image = reader.result as string;
          if (!this.newImages.some(img => img.image === base64Image)) {
            this.newImages.push({ image: base64Image, animalId: 0 });
          }
        };
        reader.readAsDataURL(file);
      }
    }
  }

  removeImage(image: ImageCreateRequest): void {
    this.newAnimal.images = this.newAnimal.images.filter((img: ImageCreateRequest) => img.image !== image.image);
    this.newImages = this.newImages.filter((img: ImageCreateRequest) => img.image !== image.image);
    console.log('Images after removal:', this.newImages);
  }

  currentImageIndex: number = 0;

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


  deleteAdoptionPost(id: number): void {
    Swal.fire({
      title: 'Da li ste sigurni?',
      text: 'Ova akcija se ne može poništiti.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Da, obriši!'
    }).then((result) => {
      if (result.isConfirmed) {
        this.AdoptionPostService.deleteAdoptionPost(id).subscribe({
          next: (response) => {
            Swal.fire(
              'Obrisano!',
              'Post je uspješno obrisan.',
              'success'
            );
            this.getAdoptionPosts();
          },
          error: (error) => {
            console.error('Error deleting adoption post', error);
            Swal.fire(
              'Greška!',
              'Došlo je do greške prilikom brisanja.',
              'error'
            );
          }
        });
      }
    });
  }


  resetForm() {

    this.newAnimal = {
      name: '',
      gender: '',
      size: '',
      age: '',
      color: '',
      weight: 0.5,
      animalType: '',
      vaccinated: false,
      sterilized: false,
      parasiteFree: false,
      hasPassport: false,
      images: []
    };
    this.newAdoptionPost = {
      dateOfCreation: this.currentDate,
      viewCounter: 1,
      urgent: false,
      shortDescription: '',
      username:'',
      cityId: null
    };
    this.selectedCityId = null;
    this.isFormVisible = false;
  }
}



