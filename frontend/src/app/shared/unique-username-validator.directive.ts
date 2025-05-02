import {Directive, Input} from '@angular/core';
import { AbstractControl, AsyncValidator, NG_ASYNC_VALIDATORS, ValidationErrors } from '@angular/forms';
import { catchError, debounceTime, map, Observable, of } from 'rxjs';
import { UserEndpointsService } from '../endpoints/UserEndpointsService';

@Directive({
  selector: '[appUniqueUsernameValidator]',
  standalone: true,
  providers: [{ provide: NG_ASYNC_VALIDATORS, useExisting: UniqueUsernameValidatorDirective, multi: true }]
})
export class UniqueUsernameValidatorDirective implements AsyncValidator {
  @Input() currentUsername!: string | undefined;

  constructor(private userService: UserEndpointsService) { }

  validate(control: AbstractControl): Observable<ValidationErrors | null> {
    if (!control.value) {
      return of(null);
    }

    const enteredUsername = control.value;

    if (!enteredUsername) {
      return of(null);
    }

    if (this.currentUsername && enteredUsername === this.currentUsername) {
      return of(null);
    }

    return this.userService.checkUsernameAvailability({ username: control.value }).pipe(
      debounceTime(300),
      map(response => response.usernameAvailable ? null : { usernameTaken: true }),
      catchError(() => of({ usernameTaken: true }))
    );
  }
}
