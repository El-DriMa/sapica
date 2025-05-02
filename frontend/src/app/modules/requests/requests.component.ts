import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {
  AdoptionRequestByUsernameReadResponse,
  AdoptionRequestEndpointsService
} from "../../endpoints/AdoptionRequestEndpointsService";
import * as http from "node:http";
import {DatePipe, NgForOf, NgIf} from "@angular/common";
import {UserEndpointsService, UserReadResponse} from "../../endpoints/UserEndpointsService";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import Swal from "sweetalert2";
import {MyAuthService} from "../../services/auth-services/my-auth.service";

@Component({
  selector: 'app-requests',
  standalone: true,
  imports: [
    DatePipe,
    NgForOf,
    NgIf,
    RouterLink
  ],
  templateUrl: './requests.component.html',
  styleUrl: './requests.component.css'
})

    export class RequestsComponent implements OnInit {
       user: UserReadResponse | null = null;
      username = '';
      requests: AdoptionRequestByUsernameReadResponse[] = [];
      isShelter:boolean=false;
      isUser:boolean=false;

      @ViewChild('tableBody', { static: true }) tableBody!: ElementRef<HTMLTableSectionElement>;

      constructor(
        private http: HttpClient,
        private adoptionRequestService: AdoptionRequestEndpointsService,
        private userService:UserEndpointsService,
        private router:Router,
        private route: ActivatedRoute,
        private authService:MyAuthService
      ) {}

      ngOnInit(): void {
        const role=this.authService.getCurrentUser()?.Role;
        if(role=='Shelter')
        {
          this.isShelter=true;
        }
        else {
          this.isUser=true;
        }
        this.userService.loadUserProfile().subscribe({
          next: (data: UserReadResponse) => {
            this.user = data;
            this.username=data.username;
            console.log(this.username);
            this.fetchAndRenderAdoptionRequests(data.username);
          },
          error: (error) => {
            console.error('Greška pri preuzimanju korisničkih podataka:', error);
          }
        });


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
        this.router.navigate(['/requests']);

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
        this.router.navigate(['/requests']);

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
}
