import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
  standalone: true,
  styleUrls: ['./error.component.css']
})
export class ErrorComponent implements OnInit {
  errorCode: string = '';
  errorMessage: string = '';

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.errorCode = params['code'] || 'Greška';
      this.errorMessage = params['message'] || 'Dogodila se neočekivana greška.';
    });
  }

  navigateTo(path: string): void {
    this.router.navigate([path]);
  }
}
