import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from 'ngx-cookie-service';
import { EventSourcePolyfill } from 'event-source-polyfill';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private baseUrl = environment.apiUrl;

  constructor(private cookieService: CookieService) {}

  public chatStream(prompt: string): Observable<string> {
    return new Observable(observer => {
      const token = this.cookieService.get('authToken');
      if (!token) {
        observer.error(new Error('Authentication token not found.'));
        return;
      }

      const url = `${this.baseUrl}chat/stream?prompt=${encodeURIComponent(prompt)}&access_token=${encodeURIComponent(token)}`;

      const eventSource = new EventSourcePolyfill(url, {
        withCredentials: true,
        heartbeatTimeout: 120000 
      });

      eventSource.onmessage = (event: any) => {
        if (event.data) {
          observer.next(event.data);
        }
      };

      // DÜZELTME: onerror bloğu daha akıllı hale getirildi.
      eventSource.onerror = (error: any) => {
        // `readyState` EventSource'un durumunu belirtir:
        // 0: CONNECTING, 1: OPEN, 2: CLOSED
        
        // Eğer bağlantı kurulmaya çalışırken bir hata olursa, bu gerçek bir ağ hatasıdır.
        if (eventSource.readyState === EventSource.CONNECTING) {
          observer.error(new Error('A network error occurred with the chat stream.'));
        } else {
          // Eğer bağlantı zaten açıksa veya kapalıysa, bu genellikle stream'in
          // normal bir şekilde bittiği anlamına gelir. Hata gösterme, sadece tamamla.
          observer.complete();
        }
        eventSource.close();
      };

      return () => {
        if (eventSource && eventSource.readyState !== EventSource.CLOSED) {
          eventSource.close();
        }
      };
    });
  }
}