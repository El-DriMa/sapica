
import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import { AdoptionRequestEndpointsService, AdoptionRequestCreateRequest } from '../../../endpoints/AdoptionRequestEndpointsService';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {NgForOf, NgIf} from "@angular/common";
import {Router, RouterLink} from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { ChangeDetectorRef } from '@angular/core';
import {AdoptionPostReadResponse,AdoptionPostService} from "../../../endpoints/AdoptionPostEndpointsService";
import { jsPDF } from 'jspdf';



@Component({
  selector: 'app-add-adoption-request',
  templateUrl: './add-adoption-request.component.html',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    NgForOf,
    NgIf,
    RouterLink,
  ],
  styleUrls: ['./add-adoption-request.component.css'],
  standalone: true
})
export class AddAdoptionRequestComponent implements OnInit {
  showPopup = false;
  currentStep = 1;
  imageUrl: string = '';
  totalSteps: number = 3;
  animalName='';
  date  = new Date();
  adoptionPosts: AdoptionPostReadResponse[] = [];
  selectedPostId: number | null = null;
  selectedPost: AdoptionPostReadResponse | null = null;
  newAdoptionRequest: AdoptionRequestCreateRequest = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    reason: '',
    livingSpace:'',
    backyard: '',
    // backyardSize:'',
    familyMembers: 0,
    anyKids : '',
    // numberOfKids: 0,
    anyAnimalsBefore: '',
    //animalsBefore:'',
    //experience:'',
    timeCommitment: 0,
    preferredCharacteristic:'',
    age: 0,
    cityId: 0,
    adoptionPostId: 4,
  };


  errors: { [key: string]: string } = {};
  cities: { id: number, name: string, countryId: number, latitude: number, longitude: number }[] = [];
  isSubmitting = false;






  constructor(
    private fb: FormBuilder,
    private AdoptionRequestService: AdoptionRequestEndpointsService,
    private adoptionService: AdoptionRequestEndpointsService,
    private router: Router,
    private route: ActivatedRoute,
     private AdoptionPostService: AdoptionPostService
  ) {

  }
  ngOnInit(): void {
      this.loadCities();
      this.route.queryParams.subscribe((params) => {
        const postId = params['postId'];
        if (postId) {
          this.newAdoptionRequest.adoptionPostId = Number(postId);
          this.AdoptionPostService.getAdoptionPostById(postId).subscribe((response) => {
            this.selectedPost = response;
            console.log("Selected post:", this.selectedPost);
            this.imageUrl=this.selectedPost.animal.images[0].image;
            this.animalName=this.selectedPost.animal.name;
            console.log("image url",this.imageUrl );
        });}
      });
    }


  loadCities() {
    this.AdoptionRequestService.getCities().subscribe(
      (data) => {
        this.cities = data;
        console.log("Fetched cities:", this.cities);

      },
      (error) => {
        console.error("Error fetching cities", error);
      }
    );}


  validateInput(field: keyof AdoptionRequestCreateRequest) {
    const value = this.newAdoptionRequest[field];
    let message = '';

    if (field === 'firstName' && !(value as string)) {
      message = 'Ime je obavezno.';
    } else if (field === 'firstName' && (value as string).length > 50) {
      message = 'Ime mora imati manje od 50 karaktera.';
    }



    if (field === 'lastName' && !(value as string)) {
      message = 'Prezime je obavezno.';
    } else if (field === 'lastName' && (value as string).length > 50) {
      message = 'Prezime mora imati manje od 50 karaktera.';
    }

    if (field === 'age' && !(value as number)) {
      message = 'Godine su obavezne.';}
    else if (field === 'age' && ((value as number) < 18 || (value as number) > 100)) {
      message = 'Morate biti punoljetni i mlađi od 100';
    }

    if (field === 'familyMembers' && !(value as number)) {
      message = 'Broj članova  je obavezan.';}
    else if (field === 'familyMembers' && ((value as number) < 1 || (value as number) > 30)) {
      message = 'Mora biti manje od 30 članova.';
    }

    if (field === 'timeCommitment' && !(value as number)) {
      message = 'Broj sati  je obavezan.';}
    else if (field === 'timeCommitment' && ((value as number) <1 || (value as number) > 24)) {
      message = 'Mora biti manje od 24 sata.';
    }

    if(field=='cityId' &&((value as number)<=0))
    message='Grad je obavezan';


    if (field === 'backyard' && !(value as string)) {
      message = 'Obavezno je odabrati.';}


    if (field === 'anyKids' && !(value as string)) {
      message = 'Obavezno je odabrati..';}


    if (field === 'anyAnimalsBefore' && !(value as string)) {
      message = 'Obavezno je odabrati..';}


    if (field === 'preferredCharacteristic' && !(value as string)) {
      message = 'Osobine je obavezno unijeti.';
    } else if (field === 'preferredCharacteristic' && (value as string).length > 255) {
      message = 'Osobine moraju imati manje od 255 karaktera.';
    }

    if (field === 'reason' && !(value as string)) {
      message = 'Razlog je obavezan.';
    } else if (field === 'reason' && (value as string).length > 255) {
      message = 'Razlog mora imati manje od 255 karaktera.';
    }



    if (field === 'livingSpace' && !(value as string)) {
      message = 'Mjesto zivljenja je obavezno.';
    } else if (field === 'livingSpace' && (value as string).length > 20) {
      message = 'Mjesto zivljenja mora imati manje od 20 karaktera.';
    }


    if (field === 'email' && !(value as string)) {
      message = 'Email je obavezan.';
    } else if (field === 'email' && !/^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(value as string)) {
      message = 'Format nije valjan.';
    }

    if (field === 'phoneNumber' && !(value as string)) {
      message = 'Broj telefona je obavezan.';
    } else if (field === 'phoneNumber' && (!/^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$/.test(value as string))) {
      message = 'Broj telefona mora biti validan za Bosnu i Hercegovinu.';
    }

    if (message) {
      this.errors[field] = message;
    } else {
      delete this.errors[field];
    }


  }



  isValidForm() {
    return Object.values(this.errors).every((error) => !error) &&
      Object.values(this.newAdoptionRequest).every((value) => value);

  }
  requiredFields: { [key: string]: boolean } = {
    firstName: true,
    lastName: true,
    age: true,
    cityId: true,
    phoneNumber: true,
    email: true,
    livingSpace: true,
    backyard: true,
    familyMembers: true,
    anyKids: true,
    anyAnimalsBefore: true,
    timeCommitment: true,
    preferredCharacteristic: true,
    reason: true,
  };

  calculateProgress(): number {

    const totalFields = Object.keys(this.requiredFields).length;


    const validFields = (Object.keys(this.requiredFields) as Array<keyof typeof this.newAdoptionRequest>).filter((field) => {
      const value = this.newAdoptionRequest[field];

      switch (field) {
        case 'age':
          return typeof value === 'number' && value >= 18 && value <= 100;
        case 'timeCommitment':
          return typeof value === 'number' && value >= 1 && value <= 24;
        case 'familyMembers':
          return typeof value === 'number' && value >= 1 && value <= 30;
        case 'cityId':
          return  value as number >= 1 && value as number <= 100;
        case 'phoneNumber':
          return typeof value==='string' && /^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$/.test(value);
          case 'email':
            return typeof value==='string' && /^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(value);
        default:
          return value !== null && value !== '';
      }
    }).length;


    return Math.round((validFields / (totalFields)) * 100);
  }

  isStepValid(): boolean {
    switch (this.currentStep) {
      case 1:
        return (
          this.newAdoptionRequest.firstName.trim().length >= 1 && this.newAdoptionRequest.firstName.trim().length<=50&&
          this.newAdoptionRequest.lastName.trim().length >= 1 && this.newAdoptionRequest.lastName.trim().length<=50&&
        typeof this.newAdoptionRequest.age === 'number' && this.newAdoptionRequest.age >= 18 && this.newAdoptionRequest.age <= 100 &&
          this.newAdoptionRequest.cityId !== null &&
          /^(\+387|0)[6][0-7][0-9][0-9][0-9][0-9][0-9][0-9]$/.test(this.newAdoptionRequest.phoneNumber.trim())&&
          /^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$/.test(this.newAdoptionRequest.email.trim())
        );

      case 2:
        return (
          ['Kuca', 'Stan', 'Kuca sa dvorištem'].includes(this.newAdoptionRequest.livingSpace.trim()) &&
           this.newAdoptionRequest.backyard.trim().length>0 &&
          typeof this.newAdoptionRequest.familyMembers === 'number' && this.newAdoptionRequest.familyMembers > 0 && this.newAdoptionRequest.familyMembers <= 30 &&
           this.newAdoptionRequest.anyKids.trim().length>0
        );

      case 3:
        return (
          this.newAdoptionRequest.anyAnimalsBefore.trim().length>0 &&
          typeof this.newAdoptionRequest.timeCommitment === 'number' && this.newAdoptionRequest.timeCommitment > 0 && this.newAdoptionRequest.timeCommitment <= 24 &&
          this.newAdoptionRequest.preferredCharacteristic.trim().length > 0 && this.newAdoptionRequest.preferredCharacteristic.trim().length>50&&
          this.newAdoptionRequest.reason.trim().length > 0 && this.newAdoptionRequest.reason.trim().length>100
        );

      default:
        return false;
    }
  }


  closePopup() {
    this.showPopup = false;
    this.router.navigate(['/']);
  }
  nextStep() {
    if (this.currentStep < 3) {
      this.currentStep++;

    }
    console.log(this.newAdoptionRequest.cityId);
    console.log(this.getSelectedCityName())
  }


  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }
  getSelectedCityName(): string {
    for (let i = 0; i < this.cities.length; i++) {

      if (Number(this.cities[i].id) === Number(this.newAdoptionRequest.cityId)) {
        return this.cities[i].name;
      }
    }
    return 'City not found';
  }



  generatePDF() {
    const doc = new jsPDF();

    doc.setFillColor(245, 245, 220);
    doc.rect(0, 0, 210, 297, 'F');


    doc.setFont("times");
    doc.setFontSize(16);
    doc.text("Udomljavanje ljubimca - Izvještaj", 20, 20);

    // Adding form details
    doc.setFontSize(12);
    doc.text(`Ime i prezime: ${this.newAdoptionRequest.firstName} ${this.newAdoptionRequest.lastName}`, 20, 30);
    doc.text(`Godine: ${this.newAdoptionRequest.age}`, 20, 40);
    doc.text(`Telefon: ${this.newAdoptionRequest.phoneNumber}`, 20, 50);
    doc.text(`Email: ${this.newAdoptionRequest.email}`, 20, 60);
    doc.text(`Grad: ${this.getSelectedCityName()}`, 20, 70);
    doc.text(`Mjesto življenja: ${this.newAdoptionRequest.livingSpace}`, 20, 80);
    doc.text(`Da li ima dvorište: ${this.newAdoptionRequest.backyard}`, 20, 90);
    doc.text(`Broj ukucana: ${this.newAdoptionRequest.familyMembers}`, 20, 100);
    doc.text(`Ima li djecu: ${this.newAdoptionRequest.anyKids}`, 20, 110);
    doc.text(`Da li je prije imao ljubimce: ${this.newAdoptionRequest.anyAnimalsBefore}`, 20, 120);
    doc.text(`Koliko sati dnevno može posvetiti brizi o ljubimcu: ${this.newAdoptionRequest.timeCommitment}`, 20, 130);
    doc.text(`Osobine koje traži: ${this.newAdoptionRequest.preferredCharacteristic}`, 20, 140);
    doc.text(`Razlog za udomljavanje: ${this.newAdoptionRequest.reason}`, 20, 150);
    doc.addImage(this.imageUrl, 'JPEG', 20, 160, 100, 100);

    const date = new Date();
    const formattedDate = date.toLocaleString('bs-BA', { dateStyle: 'short', timeStyle: 'short' });
    doc.setFontSize(10);
    doc.text(`Datum: ${formattedDate}`, 150, doc.internal.pageSize.height - 10);
    doc.save("adoption_request_report.pdf");
  }

  onAddAdoptionRequest() {

    console.log("Payload:", this.newAdoptionRequest);
    this.isSubmitting = true;

    this.adoptionService.createAdoptionRequest(this.newAdoptionRequest).subscribe(
      (response) => {


        this.showPopup = true;
        setTimeout(() => {
          this.closePopup();
          this.router.navigate(['/']);
        }, 3000);

      },
      (error) => {


        if (error.status === 400) {
          if (typeof error.error === "string") {
            alert(error.error);
          } else {
            console.error('Unheandled backend error: ', error.error);
          }
        } else {
          console.error('Error adding adoption Request:', error);
        }
        this.isSubmitting = false;
      }
    );
  }
}
