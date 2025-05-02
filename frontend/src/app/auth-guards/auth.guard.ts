import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MyAuthService } from "../services/auth-services/my-auth.service";

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(MyAuthService);
  const router = inject(Router);

  const userAccount = authService.getCurrentUser();

  if (userAccount) {
    const allowedRoles = route.data?.['roles'] as string[]; // Retrieve roles from route data
    if (allowedRoles && allowedRoles.includes(userAccount.Role)) {
      return true;
    }
  }

  router.navigate(['/unauthorized']);
  return false;
};
