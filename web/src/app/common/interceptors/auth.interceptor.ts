import { inject } from '@angular/core';
import { HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { IdentityService } from '../services/identity.service';

export function authInterceptor(req: HttpRequest<any>, next: HttpHandlerFn) {
    const token = inject(IdentityService).getAccessToken();
    if (token) {
        const authReq = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`,
            },
        });
        return next(authReq);
    }

    return next(req);
}