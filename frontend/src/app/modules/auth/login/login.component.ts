import { Component } from '@angular/core';
import {FormsModule} from "@angular/forms";
import {MyAuthService, LoginRequest} from "../../../services/auth-services/my-auth.service";
import {Router, RouterLink} from "@angular/router";
import {NgIf} from "@angular/common";
import Swal from "sweetalert2";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    FormsModule,
    NgIf,
    RouterLink
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})

export class LoginComponent {
  usernameOrEmail: string | null = null;
  password: string | null = null;
  rememberMe: boolean = false;
  backgroundUrl: string = 'assets/background.jpg';
  loginRequest: LoginRequest = {usernameOrEmail: 'aminag03', password: 'Sifra123', rememberMe: false};
  errorMessage: string | null = null;

  constructor(private myAuthService: MyAuthService, private router: Router) {}

  isFormValid(): boolean {
    return this.usernameOrEmail?.trim() !== '' && this.password?.trim() !== '';
  }

  onSubmit(): void {
    if(!this.isFormValid()) { return; }

    this.loginRequest = {
      usernameOrEmail: this.usernameOrEmail!,
      password: this.password!,
      rememberMe: this.rememberMe
    };

    this.myAuthService.handleAsync(this.loginRequest).subscribe({
      next: (lr) => {
        this.myAuthService.setLoggedIn(lr, this.rememberMe);

        const userRole = this.myAuthService.getUserRole();

        Swal.fire({
          title: 'Prijava uspješna!',
          text: 'Uspješno ste se prijavili.',
          icon: 'success',
          confirmButtonText: 'OK',
          background: '#f9f9f9',
          confirmButtonColor: '#007bff',
          customClass: {
            popup: 'swal-text',
          },
        }).then(() => {
          if(userRole === 'Admin') {
            this.router.navigate(['/dashboard']);
          } else {
            this.router.navigate(['/']);
          }
        });
      },
      error: (error: any) => {
        if (error?.error?.message === 'User profile not activated' || error?.error?.message === 'Shelter profile not activated') {
          this.errorMessage = 'Vaš profil nije aktiviran. Molimo vas da aktivirate svoj profil prije prijave.';
        } else {
          this.errorMessage = 'Pogrešno korisničko ime ili lozinka.';
        }
        console.error('Login error: ', error);
      }
    });
  }
}
