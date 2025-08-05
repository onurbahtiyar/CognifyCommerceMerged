import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { NotificationDto } from '../models/notification.dto';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private baseUrl = `${environment.apiUrl}notifications/`;

  private _notifications$ = new BehaviorSubject<NotificationDto[]>([]);
  private _unreadCount$ = new BehaviorSubject<number>(0);

  private _newNotificationArrived$ = new Subject<NotificationDto[]>();

  public notifications$ = this._notifications$.asObservable();
  public unreadCount$ = this._unreadCount$.asObservable();
  public newNotificationArrived$ = this._newNotificationArrived$.asObservable();


  constructor(private http: HttpClient, private router: Router) {
    this.startPolling();
  }

  private startPolling(intervalMs: number = 30000): void {
    this.fetchUnreadNotifications().subscribe();

    setInterval(() => {
      this.fetchUnreadNotifications().subscribe();
    }, intervalMs);
  }

  fetchUnreadNotifications(): Observable<ApiResponse<NotificationDto[]>> {
    return this.http.get<ApiResponse<NotificationDto[]>>(`${this.baseUrl}unread`).pipe(
      tap(response => {
        if (response.success && response.data) {
          const oldNotificationIds = new Set(this._notifications$.getValue().map(n => n.notificationId));

          const newlyArrived = response.data.filter(n => !oldNotificationIds.has(n.notificationId));

          if (newlyArrived.length > 0) {
            this._newNotificationArrived$.next(newlyArrived);
          }

          this._notifications$.next(response.data);
          this._unreadCount$.next(response.data.length);
        } else {
          this._notifications$.next([]);
          this._unreadCount$.next(0);
        }
      })
    );
  }

  markAsRead(notification: NotificationDto): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}${notification.notificationId}/mark-as-read`, {}).pipe(
      tap(response => {
        if (response.success) {
          const currentNotifications = this._notifications$.getValue();
          const updatedNotifications = currentNotifications.filter(n => n.notificationId !== notification.notificationId);
          this._notifications$.next(updatedNotifications);
          this._unreadCount$.next(updatedNotifications.length);

          if (notification.url) {
            this.router.navigateByUrl(notification.url);
          }
        }
      })
    );
  }

  markAllAsRead(): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}mark-all-as-read`, {}).pipe(
      tap(response => {
        if (response.success) {
          this._notifications$.next([]);
          this._unreadCount$.next(0);
        }
      })
    );
  }
}