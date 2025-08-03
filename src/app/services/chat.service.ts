import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from 'ngx-cookie-service';
import { EventSourcePolyfill } from 'event-source-polyfill';
import { IResult } from '../models/i-result.model';
import { HttpClient } from '@angular/common/http';
import { ChatSessionDto, ChatMessageDto } from '../models/chat.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private baseUrl = environment.apiUrl + 'chat/';

  constructor(private cookieService: CookieService,
    private http: HttpClient,
  ) {}

  public chatStream(prompt: string, sessionId?: string): Observable<string> {
    return new Observable(observer => {
      const token = this.cookieService.get('authToken');
      if (!token) {
        observer.error(new Error('Authentication token not found.'));
        return;
      }

      // URL dinamik olarak oluşturuluyor.
      let url = `${this.baseUrl}stream?prompt=${encodeURIComponent(prompt)}&access_token=${encodeURIComponent(token)}`;
      if (sessionId) {
        url += `&sessionId=${sessionId}`;
      }

      const eventSource = new EventSourcePolyfill(url, {
        withCredentials: true,
        heartbeatTimeout: 120000 
      });

      eventSource.onmessage = (event: any) => {
        if (event.data) { observer.next(event.data); }
      };

      eventSource.onerror = () => {
        observer.complete();
        eventSource.close();
      };

      return () => { eventSource.close(); };
    });
  }


  public getAllSessions(): Observable<ApiResponse<ChatSessionDto[]>> {
    return this.http.get<ApiResponse<ChatSessionDto[]>>(`${this.baseUrl}sessions`);
  }
  
  public getMessagesBySessionId(sessionId: string): Observable<ApiResponse<ChatMessageDto[]>> {
    return this.http.get<ApiResponse<ChatMessageDto[]>>(`${this.baseUrl}sessions/${sessionId}/messages`);
  }

  public deleteSession(sessionId: string): Observable<IResult> {
    return this.http.delete<IResult>(`${this.baseUrl}sessions/${sessionId}`);
  }

}