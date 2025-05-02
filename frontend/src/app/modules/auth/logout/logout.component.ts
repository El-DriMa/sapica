import {Component, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Router} from '@angular/router';
import {MyConfig} from '../../../my-config';
import {MyAuthService} from '../../../services/auth-services/my-auth.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css'],
  standalone: true
})
export class LogoutComponent implements OnInit {
  private apiUrl = `${MyConfig.api_address}/logout`;
  backgroundUrl: string = 'assets/background.jpg';

  constructor(
    private httpClient: HttpClient,
    private authService: MyAuthService,
    private router: Router
  ) {
  }

  ngOnInit(): void {
    this.logout();
  }

  logout(): void {
    this.httpClient.post<void>(this.apiUrl, {}).subscribe({
      next: () => this.handleLogoutSuccessOrError(),
      error: (error) => {
        console.error('Error during logout:', error);
        this.handleLogoutSuccessOrError();
      }
    });
  }

  private handleLogoutSuccessOrError(): void {
    this.authService.setLoggedIn(null, false);
    this.authService.setLoggedIn(null, true);
    setTimeout(() => {
      this.router.navigate(['/login']);
    }, 3000);
  }
}
