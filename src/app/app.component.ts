import { Component, OnInit, ViewContainerRef, AfterViewInit, ViewChild, ElementRef, HostListener } from '@angular/core';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './services/auth.service';
import { ThemeService } from './services/theme.service';
import { NotificationService } from './services/notification.service';
import { CustomToastService } from './services/custom-toast.service';
import { NotificationDto } from './models/notification.dto';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, AfterViewInit {
  isSidebarOpen = false;
  isLogin = false;
  currentUser: { firstName: string; lastName: string } | null = null;
  
  isNotificationPanelOpen = false;
  notifications$!: Observable<NotificationDto[]>;
  unreadCount$!: Observable<number>;

  @ViewChild('toastContainer', { read: ViewContainerRef })
  private toastContainerRef!: ViewContainerRef;

  constructor(
    private cookieService: CookieService,
    private router: Router,
    public authService: AuthService,
    public themeService: ThemeService,
    public notificationService: NotificationService,
    private toastService: CustomToastService,
    private eRef: ElementRef
  ) {}

  ngOnInit(): void {
    this.authService.isLoggedIn$.subscribe((status) => {
      this.isLogin = status;
      if (status) {
        this.loadUserInfo();
        this.notifications$ = this.notificationService.notifications$;
        this.unreadCount$ = this.notificationService.unreadCount$;
        this.listenForNewNotifications();
      } else {
        this.currentUser = null;
        this.isSidebarOpen = false;
      }
    });
  }

  ngAfterViewInit(): void {
    if (this.isLogin) {
      this.toastService.setRootViewContainerRef(this.toastContainerRef);
    }
    this.authService.isLoggedIn$.subscribe(status => {
      if (status && this.toastContainerRef) {
        this.toastService.setRootViewContainerRef(this.toastContainerRef);
      }
    });
  }

  listenForNewNotifications(): void {
    this.notificationService.newNotificationArrived$.subscribe(newNotifications => {
      newNotifications.forEach(notification => {
        this.toastService.show(notification, (notif) => {
          this.notificationService.markAsRead(notif).subscribe();
        });
      });
    });
  }
  
  loadUserInfo(): void {
    const userInfo = this.cookieService.get('userInfo');
    if (userInfo) {
      try {
        const parsed = JSON.parse(userInfo);
        this.currentUser = { firstName: parsed.FirstName, lastName: parsed.LastName };
      } catch { this.currentUser = null; }
    }
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleNotificationPanel(event: MouseEvent): void {
    event.stopPropagation();
    this.isNotificationPanelOpen = !this.isNotificationPanelOpen;
  }

  markNotificationAsRead(notification: NotificationDto): void {
    this.isNotificationPanelOpen = false;
    this.notificationService.markAsRead(notification).subscribe();
  }

  markAllNotificationsAsRead(): void {
    if ((this.unreadCount$ as any).getValue() === 0) return;
    this.isNotificationPanelOpen = false;
    this.notificationService.markAllAsRead().subscribe();
  }

  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.querySelector('#notification-panel-wrapper')?.contains(event.target)) {
      this.isNotificationPanelOpen = false;
    }
  }
}