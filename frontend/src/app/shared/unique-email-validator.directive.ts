import {Directive, Input} from '@angular/core';
import { AbstractControl, AsyncValidator, NG_ASYNC_VALIDATORS, ValidationErrors } from '@angular/forms';
import { catchError, debounceTime, map, Observable, of } from 'rxjs';
import { UserEndpointsService } from '../endpoints/UserEndpointsService';

@Directive({
  selector: '[appUniqueEmailValidator]',
  standalone: true,
  providers: [{ provide: NG_ASYNC_VALIDATORS, useExisting: UniqueEmailValidatorDirective, multi: true }]
})
export class UniqueEmailValidatorDirective implements AsyncValidator {
  @Input() currentEmail!: string | undefined;

  constructor(private userService: UserEndpointsService) { }

  validate(control: AbstractControl): Observable<ValidationErrors | null> {
    if (!control.value) {
      return of(null);
    }

    const enteredEmail = control.value;

    if (!enteredEmail) {
      return of(null);
    }

    if (this.currentEmail && enteredEmail === this.currentEmail) {
      return of(null);
    }

    return this.userService.checkEmailAvailability({ email: control.value }).pipe(
      debounceTime(300),
      map(response => response.emailAvailable ? null : { emailTaken: true }),
      catchError(() => of({ emailTaken: true }))
    );
  }
}
