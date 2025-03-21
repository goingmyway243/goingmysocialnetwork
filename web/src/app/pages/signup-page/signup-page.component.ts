import { Component, signal, ViewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { IdentityApiService } from '../../common/services/identity-api.service';
import { catchError, throwError } from 'rxjs';

@Component({
  selector: 'app-signup-page',
  standalone: true,
  imports: [
    MatStepperModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatDatepickerModule,
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './signup-page.component.html',
  styleUrl: './signup-page.component.scss'
})
export class SignupPageComponent {
  @ViewChild('stepper') stepper!: MatStepper;

  public firstForm = new FormGroup({
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required]),
    confirmPassword: new FormControl('', [
      Validators.required,
      (control) => {
        if (!control.parent) {
          return null;
        }
        const password = control.parent.get('password');
        return password && control.value !== password.value ? { passwordMismatch: true } : null;
      }
    ])
  });

  public secondForm = new FormGroup({
    name: new FormControl('', [Validators.required]),
    dateOfBirth: new FormControl('', [Validators.required])
  });

  public isLoading = signal(false);
  public error = signal<string | null>(null);

  constructor(private router: Router, private identityApiSvc: IdentityApiService) { }

  public showBackButton(): boolean {
    return !this.stepper || this.stepper.selectedIndex !== this.stepper.steps.length - 1;
  }

  public navigateToLogin() {
    this.router.navigate(['/login']);
  }

  onSubmit(stepper: MatStepper) {
    this.isLoading.set(true);
    this.error.set(null);

    const newUser = {
      email: this.firstForm.controls.email.value ?? '',
      password: this.firstForm.controls.password.value ?? '',
      fullName: this.secondForm.controls.name.value ?? '',
      dateOfBirth: new Date(this.secondForm.controls.dateOfBirth.value ?? '')
    };

    this.identityApiSvc.registerAsync(newUser)
      .pipe(catchError(err => {
        this.error.set(err.error);
        this.isLoading.set(false);
        return throwError(() => new Error("Something went wrong: " + err.error));
      }))
      .subscribe(result => {
        console.log(result);
        stepper.next();
      });
  }
}
