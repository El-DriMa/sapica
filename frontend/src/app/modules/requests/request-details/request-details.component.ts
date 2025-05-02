import { Component } from '@angular/core';
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {AdoptionRequestEndpointsService} from "../../../endpoints/AdoptionRequestEndpointsService";
import { Location } from '@angular/common';

@Component({
  selector: 'app-request-details',
  standalone: true,
  imports: [
    RouterLink
  ],
  templateUrl: './request-details.component.html',
  styleUrl: './request-details.component.css'
})
export class RequestDetailsComponent {
  adoptionRequest: any;

  showPopup: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private adoptionRequestService: AdoptionRequestEndpointsService,
    private location: Location,
    private router:Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.getAdoptionRequest(id);
    }
  }

  getAdoptionRequest(id: string): void {
    this.adoptionRequestService.getAdoptionRequestById(Number(id)).subscribe(
      (data) => {
        this.adoptionRequest = data;

      },
      (error) => {
        console.error('Greška pri dohvaćanju zahtjeva:', error);
      }
    );
  }
  goBack(): void {
    if (window.history.length > 1) {
      this.location.back();
    } else {
      this.router.navigate(['/requests']);
    }
  }

  acceptAdoptionRequest(): void {
    console.log('Prihvaćen zahtjev za udomljavanje');
    // Pozovite metodu za prihvaćanje zahtjeva
  }

  rejectAdoptionRequest(): void {
    console.log('Odbijen zahtjev za udomljavanje');
    // Pozovite metodu za odbijanje zahtjeva
  }
}
