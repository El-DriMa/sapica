import { Routes } from '@angular/router';
import { HomepageComponent } from './modules/homepage/homepage.component';
import {ErrorComponent} from "./modules/error/error/error.component";
import {UnauthorizedComponent} from "./shared/unauthorized/unauthorized/unauthorized.component";
import{AboutUsComponent}from './modules/about-us/about-us.component'
import {authGuard} from "./auth-guards/auth.guard";
import {MyProfileComponent} from "./modules/users/my-profile/my-profile.component";
import {RequestsComponent} from "./modules/requests/requests.component";
import {RequestDetailsComponent} from "./modules/requests/request-details/request-details.component";
import {DonationComponent} from "./modules/donation/donation.component";
import {DashboardComponent} from "./modules/admin/dashboard/dashboard.component";
import {ShelterProfileComponent} from "./modules/shelter-profile/shelter-profile.component";
import { AdoptionPostAdminComponent } from './modules/adoption-post/adoption-post-admin/adoption-post-admin.component';

export const routes: Routes = [
  { path: 'users', loadComponent:()=>import('./modules/users/users.component').then(m=>m.UsersComponent), canActivate: [authGuard], data: { roles: ['Admin'] } },
  { path: 'users/add', loadComponent:()=>import('./modules/users/add-user/add-user.component').then(m=>m.AddUserComponent) },
  { path: 'shelters', loadComponent:()=>import('./modules/shelters/shelters.component').then(m=>m.SheltersComponent) },
  { path: 'shelters/add',loadComponent:()=>import('./modules/shelters/add-shelter/add-shelter.component').then(m=>m.AddShelterComponent)},
  { path:'shelters/edit/:id',loadComponent:()=>import('./modules/shelters/edit-shelter/edit-shelter.component').then(m=>m.EditShelterComponent)},
  { path: 'adoptionPosts', loadComponent:()=>import('./modules/adoption-post/adoption-post.component').then(m=>m.AdoptionPostComponent)},
  {path:'adoptionPosts/admin',loadComponent:()=>import('./modules/adoption-post/adoption-post-admin/adoption-post-admin.component').then(m=>m.AdoptionPostAdminComponent)},
  { path: 'users/edit/:id', loadComponent:()=>import('./modules/users/edit-user/edit-user.component').then(m=>m.EditUserComponent), canActivate: [authGuard], data: { roles: ['User']}},
  { path: 'register' , loadComponent:()=>import('./modules/register/register.component').then(m=>m.RegisterComponent)},
  { path: 'login', loadComponent:()=>import('./modules/auth/login/login.component').then(m=>m.LoginComponent) },
  { path: 'logout', loadComponent:()=>import('./modules/auth/logout/logout.component').then(m=>m.LogoutComponent) },
  { path:'favorites',loadComponent:()=>import('./modules/favorites/favorites.component').then(m=>m.FavoritesComponent)},
  { path: 'adoptionRequest/add',loadComponent:()=>import('./modules/adoption-request/add-adoption-request/add-adoption-request.component').then(m=>m.AddAdoptionRequestComponent)},
  { path: 'users/me', component: MyProfileComponent, canActivate: [authGuard], data: { roles: ['User'] } },
  {path:'shelter/profile',component:ShelterProfileComponent, canActivate: [authGuard], data: { roles: ['Shelter'] } },
  { path: 'error', component: ErrorComponent },
  { path: 'aboutUs',loadComponent:()=>import('./modules/about-us/about-us.component').then(m=>m.AboutUsComponent) },
  { path: 'unauthorized', component: UnauthorizedComponent },
  {path: 'requests',component: RequestsComponent},
  {path:'request-details/:id',component: RequestDetailsComponent},
  {path:'donations', component: DonationComponent, canActivate: [authGuard], data: { roles: ['User'] }},
  {path: 'dashboard', component: DashboardComponent, canActivate: [authGuard], data: { roles: ['Admin']}},
  { path: '', component: HomepageComponent },
  { path: '**', redirectTo: '', pathMatch: 'full' }
];
