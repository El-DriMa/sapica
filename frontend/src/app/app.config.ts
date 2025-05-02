import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter, RouterModule } from '@angular/router';
import {HttpClientModule, HTTP_INTERCEPTORS, provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';
import { provideClientHydration} from '@angular/platform-browser';
import { MyAuthInterceptorService } from './services/auth-services/my-auth-interceptor.service';
import {ServerErrorInterceptor} from "./interceptors/service-error.interceptor";
import { IMAGE_CONFIG } from '@angular/common';


export const appConfig: ApplicationConfig = {
  providers: [
    importProvidersFrom(RouterModule.forRoot(routes),HttpClientModule),
    provideRouter(routes),
    provideClientHydration(),
   // provideHttpClient(withFetch()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MyAuthInterceptorService,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ServerErrorInterceptor,
      multi: true,
    },
    {
      provide: IMAGE_CONFIG,
      useValue: {
        disableImageSizeWarning: true,
        disableImageLazyLoadWarning: true
      }
    },
  ]
};
