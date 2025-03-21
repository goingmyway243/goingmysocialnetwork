import { Component, signal } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { IdentityApiService } from '../../common/services/identity-api.service';
import { ILoginRequest } from '../../common/dtos/identity-api.dto';
import { catchError, throwError } from 'rxjs';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {
  public showError = signal(false);

  public loginForm = new FormGroup({
    username: new FormControl(''),
    password: new FormControl('')
  });

  constructor(private router: Router, private identityApiSvc: IdentityApiService) { }

  public navigateToSignup() {
    this.router.navigate(['/signup']);
  }

  public login(evt: MouseEvent) {
    evt.preventDefault();

    if (this.loginForm.invalid) {
      return;
    }

    const request: ILoginRequest = {
      email: this.loginForm.controls.username.value!,
      password: this.loginForm.controls.password.value!
    }

    this.identityApiSvc.loginAsync(request)
      .pipe(catchError(err => {
        return throwError(() => new Error(err.error));
      }))
      .subscribe(_ => {
        this.router.navigate(['/home']);
      });

  }
}
