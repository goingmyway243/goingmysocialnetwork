import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AiAssistRequest, AiAssistResponse } from '../models/ai-assist.model';

@Injectable({ providedIn: 'root' })
export class AiApiService {
  private readonly _http = inject(HttpClient);
  private readonly _baseUrl = `${environment.apiGatewayUrl}/api/posts/ai`;

  assist(request: AiAssistRequest): Observable<AiAssistResponse> {
    return this._http.post<AiAssistResponse>(`${this._baseUrl}/assist`, request);
  }
}
