import {Component, ElementRef, OnInit, ViewChild} from '@angular/core';
import {NgClass, NgForOf, NgIf} from "@angular/common";
import {RouterLink} from "@angular/router";
import {UserEndpointsService, UserReadResponse} from "../../endpoints/UserEndpointsService";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {ShelterEndpointsService, ShelterReadResponse} from "../../endpoints/ShelterEndpointsService";
import {DonationEndpointsService} from "../../endpoints/DonationEndpointsService";
import Swal from "sweetalert2";
import {loadStripe, Stripe, StripeCardElement, StripeElements} from "@stripe/stripe-js";
import {HttpClient} from "@angular/common/http";
import {MyAuthService} from "../../services/auth-services/my-auth.service";

@Component({
  selector: 'app-donation',
  standalone: true,
  imports: [
    NgIf,
    RouterLink,
    NgForOf,
    ReactiveFormsModule,
    FormsModule,
    NgClass
  ],
  templateUrl: './donation.component.html',
  styleUrl: './donation.component.css'
})
export class DonationComponent implements OnInit {
  backgroundUrl: string = 'assets/puppies.jpg';
  isLoggedIn: boolean = true;
  user: UserReadResponse | null = null;
  shelters: ShelterReadResponse[] = [];
  customAmount: string | null = null;
  isCustomInput: boolean = false;
  errorMessage: string | null = null;
  isAmountValid: boolean = false;
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  cardElement: StripeCardElement | null = null;
  clientSecret: string | null = null;
  isProcessing: boolean = false;
  isStripeInitialized = false;
  isModalOpen: boolean = false;
  isBackgroundVisible = false;
  isShelter:boolean=false;
  isUser:boolean=false;

  @ViewChild('cardElementContainer', { static: false}) cardElementContainer!: ElementRef;
  private modalOverlayVisible: boolean = false;

  constructor(private shelterService: ShelterEndpointsService,
              private userService: UserEndpointsService,
              private donationService: DonationEndpointsService,
              private http: HttpClient,
              private authService:MyAuthService){}

  async ngOnInit() {
    const role=this.authService.getCurrentUser()?.Role;
    if(role=='Shelter')
    {
      this.isShelter=true;
    }
    else {
      this.isUser=true;
    }
    try {
      await this.initializeStripe();
      await this.loadData();
      setTimeout(() => {
        this.isBackgroundVisible = true; // Aktivira fade-in efekat
      }, 100);
    } catch (error) {
      console.error("Greška prilikom inicijalizacije:", error);
    }
  }

  private async initializeStripe() {
    if (this.isStripeInitialized) return;
    this.stripe = await loadStripe('pk_test_51Qlrr9LwYIVWR9gzebcatydQvWJyQZdf57IlYBG2AVzvv6PdM3l6sJ2bHpvaIx5bTsgtjqfcFC43UtrBJjE47GNg00Kwm1gL02');

    if (this.stripe) {
      this.elements = this.stripe.elements();
      this.cardElement = this.elements.create('card', {
        style: {
          base: {
            fontSize: '16px',
            color: '#32325d',
            '::placeholder': {
              color: '#aab7c4'
            }
          },
          invalid: {
            color: '#fa755a',
            iconColor: '#fa755a'
          }
        }
      });

      if (this.cardElementContainer?.nativeElement) {
        this.cardElement.mount(this.cardElementContainer.nativeElement);
      } else {
        console.error('cardElementContainer nije pronađen u DOM-u.');
      }

      this.isStripeInitialized = true;
    }
  }

  private async loadData() {
    Promise.all([
      this.loadShelters(),
      this.loadUserData()
    ]).catch(error => console.error("Greška pri učitavanju podataka:", error));
  }


  private loadShelters() {
    return new Promise<void>((resolve, reject) => {
      this.shelterService.getShelters().subscribe({
        next: (data) => {
          this.shelters = data;
          resolve();
        },
        error: (error) => {
          console.error("Error fetching shelters", error);
          reject(error);
        }
      });
    });
  }

  private loadUserData() {
    return new Promise<void>((resolve, reject) => {
      this.userService.loadUserProfile().subscribe({
        next: (data) => {
          this.user = data;
          resolve();
        },
        error: (error) => {
          console.error("Error fetching user data", error);
          reject(error);
        }
      });
    });
  }

  getCardElement() {
    return this.cardElement;
  }

  confirmDonation() {
    Swal.fire({
      title: 'Da li ste sigurni?',
      text: `Želite li da donirate ${this.customAmount} KM?`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Da, doniraj!',
      cancelButtonText: 'Ne, odustani',
      confirmButtonColor: '#43af17',
      cancelButtonColor: '#d63049',
    }).then((result) => {
      if (result.isConfirmed) {
        this.createPaymentIntent(parseFloat(this.customAmount!));
      }
    });
  }

  createPaymentIntent(amount: number) {
    if(!amount) return;
    this.isProcessing = true;

    this.http.post<any>('https://localhost:7291/stripe/create-payment-intent', { amount })
      .subscribe({
        next: async (response) => {
          this.clientSecret = response.clientSecret;

          const result = await this.stripe!.confirmCardPayment(this.clientSecret!, {
            payment_method: {
              card: this.getCardElement()!,
              billing_details: {
                name: 'Test User',
              },
            },
          });

          if (result.error) {
            console.error(result.error.message);
            Swal.fire({
              icon: 'error',
              title: 'Greška pri donaciji',
              text: result.error.message,
              confirmButtonColor: '#007bff'
            });
          } else if (result.paymentIntent?.status === 'succeeded') {
            this.sendDonation();

            Swal.fire({
              icon: 'success',
              title: 'Donacija uspješna',
              text: 'Vaša donacija je primljena. Hvala vam!',
              confirmButtonColor: '#3085d6',
            }).then(() => {
              this.closeModal();
            });
          }

          this.isProcessing = false;
        },
        error: (error) => {
          console.error('Error creating payment intent:', error);
          this.isProcessing = false;
        }
      });
  }
/*
  loadShelters() {
    this.shelterService.getShelters().subscribe(
      (data) => {
        console.log("Fetched shelters:", data);
        this.shelters = data;
      },
      (error) => {
        console.error("Error fetching shelters", error);
      }
    );

    console.log(this.shelters);
  }*/

  validateAmount() {
    const amountRegex = /^(?!0(\.0+)?$)([1-9]\d*(\.\d{1,2})?)$/; // Broj mora biti veći ili jednak 1, sa maksimalno 2 decimale
    if (!this.customAmount) {
      this.isAmountValid = false;
      this.errorMessage = 'Molimo unesite validan iznos.';
    } else if (!amountRegex.test(this.customAmount)) {
      this.isAmountValid = false;
      this.errorMessage =
        'Unos mora biti broj veći ili jednak 1 u formatu 80 ili 80.00 (do dvije decimale).';
    } else {
      const amount = parseFloat(this.customAmount);
      if (amount <= 0) {
        this.isAmountValid = false;
        this.errorMessage = 'Iznos mora biti veći od 0.';
      } else {
        this.isAmountValid = true;
        this.errorMessage = null;
      }
    }
  }

  setAmount(amount: number) {
    this.customAmount = amount.toString(); // Postavlja odabrani iznos
    this.isCustomInput = false;
    this.validateAmount(); // Poziva validaciju za ažurirani iznos
  }


  sendDonation() {
    if (!this.isAmountValid) {
      Swal.fire({
        icon: 'error',
        title: 'Nevalidan unos',
        text: 'Molimo provjerite uneseni iznos.',
        showCloseButton: true, // Dodano dugme za zatvaranje
        confirmButtonText: 'OK', // Tekst na glavnom dugmetu
        confirmButtonColor: '#007bff'
      });
      return;
    }

    const selectedShelterId = (document.getElementById('shelter') as HTMLSelectElement)?.value;

    if (!selectedShelterId) {
      Swal.fire({
        icon: 'error',
        title: 'Niste odabrali azil',
        text: 'Molimo odaberite azil prije nego što nastavite.',
        showCloseButton: true, // Dodano dugme za zatvaranje
        confirmButtonText: 'OK', // Tekst na glavnom dugmetu
        confirmButtonColor: '#007bff'
      });
      return;
    }

    const donationRequest = {
      userId: this.user!.id,
      shelterId: parseInt(selectedShelterId, 10),
      amount: parseFloat(this.customAmount!)
    };

    this.donationService.createDonation(donationRequest).subscribe({
      next: (response) => {
        /*Swal.fire({
          icon: 'success',
          title: 'Donacija uspješna!',
          text: 'Hvala vam na vašoj podršci!',
          showCloseButton: true, // Dodano dugme za zatvaranje
          confirmButtonText: 'Zatvori', // Tekst na glavnom dugmetu
          confirmButtonColor: '#007bff'
        });*/
        console.log('Donation saved in database');
      },
      error: (error) => {
        /*Swal.fire({
          icon: 'error',
          title: 'Greška!',
          text: 'Došlo je do greške. Molimo pokušajte ponovo.',
          showCloseButton: true, // Dodano dugme za zatvaranje
          confirmButtonText: 'Pokušaj ponovo', // Tekst na glavnom dugmetu
          confirmButtonColor: '#007bff'
        });*/
        console.log('Donation not saved in database');
      }
    });
  }

  openModal(amount: number) {
    this.isModalOpen = true;
    this.modalOverlayVisible = true;  // Aktiviraj overlay
    this.customAmount = amount.toString();
    document.body.classList.add("modal-open");
  }

  closeModal() {
    this.isModalOpen = false;
    this.modalOverlayVisible = false;  // Ukloni overlay
    document.body.classList.remove("modal-open");
  }


  protected readonly parseFloat = parseFloat;
}
