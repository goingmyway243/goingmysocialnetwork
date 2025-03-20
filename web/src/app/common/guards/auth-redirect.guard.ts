import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { IdentityService } from '../services/identity.service';

@Injectable({
  providedIn: 'root'
})
export class AuthRedirectGuard implements CanActivate {

  constructor(private identityService: IdentityService, private router: Router) {}

  canActivate(): boolean {
    const isAuthenticated = this.identityService.isAuthenticated();

    if (isAuthenticated) {
      this.router.navigate(['/']);
      return false;
    }
    
    return true;
  }
}
